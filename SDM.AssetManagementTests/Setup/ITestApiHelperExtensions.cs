using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Skyline.DataMiner.SDM.AssetManagement.Validation;
using Skyline.DataMiner.SDM.Common.Services;

namespace SDM.AssetManagement.Tests.Setup
{
    public static class ITestApiHelperExtensions
    {
        #region Validator Helpers

        /// <summary>
        /// Creates an AssetValidator from the test helper repositories.
        /// Convenient for test validation scenarios.
        /// </summary>
        public static AssetValidator CreateAssetValidator(this ITestApiHelper helper)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            var entityLoader = new SdmEntityLoader(
                assetRepository: helper.AssetManagement.Assets,
                assetClassRepository: helper.AssetManagement.AssetClasses,
                deviceTypeRepository: helper.AssetManagement.DeviceTypes,
                dataPortRepository: helper.AssetManagement.DataPorts,
                powerPortRepository: helper.AssetManagement.PowerPorts,
                rackRepository: helper.FacilityManagement.Racks,
                reservationRepository: null,
                portTypeRepository: null
            );

            return new AssetValidator(entityLoader);
        }

        /// <summary>
        /// Creates an AssetClassValidator from the test helper repositories.
        /// Convenient for test validation scenarios.
        /// </summary>
        public static AssetClassValidator CreateAssetClassValidator(this ITestApiHelper helper)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            var entityLoader = new SdmEntityLoader(
                assetRepository: helper.AssetManagement.Assets,
                assetClassRepository: helper.AssetManagement.AssetClasses,
                deviceTypeRepository: helper.AssetManagement.DeviceTypes,
                dataPortRepository: helper.AssetManagement.DataPorts,
                powerPortRepository: helper.AssetManagement.PowerPorts,
                rackRepository: helper.FacilityManagement.Racks,
                reservationRepository: null,
                portTypeRepository: null
            );

            return new AssetClassValidator(entityLoader);
        }

        /// <summary>
        /// Creates an SdmEntityLoader from the test helper repositories.
        /// Use this if you need the entity loader directly for custom validators.
        /// </summary>
        public static SdmEntityLoader CreateEntityLoader(this ITestApiHelper helper)
        {
            if (helper == null)
            {
                throw new ArgumentNullException(nameof(helper));
            }

            return new SdmEntityLoader(
                assetRepository: helper.AssetManagement.Assets,
                assetClassRepository: helper.AssetManagement.AssetClasses,
                deviceTypeRepository: helper.AssetManagement.DeviceTypes,
                dataPortRepository: helper.AssetManagement.DataPorts,
                powerPortRepository: helper.AssetManagement.PowerPorts,
                rackRepository: helper.FacilityManagement.Racks,
                reservationRepository: null,
                portTypeRepository: null
            );
        }

        #endregion
    }
}
