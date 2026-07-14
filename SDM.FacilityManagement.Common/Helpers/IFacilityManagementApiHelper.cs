namespace Skyline.DataMiner.SDM.FacilityManagement.Helpers
{
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.SDM.FacilityManagement.Models;

	public interface IFacilityManagementApiHelper
	{
		IConnection Connection { get; }

        IBulkRepository<FacilityManagerAppSettings> AppSettings { get; }

		IBulkRepository<Facility> Facilities { get; }

        IBulkRepository<Desk> Desks { get; }

        IBulkRepository<Rack> Racks { get; }

        IBulkRepository<Room> Rooms { get; }

        IBulkRepository<Floor> Floors { get; }

        IBulkRepository<Zone> Zones { get; }

        IBulkRepository<Row> Rows { get; }
    }
}