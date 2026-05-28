namespace Skyline.DataMiner.SDM.AssetManagement.GQI
{
    using System;

    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.SDM.AssetManagement.Helpers;

    /// <summary>
    /// Provides extension methods for the <see cref="GQIDMS"/> related to IpAddressApiHelper.
    /// </summary>
    public static class GqiExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dms"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IAssetManagementApiHelper GetAssetManagementApiHelper(this GQIDMS dms)
        {
            if (dms is null)
            {
                throw new ArgumentNullException(nameof(dms), "dms cannot be null.");
            }

            return new AssetManagementApiHelper(dms.GetConnection());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static IAssetManagementApiHelper GetAssetManagementApiHelper(this OnInitInputArgs args)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args), "OnInitInputArgs cannot be null.");
            }

            return GetAssetManagementApiHelper(args.DMS);
        }
    }
}