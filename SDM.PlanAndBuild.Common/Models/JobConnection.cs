namespace Skyline.DataMiner.SDM.PlanAndBuild.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    /// <summary>
    /// Records a Connection (Asset Manager entity) affected by a Plan &amp; Build Job, along with a snapshot of
    /// its cabling details at the time of the Job. Ported from InfraOpsShared's
    /// DOM_Classes.DOM.Applications.Plan_And_Build.Sections.ConnectionsOnJob.
    /// </summary>
    public sealed class JobConnection : IEquatable<JobConnection>, ISectionTrackable
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        public SdmObjectReference<Connection> ConnectionId { get; set; }

        public string Source { get; set; }

        public string Destination { get; set; }

        public string Status { get; set; }

        public SdmObjectReference<CableType> CableType { get; set; }

        public double? CableLength { get; set; }

        public static bool operator ==(JobConnection left, JobConnection right)
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

        public static bool operator !=(JobConnection left, JobConnection right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as JobConnection);
        }

        public bool Equals(JobConnection other)
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
                ConnectionId == other.ConnectionId &&
                Source == other.Source &&
                Destination == other.Destination &&
                Status == other.Status &&
                CableType == other.CableType &&
                CableLength == other.CableLength;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + ConnectionId.GetHashCode();
                hash = (hash * 23) + (Source?.GetHashCode() ?? 0);
                hash = (hash * 23) + (Destination?.GetHashCode() ?? 0);
                hash = (hash * 23) + (Status?.GetHashCode() ?? 0);
                hash = (hash * 23) + CableType.GetHashCode();
                hash = (hash * 23) + CableLength.GetHashCode();
                return hash;
            }
        }
    }
}
