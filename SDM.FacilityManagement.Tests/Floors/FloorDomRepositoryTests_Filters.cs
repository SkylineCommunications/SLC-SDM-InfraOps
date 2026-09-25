namespace SDM.FacilityManagement.Tests.Floors
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
    /// Filter tests for <see cref="FloorDomRepository"/>.
    /// These tests verify that every <see cref="FloorExposers"/> field used in a filter is
    /// correctly routed through <c>FloorDomRepository.CreateFilter</c>.
    /// The latent production bug: all <c>FloorProperties.*</c> switch cases were written as
    /// <c>"FloorProperties.Name"</c>, <c>"FloorProperties.Plan"</c>, etc., but the
    /// <see cref="FloorExposers.FloorProperties"/> exposers supply just the bare field name
    /// (<c>"Name"</c>, <c>"Plan"</c>, …), so every <c>FloorProperties</c> filter fell through
    /// to <c>default: throw new NotImplementedException()</c>. This surfaced as a
    /// <see cref="System.NotImplementedException"/> when resolving a Floor by name (e.g. during
    /// a room create that references its parent floor).
    /// </summary>
    [TestClass]
    public partial class FloorDomRepositoryTests : BaseRepositoryTest
    {
        // ---------------------------------------------------------------------------
        // FloorProperties.Name  ← primary regression test for the prefix bug
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by an exact floor name returns exactly one floor whose properties
        /// round-trip correctly. Primary regression test for the <c>"Name"</c> switch case,
        /// which previously never matched the bare <c>"Name"</c> supplied by the exposer.
        /// </summary>
        [TestMethod]
        public void FloorDomRepository_ReadFilter_Name_Equals()
        {
            Helper.PopulateFloors();

            string floorName = "Technical Floor";
            var nameFilter = FloorExposers.FloorProperties.Name.Equal(floorName);
            var expected = DemoData.Floors.Single(f => f.Name == floorName);

            var floorsRetrieved = Helper.Floors.Read(nameFilter);

            using (new AssertionScope())
            {
                floorsRetrieved.Should().NotBeNull();
                floorsRetrieved.Should().HaveCount(1);

                var floor = floorsRetrieved.First();
                floor.Name.Should().Be(expected.Name);
                floor.Plan.Should().Be(expected.Plan);
                floor.Description.Should().Be(expected.Description);
                floor.FloorId.Should().Be(expected.FloorId);
            }
        }

        /// <summary>
        /// Filtering by a substring of the floor name returns all floors whose names
        /// contain that substring (case-insensitive).
        /// </summary>
        [TestMethod]
        public void FloorDomRepository_ReadFilter_Name_Contains()
        {
            Helper.PopulateFloors();

            // "Basement Level" and "Rooftop Level" both contain "Level".
            var nameFilter = FloorExposers.FloorProperties.Name.Contains("Level", StringComparison.OrdinalIgnoreCase);
            var expected = DemoData.Floors
                .Where(f => f.Name.Contains("Level", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var floorsRetrieved = Helper.Floors.Read(nameFilter);

            using (new AssertionScope())
            {
                floorsRetrieved.Should().NotBeNull();
                floorsRetrieved.Should().HaveCount(expected.Length);
                floorsRetrieved.Select(f => f.FloorId).Should().BeEquivalentTo(expected.Select(f => f.FloorId));
            }
        }

        // ---------------------------------------------------------------------------
        // FloorProperties.Plan
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by plan returns exactly the floor with that plan.
        /// Exercises the <c>"Plan"</c> switch case (previously <c>"FloorProperties.Plan"</c>).
        /// </summary>
        [TestMethod]
        public void FloorDomRepository_ReadFilter_Plan_Equal()
        {
            Helper.PopulateFloors();

            string plan = "L1-Plan";
            var planFilter = FloorExposers.FloorProperties.Plan.Equal(plan);
            var expected = DemoData.Floors.Single(f => f.Plan == plan);

            var floorsRetrieved = Helper.Floors.Read(planFilter);

            using (new AssertionScope())
            {
                floorsRetrieved.Should().NotBeNull();
                floorsRetrieved.Should().HaveCount(1);
                floorsRetrieved.First().FloorId.Should().Be(expected.FloorId);
            }
        }

        // ---------------------------------------------------------------------------
        // FloorProperties.Description
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by a substring of the description returns all matching floors.
        /// Exercises the <c>"Description"</c> switch case (previously <c>"FloorProperties.Description"</c>).
        /// </summary>
        [TestMethod]
        public void FloorDomRepository_ReadFilter_Description_Contains()
        {
            Helper.PopulateFloors();

            // "HVAC and antenna equipment" and "Server and network equipment".
            var descriptionFilter = FloorExposers.FloorProperties.Description.Contains("equipment", StringComparison.OrdinalIgnoreCase);
            var expected = DemoData.Floors
                .Where(f => f.Description.Contains("equipment", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var floorsRetrieved = Helper.Floors.Read(descriptionFilter);

            using (new AssertionScope())
            {
                floorsRetrieved.Should().NotBeNull();
                floorsRetrieved.Should().HaveCount(expected.Length);
                floorsRetrieved.Select(f => f.FloorId).Should().BeEquivalentTo(expected.Select(f => f.FloorId));
            }
        }

        // ---------------------------------------------------------------------------
        // FloorProperties.FloorId  (exposer + switch both prefixed → always worked; baseline)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by the business <c>FloorId</c> field returns exactly one floor.
        /// The <c>FloorId</c> exposer and switch case are both prefixed
        /// (<c>"FloorProperties.FloorId"</c>), so this path was unaffected by the fix;
        /// this test documents the baseline and guards against regression.
        /// </summary>
        [TestMethod]
        public void FloorDomRepository_ReadFilter_FloorId_Equal()
        {
            Helper.PopulateFloors();

            string floorId = "FL-004";
            var floorIdFilter = FloorExposers.FloorProperties.FloorId.Equal(floorId);
            var expected = DemoData.Floors.Single(f => f.FloorId == floorId);

            var floorsRetrieved = Helper.Floors.Read(floorIdFilter);

            using (new AssertionScope())
            {
                floorsRetrieved.Should().NotBeNull();
                floorsRetrieved.Should().HaveCount(1);
                floorsRetrieved.First().Name.Should().Be(expected.Name);
            }
        }

        // ---------------------------------------------------------------------------
        // Identifier (top-level exposer, not under FloorProperties — always worked)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by the DOM object identifier returns exactly the one floor that owns
        /// that identifier. Baseline test confirming the fix did not regress it.
        /// </summary>
        [TestMethod]
        public void FloorDomRepository_ReadFilter_Identifier_Equal()
        {
            Helper.PopulateFloors();

            var floorIdentifier = DemoData.Floors[2].Identifier;
            var filter = FloorExposers.Identifier.Equal(floorIdentifier);
            var expected = DemoData.Floors.Single(filter.getLambda());

            var floorsRetrieved = Helper.Floors.Read(filter);

            using (new AssertionScope())
            {
                floorsRetrieved.Should().NotBeNull();
                floorsRetrieved.Should().HaveCount(1);

                var floor = floorsRetrieved.First();
                floor.Identifier.Should().Be(expected.Identifier);
                floor.Name.Should().Be(expected.Name);
                floor.FloorId.Should().Be(expected.FloorId);
            }
        }

        // ---------------------------------------------------------------------------
        // Compound filter: Description AND Plan
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Combining a Description Contains filter with a Plan filter using AND returns
        /// only floors that satisfy both conditions.
        /// </summary>
        [TestMethod]
        public void FloorDomRepository_ReadFilter_DescriptionContainsAndPlan_Combined()
        {
            Helper.PopulateFloors();

            // "Rooftop Level" (RF-Plan) and "Technical Floor" (TF-Plan) both contain
            // "equipment"; restricting to TF-Plan keeps only "Technical Floor".
            var combinedFilter = FloorExposers.FloorProperties.Description.Contains("equipment", StringComparison.OrdinalIgnoreCase)
                .AND(FloorExposers.FloorProperties.Plan.Equal("TF-Plan"));

            var floorsRetrieved = Helper.Floors.Read(combinedFilter);
            var expected = DemoData.Floors
                .Where(f => f.Description.Contains("equipment", StringComparison.OrdinalIgnoreCase) && f.Plan == "TF-Plan")
                .ToArray();

            using (new AssertionScope())
            {
                floorsRetrieved.Should().NotBeNull();
                floorsRetrieved.Should().HaveCount(expected.Length);
                floorsRetrieved.Select(f => f.FloorId).Should().BeEquivalentTo(expected.Select(f => f.FloorId));
            }
        }
    }
}
