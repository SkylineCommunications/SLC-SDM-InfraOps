namespace Skyline.DataMiner.SDM.InfraOpsProperties.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class PropertyLayout : ChangeTrackingBase, IEquatable<PropertyLayout>, ISectionTrackable, ISectionEmptyState
    {
        /// <summary>
        /// Gets or sets the DOM Section ID this model was read from, so it can be reused on update.
        /// </summary>
        [JsonIgnore]
        [SdmIgnore]
        System.Guid? ISectionTrackable.SectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty => SectionName == default && Order == default;

        public string SectionName
        {
            get => SectionNameField.Value;
            set => SectionNameField.Value = value;
        }

        public long? Order
        {
            get => OrderField.Value;
            set => OrderField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> SectionNameField => FieldHandler.GetOrCreateField(
            nameof(SectionName),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<long?> OrderField => FieldHandler.GetOrCreateField(
            nameof(Order),
            () => new ChangeTrackingField<long?>(null));

        public static bool operator ==(PropertyLayout left, PropertyLayout right)
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

        public static bool operator !=(PropertyLayout left, PropertyLayout right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PropertyLayout);
        }

        public bool Equals(PropertyLayout other)
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
                string.Equals(SectionName, other.SectionName, StringComparison.OrdinalIgnoreCase) &&
                Order == other.Order;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (SectionName != null ? SectionName.GetHashCode() : 0);
                hash = (hash * 23) + Order.GetHashCode();
                return hash;
            }
        }
    }
}