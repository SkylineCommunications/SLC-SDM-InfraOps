namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
	public sealed class ElementLink : SdmObject<ElementLink>
	{
		public string ElementID { get; set; }

		public bool IsPrimary { get; set; }
	}
}