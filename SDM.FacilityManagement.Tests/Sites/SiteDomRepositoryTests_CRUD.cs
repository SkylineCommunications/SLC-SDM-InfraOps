namespace SDM.FacilityManagement.Tests.Sites
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    public partial class SiteDomRepositoryTests
    {
        [TestMethod]
        public void SiteDomRepository_Create_ReadBack_PersistsSiteIdLatitudeAndLongitude()
        {
            var site = new Site { Identifier = Guid.NewGuid().ToString(), Name = "Site 1", SiteId = "ST-1", Latitude = 10.5, Longitude = 20.25 };

            Helper.Sites.Create(site);

            var reloaded = Helper.Sites.Read(SiteExposers.Identifier.Equal(site.Identifier)).SingleOrDefault();

            using (new AssertionScope())
            {
                reloaded.Should().NotBeNull();
                reloaded!.SiteId.Should().Be("ST-1");
                reloaded.Latitude.Should().Be(10.5);
                reloaded.Longitude.Should().Be(20.25);
            }
        }
    }
}
