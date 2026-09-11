namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using SharedMappers.DomIds;
    using Skyline.DataMiner.SDM.InfraOps.Core.Models;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public sealed class AssetManagerAppSettings : SdmObjectBase<AssetManagerAppSettings>, IEquatable<AssetManagerAppSettings>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private bool _isNew = true;

        public AssetManagerAppSettings()
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
        public bool Changed => FieldHandler.HasChanges;

        public bool EnableAssetHistory
        {
            get => EnableAssetHistoryField.Value;
            set => EnableAssetHistoryField.Value = value;
        }

        public SlcAsset_Management.Enums.Planandbuildjobpromptenum PlanAndBuildJobPrompt
        {
            get => PlanAndBuildJobPromptField.Value;
            set => PlanAndBuildJobPromptField.Value = value;
        }

        public bool EnableConnectionHistory
        {
            get => EnableConnectionHistoryField.Value;
            set => EnableConnectionHistoryField.Value = value;
        }

        public TimeSpan? HistoryTTL
        {
            get => HistoryTTLField.Value;
            set => HistoryTTLField.Value = value;
        }

        public long? HistoryLimit
        {
            get => HistoryLimitField.Value;
            set => HistoryLimitField.Value = value;
        }

        public bool EnableResourceLink
        {
            get => EnableResourceLinkField.Value;
            set => EnableResourceLinkField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<bool> EnableAssetHistoryField => FieldHandler.GetOrCreateField(
            nameof(EnableAssetHistory),
            () => new ChangeTrackingField<bool>(default));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SlcAsset_Management.Enums.Planandbuildjobpromptenum> PlanAndBuildJobPromptField => FieldHandler.GetOrCreateField(
            nameof(PlanAndBuildJobPrompt),
            () => new ChangeTrackingField<SlcAsset_Management.Enums.Planandbuildjobpromptenum>(default));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<bool> EnableConnectionHistoryField => FieldHandler.GetOrCreateField(
            nameof(EnableConnectionHistory),
            () => new ChangeTrackingField<bool>(default));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<TimeSpan?> HistoryTTLField => FieldHandler.GetOrCreateField(
            nameof(HistoryTTL),
            () => new ChangeTrackingField<TimeSpan?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<long?> HistoryLimitField => FieldHandler.GetOrCreateField(
            nameof(HistoryLimit),
            () => new ChangeTrackingField<long?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<bool> EnableResourceLinkField => FieldHandler.GetOrCreateField(
            nameof(EnableResourceLink),
            () => new ChangeTrackingField<bool>(default));

        public IEnumerable<TrackingFieldValueDifference> GetChanges()
        {
            return FieldHandler.GetChanges()
                .Select(kvp => new TrackingFieldValueDifference
                {
                    FieldName = kvp.Key,
                    OldValue = kvp.Value.prevVal,
                    NewValue = kvp.Value.newVal,
                });
        }

        public void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();
        }

        public static bool operator ==(AssetManagerAppSettings left, AssetManagerAppSettings right)
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

        public static bool operator !=(AssetManagerAppSettings left, AssetManagerAppSettings right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AssetManagerAppSettings);
        }

        public bool Equals(AssetManagerAppSettings other)
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
                EnableAssetHistory == other.EnableAssetHistory &&
                PlanAndBuildJobPrompt == other.PlanAndBuildJobPrompt &&
                EnableConnectionHistory == other.EnableConnectionHistory &&
                HistoryTTL == other.HistoryTTL &&
                HistoryLimit == other.HistoryLimit &&
                EnableResourceLink == other.EnableResourceLink;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + EnableAssetHistory.GetHashCode();
                hash = (hash * 23) + PlanAndBuildJobPrompt.GetHashCode();
                hash = (hash * 23) + EnableConnectionHistory.GetHashCode();
                hash = (hash * 23) + HistoryTTL.GetHashCode();
                hash = (hash * 23) + HistoryLimit.GetHashCode();
                hash = (hash * 23) + EnableResourceLink.GetHashCode();
                return hash;
            }
        }

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? AppSettingsSectionId { get; set; }

        #endregion

    }
}
