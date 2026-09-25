namespace Skyline.DataMiner.SDM.FacilityManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class ImageInfo : ChangeTrackingBase, IEquatable<ImageInfo>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty =>
            ImageFilePath == default &&
            UploadTimestamp == default;

        public string ImageFilePath
        {
            get => ImageFilePathField.Value;
            set => ImageFilePathField.Value = value;
        }

        public DateTime UploadTimestamp
        {
            get => UploadTimestampField.Value;
            set => UploadTimestampField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> ImageFilePathField => FieldHandler.GetOrCreateField(
            nameof(ImageFilePath),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<DateTime> UploadTimestampField => FieldHandler.GetOrCreateField(
            nameof(UploadTimestamp),
            () => new ChangeTrackingField<DateTime>(default));

        public static bool operator ==(ImageInfo left, ImageInfo right)
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

        public static bool operator !=(ImageInfo left, ImageInfo right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ImageInfo);
        }

        public bool Equals(ImageInfo other)
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
                string.Equals(ImageFilePath, other.ImageFilePath, StringComparison.OrdinalIgnoreCase) &&
                UploadTimestamp.Equals(other.UploadTimestamp);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (ImageFilePath != null ? ImageFilePath.GetHashCode() : 0);
                hash = (hash * 23) + UploadTimestamp.GetHashCode();
                return hash;
            }
        }
    }
}