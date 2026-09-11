namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.InfraOps.Core.Models;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class History : SdmObjectBase<History>, IEquatable<History>, IEntityTracking
    {
        [JsonIgnore]
        private bool _isNew = true;

        [JsonIgnore]
        private HistoryInfo _historyInfo;

        [JsonIgnore]
        [SdmIgnore]
        public bool Changed => _historyInfo?.Changed == true;

        [JsonIgnore]
        [SdmIgnore]
        public bool IsNew => _isNew;

        [JsonIgnore]
        [SdmIgnore]
        internal bool IsNewInternal
        {
            get => _isNew;
            set => _isNew = value;
        }

        public HistoryInfo HistoryInfo => _historyInfo ?? (_historyInfo = new HistoryInfo());

        #region Section Tracking

        /// <summary>
        /// Original DOM SectionIDs of the inline sections (built from the entity's own fields),
        /// captured on read and reused on write so each section keeps a stable identity across
        /// updates instead of getting a fresh random SectionID every save.
        /// See <see cref="Skyline.DataMiner.Utils.InfraOps.Common.Fields.ISectionTrackable"/>.
        /// </summary>
        [JsonIgnore]
        [SdmIgnore]
        internal Guid? HistoryInfoSectionId { get; set; }

        #endregion

        public IEnumerable<TrackingFieldValueDifference> GetChanges()
        {
            return _historyInfo?.GetChanges() ?? Enumerable.Empty<TrackingFieldValueDifference>();
        }

        public void ResetChangeTracking()
        {
            _historyInfo?.ResetChangeTracking();
        }

        public static bool operator ==(History left, History right)
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

        public static bool operator !=(History left, History right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as History);
        }

        public bool Equals(History other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(HistoryInfo, other.HistoryInfo);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (HistoryInfo != null ? HistoryInfo.GetHashCode() : 0);
                return hash;
            }
        }
    }
}
