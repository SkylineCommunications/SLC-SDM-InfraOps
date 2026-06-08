namespace SDM.FacilityManagement.Tests.Racks
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
    /// Filter tests for <see cref="RackDomRepository"/>.
    /// These tests verify that every <see cref="RackExposers"/> field used in a filter is
    /// correctly routed through <c>RackDomRepository.CreateFilter</c> — in particular:
    /// <list type="bullet">
    ///   <item><see cref="RackExposers.RackProperties.Name"/> — was broken: switch used <c>".Label"</c> for Label and the Name path was confirmed working.</item>
    ///   <item><see cref="RackExposers.RackProperties.Label"/> — was broken: switch used <c>".Label"</c> (leading dot) instead of <c>"Label"</c>.</item>
    /// </list>
    /// </summary>
    [TestClass]
    public partial class RackDomRepositoryTests : BaseRepositoryTest
    {
        // ---------------------------------------------------------------------------
        // RackProperties.Name
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by an exact rack name returns exactly one rack whose properties
        /// round-trip correctly through the DOM storage layer.
        /// This is the primary regression test for the <c>RackProperties.Name</c> switch case.
        /// </summary>
        [TestMethod]
        public void RackDomRepository_ReadFilter_Name_Equals()
        {
                Helper.PopulateRacks();

            string rackName = "Core Switch Enclosure";
            var nameFilter = RackExposers.RackProperties.Name.Equal(rackName);
            var expected = DemoData.Racks.Single(r => r.Name == rackName);

            var racksRetrieved = Helper.Racks.Read(nameFilter);

            using (new AssertionScope())
            {
                racksRetrieved.Should().NotBeNull();
                racksRetrieved.Should().HaveCount(1);

                var rack = racksRetrieved.First();
                rack.Name.Should().Be(expected.Name);
                rack.Model.Should().Be(expected.Model);
                rack.Position.Should().Be(expected.Position);
                rack.RackId.Should().Be(expected.RackId);
                rack.Bookable.Should().Be(expected.Bookable);
                rack.CoolingFlow.Should().Be(expected.CoolingFlow);
                rack.Label.Should().Be(expected.Label);
                rack.Description.Should().Be(expected.Description);
            }
        }

        /// <summary>
        /// Filtering by a substring of the rack name returns all racks whose names
        /// contain that substring (case-insensitive).
        /// </summary>
        [TestMethod]
        public void RackDomRepository_ReadFilter_Name_Contains()
        {
                Helper.PopulateRacks();

            // "Alpha Server Rack" and "Alpha Patch Panel" both contain "Alpha".
            var nameFilter = RackExposers.RackProperties.Name.Contains("Alpha", StringComparison.OrdinalIgnoreCase);
            var expected = DemoData.Racks
                .Where(r => r.Name.Contains("Alpha", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var racksRetrieved = Helper.Racks.Read(nameFilter);

            using (new AssertionScope())
            {
                racksRetrieved.Should().NotBeNull();
                racksRetrieved.Should().HaveCount(expected.Length);
                racksRetrieved.Select(r => r.Name).Should().BeEquivalentTo(expected.Select(r => r.Name));
            }
        }

        // ---------------------------------------------------------------------------
        // Identifier (top-level exposer, not under RackProperties)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by the DOM object identifier returns exactly the one rack that
        /// owns that identifier and all its properties are preserved.
        /// </summary>
        [TestMethod]
        public void RackDomRepository_ReadFilter_Identifier_Equal()
        {
                Helper.PopulateRacks();

            var rackIdentifier = DemoData.Racks[1].Identifier;
            var filter = RackExposers.Identifier.Equal(rackIdentifier);
            var expected = DemoData.Racks.Single(filter.getLambda());

            var racksRetrieved = Helper.Racks.Read(filter);

            using (new AssertionScope())
            {
                racksRetrieved.Should().NotBeNull();
                racksRetrieved.Should().HaveCount(1);

                var rack = racksRetrieved.First();
                rack.Identifier.Should().Be(expected.Identifier);
                rack.Name.Should().Be(expected.Name);
                rack.Model.Should().Be(expected.Model);
                rack.Position.Should().Be(expected.Position);
                rack.RackId.Should().Be(expected.RackId);
            }
        }

        // ---------------------------------------------------------------------------
        // RackProperties.Model
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by model returns all racks of that model.
        /// Exercises the <c>"Model"</c> switch case in <c>CreateFilter</c>.
        /// </summary>
        [TestMethod]
        public void RackDomRepository_ReadFilter_Model_Equal()
        {
                Helper.PopulateRacks();

            string model = "Schneider Electric";
            var modelFilter = RackExposers.RackProperties.Model.Equal(model);
            var expected = DemoData.Racks.Where(r => r.Model == model).ToArray();

            var racksRetrieved = Helper.Racks.Read(modelFilter);

            using (new AssertionScope())
            {
                racksRetrieved.Should().NotBeNull();
                // "Core Switch Enclosure" and "Edge Compute Rack" are both Schneider Electric.
                racksRetrieved.Should().HaveCount(expected.Length);
                racksRetrieved.Select(r => r.RackId).Should().BeEquivalentTo(expected.Select(r => r.RackId));
                racksRetrieved.Select(r => r.Name).Should().BeEquivalentTo(expected.Select(r => r.Name));
            }
        }

        // ---------------------------------------------------------------------------
        // RackProperties.Position  (enum → int mapping)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by rack position enum value returns only racks with that position.
        /// Exercises the <c>"Position"</c> switch case (enum-to-int cast) in <c>CreateFilter</c>.
        /// </summary>
        [TestMethod]
        public void RackDomRepository_ReadFilter_Position_Equal()
        {
                Helper.PopulateRacks();

            var position = SlcFacility_Management.Enums.RackpositionenumEnum.Bottom;
            var positionFilter = RackExposers.RackProperties.Position.Equal(position);
            // Alpha Server Rack, Core Switch Enclosure, Alpha Patch Panel, Storage Array Cabinet.
            var expected = DemoData.Racks.Where(r => r.Position == position).ToArray();

            var racksRetrieved = Helper.Racks.Read(positionFilter);

            using (new AssertionScope())
            {
                racksRetrieved.Should().NotBeNull();
                racksRetrieved.Should().HaveCount(expected.Length);
                racksRetrieved.Select(r => r.RackId).Should().BeEquivalentTo(expected.Select(r => r.RackId));
            }
        }

        // ---------------------------------------------------------------------------
        // RackProperties.Label  ← regression for the ".Label" leading-dot bug
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by label returns exactly the rack with that label.
        /// This is the direct regression test for the <c>".Label"</c> bug in
        /// <c>RackDomRepository.CreateFilter</c> — the switch case previously used
        /// <c>".Label"</c> (spurious leading dot) instead of <c>"Label"</c>, causing
        /// every label filter to fall through to the <c>default: throw new NotImplementedException()</c> arm.
        /// </summary>
        [TestMethod]
        public void RackDomRepository_ReadFilter_Label_Equal()
        {
                Helper.PopulateRacks();

            string label = "A01";
            var labelFilter = RackExposers.RackProperties.Label.Equal(label);
            var expected = DemoData.Racks.Single(r => r.Label == label);

            var racksRetrieved = Helper.Racks.Read(labelFilter);

            using (new AssertionScope())
            {
                racksRetrieved.Should().NotBeNull();
                racksRetrieved.Should().HaveCount(1);

                var rack = racksRetrieved.First();
                rack.Label.Should().Be(expected.Label);
                rack.Name.Should().Be(expected.Name);
                rack.RackId.Should().Be(expected.RackId);
            }
        }

        // ---------------------------------------------------------------------------
        // Compound filter: Model AND Position
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Combining a model filter with a position filter using AND returns only the
        /// subset of racks that satisfy both conditions simultaneously.
        /// </summary>
        [TestMethod]
        public void RackDomRepository_ReadFilter_ModelAndPosition_Equal()
        {
                Helper.PopulateRacks();

            string model = "APC NetShelter SX";
            var position = SlcFacility_Management.Enums.RackpositionenumEnum.Top;

            var combinedFilter = RackExposers.RackProperties.Model.Equal(model)
                .AND(RackExposers.RackProperties.Position.Equal(position));

            // Only "Beta Network Rack" matches both.
            var racksRetrieved = Helper.Racks.Read(combinedFilter);
            var expected = DemoData.Racks
                .Where(r => r.Model == model && r.Position == position)
                .ToArray();

            using (new AssertionScope())
            {
                racksRetrieved.Should().NotBeNull();
                racksRetrieved.Should().HaveCount(expected.Length);
                racksRetrieved.Select(r => r.RackId).Should().BeEquivalentTo(expected.Select(r => r.RackId));
            }
        }
    }
}
