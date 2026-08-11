namespace Skyline.DataMiner.SDM.FacilityManagement.Helpers
{
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.SDM.FacilityManagement.Common.Middleware;
	using Skyline.DataMiner.SDM.FacilityManagement.Models;
	using Skyline.DataMiner.SDM.FacilityManagement.Services;
	using Skyline.DataMiner.SDM.FacilityManagement.Validation;
	using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Middleware;

	public class FacilityManagementApiHelper : IFacilityManagementApiHelper
	{
		private readonly SiteValidator _siteValidator;
		private readonly FacilityValidator _facilityValidator;
		private readonly FloorValidator _floorValidator;
		private readonly RoomValidator _roomValidator;
		private readonly RowValidator _rowValidator;
		private readonly ZoneValidator _zoneValidator;
		private readonly DeskValidator _deskValidator;
		private readonly RackValidator _rackValidator;

		public FacilityManagementApiHelper(IConnection connection)
		{
			Connection = connection;
            AppSettings = new FacilityManagerAppSettingsDomRepository(connection);

            var entityLoader = new FacilityEntityLoader(this);

            _siteValidator = new SiteValidator(entityLoader);
            _facilityValidator = new FacilityValidator(entityLoader);
            _floorValidator = new FloorValidator(entityLoader);
            _roomValidator = new RoomValidator(entityLoader);
            _rowValidator = new RowValidator(entityLoader);
            _zoneValidator = new ZoneValidator(entityLoader);
            _deskValidator = new DeskValidator(entityLoader);
            _rackValidator = new RackValidator(entityLoader);

            Racks = new RackDomRepository(connection)
                .WithMiddleware(new RackValidationMiddleware(_rackValidator))
                .WithMiddleware(new IdentifierMiddleware<Rack>());


            Sites = new SiteDomRepository(connection)
                .WithMiddleware(new SiteValidationMiddleware(_siteValidator))
                .WithMiddleware(new IdentifierMiddleware<Site>());

            Facilities = new FacilityDomRepository(connection)
                .WithMiddleware(new FacilityValidationMiddleware(_facilityValidator))
                .WithMiddleware(new IdentifierMiddleware<Facility>());

            Floors = new FloorDomRepository(connection)
                .WithMiddleware(new FloorValidationMiddleware(_floorValidator))
                .WithMiddleware(new IdentifierMiddleware<Floor>());

            Rooms = new RoomDomRepository(connection)
                .WithMiddleware(new RoomValidationMiddleware(_roomValidator))
                .WithMiddleware(new IdentifierMiddleware<Room>());

            Rows = new RowDomRepository(connection)
                .WithMiddleware(new RowValidationMiddleware(_rowValidator))
                .WithMiddleware(new IdentifierMiddleware<Row>());

            Zones = new ZoneDomRepository(connection)
                .WithMiddleware(new ZoneValidationMiddleware(_zoneValidator))
                .WithMiddleware(new IdentifierMiddleware<Zone>());

            Desks = new DeskDomRepository(connection)
                .WithMiddleware(new DeskValidationMiddleware(_deskValidator))
                .WithMiddleware(new IdentifierMiddleware<Desk>());
        }

		public IConnection Connection { get; }

        public IBulkRepository<FacilityManagerAppSettings> AppSettings { get; }

		public IBulkRepository<Facility> Facilities { get; }

        public IBulkRepository<Desk> Desks { get; }

        public IBulkRepository<Rack> Racks { get; }

        public IBulkRepository<Room> Rooms { get; }

        public IBulkRepository<Floor> Floors { get; }

        public IBulkRepository<Zone> Zones { get; }

        public IBulkRepository<Row> Rows { get; }

        public IBulkRepository<Site> Sites { get; }

        public SiteValidator SiteValidator => _siteValidator;

        public FacilityValidator FacilityValidator => _facilityValidator;

        public FloorValidator FloorValidator => _floorValidator;

        public RoomValidator RoomValidator => _roomValidator;

        public RowValidator RowValidator => _rowValidator;

        public ZoneValidator ZoneValidator => _zoneValidator;

        public DeskValidator DeskValidator => _deskValidator;

        public RackValidator RackValidator => _rackValidator;
    }
}
