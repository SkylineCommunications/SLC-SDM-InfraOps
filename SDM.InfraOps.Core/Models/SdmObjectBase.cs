namespace Skyline.DataMiner.SDM.InfraOps.Core.Models
{
    using System;

    public abstract class SdmObjectBase<T> : SdmObject<T> where T : SdmObjectBase<T>
    {
        public DateTime CreatedAt { get; internal set; }

        public string CreatedBy { get; internal set; }

        public DateTime LastModified { get; internal set; }

        public string LastModifiedBy { get; internal set; }
    }
}