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

            if(!Guid.TryParse(reference.Identifier, out var guid))
            {
                return false;
            }

            return guid != Guid.Empty;
        }

        public static Guid GetIdentifierAsGuid<T>(this SdmObjectReference<T> reference) where T : SdmObject<T>
        {
            if(string.IsNullOrWhiteSpace(reference.Identifier))
            {
                return Guid.Empty;
            }

            if (!Guid.TryParse(reference.Identifier, out var guid))
            {
                throw new InvalidOperationException("The Identifier of the SdmObjectReference is not a valid GUID.");
            }

            if (guid == Guid.Empty)
            {
                throw new InvalidOperationException("The Identifier of the SdmObjectReference is an empty GUID.");
            }

            return guid;
        }
    }
}
