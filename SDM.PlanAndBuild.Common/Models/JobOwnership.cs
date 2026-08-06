namespace Skyline.DataMiner.SDM.PlanAndBuild.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public class JobOwnership : ChangeTrackingBase, ISectionTrackable
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        public Guid? AssignedTo
        {
            get => AssignedToField.Value;
            set => AssignedToField.Value = value;
        }

        public Guid? AssignmentGroup
        {
            get => AssignmentGroupField.Value;
            set => AssignmentGroupField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid?> AssignedToField => FieldHandler.GetOrCreateField(
            nameof(AssignedTo),
            () => new ChangeTrackingField<Guid?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid?> AssignmentGroupField => FieldHandler.GetOrCreateField(
            nameof(AssignmentGroup),
            () => new ChangeTrackingField<Guid?>(null));
    }
}
