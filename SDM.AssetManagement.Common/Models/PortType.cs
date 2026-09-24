namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public class PortType : SdmObject<PortType>, IReadOnlyModuleIdReferencer
    {
        [JsonIgnore]
        private CategoryRelation _categoryLinks;
        [JsonIgnore]
        private CableRelation _cableFKs;

        public string Name { get; set; }

        public string Description { get; set; }

        public CategoryRelation CategoryLinks => _categoryLinks ?? (_categoryLinks = new CategoryRelation());

        public CableRelation CableFKs => _cableFKs ?? (_cableFKs = new CableRelation());

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? PortTypePropertiesSectionId { get; set; }

        #endregion

        #region Module Tracking

        public string ModuleId => SlcAsset_Management.ModuleId;

        #endregion

    }
}
