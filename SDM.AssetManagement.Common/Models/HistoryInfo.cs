namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using Newtonsoft.Json;
    using SharedMappers.DomIds;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class HistoryInfo : ChangeTrackingBase, IEquatable<HistoryInfo>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty => Description == default
                               && Job == default
                               && ModifiedInstanceID == default
                               && ModifiedInstanceDefinitionID == default
                               && ExtraInfo == default
                               && TypeOfHistory == default;

        public string Description
        {
            get => DescriptionField.Value;
            set => DescriptionField.Value = value;
        }

        public Guid? Job
        {
            get => JobField.Value;
            set => JobField.Value = value;
        }

        /// <summary>
        /// Gets the user that created this history entry.
        /// Populated from the DOM instance's "created by" metadata on read; not a section field
        /// and not settable by consumers.
        /// </summary>
        public string User { get; internal set; }

        public string ModifiedInstanceID
        {
            get => ModifiedInstanceIDField.Value;
            set => ModifiedInstanceIDField.Value = value;
        }

        public string ModifiedInstanceDefinitionID
        {
            get => ModifiedInstanceDefinitionIDField.Value;
            set => ModifiedInstanceDefinitionIDField.Value = value;
        }

        public string ExtraInfo
        {
            get => ExtraInfoField.Value;
            set => ExtraInfoField.Value = value;
        }

        public SlcAsset_Management.Enums.TypeOfHistoryEnum? TypeOfHistory
        {
            get => TypeOfHistoryField.Value;
            set => TypeOfHistoryField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> DescriptionField => FieldHandler.GetOrCreateField(
            nameof(Description),
            () => new ChangeTrackingField<string>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid?> JobField => FieldHandler.GetOrCreateField(
            nameof(Job),
            () => new ChangeTrackingField<Guid?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> ModifiedInstanceIDField => FieldHandler.GetOrCreateField(
            nameof(ModifiedInstanceID),
            () => new ChangeTrackingField<string>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> ModifiedInstanceDefinitionIDField => FieldHandler.GetOrCreateField(
            nameof(ModifiedInstanceDefinitionID),
            () => new ChangeTrackingField<string>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> ExtraInfoField => FieldHandler.GetOrCreateField(
            nameof(ExtraInfo),
            () => new ChangeTrackingField<string>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SlcAsset_Management.Enums.TypeOfHistoryEnum?> TypeOfHistoryField => FieldHandler.GetOrCreateField(
            nameof(TypeOfHistory),
            () => new ChangeTrackingField<SlcAsset_Management.Enums.TypeOfHistoryEnum?>(null));

        public static bool operator ==(HistoryInfo left, HistoryInfo right)
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

        public static bool operator !=(HistoryInfo left, HistoryInfo right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as HistoryInfo);
        }

        public bool Equals(HistoryInfo other)
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
                Job.Equals(other.Job) &&
                string.Equals(ModifiedInstanceID, other.ModifiedInstanceID, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(ModifiedInstanceDefinitionID, other.ModifiedInstanceDefinitionID, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(ExtraInfo, other.ExtraInfo, StringComparison.OrdinalIgnoreCase) &&
                TypeOfHistory == other.TypeOfHistory;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (Description != null ? Description.GetHashCode() : 0);
                hash = (hash * 23) + Job.GetHashCode();
                hash = (hash * 23) + (ModifiedInstanceID != null ? ModifiedInstanceID.GetHashCode() : 0);
                hash = (hash * 23) + (ModifiedInstanceDefinitionID != null ? ModifiedInstanceDefinitionID.GetHashCode() : 0);
                hash = (hash * 23) + (ExtraInfo != null ? ExtraInfo.GetHashCode() : 0);
                hash = (hash * 23) + TypeOfHistory.GetHashCode();
                return hash;
            }
        }
    }
}