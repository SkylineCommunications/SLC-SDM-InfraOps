namespace Skyline.DataMiner.SDM.Common.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Repositories;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Repositories;

    /// <summary>
    /// Shared service for loading SDM object references across domains.
    /// Centralizes entity loading logic to avoid duplication.
    /// </summary>
    public class SdmEntityLoader
    {
        private readonly IAssetQueryRepository _assetRepository;
        private readonly IAssetClassQueryRepository _assetClassRepository;
        private readonly IDeviceTypeQueryRepository _deviceTypeRepository;
        private readonly IRackQueryRepository _rackRepository;
        private readonly IDataPortQueryRepository _dataPortRepository;
        private readonly IPowerPortQueryRepository _powerPortRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="SdmEntityLoader"/> class.
        /// All repositories are optional - only provide the ones you need.
        /// </summary>
        public SdmEntityLoader(
            IAssetQueryRepository assetRepository = null,
            IAssetClassQueryRepository assetClassRepository = null,
            IDeviceTypeQueryRepository deviceTypeRepository = null,
            IRackQueryRepository rackRepository = null,
            IDataPortQueryRepository dataPortRepository = null,
            IPowerPortQueryRepository powerPortRepository = null)
        {
            _assetRepository = assetRepository;
            _assetClassRepository = assetClassRepository;
            _deviceTypeRepository = deviceTypeRepository;
            _rackRepository = rackRepository;
            _dataPortRepository = dataPortRepository;
            _powerPortRepository = powerPortRepository;
        }

        #region Single Entity Loaders

        /// <summary>
        /// Loads an Asset by its reference.
        /// </summary>
        public Asset LoadAsset(SdmObjectReference<Asset> reference)
        {
            if (_assetRepository == null || reference == null || !reference.HasValue())
            {
                return null;
            }

            try
            {
                var filter = AssetExposers.Identifier.Equal(reference.Identifier);
                return _assetRepository.Read(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load Asset: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Loads an AssetClass by its reference.
        /// </summary>
        public AssetClass LoadAssetClass(SdmObjectReference<AssetClass> reference)
        {
            if (_assetClassRepository == null || reference == null || !reference.HasValue())
            {
                return null;
            }

            try
            {
                var filter = AssetClassExposers.Identifier.Equal(reference.Identifier);
                return _assetClassRepository.Read(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load AssetClass: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Loads a DeviceType by its reference.
        /// </summary>
        public DeviceType LoadDeviceType(SdmObjectReference<DeviceType> reference)
        {
            if (_deviceTypeRepository == null || reference == null || !reference.HasValue())
            {
                return null;
            }

            try
            {
                var filter = DeviceTypeExposers.Identifier.Equal(reference.Identifier);
                return _deviceTypeRepository.Read(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load DeviceType: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Loads a Rack by its Guid identifier.
        /// </summary>
        public Rack LoadRack(SdmObjectReference<Rack> reference)
        {
            if (_rackRepository == null || reference == null || !reference.HasValue())
            {
                return null;
            }

            try
            {
                var filter = RackExposers.Identifier.Equal(reference.Identifier);
                return _rackRepository.Read(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load Rack: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Loads all DataPorts for an Asset.
        /// </summary>
        public List<DataPort> LoadDataPorts(Asset asset)
        {
            if (_dataPortRepository == null || asset == null || string.IsNullOrEmpty(asset.Identifier))
            {
                return new List<DataPort>();
            }

            try
            {
                var filter = DataPortExposers.Asset.Equal(asset);
                return _dataPortRepository.Read(filter).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load DataPorts: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Loads all PowerPorts for an Asset.
        /// </summary>
        public List<PowerPort> LoadPowerPorts(Asset asset)
        {
            if (_powerPortRepository == null || asset == null || string.IsNullOrEmpty(asset.Identifier))
            {
                return new List<PowerPort>();
            }

            try
            {
                var filter = PowerPortExposers.Asset.Equal(asset);
                return _powerPortRepository.Read(filter).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load PowerPorts: {ex.Message}", ex);
            }
        }

        #endregion

        #region Hierarchical Loaders

        /// <summary>
        /// Loads DeviceType through Asset hierarchy (Asset -> AssetClass -> DeviceType).
        /// </summary>
        public DeviceType LoadDeviceTypeFromAsset(Asset asset)
        {
            if (asset == null || !asset.AssetClassId.HasValue())
            {
                return null;
            }

            var assetClass = LoadAssetClass(asset.AssetClassId);
            if (assetClass == null || !assetClass.DeviceTypeId.HasValue())
            {
                return null;
            }

            return LoadDeviceType(assetClass.DeviceTypeId);
        }

        /// <summary>
        /// Loads AssetClass and DeviceType through Asset reference.
        /// Returns tuple with both entities, either can be null.
        /// </summary>
        public (AssetClass AssetClass, DeviceType DeviceType) LoadAssetClassAndDeviceType(Asset asset)
        {
            if (asset == null || !asset.AssetClassId.HasValue())
            {
                return (null, null);
            }

            var assetClass = LoadAssetClass(asset.AssetClassId);
            if (assetClass == null || !assetClass.DeviceTypeId.HasValue())
            {
                return (assetClass, null);
            }

            var deviceType = LoadDeviceType(assetClass.DeviceTypeId);
            return (assetClass, deviceType);
        }

        #endregion
    }
}