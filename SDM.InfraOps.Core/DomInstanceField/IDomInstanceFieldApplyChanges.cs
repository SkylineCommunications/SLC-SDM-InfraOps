namespace Skyline.DataMiner.Utils.InfraOps.Common.Fields
{
	internal interface IDomInstanceFieldApplyChanges : IChangeTrackingField
	{
		void ApplyChanges();
	}
}