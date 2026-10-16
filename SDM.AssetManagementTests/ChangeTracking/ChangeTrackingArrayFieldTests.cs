namespace SDM.AssetManagement.Tests.ChangeTracking
{
    using System.Linq;

    using FluentAssertions;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    [TestClass]
    public class ChangeTrackingArrayFieldTests
    {
        [TestMethod]
        public void Changed_ValueTypeList_Untouched_IsFalse()
        {
            var field = new ChangeTrackingArrayField<int>(new[] { 1, 2 });

            field.Changed.Should().BeFalse();
        }

        [TestMethod]
        public void Changed_ReferenceTypeList_Untouched_IsFalse()
        {
            var field = new ChangeTrackingArrayField<string>(new[] { "a", "b" });

            field.Changed.Should().BeFalse();
        }

        [TestMethod]
        public void Changed_ValueTypeList_ItemReplaced_IsTrue()
        {
            var field = new ChangeTrackingArrayField<int>(new[] { 1, 2 });
            field.Value[1] = 3;

            field.Changed.Should().BeTrue();
        }

        [TestMethod]
        public void Asset_OperationalFlags_Untouched_IsNotReportedAsChange()
        {
            var asset = new Asset { Name = "Original" };
            _ = asset.OperationalFlags;
            asset.ResetChangeTracking();

            asset.GetChanges().Select(change => change.FieldName)
                .Should().NotContain(nameof(Asset.OperationalFlags));
        }
    }
}
