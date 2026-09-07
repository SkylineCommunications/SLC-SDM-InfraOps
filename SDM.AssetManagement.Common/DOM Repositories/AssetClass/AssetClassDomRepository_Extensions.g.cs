
namespace Skyline.DataMiner.SDM
{
	using Skyline.DataMiner.SDM.Middleware;
	using Skyline.DataMiner.SDM.AssetManagement.Models;
	using Skyline.DataMiner.SDM;

	public static class AssetClassDomRepository_Extensions
	{

		public static IAssetClassRepository WithMiddleware(
			this IAssetClassRepository repository,
			IMiddlewareMarker<AssetManagement.Models.AssetClass> middleware)
		{
			return new AssetClassDomRepository_Middleware(repository, middleware);
		}
	}
}