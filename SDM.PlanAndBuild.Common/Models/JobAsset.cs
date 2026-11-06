namespace Skyline.DataMiner.SDM.PlanAndBuild.Models
{
    using System;

    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class JobAsset : ChangeTrackingBase, IEquatable<JobAsset>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty =>
            !AssetId.HasValue() &&
            Action == default &&
            !ActionField.IsSet &&
            AssetName == default &&
            AssetClassName == default &&
            IPAddress == default;

        public SdmObjectReference<Asset> AssetId
        {
            get => AssetIdField.Value;
            set => AssetIdField.Value = value;
        }

        public SlcPlan_And_Build.Enums.ActionforassetenumEnum Action
        {
            get => ActionField.Value;
            set => ActionField.Value = value;
        }

        public string AssetName
        {
            get => AssetNameField.Value;
            set => AssetNameField.Value = value;
        }

        public string AssetClassName
        {
            get => AssetClassNameField.Value;
            set => AssetClassNameField.Value = value;
        }

        public string IPAddress
        {
            get => IPAddressField.Value;
            set => IPAddressField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<Asset>> AssetIdField => FieldHandler.GetOrCreateField(
            nameof(AssetId),
            () => new ChangeTrackingField<SdmObjectReference<Asset>>(default, reference => reference.Identifier));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SlcPlan_And_Build.Enums.ActionforassetenumEnum> ActionField => FieldHandler.GetOrCreateField(
            nameof(Action),
            () => new ChangeTrackingField<SlcPlan_And_Build.Enums.ActionforassetenumEnum>(default));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> AssetNameField => FieldHandler.GetOrCreateField(
            nameof(AssetName),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> AssetClassNameField => FieldHandler.GetOrCreateField(
            nameof(AssetClassName),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> IPAddressField => FieldHandler.GetOrCreateField(
            nameof(IPAddress),
            () => new ChangeTrackingStringField(null));

        public static bool operator ==(JobAsset left, JobAsset right)
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

        public static bool operator !=(JobAsset left, JobAsset right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as JobAsset);
        }

        public bool Equals(JobAsset other)
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
                AssetId == other.AssetId &&
                Action == other.Action &&
                AssetName == other.AssetName &&
                AssetClassName == other.AssetClassName &&
                IPAddress == other.IPAddress;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (AssetId.Identifier?.GetHashCode() ?? 0);
                hash = (hash * 23) + Action.GetHashCode();
                hash = (hash * 23) + (AssetName?.GetHashCode() ?? 0);
                hash = (hash * 23) + (AssetClassName?.GetHashCode() ?? 0);
                hash = (hash * 23) + (IPAddress?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
