namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public class History : SdmObject<History>, IEntityTracking
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

        public void ResetChangeTracking()
        {
            _historyInfo?.ResetChangeTracking();
        }

    }
}
