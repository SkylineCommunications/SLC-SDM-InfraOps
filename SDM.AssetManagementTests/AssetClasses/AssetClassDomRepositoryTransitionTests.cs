namespace SDM.AssetManagement.Tests.AssetClasses
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    [TestClass]
    public class AssetClassDomRepositoryTransitionTests
    {
        private ITestApiHelper _helper = null!;

        [TestInitialize]
        public void Initialize()
        {
            _helper = RepositoryInitialize.InitializeWithAssetClassBehavior();
            _helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);
        }

        private AssetClass CreateAssetClass(
            SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum state)
        {
            // Deterministic, rack-mountable, non-PowerProvider device type (see RackMountableDeviceType).
            var deviceType = _helper.TestData.RackMountableDeviceType();
            // The in-memory DOM double honors an explicitly supplied State on Create, matching CreateJobAt.
            return _helper.AssetManagement.AssetClasses.Create(new AssetClass
            {
                Name = "Transition class " + Guid.NewGuid(),
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                State = state,
                Height = 1,
                Width = 1,
                Depth = 1,
                HeightU = 1,
                Weight = 1,
            });
        }

        [DataTestMethod]
        [DataRow(SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Draft, SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active)]
        [DataRow(SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active, SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Deprecated)]
        [DataRow(SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Draft, SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Deprecated)]
        public void TransitionTo_ValidPath_ShouldPersist(
            SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum from,
            SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum to)
        {
            var assetClass = CreateAssetClass(from);
            var result = _helper.AssetManagement.AssetClasses.TransitionTo(assetClass, to);

            result.State.Should().Be(to);
            var reread = _helper.AssetManagement.AssetClasses.Read(
                AssetClassExposers.Identifier.Equal(result.Identifier)).Single();
            reread.State.Should().Be(to);
        }

        [DataTestMethod]
        [DataRow(SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active, SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Draft)]
        [DataRow(SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Deprecated, SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active)]
        [DataRow(SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Draft, SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Draft)]
        public void TransitionTo_InvalidPath_ShouldThrow(
            SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum from,
            SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum to)
        {
            var assetClass = CreateAssetClass(from);
            Action act = () => _helper.AssetManagement.AssetClasses.TransitionTo(assetClass, to);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage($"State transition from {from} to {to} is not allowed.");
        }
    }
}