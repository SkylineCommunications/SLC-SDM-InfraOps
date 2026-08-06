namespace Skyline.DataMiner.SDM.InfraOpsProperties.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class PropertyOption : IEquatable<PropertyOption>, ISectionTrackable
    {
        public string Option { get; set; }

        /// <summary>
        /// Gets or sets the DOM Section ID this element was read from, so it can be reused on update.
        /// </summary>
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        public static bool operator ==(PropertyOption left, PropertyOption right)
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

        public static bool operator !=(PropertyOption left, PropertyOption right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PropertyOption);
        }

        public bool Equals(PropertyOption other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Option == other.Option;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (Option?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
