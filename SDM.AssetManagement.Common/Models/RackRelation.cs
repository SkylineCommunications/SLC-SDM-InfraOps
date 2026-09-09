using Skyline.DataMiner.SDM.Extensions;
using Skyline.DataMiner.SDM.FacilityManagement.Models;
using System;
using Newtonsoft.Json;
using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    public sealed class RackRelation : ChangeTrackingBase, IEquatable<RackRelation>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }
        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty => !Rack.HasValue();

        public SdmObjectReference<Rack> Rack
        {
            get => RackField.Value;
            set => RackField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<Rack>> RackField => FieldHandler.GetOrCreateField(
            nameof(Rack),
            () => new ChangeTrackingField<SdmObjectReference<Rack>>(default));

        public static bool operator ==(RackRelation left, RackRelation right)
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

        public static bool operator !=(RackRelation left, RackRelation right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as RackRelation);
        }

        public bool Equals(RackRelation other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Rack == other.Rack;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (Rack != null ? Rack.GetHashCode() : 0);
                return hash;
            }
        }
    }
}