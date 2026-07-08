namespace Skyline.DataMiner.SDM.PlanAndBuild.Protocol
{
    using System;

    using Skyline.DataMiner.Scripting;
    using Skyline.DataMiner.SDM.PlanAndBuild.Helpers;

    /// <summary>
    /// Provides extension methods for the <see cref="SLProtocol"/> class for obtaining the Plan and Build API helper.
    /// </summary>
    public static class ProtocolExtensions
    {
        /// <summary>
        /// Creates an instance of a Plan and Build API helper for the specified SLProtocol.
        /// </summary>
        /// <param name="protocol">The SLProtocol instance for which to create the Plan and Build API helper. Cannot be null.</param>
        /// <returns>An object that implements IPlanAndBuildApiHelper for interacting with the Plan and Build API using the
        /// provided protocol.</returns>
        /// <exception cref="ArgumentNullException">Thrown if protocol is null.</exception>
        public static IPlanAndBuildApiHelper GetPlanAndBuildApiHelper(this SLProtocol protocol)
        {
            if (protocol is null)
            {
                throw new ArgumentNullException(nameof(protocol), "protocol cannot be null.");
            }

            return new PlanAndBuildApiHelper(protocol.SLNet.RawConnection);
        }
    }
}