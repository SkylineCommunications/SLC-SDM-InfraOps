namespace Skyline.DataMiner.SDM.FacilityManagement.Helpers
{
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.SDM.FacilityManagement.Models;

	public interface IFacilityManagementApiHelper
	{
		IConnection Connection { get; }

		IBulkRepository<Facility> Facilities { get; }

        IBulkRepository<Rack> Racks { get; }
    }
}