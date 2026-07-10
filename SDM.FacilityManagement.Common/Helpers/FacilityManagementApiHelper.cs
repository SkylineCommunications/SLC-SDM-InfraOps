namespace Skyline.DataMiner.SDM.FacilityManagement.Helpers
{
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.SDM.FacilityManagement.Models;

	public class FacilityManagementApiHelper : IFacilityManagementApiHelper
	{
		public FacilityManagementApiHelper(IConnection connection)
		{
			Connection = connection;
            AppSettings = new FacilityManagerAppSettingsDomRepository(connection);
			Facilities = new FacilityDomRepository(connection);
            Desks = new DeskDomRepository(connection);
            Racks = new RackDomRepository(connection);
            Rooms = new RoomDomRepository(connection);
            Floors = new FloorDomRepository(connection);
            Zones = new ZoneDomRepository(connection);
            Rows = new RowDomRepository(connection);
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

    }
}
