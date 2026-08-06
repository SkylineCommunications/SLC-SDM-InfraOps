namespace Skyline.DataMiner.SDM.FacilityManagement.Helpers
{
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.SDM.FacilityManagement.Models;
	using Skyline.DataMiner.SDM.FacilityManagement.Validation;

	public interface IFacilityManagementApiHelper
	{
		IConnection Connection { get; }

        IFacilityManagementExternalReferenceChecker ExternalReferenceChecker { get; }

        IBulkRepository<FacilityManagerAppSettings> AppSettings { get; }

		IBulkRepository<Facility> Facilities { get; }

        IBulkRepository<Desk> Desks { get; }

        IBulkRepository<Rack> Racks { get; }

        IBulkRepository<Room> Rooms { get; }

        IBulkRepository<Floor> Floors { get; }

        IBulkRepository<Zone> Zones { get; }

        IBulkRepository<Row> Rows { get; }

        IBulkRepository<Site> Sites { get; }
    }
}