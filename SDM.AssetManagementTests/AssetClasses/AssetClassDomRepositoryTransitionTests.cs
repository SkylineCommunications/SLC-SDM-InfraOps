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
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    [TestClass]
    public class AssetClassDomRepositoryTransitionTests
    {
        private ITestApiHelper _helper = null!;

        [TestInitialize]
        public void Initialize()
        {
            _helper = RepositoryInitialize.Initialize();
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
                Manufacturer = SDM.AssetManagement.Tests.Setup.PeopleApiMock.NewManufacturer(),
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
        public void TransitionTo_InvalidPath_ShouldThrow(
            SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum from,
            SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum to)
        {
            var assetClass = CreateAssetClass(from);
            Action act = () => _helper.AssetManagement.AssetClasses.TransitionTo(assetClass, to);

            act.Should().Throw<InvalidOperationException>()
                .WithMessage($"State transition from {from} to {to} is not allowed.");
        }

        [TestMethod]
        public void ValidateTransition_NotAllowed_ShouldReturnInvalidWithoutThrowing()
        {
            var assetClass = CreateAssetClass(SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active);

            var result = _helper.AssetManagement.AssetClassValidator.ValidateTransition(
                assetClass,
                SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Draft);

            result.IsValid.Should().BeFalse();
            result.GetCombinedFailureReasons(Environment.NewLine).Should().Contain("is not allowed");
        }

        [TestMethod]
        public void ValidateTransition_Allowed_ShouldReturnValid()
        {
            var assetClass = CreateAssetClass(SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Draft);

            var result = _helper.AssetManagement.AssetClassValidator.ValidateTransition(
                assetClass,
                SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active);

            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void TransitionTo_WhenTransitionRuleFails_ShouldNotTransition()
        {
            var assetClass = CreateAssetClass(SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Draft);
            var repository = new AssetClassDomRepository(_helper.Connection);
            repository.TransitionValidator =
                new BlockingTransitionValidator<AssetClass, SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum>(a => a.State);

            Action act = () => repository.TransitionTo(
                assetClass,
                SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active);

            act.Should().Throw<ValidationException>().WithMessage($"*{BlockingTransitionValidator<AssetClass, SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum>.Reason}*");
            var reread = _helper.AssetManagement.AssetClasses.Read(
                AssetClassExposers.Identifier.Equal(assetClass.Identifier)).Single();
            reread.State.Should().Be(SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Draft);
        }

        [TestMethod]
        public void TransitionTo_Deprecated_WhenNonDisposedAssetUsesClass_ShouldThrowAndNotTransition()
        {
            var assetClass = CreateAssetClass(SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active);
            CreateAssetOfClass(assetClass, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available);

            Action act = () => _helper.AssetManagement.AssetClasses.TransitionTo(
                assetClass,
                SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Deprecated);

            act.Should().Throw<ValidationException>().WithMessage("*cannot be deprecated*");
            var reread = _helper.AssetManagement.AssetClasses.Read(
                AssetClassExposers.Identifier.Equal(assetClass.Identifier)).Single();
            reread.State.Should().Be(SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active);
        }

        [TestMethod]
        public void TransitionTo_Deprecated_WhenOnlyDisposedAssetsUseClass_ShouldPersist()
        {
            var assetClass = CreateAssetClass(SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active);
            CreateAssetOfClass(assetClass, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Disposed);

            var result = _helper.AssetManagement.AssetClasses.TransitionTo(
                assetClass,
                SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Deprecated);

            result.State.Should().Be(SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Deprecated);
        }

        private void CreateAssetOfClass(AssetClass assetClass, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum state)
        {
            _helper.AssetManagement.Assets.Create(new Asset
            {
                Name = "Class user " + Guid.NewGuid(),
                AssetID = Guid.NewGuid().ToString(),
                AssetClassId = new SdmObjectReference<AssetClass>(assetClass.Identifier),
                State = state,
            });
        }
    }
}
