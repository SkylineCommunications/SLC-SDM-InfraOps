namespace Skyline.DataMiner.SDM.PlanAndBuild.Helpers
{
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Deletion;
    using Skyline.DataMiner.SDM.AssetManagement.Helpers;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.PlanAndBuild.Deletion;
    using Skyline.DataMiner.SDM.FacilityManagement.Helpers;
    using Skyline.DataMiner.SDM.PlanAndBuild.Middleware;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.SDM.PlanAndBuild.Validation;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;

    public class PlanAndBuildApiHelper : IPlanAndBuildApiHelper, IAssetDeletionJobCleanup
    {
        public PlanAndBuildApiHelper(IConnection connection)
            : this(
                connection,
                connection.GetPeopleAndOrganizationsApi(),
                new PlanAndBuildExternalReferenceChecker(
                    connection,
                    new FacilityManagementApiHelper(connection)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlanAndBuildApiHelper"/> class with an explicit
        /// <see cref="IPeopleAndOrganizationsApi"/> instance. Mainly intended for unit tests, where a mocked
        /// People &amp; Organizations API is supplied instead of the real one resolved from <paramref name="connection"/>.
        /// </summary>
        internal PlanAndBuildApiHelper(
            IConnection connection,
            IPeopleAndOrganizationsApi peopleApi,
            IPlanAndBuildExternalReferenceChecker externalReferenceChecker = null)
        {
            Connection = connection;
            PandOApiHelper = peopleApi;

            // Raw repositories - used internally by validators to query other entities (e.g. uniqueness/in-use checks).
            var jobRepository = new PlanAndBuildJobDomRepository(connection);
            var jobTypeRepository = new JobTypeDomRepository(connection);
            var appSettingsRepository = new PlanAndBuildAppSettingsDomRepository(connection);

            var jobValidator = new PlanAndBuildJobValidator(this, PandOApiHelper, externalReferenceChecker);
            var jobTypeValidator = new JobTypeValidator(this);
            var appSettingsValidator = new PlanAndBuildAppSettingsValidator();

            // Wired so UpdateAndTransitionTo/TransitionAndUpdate (which call this repository's own internal
            // Update() directly, bypassing PlanAndBuildJobValidationMiddleware) still enforce business-rule
            // validation on the field updates they persist.
            jobRepository.Validator = jobValidator;

            Jobs = jobRepository
                .WithMiddleware(new PlanAndBuildJobValidationMiddleware(jobValidator))
                .WithMiddleware(new JobIdAllocationMiddleware(this))
                .WithMiddleware(new IdentifierMiddleware<PlanAndBuildJob>());

            JobTypes = jobTypeRepository
                .WithMiddleware(new JobTypeValidationMiddleware(jobTypeValidator))
                .WithMiddleware(new IdentifierMiddleware<JobType>());

            AppSettings = appSettingsRepository;

            JobValidator = jobValidator;
            JobTypeValidator = jobTypeValidator;
            AppSettingsValidator = appSettingsValidator;
        }

        void IAssetDeletionJobCleanup.RemoveDeletedAssetReferences(
            AssetDeletionRecoveryContext context)
        {
            if (context == null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

            var snapshotsById = context.ConnectionSnapshots
                .GroupBy(snapshot => snapshot.ConnectionIdentifier, System.StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.Last(), System.StringComparer.OrdinalIgnoreCase);

            // System cleanup bypasses user-edit validation so references can also be cleaned
            // on Resolved or Cancelled Jobs, whose Asset and Connection collections are read-only to users.
            var repository = new PlanAndBuildJobDomRepository(Connection);
            var jobsToUpdate = new List<PlanAndBuildJob>();
            foreach (var job in ReadJobsForDeletedAsset(repository, context.AssetIdentifier, snapshotsById.Keys))
            {
                var assetsChanged = AdjustAssetsUsed(job, context);
                var connectionsChanged = AdjustConnectionsOnJob(job, context.Policy.ConnectionJobEntryMode, snapshotsById);
                if (assetsChanged || connectionsChanged)
                {
                    jobsToUpdate.Add(job);
                }
            }

            if (jobsToUpdate.Count > 0)
            {
                repository.Update(jobsToUpdate);
            }
        }

        private static List<PlanAndBuildJob> ReadJobsForDeletedAsset(
            PlanAndBuildJobDomRepository repository,
            string assetIdentifier,
            IEnumerable<string> connectionIdentifiers)
        {
            var assetJobs = repository.Read(
                PlanAndBuildJobExposers.AssetsUsed.AssetId.Contains(new SdmObjectReference<Asset>(assetIdentifier)));
            var connectionJobs = repository.ReadByBigOrFilter(
                connectionIdentifiers,
                identifier => PlanAndBuildJobExposers.ConnectionsOnJob.ConnectionId.Contains(
                    new SdmObjectReference<AssetManagement.Models.Connection>(identifier)));

            return assetJobs.Concat(connectionJobs)
                .GroupBy(job => job.Identifier, System.StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();
        }

        private static bool AdjustAssetsUsed(PlanAndBuildJob job, AssetDeletionRecoveryContext context)
        {
            var assetsUsed = job.AssetsUsed ?? new List<JobAsset>();
            if (context.Policy.AssetJobEntryMode == JobEntryRemovalMode.Remove)
            {
                var remainingAssets = assetsUsed
                    .Where(item => !ReferencesDeletedAsset(item, context.AssetIdentifier))
                    .ToList();
                if (remainingAssets.Count == assetsUsed.Count)
                {
                    return false;
                }

                job.AssetsUsed = remainingAssets;
                return true;
            }

            var changed = false;
            foreach (var entry in assetsUsed.Where(item => ReferencesDeletedAsset(item, context.AssetIdentifier)))
            {
                changed |= UpdateRemovedAssetSnapshot(entry, context);
            }

            if (changed)
            {
                job.AssetsUsed = assetsUsed;
            }

            return changed;
        }

        private static bool ReferencesDeletedAsset(JobAsset entry, string assetIdentifier)
        {
            return entry?.AssetId != null &&
                entry.AssetId.HasValue() &&
                string.Equals(entry.AssetId.Identifier, assetIdentifier, System.StringComparison.OrdinalIgnoreCase);
        }

        private static bool UpdateRemovedAssetSnapshot(JobAsset entry, AssetDeletionRecoveryContext context)
        {
            var removed = SharedMappers.DomIds.SlcPlan_And_Build.Enums.ActionforassetenumEnum.Removed;
            if (entry.Action == removed &&
                entry.AssetName == context.AssetName &&
                entry.AssetClassName == context.AssetClassName &&
                entry.IPAddress == context.IPAddress)
            {
                return false;
            }

            entry.Action = removed;
            entry.AssetName = context.AssetName;
            entry.AssetClassName = context.AssetClassName;
            entry.IPAddress = context.IPAddress;
            return true;
        }

        private static bool AdjustConnectionsOnJob(
            PlanAndBuildJob job,
            JobEntryRemovalMode mode,
            IReadOnlyDictionary<string, AssetDeletionConnectionSnapshot> snapshotsById)
        {
            var connections = job.ConnectionsOnJob?.ToList() ?? new List<JobConnection>();
            var changed = false;
            foreach (var entry in connections.ToList())
            {
                if (entry?.ConnectionId == null ||
                    !entry.ConnectionId.HasValue() ||
                    !snapshotsById.TryGetValue(entry.ConnectionId.Identifier, out var snapshot))
                {
                    continue;
                }

                if (mode == JobEntryRemovalMode.Remove)
                {
                    connections.Remove(entry);
                    changed = true;
                    continue;
                }

                changed |= UpdateRemovedConnectionSnapshot(entry, snapshot);
            }

            if (changed)
            {
                job.ConnectionsOnJob = connections;
            }

            return changed;
        }

        private static bool UpdateRemovedConnectionSnapshot(
            JobConnection entry,
            AssetDeletionConnectionSnapshot snapshot)
        {
            if (string.Equals(entry.Source, snapshot.Source, System.StringComparison.Ordinal) &&
                string.Equals(entry.Destination, snapshot.Destination, System.StringComparison.Ordinal) &&
                string.Equals(entry.CableType.Identifier, snapshot.CableTypeIdentifier, System.StringComparison.OrdinalIgnoreCase) &&
                Equals(entry.CableLength, snapshot.CableLength) &&
                string.Equals(entry.Status, "Removed", System.StringComparison.Ordinal))
            {
                return false;
            }

            entry.Source = snapshot.Source;
            entry.Destination = snapshot.Destination;
            entry.CableType = new SdmObjectReference<CableType>(snapshot.CableTypeIdentifier);
            entry.CableLength = snapshot.CableLength;
            entry.Status = "Removed";
            return true;
        }

        public IConnection Connection { get; }

        internal IPeopleAndOrganizationsApi PandOApiHelper { get; }

        public IPlanAndBuildJobRepository Jobs { get; }

        public IJobTypeRepository JobTypes { get; }

        public IBulkRepository<PlanAndBuildAppSettings> AppSettings { get; }

        public PlanAndBuildJobValidator JobValidator { get; }

        public JobTypeValidator JobTypeValidator { get; }

        public PlanAndBuildAppSettingsValidator AppSettingsValidator { get; }
    }
}
