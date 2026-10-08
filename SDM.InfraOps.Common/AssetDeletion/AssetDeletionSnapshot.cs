namespace Skyline.DataMiner.SDM.InfraOps.Orchestration.AssetDeletion
{
    using System.Collections.Generic;

    using Skyline.DataMiner.SDM.AssetManagement.Deletion;

    using Connection = Skyline.DataMiner.SDM.AssetManagement.Models.Connection;

    internal sealed class AssetDeletionSnapshot
    {
        public AssetDeletionSnapshot(
            List<Connection> connections,
            List<AssetDeletionConnectionSnapshot> connectionSnapshots)
        {
            Connections = connections;
            JobConnectionSnapshots = connectionSnapshots;
        }

        public List<Connection> Connections { get; }

        public List<AssetDeletionConnectionSnapshot> JobConnectionSnapshots { get; }
    }
}
