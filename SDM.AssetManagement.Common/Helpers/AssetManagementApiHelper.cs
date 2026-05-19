using System;

using Skyline.DataMiner.Net;
using Skyline.DataMiner.SDM;
using Skyline.DataMiner.SDM.AssetManagement.Common.Middleware;
using Skyline.DataMiner.SDM.AssetManagement.Helpers;
using Skyline.DataMiner.SDM.AssetManagement.Models;
using Skyline.DataMiner.SDM.AssetManagement.Validation;
using Skyline.DataMiner.SDM.Common.Services;
using Skyline.DataMiner.SDM.FacilityManagement.Helpers;
using Skyline.DataMiner.SDM.Middleware;

public class AssetManagementApiHelper : IAssetManagementApiHelper
{
    private readonly AssetDomRepository _assetRepository;
    private readonly AssetClassDomRepository _assetClassRepository;
    private readonly DeviceTypeDomRepository _deviceTypeRepository;
    private readonly DataPortDomRepository _dataPortRepository;
    private readonly PowerPortDomRepository _powerPortRepository;
    private readonly SdmEntityLoader _entityLoader;
    private readonly AssetValidator _assetValidator;
    private readonly AssetClassValidator _assetClassValidator;
    //private readonly DeviceTypeValidator _deviceTypeValidator;

    // Public constructor for production use - creates its own FacilityManagementHelper
    public AssetManagementApiHelper(IConnection connection)
        : this(connection, new FacilityManagementApiHelper(connection))
    {
    }

    // Internal constructor for testing - allows injection of shared FacilityManagementHelper
    internal AssetManagementApiHelper(IConnection connection, IFacilityManagementApiHelper facilityManagementHelper)
    {
        Connection = connection;

        // DEBUG: Verify this constructor is being called
        if (facilityManagementHelper == null)
        {
            throw new InvalidOperationException("INTERNAL CONSTRUCTOR CALLED BUT facilityManagementHelper IS NULL!");
        }

        // Initialize repositories
        _assetRepository = new AssetDomRepository(connection);
        _assetClassRepository = new AssetClassDomRepository(connection);
        _deviceTypeRepository = new DeviceTypeDomRepository(connection);
        _dataPortRepository = new DataPortDomRepository(connection);
        _powerPortRepository = new PowerPortDomRepository(connection);

        _entityLoader = new SdmEntityLoader(
           assetRepository: _assetRepository,
           assetClassRepository: _assetClassRepository,
           deviceTypeRepository: _deviceTypeRepository,
           rackRepository: facilityManagementHelper?.Racks,
           dataPortRepository: _dataPortRepository,
           powerPortRepository: _powerPortRepository);

        // Initialize validators
        _assetValidator = new AssetValidator(_entityLoader);

        _assetClassValidator = new AssetClassValidator(_entityLoader);

        //_deviceTypeValidator = new DeviceTypeValidator(
        //    _deviceTypeRepository,
        //    _assetRepository);

        // Wrap with middleware
        Assets = _assetRepository
            .WithMiddleware(new AssetValidationMiddleware(_assetValidator))
            .WithMiddleware(new IdentifierMiddleware<Asset>());
            

        AssetClasses = _assetClassRepository.WithMiddleware(
            new AssetClassValidationMiddleware(_assetClassValidator));

        //DeviceTypes = _deviceTypeRepository.WithMiddleware(
        //    new DeviceTypeValidationMiddleware(_deviceTypeValidator));

        // Expose repositories directly (or wrap with middleware later)
        PowerPorts = _powerPortRepository;
        DataPorts = _dataPortRepository;
        DeviceTypes = _deviceTypeRepository;
    }

    public IConnection Connection { get; }
    public IBulkRepository<Asset> Assets { get; }
    public IBulkRepository<AssetClass> AssetClasses { get; }
    public IBulkRepository<PowerPort> PowerPorts { get; }
    public IBulkRepository<DataPort> DataPorts { get; }
    public IBulkRepository<DeviceType> DeviceTypes { get; }

    public AssetValidator AssetValidator => _assetValidator;
    public AssetClassValidator AssetClassValidator => _assetClassValidator;
    //public DeviceTypeValidator DeviceTypeValidator => _deviceTypeValidator;
}