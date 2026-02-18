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
        /// Gets an <see cref="IpAddressApiHelper"/> instance for the specified <see cref="SLProtocol"/>.
        /// </summary>
        /// <param name="protocol">The protocol instance to extend.</param>
        /// <returns>
        /// An <see cref="IpAddressApiHelper"/> initialized with the Protocol connection.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="protocol"/> is <c>null</c>.
        /// </exception>
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