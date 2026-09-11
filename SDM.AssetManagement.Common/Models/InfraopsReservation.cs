namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM;
    using System;
    using Newtonsoft.Json;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public sealed class InfraopsReservation : SdmObject<InfraopsReservation>, IEquatable<InfraopsReservation>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private JobRelation _jobFk;
        [JsonIgnore]
        private RackRelation _rackFk;
        [JsonIgnore]
        private bool _isNew = true;

        public InfraopsReservation()
        {
            _fieldHandler = new ChangeTrackingFieldHandler();
        }

        [JsonIgnore]
        [SdmIgnore]
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

        [JsonIgnore]
        [SdmIgnore]
        public bool IsNew => _isNew;

        /// <summary>
        /// Sets the IsNew flag. Used internally when loading from database.
        /// </summary>
        [JsonIgnore]
        [SdmIgnore]
        internal bool IsNewInternal
        {
            get => _isNew;
            set => _isNew = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        public bool Changed =>
            FieldHandler.HasChanges ||
            _jobFk?.Changed == true ||
            _rackFk?.Changed == true;

        public string Description
        {
            get => DescriptionField.Value;
            set => DescriptionField.Value = value;
        }

        public JobRelation JobFk
        {
            get => _jobFk ?? (_jobFk = new JobRelation());
            set => _jobFk = value ?? new JobRelation();
        }

        public RackRelation RackFk => _rackFk ?? (_rackFk = new RackRelation());

        public List<InfraopsReservationBounderies> ReservedPositions
        {
            get => ReservedPositionsField.Value ?? new List<InfraopsReservationBounderies>();
            set => ReservedPositionsField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> DescriptionField => FieldHandler.GetOrCreateField(
            nameof(Description),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal ChangeTrackingArrayField<InfraopsReservationBounderies> ReservedPositionsField => FieldHandler.GetOrCreateArrayField(
            nameof(ReservedPositions),
            () => new ChangeTrackingArrayField<InfraopsReservationBounderies>(new List<InfraopsReservationBounderies>()));

        public IEnumerable<TrackingFieldValueDifference> GetChanges()
        {
            return FieldHandler.GetChanges()
                .Select(kvp => new TrackingFieldValueDifference
                {
                    FieldName = kvp.Key,
                    OldValue = kvp.Value.prevVal,
                    NewValue = kvp.Value.newVal,
                })
                .Concat(_jobFk?.GetChanges() ?? Enumerable.Empty<TrackingFieldValueDifference>())
                .Concat(_rackFk?.GetChanges() ?? Enumerable.Empty<TrackingFieldValueDifference>());
        }

        public void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();
            _jobFk?.ResetChangeTracking();
            _rackFk?.ResetChangeTracking();

            if (ReservedPositions != null)
            {
                foreach (var reservedPosition in ReservedPositions)
                {
                    reservedPosition?.ResetChangeTracking();
                }
            }
        }

        public static bool operator ==(InfraopsReservation left, InfraopsReservation right)
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

        public static bool operator !=(InfraopsReservation left, InfraopsReservation right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as InfraopsReservation);
        }

        public bool Equals(InfraopsReservation other)
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
                string.Equals(Description, other.Description, StringComparison.OrdinalIgnoreCase) &&
                Equals(JobFk, other.JobFk) &&
                Equals(RackFk, other.RackFk) &&
                ReservedPositions.SequenceEqual(other.ReservedPositions);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (Description != null ? Description.GetHashCode() : 0);
                hash = (hash * 23) + (JobFk != null ? JobFk.GetHashCode() : 0);
                hash = (hash * 23) + (RackFk != null ? RackFk.GetHashCode() : 0);
                return hash;
            }
        }

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? ReservationPropertiesSectionId { get; set; }

        #endregion

    }
}
