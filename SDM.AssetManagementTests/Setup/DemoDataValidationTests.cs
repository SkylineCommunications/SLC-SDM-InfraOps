namespace SDM.AssetManagement.Tests.Setup
{
    using System;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Tests to validate DemoData templates and population process.
    /// If these tests fail, fix the demo data templates in DemoData.cs.
    /// </summary>
    [TestClass]
    public class DemoDataValidationTests : BaseRepositoryTest
    {
        /// <summary>
        /// Main validation test - ensures all demo data can be populated through validation middleware.
        /// If this fails, validation middleware caught an issue during Create().
        /// </summary>
        [TestMethod]
        public void DemoData_ShouldPopulateWithoutValidationErrors()
        {
                       
            // Act - This will throw if validation middleware fails
            Helper.PopulateWithDemoData(includeRacks: true);

            // Assert
            Assert.IsTrue(Helper.TestData.Assets.Any(), "Assets should be populated");
            Assert.IsTrue(Helper.TestData.AssetClasses.Any(), "AssetClasses should be populated");
            Assert.IsTrue(Helper.TestData.DeviceTypes.Any(), "DeviceTypes should be populated");
            Assert.IsTrue(Helper.TestData.DataPorts.Any(), "DataPorts should be populated");
            Assert.IsTrue(Helper.TestData.PowerPorts.Any(), "PowerPorts should be populated");
        }

        /// <summary>
        /// Validates DeviceType templates have required fields before population.
        /// </summary>
        [TestMethod]
        public void DemoData_DeviceTypes_ShouldHaveRequiredFields()
        {
            // Arrange
            var deviceTypes = DemoData.DeviceTypes.ToList();

            if (!deviceTypes.Any())
            {
                Assert.Inconclusive("No device types in DemoData to validate");
                return;
            }

            // Act & Assert
            for (int i = 0; i < deviceTypes.Count; i++)
            {
                var deviceType = deviceTypes[i];
                Assert.IsNotNull(deviceType.Name, $"DeviceType at index {i}: Name should not be null");
                Assert.IsFalse(string.IsNullOrWhiteSpace(deviceType.Name), $"DeviceType at index {i}: Name should not be empty");
            }
        }

        /// <summary>
        /// Checks for duplicate asset names in templates that would fail uniqueness validation.
        /// </summary>
        [TestMethod]
        public void DemoData_Assets_ShouldHaveUniqueNames()
        {
            // Arrange
            var assets = DemoData.BaseAssets.ToList();

            if (!assets.Any())
            {
                Assert.Inconclusive("No assets in DemoData to check");
                return;
            }

            // Act
            var duplicateNames = assets
                .Where(a => !string.IsNullOrWhiteSpace(a.Name))
                .GroupBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .ToList();

            // Assert
            if (duplicateNames.Any())
            {
                var duplicateList = string.Join(", ", duplicateNames.Select(g => $"'{g.Key}' ({g.Count()}x)"));
                Assert.Fail($"Found {duplicateNames.Count} duplicate asset name(s) in demo data templates: {duplicateList}");
            }
        }

        /// <summary>
        /// Checks for duplicate Asset IDs in templates.
        /// </summary>
        [TestMethod]
        public void DemoData_Assets_ShouldHaveUniqueAssetIDs()
        {
            // Arrange
            var assets = DemoData.BaseAssets.ToList();

            if (!assets.Any())
            {
                Assert.Inconclusive("No assets in DemoData to check");
                return;
            }

            // Act
            var duplicateIds = assets
                .Where(a => !string.IsNullOrWhiteSpace(a.AssetID))
                .GroupBy(a => a.AssetID, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .ToList();

            // Assert
            if (duplicateIds.Any())
            {
                var duplicateList = string.Join(", ", duplicateIds.Select(g => $"'{g.Key}' ({g.Count()}x)"));
                Assert.Fail($"Found {duplicateIds.Count} duplicate asset ID(s) in demo data templates: {duplicateList}");
            }
        }
    }
}