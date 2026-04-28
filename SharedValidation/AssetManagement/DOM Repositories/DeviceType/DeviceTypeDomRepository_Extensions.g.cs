
namespace Skyline.DataMiner.SDM
{
	using Skyline.DataMiner.SDM.Middleware;
	using Skyline.DataMiner.SDM.AssetManagement.Models;
	using Skyline.DataMiner.SDM;

	public static class DeviceTypeDomRepository_Extensions
	{

		public static Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.AssetManagement.Models.DeviceType> WithMiddleware(
			this Skyline.DataMiner.SDM.IBulkRepository<Skyline.DataMiner.SDM.AssetManagement.Models.DeviceType> repository,
			IMiddlewareMarker<Skyline.DataMiner.SDM.AssetManagement.Models.DeviceType> middleware)
		{
			return new DeviceTypeDomRepository_Middleware(repository, middleware);
		}
	}
}