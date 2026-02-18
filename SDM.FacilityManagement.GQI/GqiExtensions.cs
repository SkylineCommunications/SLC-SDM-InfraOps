namespace Skyline.DataMiner.SDM.FacilityManagement.GQI
{
    using System;

    using Skyline.DataMiner.Analytics.GenericInterface;
    using Skyline.DataMiner.SDM.FacilityManagement.Helpers;

    /// <summary>
    /// Provides extension methods for the <see cref="GQIDMS"/> related to IpAddressApiHelper.
    /// </summary>
    public static class GqiExtensions
    {
        /// <summary>
        /// Creates a new <see cref="IpAddressApiHelper"/> instance using the specified <see cref="GQIDMS"/>.
        /// </summary>
        /// <param name="dms">The GQI DataMiner System (DMS) instance.</param>
        /// <returns>
        /// A new <see cref="IpAddressApiHelper"/> initialized with the DMS connection.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="dms"/> is <c>null</c>.
        /// </exception>
        public static IFacilityManagementApiHelper GetFacilityManagementApiHelper(this GQIDMS dms)
        {
            if (dms is null)
            {
                throw new ArgumentNullException(nameof(dms), "dms cannot be null.");
            }

            return new FacilityManagementApiHelper(dms.GetConnection());
        }

        /// <summary>
        /// Creates a new <see cref="IpAddressApiHelper"/> instance using the <see cref="GQIDMS"/> from the specified <see cref="OnInitInputArgs"/>.
        /// </summary>
        /// <param name="args">The initialization arguments containing the DMS instance.</param>
        /// <returns>
        /// A new <see cref="IpAddressApiHelper"/> initialized with the DMS connection from <paramref name="args"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="args"/> is <c>null</c>.
        /// </exception>
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