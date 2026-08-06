namespace Skyline.DataMiner.SDM.FacilityManagement.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    internal static class FacilityReferenceValidationHelper
    {
        internal static bool ShouldValidateReferences<T>(T entity)
            where T : IEntityTracking
        {
            return entity.IsNew || entity.Changed;
        }

        internal static string GetId<T>(SdmObjectReference<T> reference)
            where T : SdmObject<T>
        {
            return reference.Identifier;
        }

        internal static bool HasId(string identifier)
        {
            return !string.IsNullOrWhiteSpace(identifier);
        }

        internal static bool HasId(Guid identifier)
        {
            return identifier != Guid.Empty;
        }

        internal static HashSet<string> ToIdentifierSet<T>(IEnumerable<T> entities)
            where T : SdmObject<T>
        {
            return (entities ?? Enumerable.Empty<T>())
                .Select(e => e.Identifier)
                .Where(HasId)
                .ToHashSet();
        }

        internal static HashSet<Guid> ToGuidSet(IEnumerable<Guid> identifiers)
        {
            return (identifiers ?? Enumerable.Empty<Guid>())
                .Where(HasId)
                .ToHashSet();
        }

        internal static void AddMissingReference<TEnum>(
            ValidationResult result,
            TEnum field,
            string target,
            string identifier)
            where TEnum : Enum
        {
            result.AddFailReason(field, $"Referenced {target} '{identifier}' does not exist.");
        }

        internal static void AddMissingReference<TEnum>(
            ValidationResult result,
            TEnum field,
            string target,
            Guid identifier)
            where TEnum : Enum
        {
            result.AddFailReason(field, $"Referenced {target} '{identifier}' does not exist.");
        }

        /// <summary>
        /// Resolves which of the supplied ids currently exist. When no lookup is available
        /// (e.g. no external reference checker), every supplied id is treated as existing so the
        /// reference check is effectively skipped instead of reporting false errors.
        /// </summary>
        internal static HashSet<Guid> GetExistingGuidReferences(
            IEnumerable<Guid> ids,
            Func<IReadOnlyCollection<Guid>, IReadOnlyCollection<Guid>> lookup)
        {
            var keys = (ids ?? Enumerable.Empty<Guid>()).Where(HasId).Distinct().ToList();
            if (lookup == null || keys.Count == 0)
            {
                return new HashSet<Guid>(keys);
            }

            return ToGuidSet(lookup(keys));
        }

        /// <summary>
        /// Validates the Room and Resource references shared by the Desk, Row and Zone validators.
        /// Only entities flagged for reference validation with a non-empty reference are checked.
        /// </summary>
        /// <typeparam name="TEntity">The entity type being validated.</typeparam>
        /// <typeparam name="TField">The validation field enum used to report failures.</typeparam>
        /// <param name="entities">The batch of entities to validate.</param>
        /// <param name="field">The field to attribute missing-reference failures to.</param>
        /// <param name="roomIdentifierSelector">Extracts the referenced Room identifier from an entity.</param>
        /// <param name="resourceIdSelector">Extracts the referenced Resource id from an entity.</param>
        /// <param name="lookupExistingRooms">Resolves which of the supplied Room identifiers exist.</param>
        /// <param name="resourceLookup">Resolves which Resource ids exist, or <c>null</c> when unavailable.</param>
        /// <returns>One <see cref="ValidationResult"/> per entity, in input order.</returns>
        internal static List<ValidationResult> ValidateRoomAndResourceReferences<TEntity, TField>(
            List<TEntity> entities,
            TField field,
            Func<TEntity, string> roomIdentifierSelector,
            Func<TEntity, Guid> resourceIdSelector,
            Func<List<string>, HashSet<string>> lookupExistingRooms,
            Func<IReadOnlyCollection<Guid>, IReadOnlyCollection<Guid>> resourceLookup)
            where TEntity : IEntityTracking
            where TField : Enum
        {
            var results = entities.Select(_ => new ValidationResult()).ToList();

            var roomCandidates = entities
                .Select((entity, index) => new { Index = index, Entity = entity, RoomIdentifier = roomIdentifierSelector(entity) })
                .Where(x => ShouldValidateReferences(x.Entity) && HasId(x.RoomIdentifier))
                .Select(x => (x.Index, x.RoomIdentifier))
                .ToList();

            var resourceCandidates = entities
                .Select((entity, index) => new { Index = index, Entity = entity, ResourceId = resourceIdSelector(entity) })
                .Where(x => ShouldValidateReferences(x.Entity) && HasId(x.ResourceId))
                .Select(x => (x.Index, x.ResourceId))
                .ToList();

            var existingRoomIds = roomCandidates.Any()
                ? lookupExistingRooms(roomCandidates.Select(x => x.RoomIdentifier).Distinct().ToList())
                : new HashSet<string>();
            var existingResourceIds = GetExistingGuidReferences(resourceCandidates.Select(x => x.ResourceId), resourceLookup);

            foreach (var (index, roomIdentifier) in roomCandidates)
            {
                if (!existingRoomIds.Contains(roomIdentifier))
                {
                    AddMissingReference(results[index], field, "Room", roomIdentifier);
                }
            }

            foreach (var (index, resourceId) in resourceCandidates)
            {
                if (!existingResourceIds.Contains(resourceId))
                {
                    AddMissingReference(results[index], field, "Resource", resourceId);
                }
            }

            return results;
        }
    }
}
