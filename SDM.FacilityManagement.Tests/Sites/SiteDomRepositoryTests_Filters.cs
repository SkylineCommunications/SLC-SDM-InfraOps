namespace SDM.FacilityManagement.Tests.Sites
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.FacilityManagement.Tests.Setup;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    /// <summary>
    /// Filter tests for <see cref="SiteDomRepository"/>.
    /// These tests verify that every <see cref="SiteExposers"/> field used in a filter is
    /// correctly routed through <c>SiteDomRepository.CreateFilter</c>.
    /// The latent production bug: all <c>SiteProperties.*</c> switch cases were written
    /// prefixed (<c>"SiteProperties.Name"</c>, <c>"SiteProperties.City"</c>, …) but the
    /// <see cref="SiteExposers.SiteProperties"/> exposers supply the bare field name
    /// (<c>"Name"</c>, <c>"City"</c>, …), so every <c>SiteProperties</c> filter fell through
    /// to <c>default: throw new NotImplementedException()</c>.
    /// </summary>
    [TestClass]
    public partial class SiteDomRepositoryTests : BaseRepositoryTest
    {
        // ---------------------------------------------------------------------------
        // SiteProperties.Name  ← primary regression test for the prefix bug
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by an exact site name returns exactly one site whose properties
        /// round-trip correctly. Primary regression test for the <c>"Name"</c> switch case.
        /// </summary>
        [TestMethod]
        public void SiteDomRepository_ReadFilter_Name_Equals()
        {
            Helper.PopulateSites();

            string siteName = "Tokyo Campus";
            var nameFilter = SiteExposers.SiteProperties.Name.Equal(siteName);
            var expected = DemoData.Sites.Single(s => s.Name == siteName);

            var sitesRetrieved = Helper.Sites.Read(nameFilter);

            using (new AssertionScope())
            {
                sitesRetrieved.Should().NotBeNull();
                sitesRetrieved.Should().HaveCount(1);

                var site = sitesRetrieved.First();
                site.Name.Should().Be(expected.Name);
                site.City.Should().Be(expected.City);
                site.Country.Should().Be(expected.Country);
                site.SiteId.Should().Be(expected.SiteId);
            }
        }

        /// <summary>
        /// Filtering by a substring of the site name returns all sites whose names
        /// contain that substring (case-insensitive).
        /// </summary>
        [TestMethod]
        public void SiteDomRepository_ReadFilter_Name_Contains()
        {
            Helper.PopulateSites();

            // "New York Campus", "London Campus", "Tokyo Campus" all contain "Campus".
            var nameFilter = SiteExposers.SiteProperties.Name.Contains("Campus", StringComparison.OrdinalIgnoreCase);
            var expected = DemoData.Sites
                .Where(s => s.Name.Contains("Campus", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var sitesRetrieved = Helper.Sites.Read(nameFilter);

            using (new AssertionScope())
            {
                sitesRetrieved.Should().NotBeNull();
                sitesRetrieved.Should().HaveCount(expected.Length);
                sitesRetrieved.Select(s => s.SiteId).Should().BeEquivalentTo(expected.Select(s => s.SiteId));
            }
        }

        // ---------------------------------------------------------------------------
        // SiteProperties.City
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by city returns exactly the site in that city.
        /// Exercises the <c>"City"</c> switch case (previously <c>"SiteProperties.City"</c>).
        /// </summary>
        [TestMethod]
        public void SiteDomRepository_ReadFilter_City_Equal()
        {
            Helper.PopulateSites();

            string city = "Berlin";
            var cityFilter = SiteExposers.SiteProperties.City.Equal(city);
            var expected = DemoData.Sites.Single(s => s.City == city);

            var sitesRetrieved = Helper.Sites.Read(cityFilter);

            using (new AssertionScope())
            {
                sitesRetrieved.Should().NotBeNull();
                sitesRetrieved.Should().HaveCount(1);
                sitesRetrieved.First().SiteId.Should().Be(expected.SiteId);
            }
        }

        // ---------------------------------------------------------------------------
        // SiteProperties.Country
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by country returns all sites in that country.
        /// Exercises the <c>"Country"</c> switch case (previously <c>"SiteProperties.Country"</c>).
        /// </summary>
        [TestMethod]
        public void SiteDomRepository_ReadFilter_Country_Equal()
        {
            Helper.PopulateSites();

            string country = "USA";
            var countryFilter = SiteExposers.SiteProperties.Country.Equal(country);
            // "New York Campus" and "Austin Office".
            var expected = DemoData.Sites.Where(s => s.Country == country).ToArray();

            var sitesRetrieved = Helper.Sites.Read(countryFilter);

            using (new AssertionScope())
            {
                sitesRetrieved.Should().NotBeNull();
                sitesRetrieved.Should().HaveCount(expected.Length);
                sitesRetrieved.Select(s => s.SiteId).Should().BeEquivalentTo(expected.Select(s => s.SiteId));
            }
        }

        // ---------------------------------------------------------------------------
        // SiteProperties.Latitude  (double)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by a minimum latitude (≥ threshold) returns all sites at or above it.
        /// Exercises the <c>"Latitude"</c> switch case (double cast).
        /// </summary>
        [TestMethod]
        public void SiteDomRepository_ReadFilter_Latitude_GreaterThanOrEqual()
        {
            Helper.PopulateSites();

            double latitudeThreshold = 50.0;
            var latitudeFilter = SiteExposers.SiteProperties.Latitude.GreaterThanOrEqual(latitudeThreshold);
            // "London Campus" (51.5074) and "Berlin Lab" (52.5200).
            var expected = DemoData.Sites.Where(s => s.Latitude >= latitudeThreshold).ToArray();

            var sitesRetrieved = Helper.Sites.Read(latitudeFilter);

            using (new AssertionScope())
            {
                sitesRetrieved.Should().NotBeNull();
                sitesRetrieved.Should().HaveCount(expected.Length);
                sitesRetrieved.Select(s => s.SiteId).Should().BeEquivalentTo(expected.Select(s => s.SiteId));
            }
        }

        // ---------------------------------------------------------------------------
        // SiteProperties.SiteId  (exposer + switch both prefixed → always worked; baseline)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by the business <c>SiteId</c> field returns exactly one site.
        /// The <c>SiteId</c> exposer and switch case are both prefixed
        /// (<c>"SiteProperties.SiteId"</c>), so this path was unaffected by the fix.
        /// </summary>
        [TestMethod]
        public void SiteDomRepository_ReadFilter_SiteId_Equal()
        {
            Helper.PopulateSites();

            string siteId = "ST-004";
            var siteIdFilter = SiteExposers.SiteProperties.SiteId.Equal(siteId);
            var expected = DemoData.Sites.Single(s => s.SiteId == siteId);

            var sitesRetrieved = Helper.Sites.Read(siteIdFilter);

            using (new AssertionScope())
            {
                sitesRetrieved.Should().NotBeNull();
                sitesRetrieved.Should().HaveCount(1);
                sitesRetrieved.First().Name.Should().Be(expected.Name);
            }
        }

        // ---------------------------------------------------------------------------
        // Identifier (top-level exposer, not under SiteProperties — always worked)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by the DOM object identifier returns exactly the one site that owns
        /// that identifier. Baseline test confirming the fix did not regress it.
        /// </summary>
        [TestMethod]
        public void SiteDomRepository_ReadFilter_Identifier_Equal()
        {
            Helper.PopulateSites();

            var siteIdentifier = DemoData.Sites[0].Identifier;
            var filter = SiteExposers.Identifier.Equal(siteIdentifier);
            var expected = DemoData.Sites.Single(filter.getLambda());

            var sitesRetrieved = Helper.Sites.Read(filter);

            using (new AssertionScope())
            {
                sitesRetrieved.Should().NotBeNull();
                sitesRetrieved.Should().HaveCount(1);

                var site = sitesRetrieved.First();
                site.Identifier.Should().Be(expected.Identifier);
                site.Name.Should().Be(expected.Name);
                site.SiteId.Should().Be(expected.SiteId);
            }
        }

        // ---------------------------------------------------------------------------
        // Compound filter: Country AND Name
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Combining a Country filter with a Name Contains filter using AND returns only
        /// sites that satisfy both conditions simultaneously.
        /// </summary>
        [TestMethod]
        public void SiteDomRepository_ReadFilter_CountryAndName_Combined()
        {
            Helper.PopulateSites();

            // USA sites: "New York Campus" and "Austin Office". Restricting to names
            // containing "Campus" keeps only "New York Campus".
            var combinedFilter = SiteExposers.SiteProperties.Country.Equal("USA")
                .AND(SiteExposers.SiteProperties.Name.Contains("Campus", StringComparison.OrdinalIgnoreCase));

            var sitesRetrieved = Helper.Sites.Read(combinedFilter);
            var expected = DemoData.Sites
                .Where(s => s.Country == "USA" && s.Name.Contains("Campus", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            using (new AssertionScope())
            {
                sitesRetrieved.Should().NotBeNull();
                sitesRetrieved.Should().HaveCount(expected.Length);
                sitesRetrieved.Select(s => s.SiteId).Should().BeEquivalentTo(expected.Select(s => s.SiteId));
            }
        }
    }
}
