namespace SDM.InfraOpsProperties.Tests.Properties
{
	using System;
	using System.Collections.Generic;

	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SharedMappers.DomIds;

	using Skyline.DataMiner.SDM.InfraOps.Core.Converters;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	[TestClass]
	public class PropertyJsonRoundTripTests
	{
		[TestMethod]
		public void Property_RoundTrip_PreservesAllScalarsLayoutAndDiscreets()
		{
			var property = new Property
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Serial Number",
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.Discrete,
				Scope = "Asset",
				Default = "N/A",
				StringSizeLimit = 256,
				IsMultiLineString = true,
				Discreets = new List<PropertyOption>
				{
					new PropertyOption { Option = "First" },
					new PropertyOption { Option = "Second" },
				},
			};
			property.Layout.SectionName = "General";
			property.Layout.Order = 7;

			string json = InfraOpsJsonConvert.SerializeObject(property);
			var restored = InfraOpsJsonConvert.DeserializeObject<Property>(json);

			restored.Identifier.Should().Be(property.Identifier);
			restored.Name.Should().Be("Serial Number");
			restored.PropertyType.Should().Be(InfraopsProperties.Enums.PropertyTypeEnum.Discrete);
			restored.Scope.Should().Be("Asset");
			restored.Default.Should().Be("N/A");
			restored.StringSizeLimit.Should().Be(256);
			restored.IsMultiLineString.Should().BeTrue();
			restored.Layout.SectionName.Should().Be("General");
			restored.Layout.Order.Should().Be(7);
			restored.Discreets.Should().HaveCount(2);
			restored.Discreets[0].Option.Should().Be("First");
			restored.Discreets[1].Option.Should().Be("Second");
			restored.Should().Be(property);
		}

		[TestMethod]
		public void Property_RoundTrip_NullStringSizeLimitAndEmptyDiscreets_ArePreserved()
		{
			var property = new Property
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Flag",
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.Boolean,
				StringSizeLimit = null,
			};

			var restored = InfraOpsJsonConvert.DeserializeObject<Property>(InfraOpsJsonConvert.SerializeObject(property));

			restored.StringSizeLimit.Should().BeNull();
			restored.Discreets.Should().BeEmpty();
			restored.Layout.Order.Should().BeNull();
			restored.Should().Be(property);
		}

		[TestMethod]
		public void Property_Serialize_WritesEnumAsName()
		{
			var property = new Property
			{
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.Discrete,
			};

			string json = InfraOpsJsonConvert.SerializeObject(property);

			json.Should().Contain("\"PropertyType\":\"Discrete\"");
		}
	}
}
