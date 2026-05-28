namespace Skyline.DataMiner.SDM.AssetManagement.Automation
{
    using System;

    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.SDM.AssetManagement.Helpers;

    /// <summary>
    /// Provides extension methods for the <see cref="IEngine"/> interface related to IpAddressApiHelper.
    /// </summary>
    public static class EngineExtensions
    {
       /// <summary>
       /// Creates an instance of an asset management API helper for the specified engine.
       /// </summary>
       /// <param name="engine">The engine instance for which to create the asset management API helper. Cannot be null.</param>
       /// <returns>An implementation of IAssetManagementApiHelper associated with the specified engine.</returns>
       /// <exception cref="ArgumentNullException">Thrown if engine is null.</exception>
        public static IAssetManagementApiHelper GetAssetManagementApiHelper(this IEngine engine)
        {
            if (engine is null)
            {
                throw new ArgumentNullException(nameof(engine), "Engine cannot be null.");
            }

            return new AssetManagementApiHelper(engine.GetUserConnection());
        }
    }
}
