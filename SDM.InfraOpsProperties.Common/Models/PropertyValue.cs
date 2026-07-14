namespace Skyline.DataMiner.SDM.InfraOpsProperties.Models
{
    using System;

    using Skyline.DataMiner.SDM;

    public sealed class PropertyValue : IEquatable<PropertyValue>
    {
        public string PropertyName { get; set; }

        public string Value { get; set; }

        public SdmObjectReference<Property> PropertyId { get; set; }

        public static bool operator ==(PropertyValue left, PropertyValue right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(PropertyValue left, PropertyValue right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PropertyValue);
        }

        public bool Equals(PropertyValue other)
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
                PropertyName == other.PropertyName &&
                Value == other.Value &&
                PropertyId == other.PropertyId;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (PropertyName?.GetHashCode() ?? 0);
                hash = (hash * 23) + (Value?.GetHashCode() ?? 0);
                hash = (hash * 23) + PropertyId.GetHashCode();
                return hash;
            }
        }
    }
}
