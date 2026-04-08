using Skyline.DataMiner.Net;
using Skyline.DataMiner.SDM;
using Skyline.DataMiner.SDM.AssetManagement.Common.Middleware;
using Skyline.DataMiner.SDM.AssetManagement.Helpers;
using Skyline.DataMiner.SDM.AssetManagement.Models;
using Skyline.DataMiner.SDM.AssetManagement.Validation;

public class AssetManagementApiHelper : IAssetManagementApiHelper
{
    private readonly AssetDomRepository _assetRepository;
    private readonly AssetClassDomRepository _assetClassRepository;
    private readonly DeviceTypeDomRepository _deviceTypeRepository;
    private readonly AssetClassValidator _assetClassValidator;
    private readonly DeviceTypeValidator _deviceTypeValidator;

    public AssetManagementApiHelper(IConnection connection)
    {
        Connection = connection;

        // Initialize repositories
        _assetRepository = new AssetDomRepository(connection);
        _assetClassRepository = new AssetClassDomRepository(connection);
        _deviceTypeRepository = new DeviceTypeDomRepository(connection);
        PowerPorts = new PowerPortDomRepository(connection);
        DataPorts = new DataPortDomRepository(connection);

        // Initialize validators
        _assetClassValidator = new AssetClassValidator(
            _assetClassRepository,
            _deviceTypeRepository);

        _deviceTypeValidator = new DeviceTypeValidator(
            _deviceTypeRepository,
            _assetRepository);

        // Wrap with middleware
        Assets = _assetRepository;  // Add middleware when ready
        AssetClasses = _assetClassRepository.WithMiddleware(
            new AssetClassValidationMiddleware(_assetClassValidator));
        DeviceTypes = _deviceTypeRepository;  // Add middleware when ready
    }

    public IConnection Connection { get; }
    public IBulkRepository<Asset> Assets { get; }
    public IBulkRepository<AssetClass> AssetClasses { get; }
    public IBulkRepository<PowerPort> PowerPorts { get; }
    public IBulkRepository<DataPort> DataPorts { get; }
    public IBulkRepository<DeviceType> DeviceTypes { get; }

    public AssetClassValidator AssetClassValidator => _assetClassValidator;
    public DeviceTypeValidator DeviceTypeValidator => _deviceTypeValidator;
}