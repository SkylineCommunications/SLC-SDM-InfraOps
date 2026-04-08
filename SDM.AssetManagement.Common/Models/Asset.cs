namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System.Collections.Generic;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;

    // [GenerateExposers]
    // [SdmDomStorage("(slc)asset_management")]
    public class Asset : SdmObject<Asset>
	{
		public string AssetId { get; set; }

		public string AssetName { get; set; }

		public SdmObjectReference<AssetClass> AssetClass { get; set; }

		public string AssetDescription { get; set; }

		public string FwOs { get; set; }

		public string SerialNumber { get; set; }

		public string HardwareVersion { get; set; }

		/// <summary>
		/// Gets or sets the network details of the asset.
		/// </summary>
		public AssetNetworkDetails NetworkDetails { get; set; } = new AssetNetworkDetails();

		/// <summary>
		/// Gets or sets the location details of the asset.
		/// </summary>
		public AssetLocation Location { get; set; } = new AssetLocation();

		/// <summary>
		/// Gets or sets the lifecycle information of the asset.
		/// </summary>
		public AssetLifecycle Lifecycle { get; set; } = new AssetLifecycle();

		/// <summary>
		/// Gets or sets the ownership information of the asset.
		/// </summary>
		public AssetOwnership Ownership { get; set; } = new AssetOwnership();

		/// <summary>
		/// Gets or sets the custody information of the asset.
		/// </summary>
		public AssetCustody Custody { get; set; } = new AssetCustody();

		/// <summary>
		/// Gets or sets the list of holders (slots) associated with the asset.
		/// </summary>
		public List<AssetHolder> Holders { get; set; } = new List<AssetHolder>();

		/// <summary>
		/// Gets or sets the list of DataMiner element links.
		/// </summary>
		public List<ElementLink> ElementLinks { get; set; } = new List<ElementLink>();

        public SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum Status { get; set; }
    }
}