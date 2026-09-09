namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class Attachment : ChangeTrackingBase, IEquatable<Attachment>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty =>
            FilePath == default &&
            AttachedAt == default &&
            AttachedBy == default;

        public string FilePath
        {
            get => FilePathField.Value;
            set => FilePathField.Value = value;
        }

        public DateTime? AttachedAt
        {
            get => AttachedAtField.Value;
            set => AttachedAtField.Value = value;
        }

        public Guid? AttachedBy
        {
            get => AttachedByField.Value;
            set => AttachedByField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> FilePathField => FieldHandler.GetOrCreateField(
            nameof(FilePath),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<DateTime?> AttachedAtField => FieldHandler.GetOrCreateField(
            nameof(AttachedAt),
            () => new ChangeTrackingField<DateTime?>(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<Guid?> AttachedByField => FieldHandler.GetOrCreateField(
            nameof(AttachedBy),
            () => new ChangeTrackingField<Guid?>(null));

        public static bool operator ==(Attachment left, Attachment right)
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

        public static bool operator !=(Attachment left, Attachment right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Attachment);
        }

        public bool Equals(Attachment other)
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
                FilePath == other.FilePath &&
                AttachedAt == other.AttachedAt &&
                AttachedBy == other.AttachedBy;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (FilePath?.GetHashCode() ?? 0);
                hash = (hash * 23) + AttachedAt.GetHashCode();
                hash = (hash * 23) + AttachedBy.GetHashCode();
                return hash;
            }
        }
    }
}