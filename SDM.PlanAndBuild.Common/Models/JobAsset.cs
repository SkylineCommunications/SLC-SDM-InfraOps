namespace Skyline.DataMiner.SDM.PlanAndBuild.Models
{
    using System;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    public sealed class JobAsset : IEquatable<JobAsset>
    {
        public SdmObjectReference<Asset> AssetId { get; set; }

        public SlcPlan_And_Build.Enums.ActionforassetenumEnum Action { get; set; }

        public static bool operator ==(JobAsset left, JobAsset right)
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

        public static bool operator !=(JobAsset left, JobAsset right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as JobAsset);
        }

        public bool Equals(JobAsset other)
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
                AssetId == other.AssetId &&
                Action == other.Action;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + AssetId.GetHashCode();
                hash = (hash * 23) + Action.GetHashCode();
                return hash;
            }
        }
    }
}
