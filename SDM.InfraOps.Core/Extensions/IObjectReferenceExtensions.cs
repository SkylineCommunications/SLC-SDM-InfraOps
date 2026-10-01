namespace Skyline.DataMiner.SDM.Extensions
{
    using System;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;

    public static class IObjectReferenceExtensions
    {
        //
        // Summary:
        //     Creates a filter that checks if the exposed Reference field has a value.
        //
        // Parameters:
        //   exposer:
        //     The exposer that identifies the field to filter on.
        //
        // Type parameters:
        //   TFilter:
        //     The type of the filter.
        //
        //   TField:
        //     The non-nullable value type being compared.
        //
        // Returns:
        //     A Skyline.DataMiner.Net.Messages.SLDataGateway.ManagedFilter`2 that matches when
        //     the field is not null.
        public static ManagedFilter<TFilter, ISdmObjectReference<TField>> HasValue<TFilter, TField>(this Exposer<TFilter, ISdmObjectReference<TField>> exposer) where TFilter : class where TField : ISdmObject
        {
            return new ManagedFilter<TFilter, ISdmObjectReference<TField>>(exposer, Comparer.NotEquals, default, (TFilter obj) => exposer.internalFunc(obj).HasValue());
        }

        //
        // Summary:
        //     Creates a filter that checks if the exposed Reference field has no value.
        //
        // Parameters:
        //   exposer:
        //     The exposer that identifies the field to filter on.
        //
        // Type parameters:
        //   TFilter:
        //     The type of the filter.
        //
        //   TField:
        //     The non-nullable value type being compared.
        //
        // Returns:
        //     A Skyline.DataMiner.Net.Messages.SLDataGateway.ManagedFilter`2 that matches when
        //     the field is null.
        public static ManagedFilter<TFilter, ISdmObjectReference<TField>> HasNoValue<TFilter, TField>(this Exposer<TFilter, ISdmObjectReference<TField>> exposer) where TFilter : class where TField : ISdmObject
        {
            return new ManagedFilter<TFilter, ISdmObjectReference<TField>>(exposer, Comparer.Equals, default, (TFilter obj) => !exposer.internalFunc(obj).HasValue());
        }

        /// <summary>
        /// Checks if the SdmObjectReference has a valid value (not null and identifier is not empty).
        /// </summary>
        public static bool HasValue<T>(this IObjectReference<T> reference)
        {
            if(!TryGetIdentifierAsGuid(reference, out var guid))
            {
                return false;
            }

            if (guid == Guid.Empty)
            {
                return false;
            }

            return true;
        }

        public static bool TryGetIdentifierAsGuid<T>(this IObjectReference<T> reference, out Guid guid)
        {
            guid = Guid.Empty;
            if (reference == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(reference.Identifier))
            {
                return false;
            }

            if (!Guid.TryParse(reference.Identifier, out guid))
            {
                return false;
            }

            return true;
        }

        public static Guid GetIdentifierAsGuid<T>(this IObjectReference<T> reference)
        {
            if (!TryGetIdentifierAsGuid(reference, out var guid))
            {
                throw new InvalidOperationException("The Identifier of the Reference is not a valid GUID.");
            }

            if (guid == Guid.Empty)
            {
                throw new InvalidOperationException("The Identifier of the Reference is an empty GUID.");
            }

            return guid;
        }
    }
}