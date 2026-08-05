namespace SDM.AssetManagement.Tests.DeviceTypes
{
    using System.Collections.Generic;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;

    [TestClass]
    public class DeviceTypeValidationHandlerTests
    {
        [TestMethod]
        public void CanDelete_WithReferencingAssetNotDisposed_ShouldFail()
        {
            // Arrange
            var referencingAssets = new List<Asset>
            {
                new Asset
                {
                    State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available,
                },
            };

            // Act
            var isValid = DeviceTypeValidationHandler.CanDelete(referencingAssets, out var result);

            // Assert
            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("There are already assets assigned to this device type not in the 'Disposed' State"));
        }

        [TestMethod]
        public void CanDelete_WithOnlyDisposedReferencingAssets_ShouldPass()
        {
            // Arrange
            var referencingAssets = new List<Asset>
            {
                new Asset
                {
                    State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed,
                },
            };

            // Act
            var isValid = DeviceTypeValidationHandler.CanDelete(referencingAssets, out var result);

            // Assert
            isValid.Should().BeTrue();
            result.IsValid.Should().BeTrue();
            result.FailureReasons.Should().BeEmpty();
        }
    }
}
