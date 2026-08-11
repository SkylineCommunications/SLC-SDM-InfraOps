namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using static Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations.ReferenceValidationHelper;

    internal static class FacilityReferenceValidationHelper
    {
        /// <summary>
        /// Validates the Room references shared by the Desk, Row and Zone validators.
        /// Only entities flagged for reference validation with a non-empty reference are checked.
        /// </summary>
        /// <typeparam name="TEntity">The entity type being validated.</typeparam>
        /// <typeparam name="TField">The validation field enum used to report failures.</typeparam>
        /// <param name="entities">The batch of entities to validate.</param>
        /// <param name="field">The field to attribute missing-reference failures to.</param>
        /// <param name="roomIdentifierSelector">Extracts the referenced Room identifier from an entity.</param>
        /// <param name="lookupExistingRooms">Resolves which of the supplied Room identifiers exist.</param>
        /// <returns>One <see cref="ValidationResult"/> per entity, in input order.</returns>
        internal static List<ValidationResult> ValidateRoomReferences<TEntity, TField>(
            List<TEntity> entities,
            TField field,
            Func<TEntity, string> roomIdentifierSelector,
            Func<List<string>, HashSet<string>> lookupExistingRooms)
            where TEntity : IEntityTracking
            where TField : Enum
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            var roomCandidates = entities
                .Select((entity, index) => new { Index = index, Entity = entity, RoomIdentifier = roomIdentifierSelector(entity) })
                .Where(x => ShouldValidateReferences(x.Entity) && HasId(x.RoomIdentifier))
                .Select(x => (x.Index, x.RoomIdentifier))
                .ToList();

            var existingRoomIds = roomCandidates.Any()
                ? lookupExistingRooms(roomCandidates.Select(x => x.RoomIdentifier).Distinct().ToList())
                : new HashSet<string>();

            foreach (var (index, roomIdentifier) in roomCandidates)
            {
                if (!existingRoomIds.Contains(roomIdentifier))
                {
                    AddMissingReference(results[index], field, "Room", roomIdentifier);
                }
            }

            return results;
        }
    }
}
