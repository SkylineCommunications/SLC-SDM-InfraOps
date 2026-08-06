namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
	using System;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

	public sealed class ElementLink : ISectionTrackable
	{
		[JsonIgnore]
		[SdmIgnore]
		Guid? ISectionTrackable.SectionId { get; set; }

		public string ElementID { get; set; }

		public bool IsPrimary { get; set; }
	}
}