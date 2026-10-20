namespace Skyline.DataMiner.SDM.PlanAndBuild.Models
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    /// <summary>
    /// Records a Connection (Asset Manager entity) affected by a Plan &amp; Build Job, along with a snapshot of
    /// its cabling details at the time of the Job. Ported from InfraOpsShared's
    /// DOM_Classes.DOM.Applications.Plan_And_Build.Sections.ConnectionsOnJob.
    /// </summary>
    public sealed class JobConnection : ChangeTrackingBase, IEquatable<JobConnection>, ISectionTrackable, ISectionEmptyState
    {
        [JsonIgnore]
        [SdmIgnore]
        Guid? ISectionTrackable.SectionId { get; set; }

        [JsonIgnore]
        [SdmIgnore]
        public bool IsEmpty =>
            !ConnectionId.HasValue() &&
            Source == default &&
            Destination == default &&
            Status == default &&
            !CableType.HasValue() &&
            CableLength == default;

        public SdmObjectReference<Connection> ConnectionId
        {
            get => ConnectionIdField.Value;
            set => ConnectionIdField.Value = value;
        }

        public string Source
        {
            get => SourceField.Value;
            set => SourceField.Value = value;
        }

        public string Destination
        {
            get => DestinationField.Value;
            set => DestinationField.Value = value;
        }

        public string Status
        {
            get => StatusField.Value;
            set => StatusField.Value = value;
        }

        public SdmObjectReference<CableType> CableType
        {
            get => CableTypeField.Value;
            set => CableTypeField.Value = value;
        }

        public double? CableLength
        {
            get => CableLengthField.Value;
            set => CableLengthField.Value = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<Connection>> ConnectionIdField => FieldHandler.GetOrCreateField(
            nameof(ConnectionId),
            () => new ChangeTrackingField<SdmObjectReference<Connection>>(default, reference => reference.Identifier));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> SourceField => FieldHandler.GetOrCreateField(
            nameof(Source),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> DestinationField => FieldHandler.GetOrCreateField(
            nameof(Destination),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> StatusField => FieldHandler.GetOrCreateField(
            nameof(Status),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<SdmObjectReference<CableType>> CableTypeField => FieldHandler.GetOrCreateField(
            nameof(CableType),
            () => new ChangeTrackingField<SdmObjectReference<CableType>>(default, reference => reference.Identifier));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<double?> CableLengthField => FieldHandler.GetOrCreateField(
            nameof(CableLength),
            () => new ChangeTrackingField<double?>(null));

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
                hash = (hash * 23) + (ConnectionId.Identifier?.GetHashCode() ?? 0);
                hash = (hash * 23) + (Source?.GetHashCode() ?? 0);
                hash = (hash * 23) + (Destination?.GetHashCode() ?? 0);
                hash = (hash * 23) + (Status?.GetHashCode() ?? 0);
                hash = (hash * 23) + (CableType.Identifier?.GetHashCode() ?? 0);
                hash = (hash * 23) + (CableLength?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
