namespace SDM.AssetManagement.Tests
{
	using System;
	using System.Linq;
	using FluentAssertions;
	using FluentAssertions.Execution;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Sections;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.AssetManagement;
	using Skyline.DataMiner.SDM.AssetManagement.Helpers;
	using Skyline.DataMiner.SDM.AssetManagement.Models;

	[TestClass]
	public partial class DeviceTypeRepositoryTests
	{
		private DeviceType referenceDeviceType;

		[TestInitialize]
		public void Init()
		{
			referenceDeviceType = new DeviceType
			{
				Identifier = Guid.NewGuid().ToString(),
				Name = "Test DeviceType",
				Description = "Test Description",
				TagsInfo = new TagsInfo
				{
					Identifier = Guid.NewGuid().ToString(),
					Tags = [SlcAsset_Management.Enums.TagOption.PowerProvider, SlcAsset_Management.Enums.TagOption.RackUnitConsumer],
				},
				HierarchyInfo = new HierarchyInfo
				{
					HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.SubCard,
				},
			};
		}

		[TestMethod]
		public void DeviceTypeRepository_EmptyDOM_Create()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();

			helper.DeviceTypes.Create(referenceDeviceType);

			AssertCreated(helper);
		}

		[TestMethod]
		public void DeviceTypeRepository_EmptyDOM_CreateOrUpdate_Create()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.DeviceTypes.CreateOrUpdate([referenceDeviceType]);

			AssertCreated(helper);
		}

		[TestMethod]
		public void DeviceTypeRepository_EmptyDOM_CreateOrUpdate_Update()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.DeviceTypes.Create(referenceDeviceType);

			var updatedDeviceType = new DeviceType
			{
				Identifier = referenceDeviceType.Identifier,
				Name = "Updated DeviceType Name",
				Description = "Updated Description",
				TagsInfo = new TagsInfo
				{
					Identifier = referenceDeviceType.TagsInfo.Identifier,
					Tags = new List<SlcAsset_Management.Enums.TagOption> { SlcAsset_Management.Enums.TagOption.RackUnitConsumer },
				},
				HierarchyInfo = new HierarchyInfo
				{
					HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis,
				},
			};

			helper.DeviceTypes.CreateOrUpdate([updatedDeviceType]);

			AssertDeviceTypeUpdateDifferences(referenceDeviceType, updatedDeviceType);
		}

		[TestMethod]
		public void DeviceTypeRepository_ReadPaged()
		{
			const int pageCount = 3;
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateDeviceTypes();

			FilterElement<DeviceType> allFilter = new TRUEFilterElement<DeviceType>();
			var pagedResult = helper.DeviceTypes.ReadPaged(allFilter, pageCount);
			var deviceTypeCount = helper.DeviceTypes.Count(allFilter);

			using (new AssertionScope())
			{
				pagedResult.Should().NotBeNull();
				pagedResult.Should().HaveCountGreaterThanOrEqualTo((int)(deviceTypeCount / pageCount));
				pagedResult.Should().AllSatisfy(page => page.Should().HaveCountLessThanOrEqualTo(pageCount));
			}
		}

		[TestMethod]
		public void DeviceTypeRepository_DeleteBulk()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateDeviceTypes();

			var filter = new ORFilterElement<DeviceType>(
				DeviceTypeExposers.Name.Equal("Decoder"),
				DeviceTypeExposers.TagsInfo.Tags.Contains(SlcAsset_Management.Enums.TagOption.PowerProvider));

			var deviceTypesToDelete = helper.DeviceTypes.Read(filter);

			helper.DeviceTypes.Delete(deviceTypesToDelete);

			using (new AssertionScope())
			{
				helper.DeviceTypes.Count(new TRUEFilterElement<DeviceType>()).Should().Be(DemoData.DeviceTypes.Count - deviceTypesToDelete.Count());
				helper.DeviceTypes.Count(DeviceTypeExposers.Name.Equal("Decoder")).Should().Be(0);
			}
		}

		[TestMethod]
		public void DeviceTypeRepository_EmptyDOM_DeleteSingle()
		{
			var helper = RepositoryInitialize.InitializeEmptyRepositories();
			helper.PopulateDeviceTypes();

			var deviceTypeToDelete = helper.DeviceTypes.Read(DeviceTypeExposers.Name.Equal("Optics Module")).First();

			helper.DeviceTypes.Delete(deviceTypeToDelete);

			helper.DeviceTypes.Count(new TRUEFilterElement<DeviceType>()).Should().Be(DemoData.DeviceTypes.Count - 1);
			helper.DeviceTypes.Count(DeviceTypeExposers.Identifier.Equal(deviceTypeToDelete.Identifier)).Should().Be(0);
		}

		private static void AssertDeviceTypeUpdateDifferences(DeviceType original, DeviceType updated)
		{
			using (new AssertionScope())
			{
				updated.Identifier.Should().Be(original.Identifier);

				// Name
				updated.Name.Should().NotBe(original.Name);
				updated.Name.Should().Be("Updated DeviceType Name");

				// Description
				updated.Description.Should().NotBe(original.Description);
				updated.Description.Should().Be("Updated Description");

				// TagsInfo.Tag
				updated.TagsInfo.Tags.Should().NotBeEquivalentTo(original.TagsInfo.Tags);
				updated.TagsInfo.Tags.Should().BeEquivalentTo([SlcAsset_Management.Enums.TagOption.RackUnitConsumer]);

				// HierarchyInfo.HierarchyRole
				updated.HierarchyInfo.HierarchyRole.Should().NotBe(original.HierarchyInfo.HierarchyRole);
				updated.HierarchyInfo.HierarchyRole.Should().Be(SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis);
			}
		}

		private void AssertCreated(IAssetManagementApiHelper helper)
		{
			using (new AssertionScope())
			{
				helper.DeviceTypes.Count(new TRUEFilterElement<DeviceType>()).Should().Be(1);

				var createdDeviceType = helper.DeviceTypes.Read(new TRUEFilterElement<DeviceType>()).First();
				createdDeviceType.Should().NotBeNull();

				createdDeviceType.Name.Should().Be(referenceDeviceType.Name);
				createdDeviceType.Description.Should().Be(referenceDeviceType.Description);

				createdDeviceType.TagsInfo.Should().NotBeNull();
				createdDeviceType.TagsInfo.Equals(referenceDeviceType.TagsInfo).Should().BeTrue();

				createdDeviceType.HierarchyInfo.Should().NotBeNull();
				createdDeviceType.HierarchyInfo.Equals(referenceDeviceType.HierarchyInfo).Should().BeTrue();
			}
		}
	}
}
