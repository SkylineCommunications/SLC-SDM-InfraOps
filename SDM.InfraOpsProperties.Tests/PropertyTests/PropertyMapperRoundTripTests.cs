namespace SDM.InfraOpsProperties.Tests.Properties
{
	using System.Collections.Generic;
	using System.Linq;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.InfraOpsProperties.Tests.Setup;

	using SharedMappers.DomIds;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	/// <summary>
	/// Round-trip tests for the Property DOM mapper (ToInstance/FromInstance), exercising every
	/// mapped field - including edge values (null StringSizeLimit, empty Options, populated Options,
	/// multi-line strings) - to catch any field the generated mapper might silently drop or mistranslate.
	/// </summary>
	[TestClass]
	public class PropertyMapperRoundTripTests : BaseRepositoryTest
	{
		[TestMethod]
		public void RoundTrip_StringPropertyWithAllScalarFieldsPopulated_ShouldPreserveEveryField()
		{
			var original = new Property
			{
				Identifier = System.Guid.NewGuid().ToString(),
				Name = "Serial Number",
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.String,
				Scope = "Asset",
				Default = "N/A",
				StringSizeLimit = 256,
				IsMultiLineString = true,
				Layout = new PropertyLayout { SectionName = "General", Order = 7 },
			};

			Helper.Properties.Create(original);

			var roundTripped = Helper.Properties.Read(PropertyExposers.Identifier.Equal(original.Identifier)).Single();

			using (new AssertionScope())
			{
				roundTripped.Identifier.Should().Be(original.Identifier);
				roundTripped.Name.Should().Be(original.Name);
				roundTripped.PropertyType.Should().Be(original.PropertyType);
				roundTripped.Scope.Should().Be(original.Scope);
				roundTripped.Default.Should().Be(original.Default);
				roundTripped.StringSizeLimit.Should().Be(original.StringSizeLimit);
				roundTripped.IsMultiLineString.Should().Be(original.IsMultiLineString);
				roundTripped.Layout.SectionName.Should().Be(original.Layout.SectionName);
				roundTripped.Layout.Order.Should().Be(original.Layout.Order);
				roundTripped.Discreets.Should().BeEmpty();
			}
		}

		[TestMethod]
		public void RoundTrip_PropertyWithNullStringSizeLimit_ShouldPreserveNull()
		{
			var original = new Property
			{
				Identifier = System.Guid.NewGuid().ToString(),
				Name = "Notes",
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.String,
				Scope = "Asset",
				Default = string.Empty,
				StringSizeLimit = null,
				IsMultiLineString = true,
				Layout = new PropertyLayout { SectionName = "General", Order = 1 },
			};

			Helper.Properties.Create(original);

			var roundTripped = Helper.Properties.Read(PropertyExposers.Identifier.Equal(original.Identifier)).Single();

			roundTripped.StringSizeLimit.Should().BeNull();
		}

		[TestMethod]
		public void RoundTrip_DiscretePropertyWithOptions_ShouldPreserveOptionsOrderAndContent()
		{
			var options = new List<string> { "Low", "Medium", "High" };
			var original = new Property
			{
				Identifier = System.Guid.NewGuid().ToString(),
				Name = "Criticality",
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.Discrete,
				Scope = "Asset",
				Default = "Low",
				Discreets = options.Select(o => new PropertyOption { Option = o }).ToList(),
				Layout = new PropertyLayout { SectionName = "General", Order = 3 },
			};

			Helper.Properties.Create(original);

			var roundTripped = Helper.Properties.Read(PropertyExposers.Identifier.Equal(original.Identifier)).Single();

			roundTripped.Discreets.Select(o => o.Option).Should().Equal(options);
		}

		[TestMethod]
		public void RoundTrip_AfterFetch_ShouldNotBeNewAndShouldHaveNoPendingChanges()
		{
			// Mapper round-trip must also reset change-tracking state correctly (see IsNew/HasChanges pitfalls).
			var original = new Property
			{
				Identifier = System.Guid.NewGuid().ToString(),
				Name = "Region",
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.String,
				Scope = "Facility",
				Default = string.Empty,
				Layout = new PropertyLayout { SectionName = "General", Order = 1 },
			};

			Helper.Properties.Create(original);

			var roundTripped = Helper.Properties.Read(PropertyExposers.Identifier.Equal(original.Identifier)).Single();

			using (new AssertionScope())
			{
				roundTripped.IsNew.Should().BeFalse();
				roundTripped.Changed.Should().BeFalse();
			}
		}
	}
}
