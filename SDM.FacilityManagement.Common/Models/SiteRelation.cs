namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public class SiteRelation : ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty =>
            !Site.HasValue();

        public SdmObjectReference<Site> Site { get; set; }
    }
}
