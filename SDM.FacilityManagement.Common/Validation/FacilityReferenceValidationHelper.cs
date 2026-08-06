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
    }
}
