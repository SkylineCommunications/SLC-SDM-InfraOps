namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    /// <summary>
    /// Marker for port models (<see cref="DataPort"/> and <see cref="PowerPort"/>).
    /// Used to build filters on fields shared by both DOM definitions via <see cref="PortExposers"/>.
    /// </summary>
    public interface IPort : ISdmObject
    {
        IPortInfo PortInfo { get; }

        SdmObjectReference<Asset> Asset { get; }
    }
}