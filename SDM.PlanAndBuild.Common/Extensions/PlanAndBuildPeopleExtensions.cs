namespace Skyline.DataMiner.SDM.PlanAndBuild.Extensions
{
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

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
    }
}
