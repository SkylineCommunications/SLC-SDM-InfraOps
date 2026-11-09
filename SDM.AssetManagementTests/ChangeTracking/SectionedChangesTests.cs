namespace SDM.AssetManagement.Tests.ChangeTracking
{
    using System.Linq;

    using FluentAssertions;

    using Skyline.DataMiner.SDM.AssetManagement.Models;

    [TestClass]
    public class SectionedChangesTests
    {
        [TestMethod]
        public void LocationAndDestinationLocation_SameFieldChanged_HaveDistinctQualifiedNames()
        {
            var asset = new Asset();
            _ = asset.Location.RackPosition;
            _ = asset.DestinationLocation.RackPosition;
            asset.ResetChangeTracking();

            asset.Location.RackPosition = 5;
            asset.DestinationLocation.RackPosition = 7;

            var changes = asset.GetChanges().Where(c => c.FieldName == nameof(AssetLocation.RackPosition)).ToList();

            changes.Should().HaveCount(2);
            changes.Select(c => c.Section).Should().BeEquivalentTo("Location", "DestinationLocation");
            changes.Select(c => c.QualifiedName).Should().OnlyHaveUniqueItems();
            changes.ToDictionary(c => c.QualifiedName).Should().HaveCount(2);
        }

        [TestMethod]
        public void RootField_HasNoSection()
        {
            var asset = new Asset();
            _ = asset.Name;
            asset.ResetChangeTracking();

            asset.Name = "Changed";

            var change = asset.GetChanges().Single(c => c.FieldName == nameof(Asset.Name));
            change.Section.Should().BeNull();
            change.QualifiedName.Should().Be(nameof(Asset.Name));
        }
    }
}
