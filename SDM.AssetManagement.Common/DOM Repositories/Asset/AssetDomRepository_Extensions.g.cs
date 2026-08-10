
namespace Skyline.DataMiner.SDM
{
	using Skyline.DataMiner.SDM.Middleware;
	using Skyline.DataMiner.SDM.AssetManagement.Models;
	using Skyline.DataMiner.SDM;

	public static class AssetDomRepository_Extensions
	{

		public static IBulkRepository<AssetManagement.Models.Asset> WithMiddleware(
			this IBulkRepository<AssetManagement.Models.Asset> repository,
			IMiddlewareMarker<AssetManagement.Models.Asset> middleware)
		{
			return new AssetDomRepository_Middleware(repository, middleware);
		}
	}
}