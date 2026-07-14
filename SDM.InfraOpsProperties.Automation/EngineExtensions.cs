namespace Skyline.DataMiner.SDM.InfraOpsProperties.Automation
{
    using System;

    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Helpers;

    /// <summary>
    /// Provides extension methods for the <see cref="IEngine"/> interface for obtaining the InfraOps Properties API helper.
    /// </summary>
    public static class EngineExtensions
    {
       /// <summary>
       /// Creates an instance of an InfraOps Properties API helper for the specified engine.
       /// </summary>
       /// <param name="engine">The engine instance for which to create the InfraOps Properties API helper. Cannot be null.</param>
       /// <returns>An implementation of IInfraOpsPropertiesApiHelper associated with the specified engine.</returns>
       /// <exception cref="ArgumentNullException">Thrown if engine is null.</exception>
        public static IInfraOpsPropertiesApiHelper GetInfraOpsPropertiesApiHelper(this IEngine engine)
        {
            if (engine is null)
            {
                throw new ArgumentNullException(nameof(engine), "Engine cannot be null.");
            }

            return new InfraOpsPropertiesApiHelper(engine.GetUserConnection());
        }
    }
}
