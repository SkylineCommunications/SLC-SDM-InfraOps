namespace SDM.AssetManagement.Tests.Assets
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
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    [TestClass]
    public class AssetDomRepositoryTransitionTests
    {
        private ITestApiHelper _helper = null!;
        private AssetClass _assetClass = null!;

        [TestInitialize]
        public void Initialize()
        {
            _helper = RepositoryInitialize.InitializeWithAssetBehavior();
            _helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);
            _assetClass = _helper.AssetManagement.AssetClasses.Create(new AssetClass
            {
                Name = "Transition asset class " + Guid.NewGuid(),
                DeviceTypeId = new SdmObjectReference<DeviceType>(_helper.TestData.DeviceTypes.First().Identifier),
                State = SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active,
                Height = 1,
                Width = 1,
                Depth = 1,
                HeightU = 1,
                Weight = 1,
            });
        }

        private Asset CreateAsset(
            SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum state,
            bool destination = false,
            bool installation = false)
        {
            var asset = new Asset
            {
                Name = "Transition asset " + Guid.NewGuid(),
                AssetID = Guid.NewGuid().ToString(),
                AssetClassId = new SdmObjectReference<AssetClass>(_assetClass.Identifier),
                State = state,
            };

            if (destination)
            {
                asset.DestinationLocation.RoomId = new SdmObjectReference<Room>(Guid.NewGuid().ToString());
            }

            if (installation)
            {
                asset.InstallationUserId = Guid.NewGuid();
                asset.InstallationDate = DateTime.UtcNow;
            }

            return _helper.AssetManagement.Assets.Create(asset);
        }

        [DataTestMethod]
        [DataRow(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available)]
        [DataRow(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.BuildPlanReady)]
        [DataRow(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed)]
        public void TransitionTo_ValidPath_ShouldPersist(
            SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum from,
            SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum to)
        {
            var asset = CreateAsset(from);
            var result = _helper.AssetManagement.Assets.TransitionTo(asset, to);

            result.State.Should().Be(to);
            var reread = _helper.AssetManagement.Assets.Read(
                AssetExposers.Identifier.Equal(result.Identifier)).Single();
            reread.State.Should().Be(to);
        }

        [DataTestMethod]
        [DataRow(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available)]
        [DataRow(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable)]
        [DataRow(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable)]
        public void TransitionTo_InvalidPath_ShouldThrow(
            SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum from,
            SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum to)
        {
            var asset = CreateAsset(from);
            Action act = () => _helper.AssetManagement.Assets.TransitionTo(asset, to);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage($"State transition from {from} to {to} is not allowed.");
        }
    }
}
