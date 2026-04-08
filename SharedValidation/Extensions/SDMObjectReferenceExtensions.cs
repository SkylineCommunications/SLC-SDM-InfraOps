namespace Skyline.DataMiner.SDM.Extensions
{
    using System;

    using Skyline.DataMiner.SDM;

    public static class SdmObjectReferenceExtensions
    {
        /// <summary>
        /// Checks if the SdmObjectReference has a valid value (not null and identifier is not empty).
        /// </summary>
        public static bool HasValue<T>(this SdmObjectReference<T> reference) where T : SdmObject<T>
        {
            if (reference == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(reference.Identifier))
            {
                return false;
            }

            // Check if identifier is a valid GUID and not empty
            if (Guid.TryParse(reference.Identifier, out var guid))
            {
                return guid != Guid.Empty;
            }

            // For non-GUID identifiers, just check it's not empty
            return true;
        }

        /// <summary>
        /// Checks if the SdmObjectReference does not have a valid value.
        /// </summary>
        public static bool IsEmpty<T>(this SdmObjectReference<T> reference) where T : SdmObject<T>
        {
            return !HasValue(reference);
        }
    }
}
