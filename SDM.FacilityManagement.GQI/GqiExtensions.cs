namespace Skyline.DataMiner.SDM.FacilityManagement.GQI
{
    using System;

    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.SDM.FacilityManagement.Helpers;

    /// <summary>
    /// Provides extension methods for creating and retrieving facility management API helpers using GQIDMS and
    /// OnInitInputArgs instances.
    /// </summary>
    public static class GqiExtensions
    {
       /// <summary>
       /// Creates and returns an instance of an API helper for facility management operations using the specified
       /// GQIDMS instance.
       /// </summary>
       /// <param name="dms">The GQIDMS instance used to establish the connection for the facility management API helper. Cannot be null.</param>
       /// <returns>An implementation of IFacilityManagementApiHelper initialized with the connection from the specified GQIDMS
       /// instance.</returns>
       /// <exception cref="ArgumentNullException">Thrown if dms is null.</exception>
        public static IFacilityManagementApiHelper GetFacilityManagementApiHelper(this GQIDMS dms)
        {
            if (dms is null)
            {
                throw new ArgumentNullException(nameof(dms), "dms cannot be null.");
            }

            return new FacilityManagementApiHelper(dms.GetConnection());
        }

        /// <summary>
        /// Gets an instance of the facility management API helper for the specified initialization arguments.
        /// </summary>
        /// <param name="args">The initialization arguments containing configuration and context information. Cannot be null.</param>
        /// <returns>An object that provides access to facility management API operations.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="args"/> parameter is null.</exception>
        public static IFacilityManagementApiHelper GetFacilityManagementApiHelper(this OnInitInputArgs args)
        {
            if (args is null)
            {
                throw new ArgumentNullException(nameof(args), "OnInitInputArgs cannot be null.");
            }

            return GetFacilityManagementApiHelper(args.DMS);
        }
    }
}