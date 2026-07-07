namespace SDM.InfraOpsProperties.Tests.PropertyValuesTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using SDM.InfraOpsProperties.Tests.Setup;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	[TestClass]
	public partial class PropertyValuesDomRepositoryTests : BaseRepositoryTest
	{
		private Skyline.DataMiner.SDM.InfraOpsProperties.Models.PropertyValues referencePropertyValues = null!;
		private Guid _linkedObjectId;

		[TestInitialize]
		public void TestInitialize()
		{
			var id = Guid.NewGuid();
			_linkedObjectId = Guid.NewGuid();
			referencePropertyValues = new Skyline.DataMiner.SDM.InfraOpsProperties.Models.PropertyValues
			{
				Identifier = id.ToString(),
				LinkedObjectID = _linkedObjectId,
				Scope = "Asset",
				SubID = null,
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Owner", Value = "Alice" },
				},
			};
		}

		[TestMethod]
		public void PropertyValuesDomRepository_EmptyDOM_Create()
		{
			Helper.PropertyValues.Create(referencePropertyValues);

			AssertCreated();
		}

		[TestMethod]
		public void PropertyValuesDomRepository_EmptyDOM_CreateOrUpdate_Create()
		{
			Helper.PropertyValues.CreateOrUpdate([referencePropertyValues]);

			AssertCreated();
		}

		[TestMethod]
		public void PropertyValuesDomRepository_EmptyDOM_CreateOrUpdate_Update()
		{
			Helper.PropertyValues.Create(referencePropertyValues);

			var updated = new Skyline.DataMiner.SDM.InfraOpsProperties.Models.PropertyValues
			{
				Identifier = referencePropertyValues.Identifier,
				LinkedObjectID = _linkedObjectId,
				Scope = "Facility",
				SubID = "Rack-9",
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Owner", Value = "Bob" },
					new PropertyValue { PropertyName = "Region", Value = "APAC" },
				},
			};

			Helper.PropertyValues.CreateOrUpdate([updated]);

			using (new AssertionScope())
			{
				updated.Scope.Should().Be("Facility");
				updated.SubID.Should().Be("Rack-9");
				updated.Values.Should().HaveCount(2);
			}
		}

		[TestMethod]
		public void PropertyValuesDomRepository_ReadPaged()
		{
			const int pageCount = 2;
			Helper.PopulatePropertyValues();

			FilterElement<Skyline.DataMiner.SDM.InfraOpsProperties.Models.PropertyValues> allFilter = new TRUEFilterElement<Skyline.DataMiner.SDM.InfraOpsProperties.Models.PropertyValues>();
			var pagedResult = Helper.PropertyValues.ReadPaged(allFilter, pageCount);
			var count = Helper.PropertyValues.Count(allFilter);

			using (new AssertionScope())
			{
				pagedResult.Should().NotBeNull();
				pagedResult.Should().HaveCount((int)Math.Ceiling(count / (double)pageCount));
			}
		}

		[TestMethod]
		public void PropertyValuesDomRepository_DeleteBulk()
		{
			Helper.PopulatePropertyValues();

			var filter = PropertyValuesExposers.Scope.Equal("Facility");
			var toDelete = Helper.PropertyValues.Read(filter);

			Helper.PropertyValues.Delete(toDelete);

			using (new AssertionScope())
			{
				Helper.PropertyValues.Count(new TRUEFilterElement<Skyline.DataMiner.SDM.InfraOpsProperties.Models.PropertyValues>()).Should().BeLessThan(DemoData.PropertyValuesList.Count);
				Helper.PropertyValues.Count(PropertyValuesExposers.Scope.Equal("Facility")).Should().Be(0);
			}
		}

		[TestMethod]
		public void PropertyValuesDomRepository_EmptyDOM_DeleteSingle()
		{
			Helper.PopulatePropertyValues();

			var toDelete = Helper.PropertyValues.Read(PropertyValuesExposers.Identifier.Equal(DemoData.PropertyValuesList[0].Identifier)).First();

			Helper.PropertyValues.Delete(toDelete);

			Helper.PropertyValues.Count(new TRUEFilterElement<Skyline.DataMiner.SDM.InfraOpsProperties.Models.PropertyValues>()).Should().Be(DemoData.PropertyValuesList.Count - 1);
			Helper.PropertyValues.Count(PropertyValuesExposers.Identifier.Equal(toDelete.Identifier)).Should().Be(0);
		}

		private void AssertCreated()
		{
			using (new AssertionScope())
			{
				Helper.PropertyValues.Count(new TRUEFilterElement<Skyline.DataMiner.SDM.InfraOpsProperties.Models.PropertyValues>()).Should().Be(1);

				var created = Helper.PropertyValues.Read(new TRUEFilterElement<Skyline.DataMiner.SDM.InfraOpsProperties.Models.PropertyValues>()).First();
				created.Should().NotBeNull();
				created.LinkedObjectID.Should().Be(_linkedObjectId);
				created.Scope.Should().Be("Asset");
				created.Values.Should().HaveCount(1);
				created.Values.First().PropertyName.Should().Be("Owner");
				created.Values.First().Value.Should().Be("Alice");
			}
		}
	}
}
