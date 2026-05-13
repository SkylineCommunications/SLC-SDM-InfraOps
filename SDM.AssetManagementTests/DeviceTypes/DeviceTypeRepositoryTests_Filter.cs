namespace SDM.AssetManagement.Tests
{
	using System.Linq;
	using FluentAssertions;
	using FluentAssertions.Execution;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.AssetManagement.Models;

	public partial class DeviceTypeRepositoryTests
	{
		[TestMethod]
		public void DeviceTypeRepository_ReadFilter_Name_Equal()
		{
			// Arrange
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateDeviceTypes();

			var refDeviceType = DemoData.DeviceTypes[3];
			var filter = DeviceTypeExposers.Name.Equal(refDeviceType.Name);

			// Act
			var deviceTypesRetrieved = helper.AssetManagement.DeviceTypes.Read(filter);

			// Assert
			using (new AssertionScope())
			{
				deviceTypesRetrieved.Should().NotBeNull();
				deviceTypesRetrieved.Count().Should().Be(1);
				var deviceType = deviceTypesRetrieved.First();

				deviceType.Name.Should().Be(refDeviceType.Name);
				deviceType.Identifier.Should().Be(refDeviceType.Identifier);
				deviceType.Description.Should().Be(refDeviceType.Description);
				deviceType.HierarchyInfo.HierarchyRole.Should().Be(refDeviceType.HierarchyInfo.HierarchyRole);
				deviceType.TagsInfo.Tags.Should().BeEmpty();
			}
		}

		[TestMethod]
		public void DeviceTypeRepository_ReadFilter_Name_Contains()
		{
			// Arrange
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateDeviceTypes();

			var filter = DeviceTypeExposers.Name.Contains("coder");

			// Act
			var deviceTypesRetrieved = helper.AssetManagement.DeviceTypes.Read(filter);
			var expected = DemoData.DeviceTypes.Where(filter.getLambda());

			// Assert
			using (new AssertionScope())
			{
				deviceTypesRetrieved.Should().NotBeNull();
				deviceTypesRetrieved.Should().BeEquivalentTo(expected);
				deviceTypesRetrieved.Should().AllSatisfy(dt => dt.Name.Should().Contain("coder")); // encoder and decoder
			}
		}

		[TestMethod]
		public void DeviceTypeRepository_ReadFilter_Description_Contains()
		{
			// Arrange
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateDeviceTypes();

			var filter = DeviceTypeExposers.Description.Contains("UPS");

			// Act
			var deviceTypesRetrieved = helper.AssetManagement.DeviceTypes.Read(filter);
			var expected = DemoData.DeviceTypes.Where(filter.getLambda());

			// Assert
			using (new AssertionScope())
			{
				deviceTypesRetrieved.Should().NotBeNull();
				deviceTypesRetrieved.Should().BeEquivalentTo(expected);
				deviceTypesRetrieved.Should().AllSatisfy(dt => dt.Description.Should().Contain("UPS"));
			}
		}

		[TestMethod]
		public void DeviceTypeRepository_NestedReadFilter_HierarchyRole_Equal()
		{
			// Arrange
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateDeviceTypes();

			var hierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis;
			var filter = DeviceTypeExposers.HierarchyInfo.HierarchyRole.Equal(hierarchyRole);

			// Act
			var deviceTypesRetrieved = helper.AssetManagement.DeviceTypes.Read(filter);
			var expected = DemoData.DeviceTypes.Where(filter.getLambda());

			// Assert
			using (new AssertionScope())
			{
				deviceTypesRetrieved.Should().NotBeNull();
				deviceTypesRetrieved.Should().BeEquivalentTo(expected);
				deviceTypesRetrieved.Should().AllSatisfy(dt => dt.HierarchyInfo.HierarchyRole.Should().Be(hierarchyRole));
			}
		}

		[TestMethod]
		public void DeviceTypeRepository_NestedReadFilter_Tags_NotContains()
		{
			// Arrange
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateDeviceTypes();

			var tag = SlcAsset_Management.Enums.TagOption.AcceptsDataConnection;
			var filter = DeviceTypeExposers.TagsInfo.Tags.NotContains(tag);

			// Act
			var deviceTypesRetrieved = helper.AssetManagement.DeviceTypes.Read(filter);
			var expected = DemoData.DeviceTypes.Where(filter.getLambda());

			// Assert
			using (new AssertionScope())
			{
				deviceTypesRetrieved.Should().NotBeNull();
				deviceTypesRetrieved.Should().BeEquivalentTo(expected);
				deviceTypesRetrieved.Should().AllSatisfy(dt => dt.TagsInfo.Tags.Should().NotContain(tag));
			}
		}
	}
}
