namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public class CableType : SdmObject<CableType>, IReadOnlyModuleIdReferencer
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public CategoryRelation CategoryLinks { get; set; }

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? CableTypePropertiesSectionId { get; set; }

        #endregion

        #region Module Tracking

        public string ModuleId => SlcAsset_Management.ModuleId;

        #endregion

    }
}
