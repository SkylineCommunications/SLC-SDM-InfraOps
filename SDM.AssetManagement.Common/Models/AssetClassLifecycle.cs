namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using Newtonsoft.Json;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class AssetClassLifecycle : ChangeTrackingBase, IEquatable<AssetClassLifecycle>, ISectionTrackable
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        public DateTime? EndOfLife
        {
            get => EndOfLifeField.Value;
            set => EndOfLifeField.Value = value;
        }

        public DateTime? EndOfService
        {
            get => EndOfServiceField.Value;
            set => EndOfServiceField.Value = value;
        }

        public TimeSpan? NominalLifetime
        {
            get => NominalLifetimeField.Value;
            set => NominalLifetimeField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<DateTime?> EndOfLifeField => FieldHandler.GetOrCreateField(
            nameof(EndOfLife),
            () => new ChangeTrackingField<DateTime?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<DateTime?> EndOfServiceField => FieldHandler.GetOrCreateField(
            nameof(EndOfService),
            () => new ChangeTrackingField<DateTime?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<TimeSpan?> NominalLifetimeField => FieldHandler.GetOrCreateField(
            nameof(NominalLifetime),
            () => new ChangeTrackingField<TimeSpan?>(null));

        public bool Equals(AssetClassLifecycle other)
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
                EndOfLife == other.EndOfLife &&
                EndOfService == other.EndOfService &&
                NominalLifetime == other.NominalLifetime;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AssetClassLifecycle);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (2 << 12) - 1;
                hash = (hash * 23) + (EndOfLife?.GetHashCode() ?? 0);
                hash = (hash * 23) + (EndOfService?.GetHashCode() ?? 0);
                hash = (hash * 23) + (NominalLifetime?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}