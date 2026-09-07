
namespace Skyline.DataMiner.SDM
{
	using Skyline.DataMiner.SDM.Middleware;
	using Skyline.DataMiner.SDM.AssetManagement.Models;
	using Skyline.DataMiner.SDM;

	public static class AssetClassDomRepository_Extensions
	{

		public static IBulkRepository<AssetManagement.Models.AssetClass> WithMiddleware(
			this IBulkRepository<AssetManagement.Models.AssetClass> repository,
			IMiddlewareMarker<AssetManagement.Models.AssetClass> middleware)
		{
			return new AssetClassDomRepository_Middleware(repository, middleware);
		}
	}
}