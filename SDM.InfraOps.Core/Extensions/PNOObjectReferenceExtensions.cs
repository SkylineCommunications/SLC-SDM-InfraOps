namespace Skyline.DataMiner.SDM.Extensions
{
    using System;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

    public static class PnoObjectReferenceExtensions
    {
        /// <summary>
        /// Checks if the PnoObjectReference has a valid value (not null and identifier is not empty).
        /// </summary>
        public static bool HasValue<T>(this PnoObjectReference<T> reference) where T : ApiObject
        {
            if (reference == null)
            {
                return false;
            }

            return reference.Identifier != Guid.Empty;
        }

        public static Guid GetIdentifierAsGuid<T>(this PnoObjectReference<T> reference) where T : ApiObject
        {
            if (!HasValue(reference))
            {
                throw new InvalidOperationException("The Identifier of the SdmObjectReference is not a valid GUID.");
            }

            var guid = reference.Identifier;

            if (guid == Guid.Empty)
            {
                throw new InvalidOperationException("The Identifier of the SdmObjectReference is an empty GUID.");
            }

            return guid;
        }
    }
}