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

        [DataTestMethod]
        [DataRow(-0.01)]
        [DataRow(-100.0)]
        public void IsCableLengthValid_WithNegativeLength_ShouldFail(double length)
        {
            var isValid = ConnectionValidationHandler.IsCableLengthValid(length, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Cable length cannot be negative."));
        }

        [DataTestMethod]
        [DataRow(0.0)]
        [DataRow(15.5)]
        public void IsCableLengthValid_WithNonNegativeLength_ShouldPass(double length)
        {
            var isValid = ConnectionValidationHandler.IsCableLengthValid(length, out var result);

            isValid.Should().BeTrue();
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void IsCableLengthValid_WithNullLength_ShouldPass()
        {
            var isValid = ConnectionValidationHandler.IsCableLengthValid(null, out var result);

            isValid.Should().BeTrue();
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void IsPortDirectionValid_SourceInputOnly_ShouldFail()
        {
            var isValid = ConnectionValidationHandler.IsPortDirectionValid(SlcAsset_Management.Enums.Outputtype.In, isSource: true, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("The source port must be of type Output or I/O."));
        }

        [DataTestMethod]
        [DataRow(SlcAsset_Management.Enums.Outputtype.Out)]
        [DataRow(SlcAsset_Management.Enums.Outputtype.IO)]
        public void IsPortDirectionValid_SourceOutputOrIo_ShouldPass(SlcAsset_Management.Enums.Outputtype outputType)
        {
            var isValid = ConnectionValidationHandler.IsPortDirectionValid(outputType, isSource: true, out var result);

            isValid.Should().BeTrue();
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void IsPortDirectionValid_DestinationOutputOnly_ShouldFail()
        {
            var isValid = ConnectionValidationHandler.IsPortDirectionValid(SlcAsset_Management.Enums.Outputtype.Out, isSource: false, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("The destination port must be of type Input or I/O."));
        }

        [DataTestMethod]
        [DataRow(SlcAsset_Management.Enums.Outputtype.In)]
        [DataRow(SlcAsset_Management.Enums.Outputtype.IO)]
        public void IsPortDirectionValid_DestinationInputOrIo_ShouldPass(SlcAsset_Management.Enums.Outputtype outputType)
        {
            var isValid = ConnectionValidationHandler.IsPortDirectionValid(outputType, isSource: false, out var result);

            isValid.Should().BeTrue();
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void IsNotSelfConnection_WithSamePort_ShouldFail()
        {
            var port = System.Guid.NewGuid();

            var isValid = ConnectionValidationHandler.IsNotSelfConnection(port, port, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Source Port is the same as destination."));
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Destination Port is the same as source."));
        }

        [TestMethod]
        public void IsNotSelfConnection_WithDifferentPorts_ShouldPass()
        {
            var isValid = ConnectionValidationHandler.IsNotSelfConnection(System.Guid.NewGuid(), System.Guid.NewGuid(), out var result);

            isValid.Should().BeTrue();
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void IsNotSelfConnection_WithEmptyPorts_ShouldPass()
        {
            var isValid = ConnectionValidationHandler.IsNotSelfConnection(System.Guid.Empty, System.Guid.Empty, out var result);

            isValid.Should().BeTrue();
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void IsEndpointAssetValid_WithNullAsset_ShouldFail()
        {
            var isValid = ConnectionValidationHandler.IsEndpointAssetValid(null, null, null, SlcAsset_Management.Enums.ConnectionType.Data, isSource: true, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("The asset must be provided."));
        }

        [DataTestMethod]
        [DataRow(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable)]
        [DataRow(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed)]
        public void IsEndpointAssetValid_WithUnusableState_ShouldFail(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum state)
        {
            var asset = new Asset { State = state };

            var isValid = ConnectionValidationHandler.IsEndpointAssetValid(asset, null, null, SlcAsset_Management.Enums.ConnectionType.Data, isSource: true, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("The asset must not be in the 'Not Available' or 'Disposed' state."));
        }

        [TestMethod]
        public void IsEndpointAssetValid_WithoutAssetClass_ShouldFail()
        {
            var asset = new Asset { State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available };

            var isValid = ConnectionValidationHandler.IsEndpointAssetValid(asset, null, null, SlcAsset_Management.Enums.ConnectionType.Data, isSource: true, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("The asset must have an asset class."));
        }

        [DataTestMethod]
        [DataRow(SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Draft)]
        [DataRow(SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Deprecated)]
        public void IsEndpointAssetValid_WithInactiveAssetClass_ShouldFail(SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum state)
        {
            var asset = new Asset { State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available };
            var assetClass = new AssetClass { State = state };

            var isValid = ConnectionValidationHandler.IsEndpointAssetValid(asset, assetClass, null, SlcAsset_Management.Enums.ConnectionType.Data, isSource: true, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("The asset's Asset Class must be active."));
        }

        [TestMethod]
        public void IsEndpointAssetValid_DataConnection_WithoutAcceptsDataConnectionTag_ShouldFail()
        {
            var asset = new Asset { State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available };
            var assetClass = new AssetClass { State = SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active };
            var deviceType = new DeviceType();

            var isValid = ConnectionValidationHandler.IsEndpointAssetValid(asset, assetClass, deviceType, SlcAsset_Management.Enums.ConnectionType.Data, isSource: true, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("The asset must accept data connections."));
        }

        [TestMethod]
        public void IsEndpointAssetValid_DataConnection_WithAcceptsDataConnectionTag_ShouldPass()
        {
            var asset = new Asset { State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available };
            var assetClass = new AssetClass { State = SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active };
            var deviceType = new DeviceType
            {
                TagsInfo =
                {
                    Tags = new System.Collections.Generic.List<SlcAsset_Management.Enums.TagOption> { SlcAsset_Management.Enums.TagOption.AcceptsDataConnection },
                },
            };

            var isValid = ConnectionValidationHandler.IsEndpointAssetValid(asset, assetClass, deviceType, SlcAsset_Management.Enums.ConnectionType.Data, isSource: false, out var result);

            isValid.Should().BeTrue();
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void IsEndpointAssetValid_PowerConnectionSource_WithoutPowerProviderTag_ShouldFail()
        {
            var asset = new Asset { State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available };
            var assetClass = new AssetClass { State = SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active };
            var deviceType = new DeviceType();

            var isValid = ConnectionValidationHandler.IsEndpointAssetValid(asset, assetClass, deviceType, SlcAsset_Management.Enums.ConnectionType.Power, isSource: true, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("The asset must be a Power Provider."));
        }

        [TestMethod]
        public void IsEndpointAssetValid_PowerConnectionSource_WithPowerProviderTag_ShouldPass()
        {
            var asset = new Asset { State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available };
            var assetClass = new AssetClass { State = SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active };
            var deviceType = new DeviceType
            {
                TagsInfo =
                {
                    Tags = new System.Collections.Generic.List<SlcAsset_Management.Enums.TagOption> { SlcAsset_Management.Enums.TagOption.PowerProvider },
                },
            };

            var isValid = ConnectionValidationHandler.IsEndpointAssetValid(asset, assetClass, deviceType, SlcAsset_Management.Enums.ConnectionType.Power, isSource: true, out var result);

            isValid.Should().BeTrue();
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void IsEndpointAssetValid_PowerConnectionDestination_WithoutPowerProviderTag_ShouldPass()
        {
            var asset = new Asset { State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available };
            var assetClass = new AssetClass { State = SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active };
            var deviceType = new DeviceType();

            var isValid = ConnectionValidationHandler.IsEndpointAssetValid(asset, assetClass, deviceType, SlcAsset_Management.Enums.ConnectionType.Power, isSource: false, out var result);

            isValid.Should().BeTrue();
            result.IsValid.Should().BeTrue();
        }
    }
}
