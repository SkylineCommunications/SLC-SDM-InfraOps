namespace SDM.FacilityManagement.Tests.Models
{
    using FluentAssertions;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    [TestClass]
    public class RackArrayTrackingTests
    {
        [TestMethod]
        public void ImageDetailsUntouched_ReportsNoChanges()
        {
            var rack = new Rack { Name = "Original" };
            _ = rack.ImageDetails;
            rack.ResetChangeTracking();

            rack.GetChanges().Should().BeEmpty();
        }
    }
}
