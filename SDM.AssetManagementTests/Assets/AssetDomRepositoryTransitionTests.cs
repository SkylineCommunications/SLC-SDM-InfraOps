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
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

    [TestClass]
    public class AssetDomRepositoryTransitionTests
    {
        private ITestApiHelper _helper = null!;
        private AssetClass _assetClass = null!;

        [TestInitialize]
        public void Initialize()
        {
            _helper = RepositoryInitialize.Initialize();
            _helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);

            // Deterministic, rack-mountable, non-PowerProvider device type (see RackMountableDeviceType).
            var deviceType = _helper.TestData.RackMountableDeviceType();

            _assetClass = _helper.AssetManagement.AssetClasses.Create(new AssetClass
            {
                Manufacturer = SDM.AssetManagement.Tests.Setup.PeopleApiMock.NewManufacturer(),
                Name = "Transition asset class " + Guid.NewGuid(),
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
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
                asset.InstallationUserId = new PnoObjectReference<Person>(Guid.NewGuid());
                asset.InstallationDate = DateTime.UtcNow;
            }

            return _helper.AssetManagement.Assets.Create(asset);
        }

        [TestMethod]
        public void TransitionTo_StatusOnly_ReportsStateChange()
        {
            var asset = CreateAsset(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available);
            var result = _helper.AssetManagement.Assets.TransitionTo(
                asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable);

            var changes = result.GetChanges().ToList();

            changes.Should().ContainSingle();
            changes[0].FieldName.Should().Be("State");
            changes[0].OldValue.Should().Be(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available);
            changes[0].NewValue.Should().Be(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable);
            result.Changed.Should().BeTrue();
        }

        [TestMethod]
        public void TransitionTo_WithPendingEdit_ReportsStateAndEdit()
        {
            var asset = CreateAsset(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available);
            var newName = "Renamed " + Guid.NewGuid();
            asset.Name = newName;

            var result = _helper.AssetManagement.Assets.TransitionTo(
                asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable);

            var names = result.GetChanges().Select(c => c.FieldName).ToList();
            names.Should().BeEquivalentTo(new[] { "State", "Name" });
            result.Name.Should().Be(newName);
        }

        [TestMethod]
        public void TransitionTo_ConsecutiveTransitions_PreservePendingEditAndReportLatestStateChange()
        {
            var asset = CreateAsset(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable);
            var originalName = asset.Name;
            var newName = "Renamed " + Guid.NewGuid();
            asset.Name = newName;

            asset = _helper.AssetManagement.Assets.TransitionTo(
                asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available);
            asset = _helper.AssetManagement.Assets.TransitionTo(
                asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable);

            asset.State.Should().Be(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable);
            asset.Name.Should().Be(newName);
            var changes = asset.GetChanges().ToList();
            changes.Select(c => c.FieldName).Should().BeEquivalentTo(new[] { "State", "Name" });
            var stateChange = changes.Single(c => c.FieldName == "State");
            stateChange.OldValue.Should().Be(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available);
            stateChange.NewValue.Should().Be(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable);
            var nameChange = changes.Single(c => c.FieldName == "Name");
            nameChange.OldValue.Should().Be(originalName);
            nameChange.NewValue.Should().Be(newName);

            var reread = _helper.AssetManagement.Assets.Read(
                AssetExposers.Identifier.Equal(asset.Identifier)).Single();
            reread.State.Should().Be(asset.State);
            reread.Name.Should().Be(originalName);
            reread.GetChanges().Should().BeEmpty();
        }

        [TestMethod]
        public void TransitionTo_MultiStepPath_ReportsInitialToFinalState()
        {
            var asset = CreateAsset(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable);

            var result = _helper.AssetManagement.Assets.TransitionTo(
                asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.BuildPlanReady);

            var change = result.GetChanges().Single(c => c.FieldName == "State");
            change.OldValue.Should().Be(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable);
            change.NewValue.Should().Be(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.BuildPlanReady);
        }

        [TestMethod]
        public void TransitionTo_StateChange_IsClearedByResetChangeTracking()
        {
            var asset = CreateAsset(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available);
            var result = _helper.AssetManagement.Assets.TransitionTo(
                asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable);

            result.ResetChangeTracking();

            result.GetChanges().Should().BeEmpty();
            result.State.Should().Be(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable);
        }

        [TestMethod]
        public void TransitionTo_StateChange_IsNotPersistedAsPendingChange()
        {
            var asset = CreateAsset(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available);
            var result = _helper.AssetManagement.Assets.TransitionTo(
                asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable);

            var reread = _helper.AssetManagement.Assets.Read(
                AssetExposers.Identifier.Equal(result.Identifier)).Single();

            reread.State.Should().Be(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable);
            reread.GetChanges().Should().BeEmpty();
        }

        [TestMethod]
        public void SameState_TransitionTo_IsNoOp_And_UpdateVariants_OnlyUpdate()
        {
            var state = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available;
            var asset = CreateAsset(state);

            _helper.AssetManagement.Assets.TransitionTo(asset, state).State.Should().Be(state);

            var name1 = "Renamed " + Guid.NewGuid();
            asset.Name = name1;
            _helper.AssetManagement.Assets.UpdateAndTransitionTo(asset, state);
            var reread = _helper.AssetManagement.Assets.Read(AssetExposers.Identifier.Equal(asset.Identifier)).Single();
            reread.State.Should().Be(state);
            reread.Name.Should().Be(name1);

            var name2 = "Renamed " + Guid.NewGuid();
            reread.Name = name2;
            _helper.AssetManagement.Assets.TransitionAndUpdate(reread, state);
            reread = _helper.AssetManagement.Assets.Read(AssetExposers.Identifier.Equal(asset.Identifier)).Single();
            reread.State.Should().Be(state);
            reread.Name.Should().Be(name2);
        }

        [TestMethod]
        public void UpdateAndTransitionTo_InvalidTransition_DoesNotPersistEdit()
        {
            var asset = CreateAsset(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed);
            var originalName = asset.Name;
            var newName = "Renamed " + Guid.NewGuid();
            asset.Name = newName;

            Action act = () => _helper.AssetManagement.Assets.UpdateAndTransitionTo(
                asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("State transition from Disposed to Available is not allowed.");
            var reread = _helper.AssetManagement.Assets.Read(
                AssetExposers.Identifier.Equal(asset.Identifier)).Single();
            reread.Name.Should().Be(originalName);
            reread.State.Should().Be(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed);
            asset.Name.Should().Be(newName);
            asset.GetChanges().Should().ContainSingle()
                .Which.FieldName.Should().Be(nameof(Asset.Name));
        }

        [TestMethod]
        public void UpdateAndTransitionTo_MissingInstallationRequirements_DoesNotPersistEdit()
        {
            var asset = CreateAsset(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available);
            var originalName = asset.Name;
            var newName = "Renamed " + Guid.NewGuid();
            asset.Name = newName;

            Action act = () => _helper.AssetManagement.Assets.UpdateAndTransitionTo(
                asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Installed);

            act.Should().Throw<Skyline.DataMiner.SDM.AssetManagement.Common.Exceptions.AssetTransitionValidationException>();
            var reread = _helper.AssetManagement.Assets.Read(
                AssetExposers.Identifier.Equal(asset.Identifier)).Single();
            reread.Name.Should().Be(originalName);
            reread.State.Should().Be(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available);
            asset.Name.Should().Be(newName);
        }

        [TestMethod]
        public void TransitionAndUpdate_TransitionFails_RestoresPendingEditWithoutPersistingIt()
        {
            var asset = CreateAsset(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed);
            var originalName = asset.Name;
            var newName = "Renamed " + Guid.NewGuid();
            asset.Name = newName;

            Action act = () => _helper.AssetManagement.Assets.TransitionAndUpdate(
                asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("State transition from Disposed to Available is not allowed.");
            asset.Name.Should().Be(newName);
            asset.GetChanges().Should().ContainSingle()
                .Which.FieldName.Should().Be(nameof(Asset.Name));
            var reread = _helper.AssetManagement.Assets.Read(
                AssetExposers.Identifier.Equal(asset.Identifier)).Single();
            reread.Name.Should().Be(originalName);
            reread.State.Should().Be(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed);
        }

        [TestMethod]
        public void TransitionAndUpdate_PersistsStateAndEdit()
        {
            var asset = CreateAsset(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available);
            var newName = "Renamed " + Guid.NewGuid();
            asset.Name = newName;

            _helper.AssetManagement.Assets.TransitionAndUpdate(
                asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable);

            var reread = _helper.AssetManagement.Assets.Read(
                AssetExposers.Identifier.Equal(asset.Identifier)).Single();
            reread.State.Should().Be(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable);
            reread.Name.Should().Be(newName);
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
