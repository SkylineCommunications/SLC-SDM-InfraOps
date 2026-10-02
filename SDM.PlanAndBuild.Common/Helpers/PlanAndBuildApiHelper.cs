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

        public void RemoveDeletedAssetReferences(
            AssetDeletionRecoveryContext context)
        {
            if (context == null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

            var assetIdentifier = context.AssetIdentifier;
            var snapshotsById = context.ConnectionSnapshots
                .GroupBy(snapshot => snapshot.ConnectionIdentifier, System.StringComparer.OrdinalIgnoreCase)
                .ToDictionary(group => group.Key, group => group.Last(), System.StringComparer.OrdinalIgnoreCase);

            var jobsToUpdate = new List<PlanAndBuildJob>();
            foreach (var job in new PlanAndBuildJobDomRepository(Connection).Read(new TRUEFilterElement<PlanAndBuildJob>()))
            {
                var changed = false;
                var assetsUsed = job.AssetsUsed ?? new List<JobAsset>();
                if (context.Policy.AssetJobEntryMode == JobEntryRemovalMode.Remove)
                {
                    var remainingAssets = assetsUsed
                        .Where(item => item?.AssetId == null ||
                            !item.AssetId.HasValue() ||
                            !string.Equals(item.AssetId.Identifier, assetIdentifier, System.StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    if (remainingAssets.Count != assetsUsed.Count)
                    {
                        job.AssetsUsed = remainingAssets;
                        changed = true;
                    }
                }
                else
                {
                    foreach (var entry in assetsUsed.Where(item =>
                        item?.AssetId != null &&
                        string.Equals(item.AssetId.Identifier, assetIdentifier, System.StringComparison.OrdinalIgnoreCase)))
                    {
                        var removed = SharedMappers.DomIds.SlcPlan_And_Build.Enums.ActionforassetenumEnum.Removed;
                        if (entry.Action == removed &&
                            entry.AssetName == context.AssetName &&
                            entry.AssetClassName == context.AssetClassName &&
                            entry.IPAddress == context.IPAddress)
                        {
                            continue;
                        }

                        entry.Action = removed;
                        entry.AssetName = context.AssetName;
                        entry.AssetClassName = context.AssetClassName;
                        entry.IPAddress = context.IPAddress;
                        changed = true;
                    }

                    if (changed)
                    {
                        job.AssetsUsed = assetsUsed;
                    }
                }

                var connectionsOnJob = job.ConnectionsOnJob ?? new List<JobConnection>();
                if (connectionsOnJob.Count > 0 && snapshotsById.Count > 0)
                {
                    var connections = connectionsOnJob.ToList();
                    var connectionsChanged = false;
                    foreach (var jobConnection in connections.ToList())
                    {
                        if (jobConnection?.ConnectionId == null ||
                            !jobConnection.ConnectionId.HasValue() ||
                            !snapshotsById.TryGetValue(jobConnection.ConnectionId.Identifier, out var snapshot))
                        {
                            continue;
                        }

                        if (context.Policy.ConnectionJobEntryMode == JobEntryRemovalMode.Remove)
                        {
                            connections.Remove(jobConnection);
                            connectionsChanged = true;
                            continue;
                        }

                        if (string.Equals(jobConnection.Source, snapshot.Source, System.StringComparison.Ordinal) &&
                            string.Equals(jobConnection.Destination, snapshot.Destination, System.StringComparison.Ordinal) &&
                            string.Equals(
                                jobConnection.CableType.Identifier,
                                snapshot.CableTypeIdentifier,
                                System.StringComparison.OrdinalIgnoreCase) &&
                            Equals(jobConnection.CableLength, snapshot.CableLength) &&
                            string.Equals(jobConnection.Status, "Removed", System.StringComparison.Ordinal))
                        {
                            continue;
                        }

                        jobConnection.Source = snapshot.Source;
                        jobConnection.Destination = snapshot.Destination;
                        jobConnection.CableType = new SdmObjectReference<CableType>(
                            snapshot.CableTypeIdentifier);
                        jobConnection.CableLength = snapshot.CableLength;
                        jobConnection.Status = "Removed";
                        connectionsChanged = true;
                    }

                    if (connectionsChanged)
                    {
                        job.ConnectionsOnJob = connections;
                        changed = true;
                    }
                }

                if (changed)
                {
                    jobsToUpdate.Add(job);
                }
            }

            if (jobsToUpdate.Count > 0)
            {
                new PlanAndBuildJobDomRepository(Connection).Update(jobsToUpdate);
            }
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
