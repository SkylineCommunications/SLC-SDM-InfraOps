namespace Skyline.DataMiner.SDM.PlanAndBuild.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Extensions;

    /// <summary>
    /// Extension methods that resolve the Person/Team referenced by <see cref="JobOwnership"/> and
    /// <see cref="JobAttachment"/> Guid fields via the People &amp; Organizations API.
    /// Kept as extension methods (rather than model properties) so <see cref="JobOwnership"/>/
    /// <see cref="JobAttachment"/> remain plain POCOs with no embedded service dependency.
    /// </summary>
    public static class PlanAndBuildPeopleExtensions
    {
        /// <summary>
        /// Resolves the <see cref="Person"/> referenced by <see cref="JobOwnership.AssignedTo"/>, or <c>null</c>
        /// if not set or no matching Person exists.
        /// </summary>
        public static Person GetAssignedToPerson(this JobOwnership ownership, IPeopleAndOrganizationsApi peopleApi)
        {
            if (ownership?.AssignedTo == null || peopleApi == null)
            {
                return null;
            }

            return peopleApi.People.Read(ownership.AssignedTo.Value);
        }

        /// <summary>
        /// Resolves the <see cref="Team"/> referenced by <see cref="JobOwnership.AssignmentGroup"/>, or <c>null</c>
        /// if not set or no matching Team exists.
        /// </summary>
        public static Team GetAssignmentGroupTeam(this JobOwnership ownership, IPeopleAndOrganizationsApi peopleApi)
        {
            if (ownership?.AssignmentGroup == null || peopleApi == null)
            {
                return null;
            }

            return peopleApi.Teams.Read(ownership.AssignmentGroup.Value);
        }

        /// <summary>
        /// Resolves the <see cref="Person"/> referenced by <see cref="JobAttachment.AttachedBy"/>, or <c>null</c>
        /// if not set or no matching Person exists.
        /// </summary>
        public static Person GetAttachedByPerson(this JobAttachment attachment, IPeopleAndOrganizationsApi peopleApi)
        {
            if (attachment?.AttachedBy == null || peopleApi == null)
            {
                return null;
            }

            return peopleApi.People.Read(attachment.AttachedBy.Value);
        }

        /// <summary>
        /// Reads the ids of all existing People matching any of the given <paramref name="personIds"/>, using a
        /// single batched big-OR query (see <see cref="BulkRepositoryQueryExtensions.ReadByBigOrFilter{T, TKey}"/>)
        /// instead of one existence-check query per id. Used for bulk validation of
        /// <see cref="JobOwnership.AssignedTo"/>/<see cref="JobAttachment.AttachedBy"/> instead of looping
        /// <c>peopleApi.People.Count(...)</c> once per candidate id.
        /// </summary>
        /// <param name="peopleApi">The People &amp; Organizations API to query.</param>
        /// <param name="personIds">The Person ids to look up. Duplicates are handled gracefully.</param>
        public static HashSet<Guid> GetExistingPersonIds(this IPeopleAndOrganizationsApi peopleApi, IEnumerable<Guid> personIds)
        {
            if (peopleApi == null)
            {
                throw new ArgumentNullException(nameof(peopleApi));
            }

            var keys = personIds?.Distinct().ToList() ?? new List<Guid>();

            return peopleApi.People
                .ReadByBigOrFilter(keys, id => PersonExposers.Id.Equal(id))
                .Select(p => p.Id)
                .ToHashSet();
        }

        /// <summary>
        /// Reads the ids of all existing Teams matching any of the given <paramref name="teamIds"/>, using a
        /// single batched big-OR query (see <see cref="BulkRepositoryQueryExtensions.ReadByBigOrFilter{T, TKey}"/>)
        /// instead of one existence-check query per id. Used for bulk validation of
        /// <see cref="JobOwnership.AssignmentGroup"/> instead of looping <c>peopleApi.Teams.Count(...)</c> once
        /// per candidate id.
        /// </summary>
        /// <param name="peopleApi">The People &amp; Organizations API to query.</param>
        /// <param name="teamIds">The Team ids to look up. Duplicates are handled gracefully.</param>
        public static HashSet<Guid> GetExistingTeamIds(this IPeopleAndOrganizationsApi peopleApi, IEnumerable<Guid> teamIds)
        {
            if (peopleApi == null)
            {
                throw new ArgumentNullException(nameof(peopleApi));
            }

            var keys = teamIds?.Distinct().ToList() ?? new List<Guid>();

            return peopleApi.Teams
                .ReadByBigOrFilter(keys, id => TeamExposers.Id.Equal(id))
                .Select(t => t.Id)
                .ToHashSet();
        }
    }
}
