namespace Skyline.DataMiner.SDM.Common.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Models;

    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Helpers;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;
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

        public List<DeviceType> GetDeviceTypesByDomIds(List<string> identifiers)
        {
            if (assetManagerApiHelper?.DeviceTypes == null || identifiers == null || !identifiers.Any())
            {
                return new List<DeviceType>();
            }

            return Tools.RetrieveBigOrFilter(
                identifiers,
                id => DeviceTypeExposers.Identifier.Equal(id),
                filter => assetManagerApiHelper.DeviceTypes.Read(filter).ToList());
        }

        public List<AssetClass> GetAssetClassesByDomIds(List<string> identifiers)
        {
            if (assetManagerApiHelper?.AssetClasses == null || identifiers == null || !identifiers.Any())
            {
                return new List<AssetClass>();
            }

            return Tools.RetrieveBigOrFilter(
                identifiers,
                id => AssetClassExposers.Identifier.Equal(id),
                filter => assetManagerApiHelper.AssetClasses.Read(filter).ToList());
        }

        public List<Asset> GetAssetsByAssetClassIds(List<string> assetClassIds)
        {
            if (assetManagerApiHelper?.Assets == null || assetClassIds == null || !assetClassIds.Any())
            {
                return new List<Asset>();
            }

            return Tools.RetrieveBigOrFilter(
                assetClassIds,
                id => AssetExposers.AssetClass.Equal(new SdmObjectReference<AssetClass>(id)),
                filter => assetManagerApiHelper.Assets.Read(filter).ToList());
        }

        public List<AssetClass> GetAssetClassesByDeviceTypeIds(List<string> deviceTypeIds)
        {
            if (assetManagerApiHelper?.AssetClasses == null || deviceTypeIds == null || !deviceTypeIds.Any())
            {
                return new List<AssetClass>();
            }

            return Tools.RetrieveBigOrFilter(
                deviceTypeIds,
                id => AssetClassExposers.DeviceTypeId.Equal(new SdmObjectReference<DeviceType>(id)),
                filter => assetManagerApiHelper.AssetClasses.Read(filter).ToList());
        }

        public List<DataPort> GetDataPortsByAssetIds(List<string> assetIds)
        {
            if (assetManagerApiHelper?.DataPorts == null || assetIds == null || !assetIds.Any())
            {
                return new List<DataPort>();
            }

            return Tools.RetrieveBigOrFilter(
                assetIds,
                id => DataPortExposers.Asset.Equal(new SdmObjectReference<Asset>(id)),
                filter => assetManagerApiHelper.DataPorts.Read(filter).ToList());
        }

        public List<PowerPort> GetPowerPortsByAssetIds(List<string> assetIds)
        {
            if (assetManagerApiHelper?.PowerPorts == null || assetIds == null || !assetIds.Any())
            {
                return new List<PowerPort>();
            }

            return Tools.RetrieveBigOrFilter(
                assetIds,
                id => PowerPortExposers.Asset.Equal(new SdmObjectReference<Asset>(id)),
                filter => assetManagerApiHelper.PowerPorts.Read(filter).ToList());
        }

        /// <summary>
        /// Retrieves all PortTypes whose identifier matches any of the provided identifiers.
        /// Uses <see cref="Tools.RetrieveBigOrFilter"/> to safely handle large sets without
        /// creating an oversized OR filter in a single call. Suitable for bulk scenarios.
        /// </summary>
        public List<PortType> GetPortTypesByDomIds(List<string> identifiers)
        {
            if (assetManagerApiHelper?.PortTypes == null || identifiers == null || !identifiers.Any())
            {
                return new List<PortType>();
            }

            return Tools.RetrieveBigOrFilter(
                identifiers,
                id => PortTypeExposers.Identifier.Equal(id),
                filter => assetManagerApiHelper.PortTypes.Read(filter).ToList());
        }

        public List<CableType> GetCableTypesByDomIds(List<string> identifiers)
        {
            if (assetManagerApiHelper?.CableTypes == null || identifiers == null || !identifiers.Any())
            {
                return new List<CableType>();
            }

            return Tools.RetrieveBigOrFilter(
                identifiers,
                id => CableTypeExposers.Identifier.Equal(id),
                filter => assetManagerApiHelper.CableTypes.Read(filter).ToList());
        }

        public List<DataPort> GetDataPortsByDomIds(List<string> identifiers)
        {
            if (assetManagerApiHelper?.DataPorts == null || identifiers == null || !identifiers.Any())
            {
                return new List<DataPort>();
            }

            return Tools.RetrieveBigOrFilter(
                identifiers,
                id => DataPortExposers.Identifier.Equal(id),
                filter => assetManagerApiHelper.DataPorts.Read(filter).ToList());
        }

        public List<PowerPort> GetPowerPortsByDomIds(List<string> identifiers)
        {
            if (assetManagerApiHelper?.PowerPorts == null || identifiers == null || !identifiers.Any())
            {
                return new List<PowerPort>();
            }

            return Tools.RetrieveBigOrFilter(
                identifiers,
                id => PowerPortExposers.Identifier.Equal(id),
                filter => assetManagerApiHelper.PowerPorts.Read(filter).ToList());
        }

        public List<DataPort> GetDataPortsByPortTypeIds(List<string> portTypeIds)
        {
            if (assetManagerApiHelper?.DataPorts == null || portTypeIds == null || !portTypeIds.Any())
            {
                return new List<DataPort>();
            }

            return Tools.RetrieveBigOrFilter(
                portTypeIds,
                id => DataPortExposers.DataPortInfo.Type.Equal(new SdmObjectReference<PortType>(id)),
                filter => assetManagerApiHelper.DataPorts.Read(filter).ToList());
        }

        public List<PowerPort> GetPowerPortsByPortTypeIds(List<string> portTypeIds)
        {
            if (assetManagerApiHelper?.PowerPorts == null || portTypeIds == null || !portTypeIds.Any())
            {
                return new List<PowerPort>();
            }

            var portTypeGuids = portTypeIds
                .Where(id => Guid.TryParse(id, out _))
                .Select(Guid.Parse)
                .Distinct()
                .ToList();

            if (!portTypeGuids.Any())
            {
                return new List<PowerPort>();
            }

            var powerPortTypeExposer = new Exposer<PowerPort, Guid>(
                obj => Guid.Parse(obj.PowerPortInfo.PortType.Identifier),
                "PowerPortInfo.PortType");

            return Tools.RetrieveBigOrFilter(
                portTypeGuids,
                id => powerPortTypeExposer.Equal(id),
                filter => assetManagerApiHelper.PowerPorts.Read(filter).ToList());
        }

        public List<AssetClass> GetAssetClassesByDataPortTypeIds(List<string> portTypeIds)
        {
            if (assetManagerApiHelper?.AssetClasses == null || portTypeIds == null || !portTypeIds.Any())
            {
                return new List<AssetClass>();
            }

            var portTypeGuids = portTypeIds
                .Where(id => Guid.TryParse(id, out _))
                .Select(Guid.Parse)
                .Distinct()
                .ToList();

            if (!portTypeGuids.Any())
            {
                return new List<AssetClass>();
            }

            var dataPortTypeExposer = new Exposers.CollectionExposer<AssetClass, Guid>(
                obj => obj.DataPorts
                    .Where(port => port?.Type != null && port.Type.HasValue())
                    .Select(port => Guid.Parse(port.Type.Identifier)),
                "DataPorts.Type");

            var matches = Tools.RetrieveBigOrFilter(
                portTypeGuids,
                id => dataPortTypeExposer.Contains(id),
                filter => assetManagerApiHelper.AssetClasses.Read(filter).ToList());

            if (matches.Any())
            {
                return matches;
            }

            var lookup = new HashSet<string>(portTypeIds);
            return assetManagerApiHelper.AssetClasses.Read(new TRUEFilterElement<AssetClass>())
                .Where(assetClass => assetClass.DataPorts.Any(port => port?.Type != null
                    && port.Type.HasValue()
                    && lookup.Contains(port.Type.Identifier)))
                .ToList();
        }

        public List<AssetClass> GetAssetClassesByPowerPortTypeIds(List<string> portTypeIds)
        {
            if (assetManagerApiHelper?.AssetClasses == null || portTypeIds == null || !portTypeIds.Any())
            {
                return new List<AssetClass>();
            }

            var portTypeGuids = portTypeIds
                .Where(id => Guid.TryParse(id, out _))
                .Select(Guid.Parse)
                .Distinct()
                .ToList();

            if (!portTypeGuids.Any())
            {
                return new List<AssetClass>();
            }

            var powerPortTypeExposer = new Exposers.CollectionExposer<AssetClass, Guid>(
                obj => obj.PowerPorts
                    .Where(port => port?.PortType != null && port.PortType.HasValue())
                    .Select(port => Guid.Parse(port.PortType.Identifier)),
                "PowerPorts.PortType");

            var matches = Tools.RetrieveBigOrFilter(
                portTypeGuids,
                id => powerPortTypeExposer.Contains(id),
                filter => assetManagerApiHelper.AssetClasses.Read(filter).ToList());

            if (matches.Any())
            {
                return matches;
            }

            var lookup = new HashSet<string>(portTypeIds);
            return assetManagerApiHelper.AssetClasses.Read(new TRUEFilterElement<AssetClass>())
                .Where(assetClass => assetClass.PowerPorts.Any(port => port?.PortType != null
                    && port.PortType.HasValue()
                    && lookup.Contains(port.PortType.Identifier)))
                .ToList();
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

        public List<AssetManagement.Models.Connection> GetConnectionsByCableTypeIds(List<string> cableTypeIds)
        {
            if (assetManagerApiHelper?.Connections == null || cableTypeIds == null || !cableTypeIds.Any())
            {
                return new List<AssetManagement.Models.Connection>();
            }

            return Tools.RetrieveBigOrFilter(
                cableTypeIds,
                id => ConnectionExposers.CableType.Equal(new SdmObjectReference<CableType>(id)),
                filter => assetManagerApiHelper.Connections.Read(filter).ToList());
        }

        public List<PortType> GetPortTypesByCableTypeIds(List<string> cableTypeIds)
        {
            if (assetManagerApiHelper?.PortTypes == null || cableTypeIds == null || !cableTypeIds.Any())
            {
                return new List<PortType>();
            }

            return Tools.RetrieveBigOrFilter(
                cableTypeIds,
                id => PortTypeExposers.CableFKs.CableTypeFks.Contains(new SdmObjectReference<CableType>(id)),
                filter => assetManagerApiHelper.PortTypes.Read(filter).ToList());
        }

        public List<AssetManagement.Models.Connection> GetConnectionsByPortIds(List<string> portIds)
        {
            if (assetManagerApiHelper?.Connections == null || portIds == null || !portIds.Any())
            {
                return new List<AssetManagement.Models.Connection>();
            }

            var portGuids = portIds
                .Where(id => Guid.TryParse(id, out _))
                .Select(Guid.Parse)
                .Distinct()
                .ToList();

            if (!portGuids.Any())
            {
                return new List<AssetManagement.Models.Connection>();
            }

            return Tools.RetrieveBigOrFilter(
                portGuids,
                portId => ConnectionExposers.Source.Port.Equal(portId)
                    .OR(ConnectionExposers.Destination.Port.Equal(portId)),
                filter => assetManagerApiHelper.Connections.Read(filter).ToList());
        }

        public List<Rack> GetRacksByDomIds(List<string> identifiers)
        {
            if (facilityManagerApiHelper?.Racks == null || identifiers == null || !identifiers.Any())
            {
                return new List<Rack>();
            }

            return Tools.RetrieveBigOrFilter(
                identifiers,
                id => RackExposers.Identifier.Equal(id),
                filter => facilityManagerApiHelper.Racks.Read(filter).ToList());
        }

        public List<Facility> GetFacilitiesByDomIds(List<string> identifiers)
        {
            if (facilityManagerApiHelper?.Facilities == null || identifiers == null || !identifiers.Any())
            {
                return new List<Facility>();
            }

            return Tools.RetrieveBigOrFilter(
                identifiers,
                id => FacilityExposers.Identifier.Equal(id),
                filter => facilityManagerApiHelper.Facilities.Read(filter).ToList());
        }

        public List<Room> GetRoomsByDomIds(List<string> identifiers)
        {
            if (facilityManagerApiHelper?.Rooms == null || identifiers == null || !identifiers.Any())
            {
                return new List<Room>();
            }

            return Tools.RetrieveBigOrFilter(
                identifiers,
                id => RoomExposers.Identifier.Equal(id),
                filter => facilityManagerApiHelper.Rooms.Read(filter).ToList());
        }

        public List<Desk> GetDesksByDomIds(List<string> identifiers)
        {
            if (facilityManagerApiHelper?.Desks == null || identifiers == null || !identifiers.Any())
            {
                return new List<Desk>();
            }

            return Tools.RetrieveBigOrFilter(
                identifiers,
                id => DeskExposers.Identifier.Equal(id),
                filter => facilityManagerApiHelper.Desks.Read(filter).ToList());
        }



        /// <summary>
        /// Builds a filter that excludes the supplied asset identifiers. Returns a match-all filter
        /// when no valid identifiers are supplied.
        /// </summary>
        private static FilterElement<Asset> BuildExcludeAssetIdsFilter(List<string> excludeAssetIds)
        {
            FilterElement<Asset> filter = new TRUEFilterElement<Asset>();

            if (excludeAssetIds == null || !excludeAssetIds.Any())
            {
                return filter;
            }

            var validIdentifiers = excludeAssetIds.Where(id => !string.IsNullOrWhiteSpace(id)).ToList();

            if (validIdentifiers.Any())
            {
                var clauses = validIdentifiers
                    .Select(id => AssetExposers.Identifier.NotEqual(id))
                    .Cast<FilterElement<Asset>>()
                    .ToArray();
                filter = filter.AND(new ANDFilterElement<Asset>(clauses));
            }

            return filter;
        }

        /// <summary>
        /// Finds assets in a rack, excluding optional asset identifiers.
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
                var filter = BuildExcludeAssetIdsFilter(excludeAssetIds);

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
                var filter = BuildExcludeAssetIdsFilter(excludeAssetIds);

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