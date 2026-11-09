namespace SDM.FacilityManagement.Tests.Sites
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.Converters;

    [TestClass]
    public class SiteJsonRoundTripTests
    {
        [TestMethod]
        public void Site_RoundTrip_PreservesAllProperties()
        {
            var site = new Site
            {
                Identifier = Guid.NewGuid().ToString(),
                SiteId = "SITE-1",
                Name = "Site",
                Description = "Campus",
                Address = "Main Street 1",
                City = "Ghent",
                ZipCode = "9000",
                Country = "Belgium",
                Latitude = 51.0543,
                Longitude = 3.7174,
            };

            string json = InfraOpsJsonConvert.SerializeObject(site);
            var restored = InfraOpsJsonConvert.DeserializeObject<Site>(json);

            restored.Identifier.Should().Be(site.Identifier);
            restored.SiteId.Should().Be("SITE-1");
            restored.Name.Should().Be("Site");
            restored.Description.Should().Be("Campus");
            restored.Address.Should().Be("Main Street 1");
            restored.City.Should().Be("Ghent");
            restored.ZipCode.Should().Be("9000");
            restored.Country.Should().Be("Belgium");
            restored.Latitude.Should().Be(51.0543);
            restored.Longitude.Should().Be(3.7174);
            restored.Should().Be(site);
            InfraOpsJsonConvert.SerializeObject(restored).Should().Be(json);
        }

        [TestMethod]
        public void Site_AuditFields_ExportedButNotImported()
        {
            var site = new Site
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Site",
                CreatedAt = new DateTime(2023, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                CreatedBy = "creator",
                LastModified = new DateTime(2024, 6, 7, 8, 9, 10, DateTimeKind.Utc),
                LastModifiedBy = "modifier",
                State = SlcFacility_Management.Behaviors.Site_Behaviour.StatusesEnum.Active,
            };

            string json = InfraOpsJsonConvert.SerializeObject(site);
            var restored = InfraOpsJsonConvert.DeserializeObject<Site>(json);

            json.Should().Contain("\"CreatedBy\":\"creator\"");
            json.Should().Contain("\"LastModifiedBy\":\"modifier\"");
            restored.Name.Should().Be("Site");
            restored.CreatedAt.Should().Be(default(DateTime));
            restored.CreatedBy.Should().BeNull();
            restored.LastModified.Should().Be(default(DateTime));
            restored.LastModifiedBy.Should().BeNull();
            restored.State.Should().Be(default(SlcFacility_Management.Behaviors.Site_Behaviour.StatusesEnum));
        }
    }
}
