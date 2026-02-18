namespace Skyline.DataMiner.SDM.FacilityManagement.Automation
{
    using System;

    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.SDM.FacilityManagement.Helpers;

    /// <summary>
    /// Provides extension methods for the <see cref="IEngine"/> interface related to IpAddressApiHelper.
    /// </summary>
    public static class EngineExtensions
    {
        /// <summary>
        /// Gets an <see cref="IpAddressApiHelper"/> instance for the specified <see cref="IEngine"/>.
        /// </summary>
        /// <param name="engine">The engine to retrieve the IpAddressApiHelper for.</param>
        /// <returns>An <see cref="IpAddressApiHelper"/> instance associated with the engine's user connection.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="engine"/> is <c>null</c>.</exception>
        public static IFacilityManagementApiHelper GetFacilityManagementApiHelper(this IEngine engine)
        {
            if (engine is null)
            {
                throw new ArgumentNullException(nameof(engine), "Engine cannot be null.");
            }

            return new FacilityManagementApiHelper(engine.GetUserConnection());
        }
    }
}