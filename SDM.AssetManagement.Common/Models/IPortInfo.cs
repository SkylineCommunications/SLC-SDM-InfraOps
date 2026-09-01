namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using SharedMappers.DomIds;

    public interface IPortInfo
    {
        string Name { get; }

        SdmObjectReference<PortType> PortType { get; }

        long? PortNumber { get; }

        SlcAsset_Management.Enums.Outputtype? OutputType { get; }

        SlcAsset_Management.Enums.PortExposureEnum PortExposure { get; }

        string Label { get; }
    }
}