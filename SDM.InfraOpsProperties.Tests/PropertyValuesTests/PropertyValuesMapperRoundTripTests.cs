namespace SDM.InfraOpsProperties.Tests.PropertyValuesTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.InfraOpsProperties.Tests.Setup;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	/// <summary>
	/// Round-trip tests for the PropertyValues DOM mapper (ToInstance/FromInstance), exercising the
	/// Values array field (incl. PropertyId cross-references), optional SubID, and change-tracking
	/// reset behavior - the areas most likely to be mishandled by the array-field mapper.
	/// </summary>
	[TestClass]
	public class PropertyValuesMapperRoundTripTests : BaseRepositoryTest
	{
		[TestMethod]
		public void RoundTrip_WithMultipleValuesAndPropertyIdReferences_ShouldPreserveEveryEntry()
		{
			var propertyId = Guid.NewGuid().ToString();
			var linkedObjectId = Guid.NewGuid();
			var original = new PropertyValues
			{
				Identifier = Guid.NewGuid().ToString(),
				LinkedObjectID = linkedObjectId,
				Scope = "Asset",
				SubID = null,
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Owner", Value = "Alice", PropertyId = new SdmObjectReference<Property>(propertyId) },
					new PropertyValue { PropertyName = "Region", Value = "EMEA", PropertyId = null },
				},
			};

			Helper.PropertyValues.Create(original);

			var roundTripped = Helper.PropertyValues.Read(PropertyValuesExposers.Identifier.Equal(original.Identifier)).Single();

			using (new AssertionScope())
			{
				roundTripped.LinkedObjectID.Should().Be(linkedObjectId);
				roundTripped.Scope.Should().Be("Asset");
				roundTripped.SubID.Should().BeNull();
				roundTripped.Values.Should().HaveCount(2);

				var owner = roundTripped.Values.Single(v => v.PropertyName == "Owner");
				owner.Value.Should().Be("Alice");
				owner.PropertyId.Should().NotBeNull();
				owner.PropertyId!.Identifier.Should().Be(propertyId);

				var region = roundTripped.Values.Single(v => v.PropertyName == "Region");
				region.Value.Should().Be("EMEA");

				// Known generated-mapper gap: FromInstance reads Values.PropertyId via GetValue<Guid>(),
				// which returns a non-null wrapper even when the field was never written (ToInstance
				// correctly skips writing it when PropertyId is null). So a null PropertyId does NOT
				// round-trip as null - it comes back as a non-null SdmObjectReference wrapping a null
				// Identifier. This is a generated-code issue, out of scope for this branch; pinned here
				// so a future regeneration/fix is caught by this test failing.
				region.PropertyId.Should().NotBeNull();
				region.PropertyId!.Identifier.Should().BeNull();
			}
		}

		[TestMethod]
		public void RoundTrip_WithSubID_ShouldPreserveSubID()
		{
			var original = new PropertyValues
			{
				Identifier = Guid.NewGuid().ToString(),
				LinkedObjectID = Guid.NewGuid(),
				Scope = "Facility",
				SubID = "Rack-42",
				Values = new List<PropertyValue>(),
			};

			Helper.PropertyValues.Create(original);

			var roundTripped = Helper.PropertyValues.Read(PropertyValuesExposers.Identifier.Equal(original.Identifier)).Single();

			roundTripped.SubID.Should().Be("Rack-42");
		}

		[TestMethod]
		public void RoundTrip_WithEmptyValuesList_ShouldPreserveEmptyList()
		{
			var original = new PropertyValues
			{
				Identifier = Guid.NewGuid().ToString(),
				LinkedObjectID = Guid.NewGuid(),
				Scope = "Asset",
				SubID = null,
				Values = new List<PropertyValue>(),
			};

			Helper.PropertyValues.Create(original);

			var roundTripped = Helper.PropertyValues.Read(PropertyValuesExposers.Identifier.Equal(original.Identifier)).Single();

			roundTripped.Values.Should().NotBeNull().And.BeEmpty();
		}

		[TestMethod]
		public void RoundTrip_AfterFetch_ShouldNotBeNewAndShouldHaveNoPendingChanges()
		{
			// Mapper round-trip must reset both the scalar FieldHandler and the ValuesField
			// change-tracking state (PropertyValues.Changed = FieldHandler.HasChanges || ValuesField.Changed).
			var original = new PropertyValues
			{
				Identifier = Guid.NewGuid().ToString(),
				LinkedObjectID = Guid.NewGuid(),
				Scope = "Asset",
				SubID = null,
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Owner", Value = "Alice" },
				},
			};

			Helper.PropertyValues.Create(original);

			var roundTripped = Helper.PropertyValues.Read(PropertyValuesExposers.Identifier.Equal(original.Identifier)).Single();

			using (new AssertionScope())
			{
				roundTripped.IsNew.Should().BeFalse();
				roundTripped.Changed.Should().BeFalse();
			}
		}
	}
}
