using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skyline.DataMiner.SDM.AssetManagement.Common.Models
{
    [GenerateExposers]
    [SdmDomStorage("(slc)asset_management")]
    internal class TestModelEnum:SdmObject<TestModelEnum>
    {
        
        public MyTestEnum ThisIsAnEnum { get; set; }
    }

    public enum  MyTestEnum
    {
        A,
        B
    }
}
