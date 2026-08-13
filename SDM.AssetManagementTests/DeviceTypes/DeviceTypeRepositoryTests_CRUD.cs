namespace SDM.AssetManagement.Tests.DeviceTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// CRUD tests for DeviceType repository operations.
    /// </summary>
    [TestClass]
    public class DeviceTypeRepositoryTests_CRUD : BaseRepositoryTest
    {
        private DeviceType referenceDeviceType = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            referenceDeviceType = new DeviceType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Test DeviceType",
                Description = "Test Description",
                TagsInfo =
                {
                    Tags = new List<SlcAsset_Management.Enums.TagOption> 
                    { 
                        SlcAsset_Management.Enums.TagOption.PowerProvider, 
                        SlcAsset_Management.Enums.TagOption.RackUnitConsumer 
                    },
                },
                HierarchyInfo =
                {
                    HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.SubCard,
                },
            };
        }

        #region Create Tests

        [TestMethod]
        public void Create_WithValidData_ShouldPersistDeviceType()
        {

            // Act
            Helper.AssetManagement.DeviceTypes.Create(referenceDeviceType);

            // Assert
            AssertCreated();
        }

        [TestMethod]
        public void CreateOrUpdate_WithNewDeviceType_ShouldCreate()
        {
            // Arrange
            

            // Act
            Helper.AssetManagement.DeviceTypes.CreateOrUpdate([referenceDeviceType]);

            // Assert
            AssertCreated();
        }

        [TestMethod]
        public void CreateOrUpdate_WithExistingDeviceType_ShouldUpdate()
        {
            // Arrange
            
            Helper.AssetManagement.DeviceTypes.Create(referenceDeviceType);

            var updatedDeviceType = new DeviceType
            {
                Identifier = referenceDeviceType.Identifier,
                Name = "Updated DeviceType Name",
                Description = "Updated Description",
                TagsInfo =
                {
                    Tags = new List<SlcAsset_Management.Enums.TagOption> 
                    { 
                        SlcAsset_Management.Enums.TagOption.RackUnitConsumer 
                    },
                },
                HierarchyInfo =
                {
                    HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis,
                },
            };

            // Act
            Helper.AssetManagement.DeviceTypes.CreateOrUpdate([updatedDeviceType]);

            // Assert
            var persisted = Helper.AssetManagement.DeviceTypes.Read(new TRUEFilterElement<DeviceType>()).First();
            AssertDeviceTypeUpdateDifferences(referenceDeviceType, persisted);
        }

        #endregion

        #region Read Tests

        [TestMethod]
        public void ReadPaged_WithValidFilter_ShouldReturnPages()
        {
            // Arrange
            const int pageSize = 3;
            
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);

            var allFilter = new TRUEFilterElement<DeviceType>();
            var totalCount = Helper.TestData.DeviceTypes.Count;

            // Act
            var pagedResult = Helper.AssetManagement.DeviceTypes.ReadPaged(allFilter, pageSize);

            // Assert
            using (new AssertionScope())
            {
                pagedResult.Should().NotBeNull();
                pagedResult.Should().HaveCountGreaterOrEqualTo((int)(totalCount / pageSize), 
                    "should have at least the expected number of pages");
                pagedResult.Should().AllSatisfy(page => 
                    page.Should().HaveCountLessOrEqualTo(pageSize), 
                    "each page should not exceed page size");
            }
        }

        #endregion

        #region Delete Tests

        [TestMethod]
        public void Delete_Single_ShouldRemoveDeviceType()
        {
            // Arrange
            
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);

            var initialCount = Helper.TestData.DeviceTypes.Count;
            var deviceTypeToDelete = Helper.AssetManagement.DeviceTypes
                .Read(DeviceTypeExposers.Name.Equal("Optics Module"))
                .First();

            // Act
            Helper.AssetManagement.DeviceTypes.Delete(deviceTypeToDelete);

            // Assert
            using (new AssertionScope())
            {
                Helper.AssetManagement.DeviceTypes.Count(new TRUEFilterElement<DeviceType>())
                    .Should().Be(initialCount - 1, "one device type should be deleted");

                Helper.AssetManagement.DeviceTypes.Count(DeviceTypeExposers.Identifier.Equal(deviceTypeToDelete.Identifier))
                    .Should().Be(0, "deleted device type should not exist");
            }
        }

        [TestMethod]
        public void Delete_Bulk_ShouldRemoveMultipleDeviceTypes()
        {
            // Arrange
            
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);

            var initialCount = Helper.TestData.DeviceTypes.Count;

            var filter = new ORFilterElement<DeviceType>(
                DeviceTypeExposers.Name.Equal("Decoder"),
                DeviceTypeExposers.TagsInfo.Tags.Contains(SlcAsset_Management.Enums.TagOption.PowerProvider));

            var deviceTypesToDelete = Helper.AssetManagement.DeviceTypes.Read(filter).ToList();
            var deleteCount = deviceTypesToDelete.Count;

            // Act
            Helper.AssetManagement.DeviceTypes.Delete(deviceTypesToDelete);

            // Assert
            using (new AssertionScope())
            {
                Helper.AssetManagement.DeviceTypes.Count(new TRUEFilterElement<DeviceType>())
                    .Should().Be(initialCount - deleteCount, $"{deleteCount} device types should be deleted");

                Helper.AssetManagement.DeviceTypes.Count(DeviceTypeExposers.Name.Equal("Decoder"))
                    .Should().Be(0, "Decoder should be deleted");
            }
        }

        #endregion

        #region Assertion Helpers

        private static void AssertDeviceTypeUpdateDifferences(DeviceType original, DeviceType updated)
        {
            using (new AssertionScope())
            {
                // Identifiers remain the same
                updated.Identifier.Should().Be(original.Identifier);

                // Updated fields
                updated.Name.Should().Be("Updated DeviceType Name");
                updated.Description.Should().Be("Updated Description");

                // TagsInfo changes
                updated.TagsInfo.Tags.Should().NotBeEquivalentTo(original.TagsInfo.Tags);
                updated.TagsInfo.Tags.Should().BeEquivalentTo(new List<SlcAsset_Management.Enums.TagOption> 
                { 
                    SlcAsset_Management.Enums.TagOption.RackUnitConsumer 
                });

                // HierarchyInfo changes
                updated.HierarchyInfo.HierarchyRole.Should().Be(SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis);
            }
        }

        private void AssertCreated()
        {
            using (new AssertionScope())
            {
                Helper.AssetManagement.DeviceTypes.Count(new TRUEFilterElement<DeviceType>()).Should().Be(1);

                var created = Helper.AssetManagement.DeviceTypes.Read(new TRUEFilterElement<DeviceType>()).First();

                // Basic properties
                created.Should().NotBeNull();
                created.Name.Should().Be(referenceDeviceType.Name);
                created.Description.Should().Be(referenceDeviceType.Description);

                // TagsInfo
                created.TagsInfo.Should().NotBeNull();
                created.TagsInfo.Tags.Should().BeEquivalentTo(referenceDeviceType.TagsInfo.Tags);

                // HierarchyInfo
                created.HierarchyInfo.Should().NotBeNull();
                created.HierarchyInfo.HierarchyRole.Should().Be(referenceDeviceType.HierarchyInfo.HierarchyRole);
            }
        }

        #endregion
    }
}
