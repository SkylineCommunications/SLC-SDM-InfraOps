using System.Collections.Generic;

using SharedMappers.DomIds;
using System;
using Newtonsoft.Json;
using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    public class CategoryRelation : ISectionTrackable
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        public List<SlcAsset_Management.Enums.CategoriesEnum> Categories { get; set; }
    }
}
