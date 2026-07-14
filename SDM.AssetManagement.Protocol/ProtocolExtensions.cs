namespace Skyline.DataMiner.SDM.AssetManagement.Protocol
{
    using System;

    using Skyline.DataMiner.Scripting;
    using Skyline.DataMiner.SDM.AssetManagement.Helpers;

    /// <summary>
    /// Provides extension methods for the <see cref="SLProtocol"/> class to support IpAddressApiHelper operations.
    /// </summary>
    public static class ProtocolExtensions
    {
        /// <summary>
        /// Creates an instance of an asset management API helper for the specified SLProtocol.
        /// </summary>
        /// <param name="protocol">The SLProtocol instance for which to create the asset management API helper. Cannot be null.</param>
        /// <returns>An object that implements IAssetManagementApiHelper for interacting with the asset management API using the
        /// provided protocol.</returns>
        /// <exception cref="ArgumentNullException">Thrown if protocol is null.</exception>
        public static IAssetManagementApiHelper GetAssetManagementApiHelper(this SLProtocol protocol)
        {
            if (protocol is null)
            {
                throw new ArgumentNullException(nameof(protocol), "protocol cannot be null.");
            }

            return new AssetManagementApiHelper(protocol.SLNet.RawConnection);
        }
    }
}