namespace Skyline.DataMiner.SDM.InfraOpsProperties.Protocol
{
    using System;

    using Skyline.DataMiner.Scripting;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Helpers;

    /// <summary>
    /// Provides extension methods for the <see cref="SLProtocol"/> class for obtaining the InfraOps Properties API helper.
    /// </summary>
    public static class ProtocolExtensions
    {
        /// <summary>
        /// Creates an instance of an InfraOps Properties API helper for the specified SLProtocol.
        /// </summary>
        /// <param name="protocol">The SLProtocol instance for which to create the InfraOps Properties API helper. Cannot be null.</param>
        /// <returns>An object that implements IInfraOpsPropertiesApiHelper for interacting with the InfraOps Properties API using the
        /// provided protocol.</returns>
        /// <exception cref="ArgumentNullException">Thrown if protocol is null.</exception>
        public static IInfraOpsPropertiesApiHelper GetInfraOpsPropertiesApiHelper(this SLProtocol protocol)
        {
            if (protocol is null)
            {
                throw new ArgumentNullException(nameof(protocol), "protocol cannot be null.");
            }

            return new InfraOpsPropertiesApiHelper(protocol.SLNet.RawConnection);
        }
    }
}