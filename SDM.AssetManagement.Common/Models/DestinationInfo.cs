using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Skyline.DataMiner.SDM.AssetManagement.Models;

namespace Skyline.DataMiner.SDM.AssetManagement.Common.Models
{
    public class DestinationInfo
    {
        public string CableTag { get; set; }

        public Guid Port { get; set; }

        public SdmObjectReference<PortType> PortType { get; set; }
    }
}
