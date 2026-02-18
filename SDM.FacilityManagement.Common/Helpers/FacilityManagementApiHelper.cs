namespace Skyline.DataMiner.SDM.FacilityManagement.Helpers
{
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.SDM.FacilityManagement.Models;

	public class FacilityManagementApiHelper : IFacilityManagementApiHelper
	{
		public FacilityManagementApiHelper(IConnection connection)
		{
			Connection = connection;
			Facilities = new FacilityDomRepository(connection);
		}

		public IConnection Connection { get; }

		public IBulkRepository<Facility> Facilities { get; }
	}
}
