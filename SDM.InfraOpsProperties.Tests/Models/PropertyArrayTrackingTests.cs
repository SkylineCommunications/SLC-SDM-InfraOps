namespace SDM.InfraOpsProperties.Tests.Models
{
    using FluentAssertions;

    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

    [TestClass]
    public class PropertyArrayTrackingTests
    {
        [TestMethod]
        public void Property_DiscreetsUntouched_ReportsNoChanges()
        {
            var property = new Property { Name = "Original" };
            _ = property.Discreets;
            property.ResetChangeTracking();

            property.GetChanges().Should().BeEmpty();
        }

        [TestMethod]
        public void PropertyValues_ValuesUntouched_ReportsNoChanges()
        {
            var values = new PropertyValues();
            _ = values.Values;
            values.ResetChangeTracking();

            values.GetChanges().Should().BeEmpty();
        }
    }
}
