namespace SDM.InfraOpsProperties.Tests.Setup
{
	using System.Collections.Generic;

	using SharedMappers.DomIds;

	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	public static class DemoData
	{
		public static readonly List<Property> Properties =
		[
			WithLayout(new Property
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Asset Owner",
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.String,
				Scope = "Asset",
				Default = "Unassigned",
				StringSizeLimit = 128,
				IsMultiLineString = false,
			}, "General", 1),
			WithLayout(new Property
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Maintenance Notes",
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.String,
				Scope = "Asset",
				Default = string.Empty,
				StringSizeLimit = 2000,
				IsMultiLineString = true,
			}, "Maintenance", 2),
			WithLayout(new Property
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Criticality",
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.Discrete,
				Scope = "Asset",
				Default = "Low",
				Discreets = new List<PropertyOption> { new PropertyOption { Option = "Low" }, new PropertyOption { Option = "Medium" }, new PropertyOption { Option = "High" } },
			}, "General", 3),
			WithLayout(new Property
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Is Bookable",
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.Boolean,
				Scope = "Facility",
				Default = "false",
			}, "Booking", 1),
			WithLayout(new Property
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Region",
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.Discrete,
				Scope = "Facility",
				Default = "EMEA",
				Discreets = new List<PropertyOption> { new PropertyOption { Option = "EMEA" }, new PropertyOption { Option = "APAC" }, new PropertyOption { Option = "AMER" } },
			}, "General", 2),
		];

		private static Property WithLayout(Property property, string sectionName, int order)
		{
			property.Layout.SectionName = sectionName;
			property.Layout.Order = order;
			return property;
		}

		public static readonly List<PropertyValues> PropertyValuesList =
		[
			new PropertyValues
			{
				Identifier = Guid.NewGuid().ToString(),
				LinkedObjectID = Guid.NewGuid(),
				Scope = "Asset",
				SubID = null,
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Asset Owner", Value = "John Doe", PropertyId = new Skyline.DataMiner.SDM.SdmObjectReference<Property>(Properties[0].Identifier) },
					new PropertyValue { PropertyName = "Criticality", Value = "High", PropertyId = new Skyline.DataMiner.SDM.SdmObjectReference<Property>(Properties[2].Identifier) },
				},
			},
			new PropertyValues
			{
				Identifier = Guid.NewGuid().ToString(),
				LinkedObjectID = Guid.NewGuid(),
				Scope = "Asset",
				SubID = null,
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Asset Owner", Value = "Jane Smith", PropertyId = new Skyline.DataMiner.SDM.SdmObjectReference<Property>(Properties[0].Identifier) },
					new PropertyValue { PropertyName = "Criticality", Value = "Low", PropertyId = new Skyline.DataMiner.SDM.SdmObjectReference<Property>(Properties[2].Identifier) },
				},
			},
			new PropertyValues
			{
				Identifier = Guid.NewGuid().ToString(),
				LinkedObjectID = Guid.NewGuid(),
				Scope = "Facility",
				SubID = "Rack-1",
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Is Bookable", Value = "true", PropertyId = new Skyline.DataMiner.SDM.SdmObjectReference<Property>(Properties[3].Identifier) },
					new PropertyValue { PropertyName = "Region", Value = "EMEA", PropertyId = new Skyline.DataMiner.SDM.SdmObjectReference<Property>(Properties[4].Identifier) },
				},
			},
			new PropertyValues
			{
				Identifier = Guid.NewGuid().ToString(),
				LinkedObjectID = Guid.NewGuid(),
				Scope = "Facility",
				SubID = "Rack-2",
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Region", Value = "APAC", PropertyId = new Skyline.DataMiner.SDM.SdmObjectReference<Property>(Properties[4].Identifier) },
				},
			},
		];
	}
}
