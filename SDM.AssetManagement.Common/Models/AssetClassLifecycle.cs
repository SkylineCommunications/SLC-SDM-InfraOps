namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.Runtime.Serialization;

    using Newtonsoft.Json;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public class AssetClassLifecycle : IEquatable<AssetClassLifecycle>
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;

        public AssetClassLifecycle()
        {
            _fieldHandler = new ChangeTrackingFieldHandler();
        }

        [JsonIgnore]
        private ChangeTrackingFieldHandler FieldHandler
        {
            get
            {
                if (_fieldHandler == null)
                {
                    _fieldHandler = new ChangeTrackingFieldHandler();
                }
                return _fieldHandler;
            }
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ResetChangeTracking();
        }

        #region Public Properties

        public DateTime EndOfLife
        {
            get => EndOfLifeField.Value;
            set => EndOfLifeField.Value = value;
        }

        public DateTime EndOfService
        {
            get => EndOfServiceField.Value;
            set => EndOfServiceField.Value = value;
        }

        public TimeSpan NominalLifetime
        {
            get => NominalLifetimeField.Value;
            set => NominalLifetimeField.Value = value;
        }

        #endregion

        #region Internal Tracking Fields

        [JsonIgnore]
        internal IChangeTrackingField<DateTime> EndOfLifeField => FieldHandler.GetOrCreateField(
            nameof(EndOfLife),
            () => new ChangeTrackingField<DateTime>(default));

        [JsonIgnore]
        internal IChangeTrackingField<DateTime> EndOfServiceField => FieldHandler.GetOrCreateField(
            nameof(EndOfService),
            () => new ChangeTrackingField<DateTime>(default));

        [JsonIgnore]
        internal IChangeTrackingField<TimeSpan> NominalLifetimeField => FieldHandler.GetOrCreateField(
            nameof(NominalLifetime),
            () => new ChangeTrackingField<TimeSpan>(default));

        #endregion

        public void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();
        }

        #region IEquatable Implementation

        public static bool operator ==(AssetClassLifecycle left, AssetClassLifecycle right)
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

        public static bool operator !=(AssetClassLifecycle left, AssetClassLifecycle right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AssetClassLifecycle);
        }

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
                EndOfLife.Equals(other.EndOfLife) &&
                EndOfService.Equals(other.EndOfService) &&
                NominalLifetime.Equals(other.NominalLifetime);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + EndOfLife.GetHashCode();
                hash = (hash * 23) + EndOfService.GetHashCode();
                hash = (hash * 23) + NominalLifetime.GetHashCode();
                return hash;
            }
        }

        #endregion
    }
}