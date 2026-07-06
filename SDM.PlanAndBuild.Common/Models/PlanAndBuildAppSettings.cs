namespace Skyline.DataMiner.SDM.PlanAndBuild.Models
{
    using Skyline.DataMiner.SDM;

    [GenerateExposers]
    [SdmDomStorage("(slc)plan_and_build")]
    public class PlanAndBuildAppSettings : SdmObject<PlanAndBuildAppSettings>
    {
        public string JobIDPrefix { get; set; }

        public long JobIDNextSequence { get; set; }

        public long JobIDIncrement { get; set; }

        public long JobIDStartingSeed { get; set; }

        public long JobIDMinimumDigits { get; set; }
    }
}
