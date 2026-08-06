using Skyline.DataMiner.SDM.FacilityManagement.Models;
using System;
using Newtonsoft.Json;
using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    public class RackRelation : ISectionTrackable
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        public SdmObjectReference<Rack> Rack { get; set; }
    } 
}