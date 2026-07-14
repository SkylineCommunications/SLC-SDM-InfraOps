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
       /// Creates and returns an instance of IFacilityManagementApiHelper for the specified engine.
       /// </summary>
       /// <param name="engine">The engine instance used to obtain the user connection. Cannot be null.</param>
       /// <returns>An IFacilityManagementApiHelper instance associated with the provided engine.</returns>
       /// <exception cref="ArgumentNullException">Thrown if engine is null.</exception>
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