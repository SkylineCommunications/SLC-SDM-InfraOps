namespace Skyline.DataMiner.SDM.FacilityManagement.Protocol
{
    using System;

    using Skyline.DataMiner.Scripting;
    using Skyline.DataMiner.SDM.FacilityManagement.Helpers;

    /// <summary>
    /// Provides extension methods for working with protocol instances to enable facility management API operations.
    /// </summary>
    public static class ProtocolExtensions
    {
       /// <summary>
       /// Creates an instance of an object that provides facility management API operations for the specified protocol.
       /// </summary>
       /// <param name="protocol">The protocol instance used to access facility management API functionality. Cannot be null.</param>
       /// <returns>An object that implements the IFacilityManagementApiHelper interface for interacting with facility management
       /// APIs.</returns>
       /// <exception cref="ArgumentNullException">Thrown if protocol is null.</exception>
        public static IFacilityManagementApiHelper GetFacilityManagementApiHelper(this SLProtocol protocol)
        {
            if (protocol is null)
            {
                throw new ArgumentNullException(nameof(protocol), "protocol cannot be null.");
            }

            return new FacilityManagementApiHelper(protocol.SLNet.RawConnection);
        }
    }
}