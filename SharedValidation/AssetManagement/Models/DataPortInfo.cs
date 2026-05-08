using System;
namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;

    public class DataPortInfo : IEquatable<DataPortInfo>
    {
        public string Name { get; set; }

        public long PortNumber { get; set; }

        public SlcAsset_Management.Enums.Outputtype OutputType { get; set; }

        public SlcAsset_Management.Enums.PortExposureEnum PortExposure { get; set; }

        public SdmObjectReference<PortType> Type { get; set; }

        public string Label { get; set; }

        public bool Equals(DataPortInfo other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return
                string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
                PortNumber == other.PortNumber &&
                OutputType == other.OutputType &&
                PortExposure == other.PortExposure &&
                Type == other.Type &&
                string.Equals(Label, other.Label, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DataPortInfo);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (2 << 12) - 1;
                hash = (hash * 23) + (Name != null ? Name.GetHashCode() : 0);
                hash = (hash * 23) + PortNumber.GetHashCode();
                hash = (hash * 23) + OutputType.GetHashCode();
                hash = (hash * 23) + PortExposure.GetHashCode();
                hash = (hash * 23) + Type.GetHashCode();
                hash = (hash * 23) + (Label != null ? Label.GetHashCode() : 0);
                return hash;
            }
        }
    }
}
