namespace Skyline.DataMiner.SDM.Extensions
{
    using System;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;

    public static class IObjectReferenceExtensions
    {
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