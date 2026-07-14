namespace SharedCommonLibrary.AssetManagement.Models
{
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public class InfraopsReservation : SdmObject<InfraopsReservation>
    {
        public string Description { get; set; }

        public JobRelation JobFk { get; set; }

        public RackRelation RackFk { get; set; }

        public List<InfraopsReservationBounderies> ReservedPositions { get; set; }
    }
}
