
namespace Skyline.DataMiner.SDM
{
	using Skyline.DataMiner.SDM.Middleware;
	using Skyline.DataMiner.SDM.AssetManagement.Models;
	using Skyline.DataMiner.SDM;

	public static class DataPortDomRepository_Extensions
	{

		public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.AssetManagement.Models.DataPort> WithMiddleware(
			this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.AssetManagement.Models.DataPort> repository,
			IMiddlewareMarker<Skyline.DataMiner.SDM.AssetManagement.Models.DataPort> middleware)
		{
			return new DataPortDomRepository_Middleware(repository, middleware);
		}
	}
}