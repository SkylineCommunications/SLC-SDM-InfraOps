namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using Newtonsoft.Json;
    using SharedMappers.DomIds;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public class HistoryInfo : ChangeTrackingBase, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty => Description == default
                               && Job == Guid.Empty
                               && ModifiedInstanceID == default
                               && ModifiedInstanceDefinitionID == default
                               && ExtraInfo == default
                               && TypeOfHistory == default;

        public string Description
        {
            get => DescriptionField.Value;
            set => DescriptionField.Value = value;
        }

        public Guid Job
        {
            get => JobField.Value;
            set => JobField.Value = value;
        }

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
        internal IChangeTrackingField<Guid> JobField => FieldHandler.GetOrCreateField(
            nameof(Job),
            () => new ChangeTrackingField<Guid>(Guid.Empty));

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
    }
}