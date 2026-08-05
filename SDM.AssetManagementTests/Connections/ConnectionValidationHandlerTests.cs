namespace SDM.AssetManagement.Tests.Connections
{
    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    [TestClass]
    public class ConnectionValidationHandlerTests
    {
        [TestMethod]
        public void IsEndpointAssetStateValid_WithNullAsset_ShouldFail()
        {
            var isValid = ConnectionValidationHandler.IsSourceAssetStateValid(null, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("The asset must be provided."));
        }

        [TestMethod]
        public void IsSourceAssetStateValid_WithNotAvailableAsset_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable,
            };

            // Act
            var isValid = ConnectionValidationHandler.IsSourceAssetStateValid(asset, out var result);

            // Assert
            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("The asset must not be in the 'Not Available' or 'Disposed' state."));
        }

        [TestMethod]
        public void IsDestinationAssetStateValid_WithDisposedAsset_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed,
            };

            // Act
            var isValid = ConnectionValidationHandler.IsDestinationAssetStateValid(asset, out var result);

            // Assert
            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("The asset must not be in the 'Not Available' or 'Disposed' state."));
        }

        [TestMethod]
        public void IsEndpointAssetStateValid_WithAvailableAsset_ShouldPass()
        {
            // Arrange
            var asset = new Asset
            {
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available,
            };

            // Act
            var isValid = ConnectionValidationHandler.IsSourceAssetStateValid(asset, out var result);

            // Assert
            isValid.Should().BeTrue();
            result.IsValid.Should().BeTrue();
            result.FailureReasons.Should().BeEmpty();
        }
    }
}
