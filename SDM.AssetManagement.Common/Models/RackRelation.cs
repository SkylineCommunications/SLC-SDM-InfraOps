using Skyline.DataMiner.SDM.Extensions;
using Skyline.DataMiner.SDM.FacilityManagement.Models;
using System;
using Newtonsoft.Json;
using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    public class RackRelation : ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }
        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty => !Rack.HasValue();

        public SdmObjectReference<Rack> Rack { get; set; }
    } 
}