namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
	using System;
	using System.Collections.Generic;
	using Newtonsoft.Json;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.AssetManagement;

	//[GenerateExposers]
	//[SdmDomStorage("(slc)asset_management")]
	public class AssetClass : SdmObject<AssetClass>
	{
		[JsonIgnore]
		public Guid Id { get; set; }

		public string DeviceName { get; set; }

		public SdmObjectReference<DeviceType> DeviceTypeId { get; set; }

		public string DeviceDescription { get; set; }

		public Guid Manufacturer { get; set; }

		public double Depth { get; set; }

		public double Height { get; set; }

		public double Width { get; set; }

		public double HeightU { get; set; }

		public double Weight { get; set; }

		public string FrontImage { get; set; }

		public string BackImage { get; set; }

		public double TypicalPowerConsumption { get; set; }

		public double MaximumPowerConsumption { get; set; }

		public SlcAssetManagement.Enums.PowerSupply PowerSupply { get; set; }

		public AssetClassLifecycle Lifecycle { get; set; } = new AssetClassLifecycle();

		public List<DataPortInfo> DataPorts { get; set; } = new List<DataPortInfo>();

		public List<PowerPortInfo> PowerPorts { get; set; } = new List<PowerPortInfo>();

		public List<AssetHolder> Holders { get; set; } = new List<AssetHolder>();
	}
}