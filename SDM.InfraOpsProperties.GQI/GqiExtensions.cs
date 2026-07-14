namespace Skyline.DataMiner.SDM.InfraOpsProperties.GQI
{
    using System;

    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Helpers;

    /// <summary>
    /// Provides extension methods for obtaining an InfraOps Properties API helper from GQIDMS and OnInitInputArgs
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
        public static IInfraOpsPropertiesApiHelper GetInfraOpsPropertiesApiHelper(this GQIDMS dms)
        {
            if (dms is null)
            {
                throw new ArgumentNullException(nameof(dms), "dms cannot be null.");
            }

            return new InfraOpsPropertiesApiHelper(dms.GetConnection());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IInfraOpsPropertiesApiHelper GetInfraOpsPropertiesApiHelper(this OnInitInputArgs args)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args), "OnInitInputArgs cannot be null.");
            }

            return GetInfraOpsPropertiesApiHelper(args.DMS);
        }
    }
}