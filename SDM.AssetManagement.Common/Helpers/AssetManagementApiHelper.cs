using System;

using Skyline.DataMiner.Net;
using Skyline.DataMiner.SDM;
using Skyline.DataMiner.SDM.AssetManagement.Common.Middleware;
using Skyline.DataMiner.SDM.AssetManagement.Models;
using Skyline.DataMiner.SDM.AssetManagement.Helpers;
using Skyline.DataMiner.SDM.AssetManagement.Validation;
using Skyline.DataMiner.SDM.Common.Services;
using Skyline.DataMiner.SDM.FacilityManagement.Helpers;

using Connection = Skyline.DataMiner.SDM.AssetManagement.Models.Connection;

public class AssetManagementApiHelper : IAssetManagementApiHelper
{
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
        // DEBUG: Verify this constructor is being called
        if (facilityManagementHelper == null)
        {
            throw new InvalidOperationException("INTERNAL CONSTRUCTOR CALLED BUT facilityManagementHelper IS NULL!");
        }

        // Initialize repositories
        var assetRepository = new AssetDomRepository(connection);
        var assetClassRepository = new AssetClassDomRepository(connection);
        var deviceTypeRepository = new DeviceTypeDomRepository(connection);
        var dataPortRepository = new DataPortDomRepository(connection);
        var powerPortRepository = new PowerPortDomRepository(connection);
        var portTypeRepository = new PortTypeDomRepository(connection);
        var connectionDomRepository = new ConnectionDomRepository(connection);

        var entityLoader = new SdmEntityLoader(
           assetRepository: assetRepository,
           assetClassRepository: assetClassRepository,
           deviceTypeRepository: deviceTypeRepository,
           rackRepository: facilityManagementHelper.Racks,
           dataPortRepository: dataPortRepository,
           powerPortRepository: powerPortRepository,
           portTypeRepository: portTypeRepository);

        // Initialize validators
        _assetValidator = new AssetValidator(entityLoader);

        _assetClassValidator = new AssetClassValidator(entityLoader);

        //_deviceTypeValidator = new DeviceTypeValidator(
        //    _deviceTypeRepository,
        //    _assetRepository);

        // Wrap with middleware
        Assets = assetRepository
            .WithMiddleware(new AssetValidationMiddleware(_assetValidator))
            .WithMiddleware(new IdentifierMiddleware<Asset>());
            

        AssetClasses = assetClassRepository.WithMiddleware(new AssetClassValidationMiddleware(_assetClassValidator))
            .WithMiddleware(new IdentifierMiddleware<AssetClass>());

        //DeviceTypes = _deviceTypeRepository.WithMiddleware(
        //    new DeviceTypeValidationMiddleware(_deviceTypeValidator));

        // Expose repositories directly (or wrap with middleware later)
        PowerPorts = powerPortRepository;
        DataPorts = dataPortRepository;
        DeviceTypes = deviceTypeRepository;
        PortTypes = portTypeRepository;
        Connections = connectionDomRepository;
    }

    public IAssetRepository Assets { get; }
    public IBulkRepository<AssetClass> AssetClasses { get; }
    public IBulkRepository<PowerPort> PowerPorts { get; }
    public IBulkRepository<DataPort> DataPorts { get; }
    public IBulkRepository<DeviceType> DeviceTypes { get; }
    public IBulkRepository<PortType> PortTypes { get; }
    public IBulkRepository<Connection> Connections { get; }

    public AssetValidator AssetValidator => _assetValidator;

    public AssetClassValidator AssetClassValidator => _assetClassValidator;
    
}