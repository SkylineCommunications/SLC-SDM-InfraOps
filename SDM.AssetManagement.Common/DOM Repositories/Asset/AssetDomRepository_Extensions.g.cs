
namespace Skyline.DataMiner.SDM
{
	using Skyline.DataMiner.SDM.Middleware;
	using Skyline.DataMiner.SDM.AssetManagement.Models;
	using Skyline.DataMiner.SDM;

	public static class AssetDomRepository_Extensions
	{

		public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.AssetManagement.Models.Asset> WithMiddleware(
			this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.AssetManagement.Models.Asset> repository,
			IMiddlewareMarker<Skyline.DataMiner.SDM.AssetManagement.Models.Asset> middleware)
		{
			return new AssetDomRepository_Middleware(repository, middleware);
		}
	}
}