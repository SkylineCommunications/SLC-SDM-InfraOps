namespace SDM.FacilityManagement.Tests.Zones
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.FacilityManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    /// <summary>
    /// Filter tests for <see cref="ZoneDomRepository"/>.
    /// These tests verify that every <see cref="ZoneExposers"/> field used in a filter is
    /// correctly routed through <c>ZoneDomRepository.CreateFilter</c>.
    /// The latent production bug: all <c>ZoneProperties.*</c> switch cases were written
    /// prefixed (<c>"ZoneProperties.Name"</c>, <c>"ZoneProperties.ThermalType"</c>, …) but the
    /// <see cref="ZoneExposers.ZoneProperties"/> exposers supply the bare field name
    /// (<c>"Name"</c>, <c>"ThermalType"</c>, …), so every <c>ZoneProperties</c> filter fell
    /// through to <c>default: throw new NotImplementedException()</c>.
    /// </summary>
    [TestClass]
    public partial class ZoneDomRepositoryTests : BaseRepositoryTest
    {
        // ---------------------------------------------------------------------------
        // ZoneProperties.Name  ← primary regression test for the prefix bug
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by an exact zone name returns exactly one zone whose properties
        /// round-trip correctly. Primary regression test for the <c>"Name"</c> switch case.
        /// </summary>
        [TestMethod]
        public void ZoneDomRepository_ReadFilter_Name_Equals()
        {
            Helper.PopulateZones();

            string zoneName = "Cooling Plant";
            var nameFilter = ZoneExposers.ZoneProperties.Name.Equal(zoneName);
            var expected = DemoData.Zones.Single(z => z.Name == zoneName);

            var zonesRetrieved = Helper.Zones.Read(nameFilter);

            using (new AssertionScope())
            {
                zonesRetrieved.Should().NotBeNull();
                zonesRetrieved.Should().HaveCount(1);

                var zone = zonesRetrieved.First();
                zone.Name.Should().Be(expected.Name);
                zone.Plan.Should().Be(expected.Plan);
                zone.Description.Should().Be(expected.Description);
                zone.ThermalType.Should().Be(expected.ThermalType);
                zone.Width.Should().Be(expected.Width);
                zone.ZoneId.Should().Be(expected.ZoneId);
            }
        }

        /// <summary>
        /// Filtering by a substring of the zone name returns all zones whose names
        /// contain that substring (case-insensitive).
        /// </summary>
        [TestMethod]
        public void ZoneDomRepository_ReadFilter_Name_Contains()
        {
            Helper.PopulateZones();

            // "Hot Aisle A" and "Hot Aisle B" both contain "Hot Aisle".
            var nameFilter = ZoneExposers.ZoneProperties.Name.Contains("Hot Aisle", StringComparison.OrdinalIgnoreCase);
            var expected = DemoData.Zones
                .Where(z => z.Name.Contains("Hot Aisle", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var zonesRetrieved = Helper.Zones.Read(nameFilter);

            using (new AssertionScope())
            {
                zonesRetrieved.Should().NotBeNull();
                zonesRetrieved.Should().HaveCount(expected.Length);
                zonesRetrieved.Select(z => z.ZoneId).Should().BeEquivalentTo(expected.Select(z => z.ZoneId));
            }
        }

        // ---------------------------------------------------------------------------
        // ZoneProperties.ThermalType  (enum → int mapping)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by thermal type enum value returns only zones with that thermal type.
        /// Exercises the <c>"ThermalType"</c> switch case (enum-to-int cast).
        /// </summary>
        [TestMethod]
        public void ZoneDomRepository_ReadFilter_ThermalType_Equal()
        {
            Helper.PopulateZones();

            var thermalType = SlcFacility_Management.Enums.ThermalType.Cold;
            var thermalTypeFilter = ZoneExposers.ZoneProperties.ThermalType.Equal(thermalType);
            // "Cold Aisle A", "Cold Aisle B", "Cooling Plant".
            var expected = DemoData.Zones.Where(z => z.ThermalType == thermalType).ToArray();

            var zonesRetrieved = Helper.Zones.Read(thermalTypeFilter);

            using (new AssertionScope())
            {
                zonesRetrieved.Should().NotBeNull();
                zonesRetrieved.Should().HaveCount(expected.Length);
                zonesRetrieved.Select(z => z.ZoneId).Should().BeEquivalentTo(expected.Select(z => z.ZoneId));
            }
        }

        // ---------------------------------------------------------------------------
        // ZoneProperties.XPosition  (double)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by an exact X position returns all zones at that coordinate.
        /// Exercises the <c>"XPosition"</c> switch case (double cast).
        /// </summary>
        [TestMethod]
        public void ZoneDomRepository_ReadFilter_XPosition_Equal()
        {
            Helper.PopulateZones();

            double xPosition = 5.0;
            var xPositionFilter = ZoneExposers.ZoneProperties.XPosition.Equal(xPosition);
            // "Hot Aisle A" and "Hot Aisle B" are both at X = 5.
            var expected = DemoData.Zones.Where(z => z.XPosition == xPosition).ToArray();

            var zonesRetrieved = Helper.Zones.Read(xPositionFilter);

            using (new AssertionScope())
            {
                zonesRetrieved.Should().NotBeNull();
                zonesRetrieved.Should().HaveCount(expected.Length);
                zonesRetrieved.Select(z => z.ZoneId).Should().BeEquivalentTo(expected.Select(z => z.ZoneId));
            }
        }

        // ---------------------------------------------------------------------------
        // ZoneProperties.Width  (double)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by a minimum width (≥ threshold) returns all zones at or above it.
        /// Exercises the <c>"Width"</c> switch case (double cast).
        /// </summary>
        [TestMethod]
        public void ZoneDomRepository_ReadFilter_Width_GreaterThanOrEqual()
        {
            Helper.PopulateZones();

            double widthThreshold = 8.0;
            var widthFilter = ZoneExposers.ZoneProperties.Width.GreaterThanOrEqual(widthThreshold);
            // "Neutral Staging" (8.0) and "Cooling Plant" (10.0).
            var expected = DemoData.Zones.Where(z => z.Width >= widthThreshold).ToArray();

            var zonesRetrieved = Helper.Zones.Read(widthFilter);

            using (new AssertionScope())
            {
                zonesRetrieved.Should().NotBeNull();
                zonesRetrieved.Should().HaveCount(expected.Length);
                zonesRetrieved.Select(z => z.ZoneId).Should().BeEquivalentTo(expected.Select(z => z.ZoneId));
            }
        }

        // ---------------------------------------------------------------------------
        // ZoneProperties.ZoneId  (exposer + switch both prefixed → always worked; baseline)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by the business <c>ZoneId</c> field returns exactly one zone.
        /// The <c>ZoneId</c> exposer and switch case are both prefixed
        /// (<c>"ZoneProperties.ZoneId"</c>), so this path was unaffected by the fix.
        /// </summary>
        [TestMethod]
        public void ZoneDomRepository_ReadFilter_ZoneId_Equal()
        {
            Helper.PopulateZones();

            string zoneId = "ZN-003";
            var zoneIdFilter = ZoneExposers.ZoneProperties.ZoneId.Equal(zoneId);
            var expected = DemoData.Zones.Single(z => z.ZoneId == zoneId);

            var zonesRetrieved = Helper.Zones.Read(zoneIdFilter);

            using (new AssertionScope())
            {
                zonesRetrieved.Should().NotBeNull();
                zonesRetrieved.Should().HaveCount(1);
                zonesRetrieved.First().Name.Should().Be(expected.Name);
            }
        }

        // ---------------------------------------------------------------------------
        // Identifier (top-level exposer, not under ZoneProperties — always worked)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by the DOM object identifier returns exactly the one zone that owns
        /// that identifier. Baseline test confirming the fix did not regress it.
        /// </summary>
        [TestMethod]
        public void ZoneDomRepository_ReadFilter_Identifier_Equal()
        {
            Helper.PopulateZones();

            var zoneIdentifier = DemoData.Zones[0].Identifier;
            var filter = ZoneExposers.Identifier.Equal(zoneIdentifier);
            var expected = DemoData.Zones.Single(filter.getLambda());

            var zonesRetrieved = Helper.Zones.Read(filter);

            using (new AssertionScope())
            {
                zonesRetrieved.Should().NotBeNull();
                zonesRetrieved.Should().HaveCount(1);

                var zone = zonesRetrieved.First();
                zone.Identifier.Should().Be(expected.Identifier);
                zone.Name.Should().Be(expected.Name);
                zone.ZoneId.Should().Be(expected.ZoneId);
            }
        }

        // ---------------------------------------------------------------------------
        // Compound filter: ThermalType AND Width
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Combining a ThermalType filter with a Width lower bound using AND returns only
        /// zones that satisfy both conditions simultaneously.
        /// </summary>
        [TestMethod]
        public void ZoneDomRepository_ReadFilter_ThermalTypeAndWidth_Combined()
        {
            Helper.PopulateZones();

            var thermalType = SlcFacility_Management.Enums.ThermalType.Cold;

            // Cold zones: "Cold Aisle A" (5.0), "Cold Aisle B" (6.0), "Cooling Plant" (10.0).
            // Width ≥ 6.0 keeps "Cold Aisle B" and "Cooling Plant".
            var combinedFilter = ZoneExposers.ZoneProperties.ThermalType.Equal(thermalType)
                .AND(ZoneExposers.ZoneProperties.Width.GreaterThanOrEqual(6.0));

            var zonesRetrieved = Helper.Zones.Read(combinedFilter);
            var expected = DemoData.Zones
                .Where(z => z.ThermalType == thermalType && z.Width >= 6.0)
                .ToArray();

            using (new AssertionScope())
            {
                zonesRetrieved.Should().NotBeNull();
                zonesRetrieved.Should().HaveCount(expected.Length);
                zonesRetrieved.Select(z => z.ZoneId).Should().BeEquivalentTo(expected.Select(z => z.ZoneId));
            }
        }
    }
}
