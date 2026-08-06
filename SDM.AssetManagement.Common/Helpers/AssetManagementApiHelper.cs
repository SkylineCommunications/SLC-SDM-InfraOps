using System;

using Skyline.DataMiner.Net;
using Skyline.DataMiner.SDM;
using Skyline.DataMiner.SDM.AssetManagement.Common.Middleware;
using Skyline.DataMiner.SDM.AssetManagement.Models;
using Skyline.DataMiner.SDM.AssetManagement.Helpers;
using Skyline.DataMiner.SDM.AssetManagement.Validation;
using Skyline.DataMiner.SDM.Common.Services;
using Skyline.DataMiner.SDM.FacilityManagement.Helpers;
using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;

using Connection = Skyline.DataMiner.SDM.AssetManagement.Models.Connection;

using SharedCommonLibrary.AssetManagement.Models;

public class AssetManagementApiHelper : IAssetManagementApiHelper
{
    private readonly AssetValidator _assetValidator;
    private readonly AssetClassValidator _assetClassValidator;
    private readonly DataPortValidator _dataPortValidator;
    private readonly PowerPortValidator _powerPortValidator;
    private readonly DeviceTypeValidator _deviceTypeValidator;
    private readonly PortTypeValidator _portTypeValidator;
    private readonly CableTypeValidator _cableTypeValidator;

    // Public constructor for production use - creates its own FacilityManagementHelper
    public AssetManagementApiHelper(IConnection connection, IAssetManagementExternalReferenceChecker externalReferenceChecker = null)
        : this(connection, new FacilityManagementApiHelper(connection), externalReferenceChecker)
    {
    }

    // Internal constructor for testing - allows injection of shared FacilityManagementHelper
    internal AssetManagementApiHelper(
        IConnection connection,
        IFacilityManagementApiHelper facilityManagementHelper,
        IAssetManagementExternalReferenceChecker externalReferenceChecker = null)
    {
        ExternalReferenceChecker = externalReferenceChecker;
        // DEBUG: Verify this constructor is being called
        if (facilityManagementHelper == null)
        {
            throw new InvalidOperationException("INTERNAL CONSTRUCTOR CALLED BUT facilityManagementHelper IS NULL!");
        }

        // Initialize repositories
        var assetRepository = new AssetDomRepository(connection);
        var appSettingsRepository = new AssetManagerAppSettingsDomRepository(connection);
        var assetClassRepository = new AssetClassDomRepository(connection);
        var deviceTypeRepository = new DeviceTypeDomRepository(connection);
        var dataPortRepository = new DataPortDomRepository(connection);
        var powerPortRepository = new PowerPortDomRepository(connection);
        var portTypeRepository = new PortTypeDomRepository(connection);
        var cableTypeRepository = new CableTypeDomRepository(connection);   
        var connectionDomRepository = new ConnectionDomRepository(connection);
        var reservationRepository = new InfraopsReservationDomRepository(connection);

        var entityLoader = new SdmEntityLoader(this, facilityManagementHelper);

        // Initialize validators
        _assetValidator = new AssetValidator(entityLoader);

        _assetClassValidator = new AssetClassValidator(entityLoader);
        _dataPortValidator = new DataPortValidator(entityLoader);
        _powerPortValidator = new PowerPortValidator(entityLoader);
        _deviceTypeValidator = new DeviceTypeValidator(entityLoader);
        _portTypeValidator = new PortTypeValidator(entityLoader);
        _cableTypeValidator = new CableTypeValidator(entityLoader);
        // Wrap with middleware
        Assets = assetRepository
            .WithMiddleware(new AssetValidationMiddleware(_assetValidator))
            .WithMiddleware(new IdentifierMiddleware<Asset>());

        AppSettings = appSettingsRepository;

        AssetClasses = assetClassRepository
            .WithMiddleware(new AssetClassValidationMiddleware(_assetClassValidator))
            .WithMiddleware(new IdentifierMiddleware<AssetClass>());

        PowerPorts = powerPortRepository
            .WithMiddleware(new PowerPortValidationMiddleware(_powerPortValidator))
            .WithMiddleware(new IdentifierMiddleware<PowerPort>());
        DataPorts = dataPortRepository
            .WithMiddleware(new DataPortValidationMiddleware(_dataPortValidator))
            .WithMiddleware(new IdentifierMiddleware<DataPort>());
        DeviceTypes = deviceTypeRepository
            .WithMiddleware(new DeviceTypeValidationMiddleware(_deviceTypeValidator))
            .WithMiddleware(new IdentifierMiddleware<DeviceType>());
        PortTypes = portTypeRepository
            .WithMiddleware(new PortTypeValidationMiddleware(_portTypeValidator))
            .WithMiddleware(new IdentifierMiddleware<PortType>());
        var connectionValidator = new ConnectionValidator(entityLoader);
        Connections = connectionDomRepository
            .WithMiddleware(new ConnectionValidationMiddleware(connectionValidator))
            .WithMiddleware(new IdentifierMiddleware<Connection>());
        CableTypes = cableTypeRepository
            .WithMiddleware(new CableTypeValidationMiddleware(_cableTypeValidator))
            .WithMiddleware(new IdentifierMiddleware<CableType>());
        var reservationValidator = new InfraopsReservationValidator(entityLoader);
        Reservations = reservationRepository
            .WithMiddleware(new InfraopsReservationValidationMiddleware(reservationValidator))
            .WithMiddleware(new IdentifierMiddleware<InfraopsReservation>());
    }

    public IAssetManagementExternalReferenceChecker ExternalReferenceChecker { get; }

    public IAssetRepository Assets { get; }
    public IBulkRepository<AssetManagerAppSettings> AppSettings { get; }
    public IBulkRepository<AssetClass> AssetClasses { get; }
    public IBulkRepository<PowerPort> PowerPorts { get; }
    public IBulkRepository<DataPort> DataPorts { get; }
    public IBulkRepository<DeviceType> DeviceTypes { get; }
    public IBulkRepository<PortType> PortTypes { get; }
    public IBulkRepository<Connection> Connections { get; }
    public IBulkRepository<CableType> CableTypes { get; }
    public IBulkRepository<InfraopsReservation> Reservations { get; }

    public AssetValidator AssetValidator => _assetValidator;

    public AssetClassValidator AssetClassValidator => _assetClassValidator;

    public DataPortValidator DataPortValidator => _dataPortValidator;

    public PowerPortValidator PowerPortValidator => _powerPortValidator;

    public DeviceTypeValidator DeviceTypeValidator => _deviceTypeValidator;

    public PortTypeValidator PortTypeValidator => _portTypeValidator;

    public CableTypeValidator CableTypeValidator => _cableTypeValidator;
}