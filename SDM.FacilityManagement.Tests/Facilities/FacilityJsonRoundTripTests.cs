namespace SDM.FacilityManagement.Tests.Facilities
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.Converters;

    [TestClass]
    public class FacilityJsonRoundTripTests
    {
        [TestMethod]
        public void Facility_RoundTrip_PreservesAllProperties()
        {
            var facility = new Facility
            {
                Identifier = Guid.NewGuid().ToString(),
                FacilityId = "FAC-1",
                Name = "Facility",
                Description = "Main data center",
                FacilityType = SlcFacility_Management.Enums.FacilityTypeEnum.Building,
                Address = "Main Street 1",
                City = "Ghent",
                ZipCode = "9000",
                Country = "Belgium",
                Latitude = 51.0543,
                Longitude = 3.7174,
            };
            facility.SiteFk.Site = new SdmObjectReference<Site>(Guid.NewGuid().ToString());

            string json = InfraOpsJsonConvert.SerializeObject(facility);
            var restored = InfraOpsJsonConvert.DeserializeObject<Facility>(json);

            restored.Identifier.Should().Be(facility.Identifier);
            restored.FacilityId.Should().Be("FAC-1");
            restored.Name.Should().Be("Facility");
            restored.Description.Should().Be("Main data center");
            restored.FacilityType.Should().Be(SlcFacility_Management.Enums.FacilityTypeEnum.Building);
            restored.Address.Should().Be("Main Street 1");
            restored.City.Should().Be("Ghent");
            restored.ZipCode.Should().Be("9000");
            restored.Country.Should().Be("Belgium");
            restored.Latitude.Should().Be(51.0543);
            restored.Longitude.Should().Be(3.7174);
            restored.SiteFk.Site.Should().Be(facility.SiteFk.Site);
            restored.Should().Be(facility);
            InfraOpsJsonConvert.SerializeObject(restored).Should().Be(json);
        }

        [TestMethod]
        public void Facility_AuditFields_ExportedButNotImported()
        {
            var facility = new Facility
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Facility",
                CreatedAt = new DateTime(2023, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                CreatedBy = "creator",
                LastModified = new DateTime(2024, 6, 7, 8, 9, 10, DateTimeKind.Utc),
                LastModifiedBy = "modifier",
                State = SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum.Active,
            };

            string json = InfraOpsJsonConvert.SerializeObject(facility);
            var restored = InfraOpsJsonConvert.DeserializeObject<Facility>(json);

            json.Should().Contain("\"CreatedBy\":\"creator\"");
            json.Should().Contain("\"LastModifiedBy\":\"modifier\"");
            restored.Name.Should().Be("Facility");
            restored.CreatedAt.Should().Be(default(DateTime));
            restored.CreatedBy.Should().BeNull();
            restored.LastModified.Should().Be(default(DateTime));
            restored.LastModifiedBy.Should().BeNull();
            restored.State.Should().Be(default(SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum));
        }

        [TestMethod]
        public void Facility_Serialize_WritesReferencesAsStringsAndEnumsAsNames()
        {
            var siteId = Guid.NewGuid().ToString();
            var facility = new Facility
            {
                FacilityType = SlcFacility_Management.Enums.FacilityTypeEnum.Container,
            };
            facility.SiteFk.Site = new SdmObjectReference<Site>(siteId);

            string json = InfraOpsJsonConvert.SerializeObject(facility);

            json.Should().Contain($"\"Site\":\"{siteId}\"");
            json.Should().Contain("\"FacilityType\":\"Container\"");
        }

        [TestMethod]
        public void Facility_Serialize_EmptyReferencesAreNull()
        {
            string json = InfraOpsJsonConvert.SerializeObject(new Facility());

            json.Should().Contain("\"Site\":null");
        }

        [TestMethod]
        public void Facility_Deserialize_NullAndEmptyReferencesBecomeDefault()
        {
            var fromNull = InfraOpsJsonConvert.DeserializeObject<Facility>("{\"SiteFk\":{\"Site\":null}}");
            var fromEmpty = InfraOpsJsonConvert.DeserializeObject<Facility>("{\"SiteFk\":{\"Site\":\"\"}}");

            fromNull.SiteFk.Site.Identifier.Should().BeNullOrEmpty();
            fromEmpty.SiteFk.Site.Identifier.Should().BeNullOrEmpty();
        }
    }
}
