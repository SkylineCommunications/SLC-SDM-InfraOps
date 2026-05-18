namespace Skyline.DataMiner.SDM.Common.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharedCommonLibrary.AssetManagement.Models;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    /// <summary>
    /// Shared service for loading and querying SDM entities across domains.
    /// Centralizes all data access logic to avoid duplication.
    /// Acts as a facade for all repository operations.
    /// <para>
    /// Note: This class only performs read operations (Count, Read) on the repositories.
    /// While IBulkRepository provides CRUD operations, this loader is designed for queries only.
    /// </para>
    /// </summary>
    public class SdmEntityLoader
    {
        private readonly IBulkRepository<Asset> _assetRepository;
        private readonly IBulkRepository<AssetClass> _assetClassRepository;
        private readonly IBulkRepository<DeviceType> _deviceTypeRepository;
        private readonly IBulkRepository<Rack> _rackRepository;
        private readonly IBulkRepository<DataPort> _dataPortRepository;
        private readonly IBulkRepository<PowerPort> _powerPortRepository;
        private readonly IBulkRepository<InfraopsReservation> _reservationRepository;
        private readonly IBulkRepository<PortType> _portTypeRepository;

        public SdmEntityLoader(
            IBulkRepository<Asset> assetRepository = null,
            IBulkRepository<AssetClass> assetClassRepository = null,
            IBulkRepository<DeviceType> deviceTypeRepository = null,
            IBulkRepository<Rack> rackRepository = null,
            IBulkRepository<DataPort> dataPortRepository = null,
            IBulkRepository<PowerPort> powerPortRepository = null,
            IBulkRepository<InfraopsReservation> reservationRepository = null,
            IBulkRepository<PortType> portTypeRepository = null)
        {
            _assetRepository = assetRepository;
            _assetClassRepository = assetClassRepository;
            _deviceTypeRepository = deviceTypeRepository;
            _rackRepository = rackRepository;
            _dataPortRepository = dataPortRepository;
            _powerPortRepository = powerPortRepository;
            _reservationRepository = reservationRepository;
            _portTypeRepository = portTypeRepository;
        }

        #region Single Entity Loaders

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

        public Rack LoadRack(SdmObjectReference<Rack> rack)
        {
            if (_rackRepository == null || !rack.HasValue())
            {
                return null;
            }

            return LoadRack(rack.Identifier);
        }

        public Rack LoadRack(string rackId)
        {
            if (_rackRepository == null || string.IsNullOrEmpty(rackId))
            {
                return null;
            }

            try
            {
                var filter = RackExposers.Identifier.Equal(rackId.ToString());
                return _rackRepository.Read(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load Rack: {ex.Message}", ex);
            }
        }

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

        /// <summary>
        /// Loads a PortType by its identifier.
        /// </summary>
        /// <param name="portTypeRef">The PortType reference.</param>
        /// <returns>The PortType instance, or null if not found.</returns>
        public PortType LoadPortType(SdmObjectReference<PortType> portTypeRef)
        {
            if (_portTypeRepository == null || portTypeRef == null || !portTypeRef.HasValue())
            {
                return null;
            }

            try
            {
                var filter = PortTypeExposers.Identifier.Equal(portTypeRef.Identifier);
                return _portTypeRepository.Read(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load PortType: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Loads a PortType by its identifier string.
        /// </summary>
        /// <param name="portTypeId">The PortType identifier.</param>
        /// <returns>The PortType instance, or null if not found.</returns>
        public PortType LoadPortType(string portTypeId)
        {
            if (_portTypeRepository == null || string.IsNullOrWhiteSpace(portTypeId))
            {
                return null;
            }

            try
            {
                var filter = PortTypeExposers.Identifier.Equal(portTypeId);
                return _portTypeRepository.Read(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load PortType: {ex.Message}", ex);
            }
        }

        #endregion

        #region Query Methods (Count/Filter Operations)

        /// <summary>
        /// Counts assets with the specified name, excluding given identifiers.
        /// </summary>
        public long CountAssetsByName(string name, List<string> exceptIdentifiers = null)
        {
            if (_assetRepository == null || string.IsNullOrWhiteSpace(name))
            {
                return 0;
            }

            FilterElement<Asset> filter = AssetExposers.AssetName.Equal(name);

            // Filter out null/empty identifiers (happens with new, unsaved entities)
            if (exceptIdentifiers != null && exceptIdentifiers.Any())
            {
                var validIdentifiers = exceptIdentifiers.Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
                
                if (validIdentifiers.Any())
                {
                    var clauses = validIdentifiers
                        .Select(id => AssetExposers.Identifier.NotEqual(id))
                        .Cast<FilterElement<Asset>>()
                        .ToArray();
                    filter = filter.AND(new ANDFilterElement<Asset>(clauses));
                }
            }

            return _assetRepository.Count(filter);
        }

        /// <summary>
        /// Counts assets with the specified AssetID, excluding given identifiers.
        /// </summary>
        public long CountAssetsByAssetId(string assetId, List<string> exceptIdentifiers = null)
        {
            if (_assetRepository == null || string.IsNullOrWhiteSpace(assetId))
            {
                return 0;
            }

            FilterElement<Asset> filter = AssetExposers.AssetId.Equal(assetId);

            // Filter out null/empty identifiers (happens with new, unsaved entities)
            if (exceptIdentifiers != null && exceptIdentifiers.Any())
            {
                var validIdentifiers = exceptIdentifiers.Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
                
                if (validIdentifiers.Any())
                {
                    var clauses = validIdentifiers
                        .Select(id => AssetExposers.Identifier.NotEqual(id))
                        .Cast<FilterElement<Asset>>()
                        .ToArray();
                    filter = filter.AND(new ANDFilterElement<Asset>(clauses));
                }
            }

            return _assetRepository.Count(filter);
        }

        /// <summary>
        /// Counts assets with the specified Serial Number and AssetClass, excluding given identifiers.
        /// </summary>
        public long CountAssetsBySerialNumber(
            string serialNumber,
            SdmObjectReference<AssetClass> assetClassId,
            List<string> exceptIdentifiers = null)
        {
            if (_assetRepository == null ||
                string.IsNullOrWhiteSpace(serialNumber) ||
                assetClassId == null ||
                !assetClassId.HasValue())
            {
                return 0;
            }

            FilterElement<Asset> filter = AssetExposers.SerialNumber.Equal(serialNumber)
                .AND(AssetExposers.AssetClass.Equal(assetClassId));

            // Filter out null/empty identifiers (happens with new, unsaved entities)
            if (exceptIdentifiers != null && exceptIdentifiers.Any())
            {
                var validIdentifiers = exceptIdentifiers.Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
                
                if (validIdentifiers.Any())
                {
                    var clauses = validIdentifiers
                        .Select(id => AssetExposers.Identifier.NotEqual(id))
                        .Cast<FilterElement<Asset>>()
                        .ToArray();
                    filter = filter.AND(new ANDFilterElement<Asset>(clauses));
                }
            }

            return _assetRepository.Count(filter);
        }

        /// <summary>
        /// Counts AssetClasses with the specified name, excluding given identifiers.
        /// </summary>
        public long CountAssetClassesByName(string name, List<string> exceptIdentifiers = null)
        {
            if (_assetClassRepository == null || string.IsNullOrWhiteSpace(name))
            {
                return 0;
            }

            FilterElement<AssetClass> filter = AssetClassExposers.DeviceName.Equal(name);

            // Filter out null/empty identifiers (happens with new, unsaved entities)
            if (exceptIdentifiers != null && exceptIdentifiers.Any())
            {
                var validIdentifiers = exceptIdentifiers.Where(id => !string.IsNullOrWhiteSpace(id)).ToList();
                
                if (validIdentifiers.Any())
                {
                    var clauses = validIdentifiers
                        .Select(id => AssetClassExposers.Identifier.NotEqual(id))
                        .Cast<FilterElement<AssetClass>>()
                        .ToArray();
                    filter = filter.AND(new ANDFilterElement<AssetClass>(clauses));
                }
            }

            return _assetClassRepository.Count(filter);
        }


        /// <summary>
        /// Finds assets in a specific rack, excluding specified asset identifiers.
        /// </summary>
        public List<Asset> FindAssetsInRack(string rackIdentifier, List<string> excludeAssetIds = null)
        {
            if (_assetRepository == null || string.IsNullOrEmpty(rackIdentifier))
            {
                return new List<Asset>();
            }

            try
            {
                FilterElement<Asset> filter = new TRUEFilterElement<Asset>();

                if (excludeAssetIds != null && excludeAssetIds.Any())
                {
                    var clauses = excludeAssetIds.Where(id => !String.IsNullOrWhiteSpace(id))
                        .Select(id => AssetExposers.Identifier.NotEqual(id))
                        .Cast<FilterElement<Asset>>()
                        .ToArray();
                    filter = filter.AND(new ANDFilterElement<Asset>(clauses));
                }

                var allAssets = _assetRepository.Read(filter);

                // Filter in memory for assets in this rack
                return allAssets
                    .Where(a => (a.Location?.RackId != null && a.Location.RackId.HasValue() && a.Location.RackId.Identifier == rackIdentifier)).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to find assets in rack: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Finds child assets of a specific parent asset, excluding specified asset identifiers.
        /// </summary>
        public List<Asset> FindChildAssets(string parentAssetIdentifier, List<string> excludeAssetIds = null)
        {
            if (_assetRepository == null || string.IsNullOrEmpty(parentAssetIdentifier))
            {
                return new List<Asset>();
            }

            try
            {
                FilterElement<Asset> filter = AssetExposers.Identifier.NotEqual(string.Empty);

                if (excludeAssetIds != null && excludeAssetIds.Any())
                {
                    var clauses = excludeAssetIds
                        .Select(id => AssetExposers.Identifier.NotEqual(id))
                        .Cast<FilterElement<Asset>>()
                        .ToArray();
                    filter = filter.AND(new ANDFilterElement<Asset>(clauses));
                }

                var allAssets = _assetRepository.Read(filter);

                // Filter in memory for child assets
                return allAssets
                    .Where(a => (a.Location?.ParentAsset != null && a.Location.ParentAsset.Identifier == parentAssetIdentifier) ||
                               (a.DestinationLocation?.ParentAsset != null && a.DestinationLocation.ParentAsset.Identifier == parentAssetIdentifier))
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to find child assets: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Finds reservations for a specific rack.
        /// </summary>
        public List<InfraopsReservation> FindReservationsInRack(Rack rack)
        {
            if (_reservationRepository == null || rack == null)
            {
                return new List<InfraopsReservation>();
            }

            try
            {
                // Query reservations for this rack
                var filter = InfraopsReservationExposers.RackFk.Rack.Equal(rack);
                return _reservationRepository.Read(filter).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to find reservations in rack: {ex.Message}", ex);
            }
        }

        #endregion
    }
}