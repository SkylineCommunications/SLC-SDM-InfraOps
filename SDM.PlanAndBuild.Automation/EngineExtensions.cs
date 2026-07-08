namespace Skyline.DataMiner.SDM.PlanAndBuild.Automation
{
    using System;

    using Skyline.DataMiner.Automation;
    using Skyline.DataMiner.SDM.PlanAndBuild.Helpers;

    /// <summary>
    /// Provides extension methods for the <see cref="IEngine"/> interface for obtaining the Plan and Build API helper.
    /// </summary>
    public static class EngineExtensions
    {
       /// <summary>
       /// Creates an instance of a Plan and Build API helper for the specified engine.
       /// </summary>
       /// <param name="engine">The engine instance for which to create the Plan and Build API helper. Cannot be null.</param>
       /// <returns>An implementation of IPlanAndBuildApiHelper associated with the specified engine.</returns>
       /// <exception cref="ArgumentNullException">Thrown if engine is null.</exception>
        public static IPlanAndBuildApiHelper GetPlanAndBuildApiHelper(this IEngine engine)
        {
            if (engine is null)
            {
                throw new ArgumentNullException(nameof(engine), "Engine cannot be null.");
            }

            return new PlanAndBuildApiHelper(engine.GetUserConnection());
        }
    }
}
