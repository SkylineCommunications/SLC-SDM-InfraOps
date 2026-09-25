namespace Skyline.DataMiner.SDM.Extensions
{
    using System;

    public static partial class SdmObjectExtensions
    {
        public static Guid GetIdentifierAsGuid(this ISdmObject sdmObject)
        {
            if(!Guid.TryParse(sdmObject.Identifier, out var guid))
            {
                throw new InvalidOperationException("The Identifier of the SdmObject is not a valid GUID.");
            }

            if(guid == Guid.Empty)
            {
                throw new InvalidOperationException("The Identifier of the SdmObject is an empty GUID.");
            }

            return guid;
        }
    }
}