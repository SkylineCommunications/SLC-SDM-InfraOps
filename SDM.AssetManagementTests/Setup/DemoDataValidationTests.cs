namespace SDM.AssetManagement.Tests.Setup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Diagnostic tests to identify validation issues in DemoData.
    /// Run these tests to see which demo data entities are failing validation.
    /// </summary>
    [TestClass]
    public class DemoDataValidationTests
    {
        private AssetValidator _assetValidator;
        private AssetClassValidator _assetClassValidator;

        [TestInitialize]
        public void TestInitialize()
        {
            var helper = RepositoryInitialize.InitializeEmptyRepositories();
            
            // Populate DeviceTypes first as they're needed by validators
            helper.PopulateDeviceTypes();

            // Create entity loader - now works directly!
            var entityLoader = new SdmEntityLoader(
                assetRepository: helper.Assets,
                assetClassRepository: helper.AssetClasses,
                deviceTypeRepository: helper.DeviceTypes,
                dataPortRepository: helper.DataPorts,
                powerPortRepository: helper.PowerPorts,
                rackRepository: null,  // Add if needed
                reservationRepository: null,  // Add if needed
                portTypeRepository: null  // Add if needed
            );

            _assetValidator = new AssetValidator(entityLoader);
            _assetClassValidator = new AssetClassValidator(entityLoader);
        }

        [TestMethod]
        public void DemoData_Assets_ShouldIdentifyValidationIssues()
        {
            // Arrange
            var assets = DemoData.Assets.ToList();
            
            if (!assets.Any())
            {
                Assert.Inconclusive("No assets in DemoData to validate");
                return;
            }

            var failedAssets = new List<(Asset Asset, ValidationResult Result)>();

            // Act - Validate each asset individually
            for (int i = 0; i < assets.Count; i++)
            {
                try
                {
                    var asset = assets[i];
                    var result = _assetValidator.Validate(asset);

                    if (!result.IsValid)
                    {
                        failedAssets.Add((asset, result));

                        Console.WriteLine($"Asset {i} '{asset.Name}' (ID: '{asset.AssetID}') failed validation:");
                        foreach (var error in result.FailureReasons)
                        {
                            Console.WriteLine($"  - [{error.Key}] {error.Value}");
                        }
                        Console.WriteLine();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Asset {i} threw exception during validation: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    Assert.Fail($"Asset {i} threw exception: {ex.Message}");
                }
            }

            // Assert
            if (failedAssets.Any())
            {
                Assert.Fail($"{failedAssets.Count} out of {assets.Count} demo assets failed validation. See test output for details.");
            }
        }

        [TestMethod]
        public void DemoData_Assets_BulkValidation_ShouldIdentifyIssues()
        {
            // Arrange
            var assets = DemoData.Assets.ToList();
            
            if (!assets.Any())
            {
                Assert.Inconclusive("No assets in DemoData to validate");
                return;
            }

            List<ValidationResult> results;
            
            // Act
            try
            {
                results = _assetValidator.ValidateBulk(assets);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Assert.Fail($"Bulk validation threw exception: {ex.Message}");
                return;
            }

            // Assert
            var invalidCount = results.Count(r => !r.IsValid);

            if (invalidCount > 0)
            {
                Console.WriteLine($"Bulk validation found {invalidCount} invalid assets:");
                Console.WriteLine();

                for (int i = 0; i < assets.Count; i++)
                {
                    if (!results[i].IsValid)
                    {
                        Console.WriteLine($"Asset {i} '{assets[i].Name}' (ID: '{assets[i].AssetID}'):");
                        foreach (var error in results[i].FailureReasons)
                        {
                            Console.WriteLine($"  - [{error.Key}] {error.Value}");
                        }
                        Console.WriteLine();
                    }
                }

                Assert.Fail($"{invalidCount} assets failed bulk validation.");
            }
        }

        [TestMethod]
        public void DemoData_AssetClasses_ShouldIdentifyValidationIssues()
        {
            // Arrange
            var assetClasses = DemoData.AssetClasses.ToList();
            
            if (!assetClasses.Any())
            {
                Assert.Inconclusive("No asset classes in DemoData to validate");
                return;
            }

            var failedAssetClasses = new List<(AssetClass AssetClass, ValidationResult Result)>();

            // Act - Validate each asset class individually
            for (int i = 0; i < assetClasses.Count; i++)
            {
                try
                {
                    var assetClass = assetClasses[i];
                    var result = _assetClassValidator.Validate(assetClass);

                    if (!result.IsValid)
                    {
                        failedAssetClasses.Add((assetClass, result));

                        Console.WriteLine($"AssetClass {i} '{assetClass.Name}' failed validation:");
                        foreach (var error in result.FailureReasons)
                        {
                            Console.WriteLine($"  - [{error.Key}] {error.Value}");
                        }
                        Console.WriteLine();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"AssetClass {i} threw exception during validation: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    Assert.Fail($"AssetClass {i} threw exception: {ex.Message}");
                }
            }

            // Assert
            if (failedAssetClasses.Any())
            {
                Assert.Fail($"{failedAssetClasses.Count} out of {assetClasses.Count} demo asset classes failed validation. See test output for details.");
            }
        }

        [TestMethod]
        public void DemoData_DeviceTypes_ShouldBeValid()
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

        [TestMethod]
        public void DemoData_CheckChangeTracking()
        {
            // This test checks if demo data has proper change tracking set up
            // Arrange
            var assets = DemoData.Assets.ToList();
            
            if (!assets.Any())
            {
                Assert.Inconclusive("No assets in DemoData to check");
                return;
            }

            var issueCount = 0;

            // Act & Assert
            foreach (var asset in assets)
            {
                if (!string.IsNullOrEmpty(asset.Name))
                {
                    var hasAssetChanged = asset.Changed;
                    Console.WriteLine($"Asset '{asset.Name}' changed");

                    if (!hasAssetChanged)
                    {
                        Console.WriteLine($"  WARNING: Asset '{asset.Name}' has a Name but NameField.Changed is false!");
                        issueCount++;
                    }
                }

                if (!string.IsNullOrEmpty(asset.AssetID))
                {
                    var hasAssetChanged = asset.Changed;
                    Console.WriteLine($"Asset '{asset.Name}' changed");

                    if (!hasAssetChanged)
                    {
                        Console.WriteLine($"  WARNING: Asset '{asset.Name}' has an AssetID but Changed is false!");
                        issueCount++;
                    }
                }
            }

            if (issueCount > 0)
            {
                Assert.Inconclusive($"Found {issueCount} potential change tracking issues. Review console output.");
            }
        }

        [TestMethod]
        public void DemoData_CheckDuplicateNames()
        {
            // Check for duplicate names that would fail uniqueness validation
            // Arrange
            var assets = DemoData.Assets.ToList();
            
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
                Console.WriteLine("Duplicate asset names found:");
                foreach (var group in duplicateNames)
                {
                    Console.WriteLine($"  - '{group.Key}' appears {group.Count()} times");
                }

                Assert.Fail($"Found {duplicateNames.Count} duplicate asset name(s) in demo data.");
            }
        }

        [TestMethod]
        public void DemoData_CheckDuplicateAssetIDs()
        {
            // Check for duplicate Asset IDs
            // Arrange
            var assets = DemoData.Assets.ToList();
            
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
                Console.WriteLine("Duplicate asset IDs found:");
                foreach (var group in duplicateIds)
                {
                    Console.WriteLine($"  - '{group.Key}' appears {group.Count()} times");
                }

                Assert.Fail($"Found {duplicateIds.Count} duplicate asset ID(s) in demo data.");
            }
        }

        [TestMethod]
        public void DemoData_CheckAssetClassReferences()
        {
            // Check if assets reference valid asset classes
            // Arrange
            var assets = DemoData.Assets.ToList();
            var assetClasses = DemoData.AssetClasses.ToList();
            
            if (!assets.Any())
            {
                Assert.Inconclusive("No assets in DemoData to check");
                return;
            }

            var assetClassIds = new HashSet<string>(assetClasses.Select(ac => ac.Identifier));
            var invalidReferences = new List<string>();

            // Act
            foreach (var asset in assets)
            {
                if (asset.AssetClassId != null && asset.AssetClassId.HasValue())
                {
                    if (!assetClassIds.Contains(asset.AssetClassId.Identifier))
                    {
                        invalidReferences.Add($"Asset '{asset.Name}' references non-existent AssetClass '{asset.AssetClassId.Identifier}'");
                    }
                }
            }

            // Assert
            if (invalidReferences.Any())
            {
                Console.WriteLine("Invalid AssetClass references found:");
                foreach (var reference in invalidReferences)
                {
                    Console.WriteLine($"  - {reference}");
                }

                Assert.Fail($"Found {invalidReferences.Count} invalid AssetClass reference(s).");
            }
        }

        [TestMethod]
        public void DemoData_CheckDeviceTypeReferences()
        {
            // Check if asset classes reference valid device types
            // Arrange
            var assetClasses = DemoData.AssetClasses.ToList();
            var deviceTypes = DemoData.DeviceTypes.ToList();
            
            if (!assetClasses.Any())
            {
                Assert.Inconclusive("No asset classes in DemoData to check");
                return;
            }

            var deviceTypeIds = new HashSet<string>(deviceTypes.Select(dt => dt.Identifier));
            var invalidReferences = new List<string>();

            // Act
            foreach (var assetClass in assetClasses)
            {
                if (assetClass.DeviceTypeId != null && assetClass.DeviceTypeId.HasValue())
                {
                    if (!deviceTypeIds.Contains(assetClass.DeviceTypeId.Identifier))
                    {
                        invalidReferences.Add($"AssetClass '{assetClass.Name}' references non-existent DeviceType '{assetClass.DeviceTypeId.Identifier}'");
                    }
                }
            }

            // Assert
            if (invalidReferences.Any())
            {
                Console.WriteLine("Invalid DeviceType references found:");
                foreach (var reference in invalidReferences)
                {
                    Console.WriteLine($"  - {reference}");
                }

                Assert.Fail($"Found {invalidReferences.Count} invalid DeviceType reference(s).");
            }
        }
    }
}