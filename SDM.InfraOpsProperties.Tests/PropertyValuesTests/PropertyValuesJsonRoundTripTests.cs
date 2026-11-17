namespace SDM.InfraOpsProperties.Tests.PropertyValuesTests
{
	using System;
	using System.Collections.Generic;

	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.InfraOps.Core.Converters;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	[TestClass]
	public class PropertyValuesJsonRoundTripTests
	{
		[TestMethod]
		public void PropertyValues_RoundTrip_PreservesScalarsAndValuesWithReferences()
		{
			var propertyId = Guid.NewGuid().ToString();
			var linkedObjectId = Guid.NewGuid();
			var propertyValues = new PropertyValues
			{
				Identifier = Guid.NewGuid().ToString(),
				LinkedObjectID = linkedObjectId,
				Scope = "Asset",
				SubID = "Rack-42",
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Owner", Value = "Alice", PropertyId = new SdmObjectReference<Property>(propertyId) },
					new PropertyValue { PropertyName = "Region", Value = "EMEA", PropertyId = new SdmObjectReference<Property>(Guid.NewGuid().ToString()) },
				},
			};

			string json = InfraOpsJsonConvert.SerializeObject(propertyValues);
			var restored = InfraOpsJsonConvert.DeserializeObject<PropertyValues>(json);

			restored.Identifier.Should().Be(propertyValues.Identifier);
			restored.LinkedObjectID.Should().Be(linkedObjectId);
			restored.Scope.Should().Be("Asset");
			restored.SubID.Should().Be("Rack-42");
			restored.Values.Should().HaveCount(2);
			restored.Values[0].PropertyName.Should().Be("Owner");
			restored.Values[0].Value.Should().Be("Alice");
			restored.Values[0].PropertyId.Should().Be(propertyValues.Values[0].PropertyId);
			restored.Values[1].PropertyName.Should().Be("Region");
			restored.Values[1].Value.Should().Be("EMEA");
			restored.Values[1].PropertyId.Should().Be(propertyValues.Values[1].PropertyId);
			restored.Should().Be(propertyValues);
			InfraOpsJsonConvert.SerializeObject(restored).Should().Be(json);
		}

		[TestMethod]
		public void PropertyValues_AuditFields_ExportedButNotImported()
		{
			var createdAt = new DateTime(2021, 3, 4, 5, 6, 7, DateTimeKind.Utc);
			var modifiedAt = new DateTime(2022, 8, 9, 10, 11, 12, DateTimeKind.Utc);
			var propertyValues = new PropertyValues
			{
				Identifier = Guid.NewGuid().ToString(),
				LinkedObjectID = Guid.NewGuid(),
				CreatedAt = createdAt,
				CreatedBy = "creator",
				LastModified = modifiedAt,
				LastModifiedBy = "modifier",
			};

			string json = InfraOpsJsonConvert.SerializeObject(propertyValues);
			var restored = InfraOpsJsonConvert.DeserializeObject<PropertyValues>(json);

			json.Should().Contain("\"CreatedBy\":\"creator\"");
			json.Should().Contain("\"LastModifiedBy\":\"modifier\"");
			restored.Identifier.Should().Be(propertyValues.Identifier);
			restored.CreatedAt.Should().NotBe(createdAt);
			restored.CreatedBy.Should().NotBe("creator");
			restored.LastModified.Should().NotBe(modifiedAt);
			restored.LastModifiedBy.Should().NotBe("modifier");
		}

		[TestMethod]
		public void PropertyValues_Serialize_WritesPropertyIdAsPlainString()
		{
			var propertyId = Guid.NewGuid().ToString();
			var propertyValues = new PropertyValues
			{
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Owner", PropertyId = new SdmObjectReference<Property>(propertyId) },
				},
			};

			string json = InfraOpsJsonConvert.SerializeObject(propertyValues);

			json.Should().Contain($"\"PropertyId\":\"{propertyId}\"");
		}

		[TestMethod]
		public void PropertyValues_Serialize_EmptyPropertyIdIsNull()
		{
			var propertyValues = new PropertyValues
			{
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Region", Value = "EMEA" },
				},
			};

			string json = InfraOpsJsonConvert.SerializeObject(propertyValues);

			json.Should().Contain("\"PropertyId\":null");
		}

		[TestMethod]
		public void PropertyValues_Deserialize_NullAndEmptyPropertyIdBecomeDefault()
		{
			var restored = InfraOpsJsonConvert.DeserializeObject<PropertyValues>(
				"{\"Values\":[{\"PropertyName\":\"A\",\"PropertyId\":null},{\"PropertyName\":\"B\",\"PropertyId\":\"\"}]}");

			restored.Values.Should().HaveCount(2);
			restored.Values[0].PropertyId.Identifier.Should().BeNullOrEmpty();
			restored.Values[1].PropertyId.Identifier.Should().BeNullOrEmpty();
		}

		[TestMethod]
		public void PropertyValues_RoundTrip_EmptyValuesListAndNullSubId_ArePreserved()
		{
			var propertyValues = new PropertyValues
			{
				Identifier = Guid.NewGuid().ToString(),
				LinkedObjectID = Guid.NewGuid(),
				Scope = "Facility",
				SubID = null,
			};

			var restored = InfraOpsJsonConvert.DeserializeObject<PropertyValues>(InfraOpsJsonConvert.SerializeObject(propertyValues));

			restored.SubID.Should().BeNull();
			restored.Values.Should().BeEmpty();
			restored.Should().Be(propertyValues);
		}
	}
}
