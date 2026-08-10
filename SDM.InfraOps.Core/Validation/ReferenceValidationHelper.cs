namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    /// <summary>
    /// Generic, module-agnostic helpers for validating cross-entity references.
    /// Shared across the InfraOps modules (Facility, Asset, Plan &amp; Build, Properties) so the
    /// identifier/reference plumbing lives in one place instead of being duplicated per module.
    /// </summary>
    internal static class ReferenceValidationHelper
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
    }
}
