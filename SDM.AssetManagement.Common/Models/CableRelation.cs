namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System.Collections.Generic;

    public class CableRelation
    {
        public List<SdmObjectReference<CableType>> CableTypeFks { get; set; }
    }
}
