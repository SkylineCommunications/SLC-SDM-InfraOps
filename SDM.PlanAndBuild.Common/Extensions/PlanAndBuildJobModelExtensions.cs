namespace Skyline.DataMiner.SDM.PlanAndBuild.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

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
        /// Gets a value indicating whether <see cref="PlanAndBuildJob.JobType"/> has been assigned.
        /// </summary>
        public static bool HasJobType(this PlanAndBuildJob job)
        {
            return job != null && job.JobType != null;
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
        /// Adds a location to <see cref="PlanAndBuildJob.LocationGuids"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">The location already exists.</exception>
        public static void AddLocation(this PlanAndBuildJob job, Guid newLocation)
        {
            var list = job.LocationGuids;

            if (list.Contains(newLocation))
            {
                throw new InvalidOperationException("The specified Location already exists.");
            }

            list.Add(newLocation);
            job.LocationGuids = list;
        }

        /// <summary>
        /// Removes a location from <see cref="PlanAndBuildJob.LocationGuids"/>.
        /// </summary>
        /// <exception cref="ArgumentException">No matching location was found.</exception>
        public static void RemoveLocation(this PlanAndBuildJob job, Guid location)
        {
            var list = job.LocationGuids;

            if (!list.Remove(location))
            {
                throw new ArgumentException("The specified Location was not found.");
            }

            job.LocationGuids = list;
        }

        /// <summary>
        /// Replaces <see cref="PlanAndBuildJob.LocationGuids"/> with <paramref name="locations"/>.
        /// </summary>
        public static void SetLocations(this PlanAndBuildJob job, IEnumerable<Guid> locations)
        {
            job.LocationGuids = locations?.ToList() ?? new List<Guid>();
        }

        #endregion
    }
}
