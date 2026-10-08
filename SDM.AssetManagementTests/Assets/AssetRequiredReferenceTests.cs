namespace SDM.AssetManagement.Tests.Assets
{
    using System;
    using System.Linq;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

    using Statuses = SharedMappers.DomIds.SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum;

    [TestClass]
    public class AssetRequiredReferenceTests : BaseRepositoryTest
    {
        private AssetClass assetClass = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.PopulateWithDemoData(DemoDataLayer.AssetClasses);
            assetClass = Helper.TestData.AssetClasses.First();
        }

        [DataTestMethod]
        [DataRow(Statuses.Installed)]
        [DataRow(Statuses.InService)]
        public void Create_InStateRequiringInstallationUser_WithoutUser_ShouldFail(Statuses state)
        {
            var asset = NewAsset("REQ-001");
            asset.State = state;

            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            act.Should().Throw<Exception>().WithMessage("*Installation User is required.*");
        }

        [TestMethod]
        public void Create_InNewState_WithoutInstallationUser_ShouldSucceed()
        {
            var asset = NewAsset("REQ-002");

            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            act.Should().NotThrow();
        }

        [TestMethod]
        public void Create_InstalledWithUserAndDate_ShouldNotReportMissingUser()
        {
            var asset = NewAsset("REQ-003");
            asset.State = Statuses.Installed;
            asset.InstallationUserId = new PnoObjectReference<Person>(Guid.NewGuid());
            asset.InstallationDate = DateTime.UtcNow;

            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            try
            {
                act();
            }
            catch (Exception ex)
            {
                ex.Message.Should().NotContain("Installation User is required.");
            }
        }

        private Asset NewAsset(string id)
        {
            return new Asset
            {
                AssetID = id,
                Name = $"Asset {id}",
                AssetClassId = new SdmObjectReference<AssetClass>(assetClass.Identifier),
            };
        }
    }
}
