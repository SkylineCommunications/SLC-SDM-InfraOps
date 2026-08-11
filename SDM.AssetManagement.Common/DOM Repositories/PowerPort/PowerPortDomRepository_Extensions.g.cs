
namespace Skyline.DataMiner.SDM
{
	using Skyline.DataMiner.SDM.Middleware;
	using Skyline.DataMiner.SDM.AssetManagement.Models;
	using Skyline.DataMiner.SDM;

	public static class PowerPortDomRepository_Extensions
	{

		public static IBulkRepository<AssetManagement.Models.PowerPort> WithMiddleware(
			this IBulkRepository<AssetManagement.Models.PowerPort> repository,
			IMiddlewareMarker<AssetManagement.Models.PowerPort> middleware)
		{
			return new PowerPortDomRepository_Middleware(repository, middleware);
		}
	}
}