namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using Newtonsoft.Json;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public class CableRelation : ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }
        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty => CableTypeFks == null || !CableTypeFks.Any(cableTypeFk => cableTypeFk.HasValue());

        public List<SdmObjectReference<CableType>> CableTypeFks { get; set; }
    }
}
