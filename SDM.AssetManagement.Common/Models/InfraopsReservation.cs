namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM;
    using System;
    using Newtonsoft.Json;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public class InfraopsReservation : SdmObject<InfraopsReservation>
    {
        [JsonIgnore]
        private RackRelation _rackFk;

        public string Description { get; set; }

        public JobRelation JobFk { get; set; }

        public RackRelation RackFk => _rackFk ?? (_rackFk = new RackRelation());

        public List<InfraopsReservationBounderies> ReservedPositions { get; set; }

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? ReservationPropertiesSectionId { get; set; }

        #endregion

    }
}
