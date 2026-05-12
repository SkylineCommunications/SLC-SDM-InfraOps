
namespace Skyline.DataMiner.SDM
{
	using Skyline.DataMiner.SDM.Middleware;
	using Skyline.DataMiner.SDM.AssetManagement.Models;
	using Skyline.DataMiner.SDM;

	public static class AssetClassDomRepository_Extensions
	{

		public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.AssetManagement.Models.AssetClass> WithMiddleware(
			this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.AssetManagement.Models.AssetClass> repository,
			IMiddlewareMarker<Skyline.DataMiner.SDM.AssetManagement.Models.AssetClass> middleware)
		{
			return new AssetClassDomRepository_Middleware(repository, middleware);
		}
	}
}