
namespace Skyline.DataMiner.SDM
{
	using Skyline.DataMiner.SDM.Middleware;
	using Skyline.DataMiner.SDM.AssetManagement.Models;
	using Skyline.DataMiner.SDM;

	public static class DeviceTypeDomRepository_Extensions
	{

		public static IBulkRepository<AssetManagement.Models.DeviceType> WithMiddleware(
			this IBulkRepository<AssetManagement.Models.DeviceType> repository,
			IMiddlewareMarker<AssetManagement.Models.DeviceType> middleware)
		{
			return new DeviceTypeDomRepository_Middleware(repository, middleware);
		}
	}
}