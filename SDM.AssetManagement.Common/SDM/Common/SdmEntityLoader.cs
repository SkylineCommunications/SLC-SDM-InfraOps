namespace Skyline.DataMiner.SDM.Common.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharedCommonLibrary.AssetManagement.Models;

    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Helpers;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.FacilityManagement.Helpers;
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
        private readonly IAssetManagementApiHelper assetManagerApiHelper;
        private readonly IFacilityManagementApiHelper facilityManagerApiHelper;

        public SdmEntityLoader(
            IAssetManagementApiHelper assetManagerApiHelper = null,
            IFacilityManagementApiHelper facilityManagerApiHelper = null)
        {
            this.assetManagerApiHelper = assetManagerApiHelper;
            this.facilityManagerApiHelper = facilityManagerApiHelper;
        }

        #region Single Entity Loaders

        public Asset LoadAsset(SdmObjectReference<Asset> reference)
        {
            if (assetManagerApiHelper?.Assets == null || reference == null || !reference.HasValue())
            {
                return null;
            }

            try
            {
                var filter = AssetExposers.Identifier.Equal(reference.Identifier);
                return assetManagerApiHelper.Assets.Read(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load Asset: {ex.Message}", ex);
            }
        }

        public AssetClass LoadAssetClass(SdmObjectReference<AssetClass> reference)
        {
            if (assetManagerApiHelper?.AssetClasses == null || reference == null || !reference.HasValue())
            {
                return null;
            }

            try
            {
                var filter = AssetClassExposers.Identifier.Equal(reference.Identifier);
                return assetManagerApiHelper.AssetClasses.Read(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load AssetClass: {ex.Message}", ex);
            }
        }

        public DeviceType LoadDeviceType(SdmObjectReference<DeviceType> reference)
        {
            if (assetManagerApiHelper?.DeviceTypes == null || reference == null || !reference.HasValue())
            {
                return null;
            }

            try
            {
                var filter = DeviceTypeExposers.Identifier.Equal(reference.Identifier);
                return assetManagerApiHelper.DeviceTypes.Read(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load DeviceType: {ex.Message}", ex);
            }
        }

        public Rack LoadRack(SdmObjectReference<Rack> rack)
        {
            if (!rack.HasValue())
            {
                return null;
            }

            return LoadRack(rack.Identifier);
        }

        public Rack LoadRack(string rackId)
        {
            if (facilityManagerApiHelper?.Racks == null || string.IsNullOrEmpty(rackId))
            {
                return null;
            }

            try
            {
                var filter = RackExposers.Identifier.Equal(rackId.ToString());
                return facilityManagerApiHelper.Racks.Read(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load Rack: {ex.Message}", ex);
            }
        }

        public List<DataPort> LoadDataPorts(Asset asset)
        {
            if (assetManagerApiHelper?.DataPorts == null || asset == null || string.IsNullOrEmpty(asset.Identifier))
            {
                return new List<DataPort>();
            }

            try
            {
                var filter = DataPortExposers.Asset.Equal(asset);
                return assetManagerApiHelper.DataPorts.Read(filter).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load DataPorts: {ex.Message}", ex);
            }
        }

        public List<PowerPort> LoadPowerPorts(Asset asset)
        {
            if (assetManagerApiHelper?.PowerPorts == null || asset == null || string.IsNullOrEmpty(asset.Identifier))
            {
                return new List<PowerPort>();
            }

            try
            {
                var filter = PowerPortExposers.Asset.Equal(asset);
                return assetManagerApiHelper.PowerPorts.Read(filter).ToList();
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
            if (assetManagerApiHelper?.PortTypes == null || portTypeRef == null || !portTypeRef.HasValue())
            {
                return null;
            }

            try
            {
                var filter = PortTypeExposers.Identifier.Equal(portTypeRef.Identifier);
                return assetManagerApiHelper.PortTypes.Read(filter).FirstOrDefault();
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
            if (assetManagerApiHelper?.PortTypes == null || string.IsNullOrWhiteSpace(portTypeId))
            {
                return null;
            }

            try
            {
                var filter = PortTypeExposers.Identifier.Equal(portTypeId);
                return assetManagerApiHelper.PortTypes.Read(filter).FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load PortType: {ex.Message}", ex);
            }
        }

        #endregion

        #region Query Methods (Count/Filter Operations)

        /// <summary>
        /// Counts assets with the specified name, excluding a single identifier (e.g. the current asset on edit).
        /// <para><b>Not suitable for bulk scenarios</b>: use <see cref="GetAssetsByNames"/> for bulk uniqueness checks.</para>
        /// </summary>
        public long CountAssetsByName(string name, string exceptIdentifier = null)
        {
            if (assetManagerApiHelper?.Assets == null || string.IsNullOrWhiteSpace(name))
            {
                return 0;
            }

            FilterElement<Asset> filter = AssetExposers.AssetName.Equal(name);

            if (!string.IsNullOrWhiteSpace(exceptIdentifier))
            {
                filter = filter.AND(AssetExposers.Identifier.NotEqual(exceptIdentifier));
            }

            return assetManagerApiHelper.Assets.Count(filter);
        }

        /// <summary>
        /// Counts assets with the specified AssetID, excluding a single identifier (e.g. the current asset on edit).
        /// <para><b>Not suitable for bulk scenarios</b>: use <see cref="GetAssetsByAssetIds"/> for bulk uniqueness checks.</para>
        /// </summary>
        public long CountAssetsByAssetId(string assetId, string exceptIdentifier = null)
        {
            if (assetManagerApiHelper?.Assets == null || string.IsNullOrWhiteSpace(assetId))
            {
                return 0;
            }

            FilterElement<Asset> filter = AssetExposers.AssetId.Equal(assetId);

            if (!string.IsNullOrWhiteSpace(exceptIdentifier))
            {
                filter = filter.AND(AssetExposers.Identifier.NotEqual(exceptIdentifier));
            }

            return assetManagerApiHelper.Assets.Count(filter);
        }

        /// <summary>
        /// Counts assets with the specified Serial Number and AssetClass, excluding a single identifier (e.g. the current asset on edit).
        /// <para><b>Not suitable for bulk scenarios</b>: use <see cref="GetAssetsBySerialNumbers"/> for bulk uniqueness checks.</para>
        /// </summary>
        public long CountAssetsBySerialNumber(
            string serialNumber,
            SdmObjectReference<AssetClass> assetClassId,
            string exceptIdentifier = null)
        {
            if (assetManagerApiHelper?.Assets == null ||
                string.IsNullOrWhiteSpace(serialNumber) ||
                assetClassId == null ||
                !assetClassId.HasValue())
            {
                return 0;
            }

            FilterElement<Asset> filter = AssetExposers.SerialNumber.Equal(serialNumber)
                .AND(AssetExposers.AssetClass.Equal(assetClassId));

            if (!string.IsNullOrWhiteSpace(exceptIdentifier))
            {
                filter = filter.AND(AssetExposers.Identifier.NotEqual(exceptIdentifier));
            }

            return assetManagerApiHelper.Assets.Count(filter);
        }

        /// <summary>
        /// Counts CableTypes with the specified name, excluding a single identifier (e.g. the current cable type on edit).
        /// <para><b>Not suitable for bulk scenarios</b>: use <see cref="GetCableTypesByNames"/> when checking multiple names at once.</para>
        /// </summary>
        public long CountCableTypesByName(string name, string exceptIdentifier = null)
        {
            if (assetManagerApiHelper?.CableTypes == null || string.IsNullOrWhiteSpace(name))
            {
                return 0;
            }

            FilterElement<CableType> filter = CableTypeExposers.Name.Equal(name);

            if (!string.IsNullOrWhiteSpace(exceptIdentifier))
            {
                filter = filter.AND(CableTypeExposers.Identifier.NotEqual(exceptIdentifier));
            }

            return assetManagerApiHelper.CableTypes.Count(filter);
        }

        /// <summary>
        /// Counts AssetClasses with the specified name, excluding a single identifier (e.g. the current asset class on edit).
        /// <para><b>Not suitable for bulk scenarios</b>: use <see cref="GetAssetClassesByNames"/> when checking multiple names at once.</para>
        /// </summary>
        public long CountAssetClassesByName(string name, string exceptIdentifier = null)
        {
            if (assetManagerApiHelper?.AssetClasses == null || string.IsNullOrWhiteSpace(name))
            {
                return 0;
            }

            FilterElement<AssetClass> filter = AssetClassExposers.DeviceName.Equal(name);

            if (!string.IsNullOrWhiteSpace(exceptIdentifier))
            {
                filter = filter.AND(AssetClassExposers.Identifier.NotEqual(exceptIdentifier));
            }

            return assetManagerApiHelper.AssetClasses.Count(filter);
        }

        /// <summary>
        /// Retrieves all AssetClasses whose Name matches any of the provided names.
        /// Uses <see cref="Tools.RetrieveBigOrFilter"/> to safely handle large sets without
        /// creating an oversized OR filter in a single call.
        /// </summary>
        public List<AssetClass> GetAssetClassesByNames(List<string> names)
        {
            if (assetManagerApiHelper?.AssetClasses == null || names == null || !names.Any())
            {
                return new List<AssetClass>();
            }

            return Tools.RetrieveBigOrFilter(
                names,
                name => AssetClassExposers.DeviceName.Equal(name),
                filter => assetManagerApiHelper.AssetClasses.Read(filter).ToList());
        }

        /// <summary>
        /// Retrieves all Assets whose Name matches any of the provided names.
        /// Uses <see cref="Tools.RetrieveBigOrFilter"/> to safely handle large sets without
        /// creating an oversized OR filter in a single call.
        /// </summary>
        public List<Asset> GetAssetsByNames(List<string> names)
        {
            if (assetManagerApiHelper?.Assets == null || names == null || !names.Any())
            {
                return new List<Asset>();
            }

            return Tools.RetrieveBigOrFilter(
                names,
                name => AssetExposers.AssetName.Equal(name),
                filter => assetManagerApiHelper.Assets.Read(filter).ToList());
        }

        /// <summary>
        /// Retrieves all Assets whose AssetID matches any of the provided IDs.
        /// Uses <see cref="Tools.RetrieveBigOrFilter"/> to safely handle large sets without
        /// creating an oversized OR filter in a single call.
        /// </summary>
        public List<Asset> GetAssetsByAssetIds(List<string> assetIds)
        {
            if (assetManagerApiHelper?.Assets == null || assetIds == null || !assetIds.Any())
            {
                return new List<Asset>();
            }

            return Tools.RetrieveBigOrFilter(
                assetIds,
                id => AssetExposers.AssetId.Equal(id),
                filter => assetManagerApiHelper.Assets.Read(filter).ToList());
        }

        /// <summary>
        /// Retrieves all Assets whose SerialNumber matches any of the provided serial numbers,
        /// scoped to the given AssetClass.
        /// Uses <see cref="Tools.RetrieveBigOrFilter"/> to safely handle large sets without
        /// creating an oversized OR filter in a single call.
        /// </summary>
        public List<Asset> GetAssetsBySerialNumbers(
            SdmObjectReference<AssetClass> assetClassId, List<string> serialNumbers)
        {
            if (assetManagerApiHelper?.Assets == null ||
                serialNumbers == null || !serialNumbers.Any() ||
                assetClassId == null || !assetClassId.HasValue())
            {
                return new List<Asset>();
            }

            return Tools.RetrieveBigOrFilter(
                serialNumbers,
                sn => AssetExposers.SerialNumber.Equal(sn)
                          .AND(AssetExposers.AssetClass.Equal(assetClassId)),
                filter => assetManagerApiHelper.Assets.Read(filter).ToList());
        }
        /// <summary>
        /// Retrieves all Assets whose DOM identifier matches any of the provided identifiers.
        /// Uses <see cref="Tools.RetrieveBigOrFilter"/> to safely handle large sets without
        /// creating an oversized OR filter in a single call.
        /// </summary>
        public List<Asset> GetAssetsByDomIds(List<string> identifiers)
        {
            if (assetManagerApiHelper?.Assets == null || identifiers == null || !identifiers.Any())
            {
                return new List<Asset>();
            }

            return Tools.RetrieveBigOrFilter(
                identifiers,
                id => AssetExposers.Identifier.Equal(id),
                filter => assetManagerApiHelper.Assets.Read(filter).ToList());
        }

        /// <summary>
        /// Retrieves all CableTypes whose Name matches any of the provided names.
        /// Uses <see cref="Tools.RetrieveBigOrFilter"/> to safely handle large sets without
        /// creating an oversized OR filter in a single call.
        /// </summary>
        public List<CableType> GetCableTypesByNames(List<string> names)
        {
            if (assetManagerApiHelper?.CableTypes == null || names == null || !names.Any())
            {
                return new List<CableType>();
            }

            return Tools.RetrieveBigOrFilter(
                names,
                name => CableTypeExposers.Name.Equal(name),
                filter => assetManagerApiHelper.CableTypes.Read(filter).ToList());
        }



        /// <para><b>Not suitable for bulk scenarios</b>: builds an AND clause per excluded identifier.</para>
        /// </summary>
        public List<Asset> FindAssetsInRack(string rackIdentifier, List<string> excludeAssetIds = null)
        {
            if (assetManagerApiHelper?.Assets == null || string.IsNullOrEmpty(rackIdentifier))
            {
                return new List<Asset>();
            }

            try
            {
                FilterElement<Asset> filter = new TRUEFilterElement<Asset>();

                if (excludeAssetIds != null && excludeAssetIds.Any())
                {
                    var validIdentifiers = excludeAssetIds.Where(id => !string.IsNullOrWhiteSpace(id)).ToList();

                    if (validIdentifiers.Any())
                    {
                        var clauses = validIdentifiers
                            .Select(id => AssetExposers.Identifier.NotEqual(id))
                            .Cast<FilterElement<Asset>>()
                            .ToArray();
                        filter = filter.AND(new ANDFilterElement<Asset>(clauses));
                    }
                }

                var allAssets = assetManagerApiHelper.Assets.Read(filter);

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
        /// <para><b>Not suitable for bulk scenarios</b>: builds an AND clause per excluded identifier.</para>
        /// </summary>
        public List<Asset> FindChildAssets(string parentAssetIdentifier, List<string> excludeAssetIds = null)
        {
            if (assetManagerApiHelper?.Assets == null || string.IsNullOrEmpty(parentAssetIdentifier))
            {
                return new List<Asset>();
            }

            try
            {
                FilterElement<Asset> filter = new TRUEFilterElement<Asset>();

                if (excludeAssetIds != null && excludeAssetIds.Any())
                {
                    var validIdentifiers = excludeAssetIds.Where(id => !string.IsNullOrWhiteSpace(id)).ToList();

                    if (validIdentifiers.Any())
                    {
                        var clauses = validIdentifiers
                            .Select(id => AssetExposers.Identifier.NotEqual(id))
                            .Cast<FilterElement<Asset>>()
                            .ToArray();
                        filter = filter.AND(new ANDFilterElement<Asset>(clauses));
                    }
                }

                var allAssets = assetManagerApiHelper.Assets.Read(filter.AND(AssetExposers.Location.ParentAsset.Equal(new SdmObjectReference<Asset>(parentAssetIdentifier))));

                return allAssets.ToList();
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
            if (assetManagerApiHelper?.Reservations == null || rack == null)
            {
                return new List<InfraopsReservation>();
            }

            try
            {
                // Query reservations for this rack
                var filter = InfraopsReservationExposers.RackFk.Rack.Equal(rack);
                return assetManagerApiHelper.Reservations.Read(filter).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to find reservations in rack: {ex.Message}", ex);
            }
        }

        #endregion
    }
}