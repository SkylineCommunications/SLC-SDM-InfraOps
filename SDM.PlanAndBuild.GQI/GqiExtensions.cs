namespace Skyline.DataMiner.SDM.PlanAndBuild.GQI
{
    using System;

    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.SDM.PlanAndBuild.Helpers;

    /// <summary>
    /// Provides extension methods for obtaining a Plan and Build API helper from GQIDMS and OnInitInputArgs
    /// instances.
    /// </summary>
    public static class GqiExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dms"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IPlanAndBuildApiHelper GetPlanAndBuildApiHelper(this GQIDMS dms)
        {
            if (dms is null)
            {
                throw new ArgumentNullException(nameof(dms), "dms cannot be null.");
            }

            return new PlanAndBuildApiHelper(dms.GetConnection());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IPlanAndBuildApiHelper GetPlanAndBuildApiHelper(this OnInitInputArgs args)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args), "OnInitInputArgs cannot be null.");
            }

            return GetPlanAndBuildApiHelper(args.DMS);
        }
    }
}