
namespace Skyline.DataMiner.SDM
{
	using Skyline.DataMiner.SDM.Middleware;
	using Skyline.DataMiner.SDM.AssetManagement.Models;
	using Skyline.DataMiner.SDM;

	public static class PowerPortDomRepository_Extensions
	{

		public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.AssetManagement.Models.PowerPort> WithMiddleware(
			this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.AssetManagement.Models.PowerPort> repository,
			IMiddlewareMarker<Skyline.DataMiner.SDM.AssetManagement.Models.PowerPort> middleware)
		{
			return new PowerPortDomRepository_Middleware(repository, middleware);
		}
	}
}