namespace Skyline.DataMiner.Utils.InfraOps.Common.Fields
{
	public interface IDomInstanceFieldApplyChanges : IChangeTrackingField
	{
		void ApplyChanges();
	}
}