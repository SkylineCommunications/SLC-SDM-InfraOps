using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharedMappers.DomIds;

using Skyline.DataMiner.SDM.AssetManagement.Models;

namespace Skyline.DataMiner.SDM.AssetManagement.Common.Models
{
    [GenerateExposers]
    [SdmDomStorage("(slc)asset_management")]
    public class Connection : SdmObject<Connection>
    {
        public string Notes { get; set; }

        public string Description { get; set; }

        public SlcAsset_Management.Enums.ConnectionType ConnectionType { get; set; }

        public SdmObjectReference<CableType> CableType { get; set; }

        public SourceInfo Source { get; set; }

        public DestinationInfo Destination { get; set; }
    }
}
