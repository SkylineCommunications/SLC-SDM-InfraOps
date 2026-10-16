namespace SDM.AssetManagement.Tests.ChangeTracking
{
    using System.Linq;

    using FluentAssertions;

    using SharedMappers.DomIds;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    [TestClass]
    public class ArrayTrackingUnchangedTests
    {
        [TestMethod]
        public void Asset_ArrayFieldsUntouched_ReportsNoChanges()
        {
            var asset = new Asset { Name = "Original" };
            _ = asset.OperationalFlags;
            _ = asset.Holders;
            _ = asset.ElementLinks;
            _ = asset.Attachments;
            asset.ResetChangeTracking();

            asset.GetChanges().Should().BeEmpty();
            asset.Changed.Should().BeFalse();
        }

        [TestMethod]
        public void AssetClass_ArrayFieldsUntouched_ReportsNoChanges()
        {
            var assetClass = new AssetClass { Name = "Original" };
            _ = assetClass.DataPorts;
            _ = assetClass.PowerPorts;
            _ = assetClass.Holders;
            _ = assetClass.Attachments;
            assetClass.ResetChangeTracking();

            assetClass.GetChanges().Should().BeEmpty();
        }

        [TestMethod]
        public void InfraopsReservation_ReservedPositionsUntouched_ReportsNoChanges()
        {
            var reservation = new InfraopsReservation();
            _ = reservation.ReservedPositions;
            reservation.ResetChangeTracking();

            reservation.GetChanges().Should().BeEmpty();
        }

        [TestMethod]
        public void DeviceType_TagsUntouched_ReportsNoChanges()
        {
            var deviceType = new DeviceType();
            _ = deviceType.TagsInfo.Tags;
            deviceType.ResetChangeTracking();

            deviceType.GetChanges().Should().BeEmpty();
        }

        [TestMethod]
        public void PortType_CategoriesAndCablesUntouched_ReportsNoChanges()
        {
            var portType = new PortType();
            _ = portType.CategoryLinks.Categories;
            _ = portType.CableFKs.CableTypeFks;
            portType.ResetChangeTracking();

            portType.GetChanges().Should().BeEmpty();
        }

        [TestMethod]
        public void TagsInfo_TagsUntouched_ReportsNoChanges()
        {
            var tagsInfo = new TagsInfo();
            _ = tagsInfo.Tags;
            tagsInfo.ResetChangeTracking();

            tagsInfo.GetChanges().Should().BeEmpty();
            tagsInfo.Changed.Should().BeFalse();
        }

        [TestMethod]
        public void CategoryRelation_CategoriesUntouched_ReportsNoChanges()
        {
            var relation = new CategoryRelation();
            _ = relation.Categories;
            relation.ResetChangeTracking();

            relation.GetChanges().Should().BeEmpty();
            relation.Changed.Should().BeFalse();
        }

        [TestMethod]
        public void CableRelation_CableTypesUntouched_ReportsNoChanges()
        {
            var relation = new CableRelation();
            _ = relation.CableTypeFks;
            relation.ResetChangeTracking();

            relation.GetChanges().Should().BeEmpty();
            relation.Changed.Should().BeFalse();
        }

        [TestMethod]
        public void Asset_OperationalFlagsAdded_IsReportedOnce()
        {
            var asset = new Asset { Name = "Original" };
            asset.ResetChangeTracking();
            asset.OperationalFlags.Add(SlcAsset_Management.Enums.Operationalflagsenum.Faulty);

            asset.GetChanges().Select(change => change.FieldName)
                .Should().ContainSingle(name => name == nameof(Asset.OperationalFlags));
        }
    }
}
