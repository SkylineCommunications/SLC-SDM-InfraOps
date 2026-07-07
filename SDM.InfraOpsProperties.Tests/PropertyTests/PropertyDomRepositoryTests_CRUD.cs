namespace SDM.InfraOpsProperties.Tests.Properties
{
	using System;
	using System.Linq;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using SDM.InfraOpsProperties.Tests.Setup;

	using SharedMappers.DomIds;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	[TestClass]
	public partial class PropertyDomRepositoryTests : BaseRepositoryTest
	{
		private Property referenceProperty = null!;

		[TestInitialize]
		public void TestInitialize()
		{
			var id = Guid.NewGuid();
			referenceProperty = new Property
			{
				Identifier = id.ToString(),
				Name = "Serial Number",
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.String,
				Scope = "Asset",
				Default = string.Empty,
				StringSizeLimit = 64,
				IsMultiLineString = false,
				Layout = new PropertyLayout { SectionName = "General", Order = 1 },
			};
		}

		[TestMethod]
		public void PropertyDomRepository_EmptyDOM_Create()
		{
			Helper.Properties.Create(referenceProperty);

			AssertCreated();
		}

		[TestMethod]
		public void PropertyDomRepository_EmptyDOM_CreateOrUpdate_Create()
		{
			Helper.Properties.CreateOrUpdate([referenceProperty]);

			AssertCreated();
		}

		[TestMethod]
		public void PropertyDomRepository_EmptyDOM_CreateOrUpdate_Update()
		{
			Helper.Properties.Create(referenceProperty);

			var updatedProperty = new Property
			{
				Identifier = referenceProperty.Identifier,
				Name = "Updated Serial Number",
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.Discrete,
				Scope = "Facility",
				Default = "N/A",
				Discreets = new System.Collections.Generic.List<PropertyOption> { new PropertyOption { Option = "N/A" }, new PropertyOption { Option = "Assigned" } },
				Layout = new PropertyLayout { SectionName = "Updated Section", Order = 2 },
			};

			Helper.Properties.CreateOrUpdate([updatedProperty]);
			AssertPropertyUpdateDifferences(updatedProperty);
		}

		[TestMethod]
		public void PropertyDomRepository_ReadPaged()
		{
			const int pageCount = 2;
			Helper.PopulateProperties();

			FilterElement<Property> allFilter = new TRUEFilterElement<Property>();
			var pagedResult = Helper.Properties.ReadPaged(allFilter, pageCount);
			var propertyCount = Helper.Properties.Count(allFilter);

			using (new AssertionScope())
			{
				pagedResult.Should().NotBeNull();
				pagedResult.Should().HaveCount((int)Math.Ceiling(propertyCount / (double)pageCount));
			}
		}

		[TestMethod]
		public void PropertyDomRepository_DeleteBulk()
		{
			Helper.PopulateProperties();

			var filter = new ORFilterElement<Property>(
				PropertyExposers.Scope.Equal("Facility"));
			var propertiesToDelete = Helper.Properties.Read(filter);

			Helper.Properties.Delete(propertiesToDelete);

			using (new AssertionScope())
			{
				Helper.Properties.Count(new TRUEFilterElement<Property>()).Should().BeLessThan(DemoData.Properties.Count);
				Helper.Properties.Count(PropertyExposers.Scope.Equal("Facility")).Should().Be(0);
			}
		}

		[TestMethod]
		public void PropertyDomRepository_EmptyDOM_DeleteSingle()
		{
			Helper.PopulateProperties();

			var propertyToDelete = Helper.Properties.Read(PropertyExposers.Name.Equal(DemoData.Properties[0].Name)).First();

			Helper.Properties.Delete(propertyToDelete);

			Helper.Properties.Count(new TRUEFilterElement<Property>()).Should().Be(DemoData.Properties.Count - 1);
			Helper.Properties.Count(PropertyExposers.Identifier.Equal(propertyToDelete.Identifier)).Should().Be(0);
		}

		private static void AssertPropertyUpdateDifferences(Property updated)
		{
			using (new AssertionScope())
			{
				updated.Name.Should().Be("Updated Serial Number");
				updated.PropertyType.Should().Be(InfraopsProperties.Enums.PropertyTypeEnum.Discrete);
				updated.Scope.Should().Be("Facility");
				updated.Default.Should().Be("N/A");
				updated.Discreets.Select(o => o.Option).Should().BeEquivalentTo(new[] { "N/A", "Assigned" });
				updated.Layout.SectionName.Should().Be("Updated Section");
				updated.Layout.Order.Should().Be(2);
			}
		}

		private void AssertCreated()
		{
			using (new AssertionScope())
			{
				Helper.Properties.Count(new TRUEFilterElement<Property>()).Should().Be(1);

				var createdProperty = Helper.Properties.Read(new TRUEFilterElement<Property>()).First();
				createdProperty.Should().NotBeNull();
				createdProperty.Name.Should().Be("Serial Number");
				createdProperty.PropertyType.Should().Be(InfraopsProperties.Enums.PropertyTypeEnum.String);
				createdProperty.Scope.Should().Be("Asset");
				createdProperty.StringSizeLimit.Should().Be(64);
				createdProperty.IsMultiLineString.Should().BeFalse();
				createdProperty.Layout.SectionName.Should().Be("General");
				createdProperty.Layout.Order.Should().Be(1);
			}
		}
	}
}
