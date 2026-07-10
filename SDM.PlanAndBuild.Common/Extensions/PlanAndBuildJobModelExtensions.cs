namespace Skyline.DataMiner.SDM.PlanAndBuild.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.FacilityManagement.Helpers;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;

    /// <summary>
    /// Convenience extension methods for <see cref="PlanAndBuildJob"/> and <see cref="JobOwnership"/>,
    /// mirroring InfraOpsShared's JobWrapper API (Add/Remove/Set/Clear on collections, Has*/Is* checks).
    /// Kept as extension methods so the models stay plain POCOs/change-tracked data holders with no
    /// embedded business logic.
    /// </summary>
    public static class PlanAndBuildJobModelExtensions
    {
        #region JobType / Ownership checks

        /// <summary>
        /// Gets a value indicating whether <see cref="PlanAndBuildJob.Type"/> (the JobType DOM reference)
        /// has been assigned. Note: <see cref="PlanAndBuildJob.JobType"/> is the soft-deleted legacy enum
        /// field and is intentionally not checked here.
        /// </summary>
        public static bool HasJobType(this PlanAndBuildJob job)
        {
            return job != null && job.Type != null;
        }

        /// <summary>
        /// Gets a value indicating whether <see cref="JobOwnership.AssignedTo"/> refers to a Person.
        /// </summary>
        public static bool IsAssignedToPerson(this JobOwnership ownership)
        {
            return ownership?.AssignedTo.HasValue == true && ownership.AssignedTo != Guid.Empty;
        }

        /// <summary>
        /// Gets a value indicating whether <see cref="JobOwnership.AssignmentGroup"/> refers to a Team.
        /// </summary>
        public static bool HasAssignmentGroup(this JobOwnership ownership)
        {
            return ownership?.AssignmentGroup.HasValue == true && ownership.AssignmentGroup != Guid.Empty;
        }

        #endregion

        #region ConnectionsOnJob Convenience Methods

        /// <summary>
        /// Adds a <see cref="JobConnection"/> to <see cref="PlanAndBuildJob.ConnectionsOnJob"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="connectionOnJob"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">A connection with the same <see cref="JobConnection.ConnectionId"/> already exists.</exception>
        public static void AddConnectionsOnJobItem(this PlanAndBuildJob job, JobConnection connectionOnJob)
        {
            if (connectionOnJob == null)
            {
                throw new ArgumentNullException(nameof(connectionOnJob));
            }

            var list = job.ConnectionsOnJob;

            if (list.Any(connection => connection.ConnectionId == connectionOnJob.ConnectionId))
            {
                throw new InvalidOperationException("A Connection with the same Connection Id already exists.");
            }

            list.Add(connectionOnJob);
            job.ConnectionsOnJob = list;
        }

        /// <summary>
        /// Removes the <see cref="JobConnection"/> matching <paramref name="connectionOnJob"/>'s
        /// <see cref="JobConnection.ConnectionId"/> from <see cref="PlanAndBuildJob.ConnectionsOnJob"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="connectionOnJob"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No matching connection was found.</exception>
        public static void RemoveItemFromConnectionsOnJob(this PlanAndBuildJob job, JobConnection connectionOnJob)
        {
            if (connectionOnJob == null)
            {
                throw new ArgumentNullException(nameof(connectionOnJob));
            }

            var list = job.ConnectionsOnJob;
            var found = list.FirstOrDefault(connection => connection.ConnectionId == connectionOnJob.ConnectionId);

            if (found == null)
            {
                throw new ArgumentException("The specified Connection was not found.");
            }

            list.Remove(found);
            job.ConnectionsOnJob = list;
        }

        /// <summary>
        /// Replaces <see cref="PlanAndBuildJob.ConnectionsOnJob"/> with <paramref name="connectionsOnJob"/>.
        /// </summary>
        public static void SetConnectionsOnJob(this PlanAndBuildJob job, List<JobConnection> connectionsOnJob)
        {
            job.ConnectionsOnJob = connectionsOnJob ?? new List<JobConnection>();
        }

        /// <summary>
        /// Clears all entries from <see cref="PlanAndBuildJob.ConnectionsOnJob"/>.
        /// </summary>
        public static void ClearConnectionsOnJob(this PlanAndBuildJob job)
        {
            if (job.ConnectionsOnJob.Count == 0)
            {
                return;
            }

            job.ConnectionsOnJob = new List<JobConnection>();
        }

        #endregion

        #region AssetsUsed Convenience Methods

        /// <summary>
        /// Adds a <see cref="JobAsset"/> to <see cref="PlanAndBuildJob.AssetsUsed"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="assetUsed"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">An asset with the same <see cref="JobAsset.AssetId"/> already exists.</exception>
        public static void AddAssetsUsedItem(this PlanAndBuildJob job, JobAsset assetUsed)
        {
            if (assetUsed == null)
            {
                throw new ArgumentNullException(nameof(assetUsed));
            }

            var list = job.AssetsUsed;

            if (list.Any(asset => asset.AssetId == assetUsed.AssetId))
            {
                throw new InvalidOperationException("An Asset with the same Asset Id already exists.");
            }

            list.Add(assetUsed);
            job.AssetsUsed = list;
        }

        /// <summary>
        /// Removes the <see cref="JobAsset"/> matching <paramref name="assetUsed"/>'s
        /// <see cref="JobAsset.AssetId"/> from <see cref="PlanAndBuildJob.AssetsUsed"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="assetUsed"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No matching asset was found.</exception>
        public static void RemoveItemFromAssetsUsed(this PlanAndBuildJob job, JobAsset assetUsed)
        {
            if (assetUsed == null)
            {
                throw new ArgumentNullException(nameof(assetUsed));
            }

            var list = job.AssetsUsed;
            var found = list.FirstOrDefault(asset => asset.AssetId == assetUsed.AssetId);

            if (found == null)
            {
                throw new ArgumentException("The specified Asset was not found.");
            }

            list.Remove(found);
            job.AssetsUsed = list;
        }

        /// <summary>
        /// Replaces <see cref="PlanAndBuildJob.AssetsUsed"/> with <paramref name="assetsUsed"/>.
        /// </summary>
        public static void SetAssetsUsed(this PlanAndBuildJob job, List<JobAsset> assetsUsed)
        {
            job.AssetsUsed = assetsUsed ?? new List<JobAsset>();
        }

        /// <summary>
        /// Clears all entries from <see cref="PlanAndBuildJob.AssetsUsed"/>.
        /// </summary>
        public static void ClearAssetsUsed(this PlanAndBuildJob job)
        {
            if (job.AssetsUsed.Count == 0)
            {
                return;
            }

            job.AssetsUsed = new List<JobAsset>();
        }

        #endregion

        #region JobAttachment Convenience Methods

        /// <summary>
        /// Adds a <see cref="JobAttachment"/> to <see cref="PlanAndBuildJob.Attachments"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="jobAttachment"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">An attachment with the same <see cref="JobAttachment.FilePath"/> already exists.</exception>
        public static void AddJobAttachment(this PlanAndBuildJob job, JobAttachment jobAttachment)
        {
            if (jobAttachment == null)
            {
                throw new ArgumentNullException(nameof(jobAttachment));
            }

            var list = job.Attachments;

            if (list.Any(attachment => attachment.FilePath == jobAttachment.FilePath))
            {
                throw new InvalidOperationException("An Attachment with the same File Path already exists.");
            }

            list.Add(jobAttachment);
            job.Attachments = list;
        }

        /// <summary>
        /// Removes the <see cref="JobAttachment"/> matching <paramref name="jobAttachment"/>'s
        /// <see cref="JobAttachment.FilePath"/> from <see cref="PlanAndBuildJob.Attachments"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="jobAttachment"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No matching attachment was found.</exception>
        public static void RemoveItemFromJobAttachments(this PlanAndBuildJob job, JobAttachment jobAttachment)
        {
            if (jobAttachment == null)
            {
                throw new ArgumentNullException(nameof(jobAttachment));
            }

            var list = job.Attachments;
            var found = list.FirstOrDefault(attachment => attachment.FilePath == jobAttachment.FilePath);

            if (found == null)
            {
                throw new ArgumentException("The specified Attachment was not found.");
            }

            list.Remove(found);
            job.Attachments = list;
        }

        /// <summary>
        /// Replaces <see cref="PlanAndBuildJob.Attachments"/> with <paramref name="jobAttachments"/>.
        /// </summary>
        public static void SetJobAttachments(this PlanAndBuildJob job, List<JobAttachment> jobAttachments)
        {
            job.Attachments = jobAttachments ?? new List<JobAttachment>();
        }

        /// <summary>
        /// Clears all entries from <see cref="PlanAndBuildJob.Attachments"/>.
        /// </summary>
        public static void ClearJobAttachments(this PlanAndBuildJob job)
        {
            if (job.Attachments.Count == 0)
            {
                return;
            }

            job.Attachments = new List<JobAttachment>();
        }

        #endregion

        #region Locations Convenience Methods

        /// <summary>
        /// Adds a location to <see cref="PlanAndBuildJob.Locations"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">The location already exists.</exception>
        public static void AddLocation(this PlanAndBuildJob job, Guid newLocation)
        {
            var list = job.Locations;

            if (list.Contains(newLocation))
            {
                throw new InvalidOperationException("The specified Location already exists.");
            }

            list.Add(newLocation);
            job.Locations = list;
        }

        /// <summary>
        /// Removes a location from <see cref="PlanAndBuildJob.Locations"/>.
        /// </summary>
        /// <exception cref="ArgumentException">No matching location was found.</exception>
        public static void RemoveLocation(this PlanAndBuildJob job, Guid location)
        {
            var list = job.Locations;

            if (!list.Remove(location))
            {
                throw new ArgumentException("The specified Location was not found.");
            }

            job.Locations = list;
        }

        /// <summary>
        /// Replaces <see cref="PlanAndBuildJob.Locations"/> with <paramref name="locations"/>.
        /// </summary>
        public static void SetLocations(this PlanAndBuildJob job, IEnumerable<Guid> locations)
        {
            job.Locations = locations?.ToList() ?? new List<Guid>();
        }

        #endregion

        #region Locations Resolution

        /// <summary>
        /// Resolves <see cref="PlanAndBuildJob.Locations"/> to their concrete Facility Management DOM instances
        /// (Facility, Floor, Room, Zone, Row, Desk or Rack). Performs live DOM reads against
        /// <paramref name="facilityHelper"/> - up to 7, one per Facility Management type. A location Guid that
        /// doesn't match any of the 7 known types resolves to <see cref="FacilityLocationKind.Unknown"/> with all
        /// typed properties left <c>null</c>.
        /// </summary>
        /// <param name="job">The job whose <see cref="PlanAndBuildJob.Locations"/> should be resolved.</param>
        /// <param name="facilityHelper">Facility Management API helper used to read the Facility, Floor, Room,
        /// Zone, Row, Desk and Rack repositories.</param>
        /// <returns>One <see cref="JobLocation"/> per entry in <see cref="PlanAndBuildJob.Locations"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="facilityHelper"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">A location Guid matched more than one Facility Management type.</exception>
        public static IReadOnlyCollection<JobLocation> ResolveLocations(this PlanAndBuildJob job, IFacilityManagementApiHelper facilityHelper)
        {
            if (facilityHelper == null)
            {
                throw new ArgumentNullException(nameof(facilityHelper));
            }

            var locations = job?.Locations;
            if (locations == null || locations.Count == 0)
            {
                return Array.Empty<JobLocation>();
            }

            return ResolveLocationsCore(locations, facilityHelper);
        }

        /// <summary>
        /// Resolves <see cref="PlanAndBuildJob.Locations"/> for multiple jobs at once. All Guids across
        /// <paramref name="jobs"/> are resolved together (at most 7 DOM reads total, one per Facility Management
        /// type, regardless of how many jobs are supplied), instead of re-querying per job.
        /// </summary>
        /// <param name="jobs">Jobs whose <see cref="PlanAndBuildJob.Locations"/> should be resolved.</param>
        /// <param name="facilityHelper">Facility Management API helper used to read the Facility, Floor, Room,
        /// Zone, Row, Desk and Rack repositories.</param>
        /// <returns>
        /// A list, in the same order as <paramref name="jobs"/>, pairing each job with its resolved
        /// <see cref="JobLocation"/> collection. A list (not a dictionary keyed by job) is used deliberately:
        /// <see cref="PlanAndBuildJob"/> may compare equal by <c>Identifier</c> (e.g. two unsaved jobs with no
        /// Identifier yet), which would make a job-keyed dictionary silently collapse distinct jobs.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="jobs"/> or <paramref name="facilityHelper"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">A location Guid matched more than one Facility Management type.</exception>
        public static IReadOnlyList<KeyValuePair<PlanAndBuildJob, IReadOnlyCollection<JobLocation>>> ResolveLocations(this IEnumerable<PlanAndBuildJob> jobs, IFacilityManagementApiHelper facilityHelper)
        {
            if (jobs == null)
            {
                throw new ArgumentNullException(nameof(jobs));
            }

            if (facilityHelper == null)
            {
                throw new ArgumentNullException(nameof(facilityHelper));
            }

            var jobList = jobs.Where(j => j != null).ToList();
            var allGuids = jobList.SelectMany(j => j.Locations ?? new List<Guid>()).Distinct().ToList();

            var resolvedById = allGuids.Count == 0
                ? new Dictionary<Guid, JobLocation>()
                : ResolveLocationsCore(allGuids, facilityHelper).ToDictionary(jl => jl.Id);

            var result = new List<KeyValuePair<PlanAndBuildJob, IReadOnlyCollection<JobLocation>>>(jobList.Count);
            foreach (var job in jobList)
            {
                var jobLocations = (job.Locations ?? new List<Guid>())
                    .Select(id => resolvedById.TryGetValue(id, out var jl) ? jl : new JobLocation { Id = id, Kind = FacilityLocationKind.Unknown })
                    .ToList();

                result.Add(new KeyValuePair<PlanAndBuildJob, IReadOnlyCollection<JobLocation>>(job, jobLocations));
            }

            return result;
        }

        /// <summary>
        /// Reads the Facility, Floor, Room, Zone, Row, Desk and Rack repositories (one <c>Read</c> call each,
        /// filtered to <paramref name="guids"/>) and builds the corresponding <see cref="JobLocation"/> entries.
        /// </summary>
        private static IReadOnlyCollection<JobLocation> ResolveLocationsCore(IReadOnlyCollection<Guid> guids, IFacilityManagementApiHelper facilityHelper)
        {
            var byId = guids.ToDictionary(g => g, g => new JobLocation { Id = g, Kind = FacilityLocationKind.Unknown });
            var identifiers = guids.Select(g => g.ToString()).ToArray();

            void Apply<T>(IEnumerable<T> matches, FacilityLocationKind kind, Func<T, string> getIdentifier, Action<JobLocation, T> assign)
            {
                foreach (var match in matches)
                {
                    var id = Guid.Parse(getIdentifier(match));
                    if (!byId.TryGetValue(id, out var jobLocation))
                    {
                        continue;
                    }

                    if (jobLocation.Kind != FacilityLocationKind.Unknown)
                    {
                        throw new InvalidOperationException($"Location '{id}' matched more than one Facility Management type ({jobLocation.Kind} and {kind}).");
                    }

                    jobLocation.Kind = kind;
                    assign(jobLocation, match);
                }
            }

            Apply(
                facilityHelper.Facilities.Read(new ORFilterElement<Facility>(identifiers.Select(FacilityExposers.Identifier.Equal).ToArray())),
                FacilityLocationKind.Facility, obj => obj.Identifier, (jl, obj) => jl.Facility = obj);

            Apply(
                facilityHelper.Floors.Read(new ORFilterElement<Floor>(identifiers.Select(FloorExposers.Identifier.Equal).ToArray())),
                FacilityLocationKind.Floor, obj => obj.Identifier, (jl, obj) => jl.Floor = obj);

            Apply(
                facilityHelper.Rooms.Read(new ORFilterElement<Room>(identifiers.Select(RoomExposers.Identifier.Equal).ToArray())),
                FacilityLocationKind.Room, obj => obj.Identifier, (jl, obj) => jl.Room = obj);

            Apply(
                facilityHelper.Zones.Read(new ORFilterElement<Zone>(identifiers.Select(ZoneExposers.Identifier.Equal).ToArray())),
                FacilityLocationKind.Zone, obj => obj.Identifier, (jl, obj) => jl.Zone = obj);

            Apply(
                facilityHelper.Rows.Read(new ORFilterElement<Row>(identifiers.Select(RowExposers.Identifier.Equal).ToArray())),
                FacilityLocationKind.Row, obj => obj.Identifier, (jl, obj) => jl.Row = obj);

            Apply(
                facilityHelper.Desks.Read(new ORFilterElement<Desk>(identifiers.Select(DeskExposers.Identifier.Equal).ToArray())),
                FacilityLocationKind.Desk, obj => obj.Identifier, (jl, obj) => jl.Desk = obj);

            Apply(
                facilityHelper.Racks.Read(new ORFilterElement<Rack>(identifiers.Select(RackExposers.Identifier.Equal).ToArray())),
                FacilityLocationKind.Rack, obj => obj.Identifier, (jl, obj) => jl.Rack = obj);

            return byId.Values.ToList();
        }

        #endregion
    }
}
