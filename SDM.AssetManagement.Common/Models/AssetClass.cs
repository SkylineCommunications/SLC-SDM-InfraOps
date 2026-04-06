namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
	using System;
	using System.Collections.Generic;

	using Newtonsoft.Json;

	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.AssetManagement;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public class AssetClass : SdmObject<AssetClass>
	{
		[JsonIgnore]
		public Guid Id { get; set; }

		public IChangeTrackingField<string> DeviceName { get; set; }

		public IChangeTrackingField<SdmObjectReference<DeviceType>> DeviceTypeId { get; set; }

		public string DeviceDescription { get; set; }

		public Guid Manufacturer { get; set; }

		public IChangeTrackingField<double> Depth { get; set; }

		public IChangeTrackingField<double> Height { get; set; }

		public IChangeTrackingField<double> Width { get; set; }

		public IChangeTrackingField<double> HeightU { get; set; }

		public IChangeTrackingField<double> Weight { get; set; }

		public string FrontImage { get; set; }

		public string BackImage { get; set; }

		public double TypicalPowerConsumption { get; set; }

		public double MaximumPowerConsumption { get; set; }

		public IChangeTrackingField<SlcAssetManagement.Enums.PowerSupply> PowerSupply { get; set; }

		public AssetClassLifecycle Lifecycle { get; set; } = new AssetClassLifecycle();

		public List<DataPortInfo> DataPorts { get; set; } = new List<DataPortInfo>();

		public List<PowerPortInfo> PowerPorts { get; set; } = new List<PowerPortInfo>();

		public List<AssetHolder> Holders { get; set; } = new List<AssetHolder>();
	}
}