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
        }

		public IConnection Connection { get; }

        public IBulkRepository<FacilityManagerAppSettings> AppSettings { get; }

		public IBulkRepository<Facility> Facilities { get; }

        public IBulkRepository<Desk> Desks { get; }

        public IBulkRepository<Rack> Racks { get; }

        public IBulkRepository<Room> Rooms { get; }

    }
}
