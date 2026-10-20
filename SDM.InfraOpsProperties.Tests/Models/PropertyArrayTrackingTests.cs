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

        [TestMethod]
        public void Property_Discreets_InPlaceItemMutation_IsReportedAndReverted()
        {
            var property = new Property
            {
                Name = "Original",
                Discreets = new System.Collections.Generic.List<PropertyOption>
                {
                    new PropertyOption { Option = "OptA" },
                },
            };
            property.ResetChangeTracking();
            property.IsNewInternal = false;

            property.Discreets[0].Option = "OptB";

            property.Changed.Should().BeTrue();
            var change = property.GetChanges().Should().ContainSingle(c => c.FieldName == nameof(Property.Discreets)).Subject;
            ((System.Collections.Generic.List<PropertyOption>)change.OldValue)[0].Option.Should().Be("OptA");
            ((System.Collections.Generic.List<PropertyOption>)change.NewValue)[0].Option.Should().Be("OptB");

            Skyline.DataMiner.Utils.InfraOps.Common.Fields.SdmChangeTracking.RevertFieldChanges(property);

            property.Changed.Should().BeFalse();
            property.Discreets[0].Option.Should().Be("OptA");
        }
    }
}
