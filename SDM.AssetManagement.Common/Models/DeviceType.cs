namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
	using System.Collections.Generic;
	using Skyline.DataMiner.SDM;

	//[GenerateExposers]
	//[SdmDomStorage("(slc)asset_management")]
	public class DeviceType : SdmObject<DeviceType>
	{
		public string Name { get; set; }

		public string Description { get; set; }

		public TagsInfo TagsInfo { get; set; } = new TagsInfo();

		public HierarchyInfo HierarchyInfo { get; set; } = new HierarchyInfo();
	}

	public class TagsInfo : SdmObject<TagsInfo>
	{
		public List<SlcAssetManagement.Enums.TagOption> Tags { get; set; }
	}

	public class HierarchyInfo : SdmObject<HierarchyInfo>
	{
		public SlcAssetManagement.Enums.HierarchyRole HierarchyRole { get; set; }
	}
}