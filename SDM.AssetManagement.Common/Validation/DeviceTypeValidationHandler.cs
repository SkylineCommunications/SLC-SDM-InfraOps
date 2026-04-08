namespace Skyline.DataMiner.SDM.AssetManagement.Common.Validation
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.All.Validations;
	using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Asset_Manager.Wrappers;
	using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.DomIds;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    public static class DeviceTypeValidationHandler
	{
		public enum DeviceTypeValidationField
		{
			Name,
			DeviceType,
		}

		public static ValidationResult IsRackAttacheable(DeviceType deviceType)
		{
			ValidationResult result = new ValidationResult();

			if (deviceType == null)
			{
				result.AddFailReason(DeviceTypeValidationField.DeviceType, "Device Type must be provided.");
				return result;
			}

			if (!deviceType.TagsInfo.Tags.Contains(SlcAsset_Management.Enums.TagOption.RackUnitConsumer))
			{
				result.AddFailReason(DeviceTypeValidationField.DeviceType, "Device Type lacks the 'Rack Unit Consumer' Tag.");
				return result;
			}

			return result;
		}
	}
}