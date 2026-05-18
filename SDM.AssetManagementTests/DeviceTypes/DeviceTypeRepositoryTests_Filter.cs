namespace SDM.AssetManagement.Tests.DeviceTypes
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

    /// <summary>
    /// Filter and query tests for DeviceType repository operations.
    /// </summary>
    [TestClass]
    public class DeviceTypeRepositoryTests_Filter : BaseRepositoryTest
    {
        #region Basic Field Filters

        [TestMethod]
        public void ReadFilter_Name_Equal_ShouldReturnMatchingDeviceType()
        {
            // Arrange
            
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);

            var targetDeviceType = Helper.TestData.DeviceTypes.Skip(3).First();
            var filter = DeviceTypeExposers.Name.Equal(targetDeviceType.Name);

            // Act
            var results = Helper.AssetManagement.DeviceTypes.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().HaveCount(1, $"should find device type with name '{targetDeviceType.Name}'");
                var deviceType = results.First();
                deviceType.Name.Should().Be(targetDeviceType.Name);
                deviceType.Identifier.Should().Be(targetDeviceType.Identifier);
            }
        }

        [TestMethod]
        public void ReadFilter_Name_Contains_ShouldReturnMatchingDeviceTypes()
        {
            // Arrange
            
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);

            const string namePattern = "coder";
            var filter = DeviceTypeExposers.Name.Contains(namePattern);

            // Act
            var results = Helper.AssetManagement.DeviceTypes.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find device types with '{namePattern}' in name");
                results.Should().OnlyContain(dt => dt.Name.Contains(namePattern), 
                    "all results should contain 'coder' (e.g., Encoder, Decoder)");
            }
        }

        [TestMethod]
        public void ReadFilter_Description_Contains_ShouldReturnMatchingDeviceTypes()
        {
            // Arrange
            
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);

            const string descriptionPattern = "UPS";
            var filter = DeviceTypeExposers.Description.Contains(descriptionPattern);

            // Act
            var results = Helper.AssetManagement.DeviceTypes.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find device types with '{descriptionPattern}' in description");
                results.Should().OnlyContain(dt => dt.Description.Contains(descriptionPattern));
            }
        }

        #endregion

        #region Nested Object Filters

        [TestMethod]
        public void ReadFilter_HierarchyRole_Equal_ShouldReturnMatchingDeviceTypes()
        {
            // Arrange
            
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);

            var hierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis;
            var filter = DeviceTypeExposers.HierarchyInfo.HierarchyRole.Equal(hierarchyRole);

            // Act
            var results = Helper.AssetManagement.DeviceTypes.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find device types with hierarchy role '{hierarchyRole}'");
                results.Should().OnlyContain(dt => dt.HierarchyInfo.HierarchyRole == hierarchyRole);
            }
        }

        [TestMethod]
        public void ReadFilter_Tags_NotContains_ShouldReturnDeviceTypesWithoutTag()
        {
            // Arrange
            
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);

            var excludedTag = SlcAsset_Management.Enums.TagOption.AcceptsDataConnection;
            var filter = DeviceTypeExposers.TagsInfo.Tags.NotContains(excludedTag);

            // Act
            var results = Helper.AssetManagement.DeviceTypes.Read(filter).ToList();

            // Assert
            using (new AssertionScope())
            {
                results.Should().NotBeEmpty($"should find device types without tag '{excludedTag}'");
                results.Should().OnlyContain(dt => !dt.TagsInfo.Tags.Contains(excludedTag));
            }
        }

        #endregion
    }
}
