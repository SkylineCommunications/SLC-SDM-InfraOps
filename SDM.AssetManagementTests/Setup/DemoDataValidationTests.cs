namespace SDM.AssetManagement.Tests.Setup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Diagnostic tests to identify validation issues in DemoData.
    /// Run these tests to see which demo data entities are failing validation.
    /// </summary>
    [TestClass]
    public class DemoDataValidationTests
    {
        [TestMethod]
        public void DemoData_Assets_ShouldIdentifyValidationIssues()
        {
            // Arrange - Need full population chain
            var helper = RepositoryInitialize.InitializeEmptyRepositories();
            helper.PopulateRacks()
                .PopulateDeviceTypes()
                .PopulateAssetClasses()
                .PopulateAssets();

            // Create validator from helper
            var assetValidator = helper.CreateAssetValidator();

            // Now get the PERSISTED assets (not templates)
            var assets = helper.AssetManagement.Assets
                .Read(new TRUEFilterElement<Asset>())
                .ToList();

            if (!assets.Any())
            {
                Assert.Inconclusive("No assets were populated");
                return;
            }

            var failedAssets = new List<(Asset Asset, ValidationResult Result)>();

            // Act - Validate each asset individually
            for (int i = 0; i < assets.Count; i++)
            {
                try
                {
                    var asset = assets[i];
                    var result = assetValidator.Validate(asset);

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
                Assert.Fail($"{failedAssets.Count} out of {assets.Count} populated assets failed validation. See test output for details.");
            }
        }

        [TestMethod]
        public void DemoData_Assets_BulkValidation_ShouldIdentifyIssues()
        {
            // Arrange
            var helper = RepositoryInitialize.InitializeEmptyRepositories();

            helper.PopulateRacks()
                .PopulateDeviceTypes()
                .PopulateAssetClasses()
                .PopulateAssets();

            var validator = helper.CreateAssetValidator();

            var assets = helper.AssetManagement.Assets
                .Read(new TRUEFilterElement<Asset>())
                .ToList();

            if (!assets.Any())
            {
                Assert.Inconclusive("No assets were populated");
                return;
            }

            List<ValidationResult> results;

            // Act
            try
            {
                results = validator.ValidateBulk(assets);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Bulk validation threw exception: {ex.Message}\n{ex.StackTrace}");
                return;
            }

            // Assert
            var validCount = results.Count(r => r.IsValid);
            var invalidCount = results.Count(r => !r.IsValid);

            if (invalidCount > 0)
            {
                // Group errors by field for summary
                var failuresByField = new Dictionary<string, List<string>>();

                for (int i = 0; i < assets.Count; i++)
                {
                    if (!results[i].IsValid)
                    {
                        var asset = assets[i];

                        foreach (var error in results[i].FailureReasons)
                        {
                            if (!failuresByField.ContainsKey(error.Key))
                            {
                                failuresByField[error.Key] = new List<string>();
                            }

                            failuresByField[error.Key].Add($"{asset.Name}: {error.Value}");
                        }
                    }
                }

                // Build concise failure message
                var errorSummary = string.Join("\n",
                    failuresByField.OrderByDescending(x => x.Value.Count)
                    .Select(kvp => $"\n[{kvp.Key}] - {kvp.Value.Count} failure(s):\n  " +
                                    string.Join("\n  ", kvp.Value.Take(3)) +
                                    (kvp.Value.Count > 3 ? $"\n  ... and {kvp.Value.Count - 3} more" : "")));

                Assert.Fail($@"Bulk Validation Failed: {invalidCount}/{assets.Count} assets invalid {errorSummary}");
            }
            else
            {
                Console.WriteLine("✅ All assets passed bulk validation successfully!");
            }
        }

        [TestMethod]
        public void DemoData_AssetClasses_ShouldIdentifyValidationIssues()
        {
            // Arrange - Need to populate DeviceTypes first, then AssetClasses
            var helper = RepositoryInitialize.InitializeEmptyRepositories();
            helper.PopulateDeviceTypes()
                .PopulateAssetClasses();

            // Create validator from helper
            var assetClassValidator = helper.CreateAssetClassValidator();

            // Now get the PERSISTED asset classes (not templates)
            var assetClasses = helper.AssetManagement.AssetClasses
                .Read(new TRUEFilterElement<AssetClass>())
                .ToList();

            if (!assetClasses.Any())
            {
                Assert.Inconclusive("No asset classes were populated");
                return;
            }

            var failedAssetClasses = new List<(AssetClass AssetClass, ValidationResult Result)>();

            // Act - Validate each asset class individually
            for (int i = 0; i < assetClasses.Count; i++)
            {
                try
                {
                    var assetClass = assetClasses[i];
                    var result = assetClassValidator.Validate(assetClass);

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
                Assert.Fail($"{failedAssetClasses.Count} out of {assetClasses.Count} populated asset classes failed validation. See test output for details.");
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