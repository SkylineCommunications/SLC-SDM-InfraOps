using Skyline.DataMiner.Net;
using Skyline.DataMiner.SDM;
using Skyline.DataMiner.SDM.AssetManagement.Common.Middleware;
using Skyline.DataMiner.SDM.AssetManagement.Helpers;
using Skyline.DataMiner.SDM.AssetManagement.Models;
using Skyline.DataMiner.SDM.AssetManagement.Validation;
using Skyline.DataMiner.SDM.Common.Services;
using Skyline.DataMiner.SDM.FacilityManagement.Models;

public class AssetManagementApiHelper : IAssetManagementApiHelper
{
    private readonly AssetDomRepository _assetRepository;
    private readonly AssetClassDomRepository _assetClassRepository;
    private readonly DeviceTypeDomRepository _deviceTypeRepository;
    private readonly DataPortDomRepository _dataPortRepository;
    private readonly PowerPortDomRepository _powerPortRepository;
    private readonly RackDomRepository _rackRepository;
    private readonly SdmEntityLoader _entityLoader;
    private readonly AssetValidator _assetValidator;
    private readonly AssetClassValidator _assetClassValidator;
    //private readonly DeviceTypeValidator _deviceTypeValidator;

    public AssetManagementApiHelper(IConnection connection)
    {
        Connection = connection;

        // Initialize repositories
        _assetRepository = new AssetDomRepository(connection);
        _assetClassRepository = new AssetClassDomRepository(connection);
        _deviceTypeRepository = new DeviceTypeDomRepository(connection);
        _dataPortRepository = new DataPortDomRepository(connection);
        _powerPortRepository = new PowerPortDomRepository(connection);
        _rackRepository = new RackDomRepository(connection);

        _entityLoader = new SdmEntityLoader(
           assetRepository: _assetRepository,
           assetClassRepository: _assetClassRepository,
           deviceTypeRepository: _deviceTypeRepository,
           rackRepository: _rackRepository,
           dataPortRepository: _dataPortRepository,
           powerPortRepository: _powerPortRepository);

        // Initialize validators
        _assetValidator = new AssetValidator(
            _assetRepository,
            _entityLoader);

        _assetClassValidator = new AssetClassValidator(_entityLoader);

        //_deviceTypeValidator = new DeviceTypeValidator(
        //    _deviceTypeRepository,
        //    _assetRepository);

        // Wrap with middleware
        Assets = _assetRepository.WithMiddleware(
            new AssetValidationMiddleware(_assetValidator));

        AssetClasses = _assetClassRepository.WithMiddleware(
            new AssetClassValidationMiddleware(_assetClassValidator));

        //DeviceTypes = _deviceTypeRepository.WithMiddleware(
        //    new DeviceTypeValidationMiddleware(_deviceTypeValidator));

        // Expose repositories directly (or wrap with middleware later)
        PowerPorts = _powerPortRepository;
        DataPorts = _dataPortRepository;
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