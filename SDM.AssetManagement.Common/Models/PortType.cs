namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public class PortType : SdmObject<PortType>
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public CategoryRelation CategoryLinks { get; set; }

        public CableRelation CableFKs { get; set; }
    }
}
