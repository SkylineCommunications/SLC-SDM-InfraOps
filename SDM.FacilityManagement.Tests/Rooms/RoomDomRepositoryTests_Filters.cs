namespace SDM.FacilityManagement.Tests.Rooms
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
    /// Filter tests for <see cref="RoomDomRepository"/>.
    /// These tests verify that every <see cref="RoomExposers"/> field used in a filter is
    /// correctly routed through <c>RoomDomRepository.CreateFilter</c>.
    /// The latent production bug: all <c>RoomProperties.*</c> switch cases were written as
    /// <c>"RoomProperties.Name"</c>, <c>"RoomProperties.Plan"</c>, etc., but
    /// <see cref="RoomExposers.RoomProperties"/> exposers supply just the bare field name
    /// (<c>"Name"</c>, <c>"Plan"</c>, …), so every <c>RoomProperties</c> filter fell through
    /// to <c>default: throw new NotImplementedException()</c>.
    /// </summary>
    [TestClass]
    public partial class RoomDomRepositoryTests : BaseRepositoryTest
    {
        // ---------------------------------------------------------------------------
        // RoomProperties.Name  ← primary regression test for the prefix bug
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by an exact room name returns exactly one room whose properties
        /// round-trip correctly through the DOM storage layer.
        /// This is the primary regression test for the <c>"RoomProperties.Name"</c> bug:
        /// the switch case previously never matched <c>"Name"</c> (the value delivered by
        /// <see cref="RoomExposers.RoomProperties.Name"/>), so this filter always threw
        /// <see cref="System.NotImplementedException"/>.
        /// </summary>
        [TestMethod]
        public void RoomDomRepository_ReadFilter_Name_Equals()
        {
                Helper.PopulateRooms();

            string roomName = "Network Operations Center";
            var nameFilter = RoomExposers.RoomProperties.Name.Equal(roomName);
            var expected = DemoData.Rooms.Single(r => r.Name == roomName);

            var roomsRetrieved = Helper.Rooms.Read(nameFilter);

            using (new AssertionScope())
            {
                roomsRetrieved.Should().NotBeNull();
                roomsRetrieved.Should().HaveCount(1);

                var room = roomsRetrieved.First();
                room.Name.Should().Be(expected.Name);
                room.Plan.Should().Be(expected.Plan);
                room.Description.Should().Be(expected.Description);
                room.Width.Should().Be(expected.Width);
                room.Depth.Should().Be(expected.Depth);
                room.RoomId.Should().Be(expected.RoomId);
            }
        }

        /// <summary>
        /// Filtering by a substring of the room name returns all rooms whose names
        /// contain that substring (case-insensitive).
        /// Exercises the same <c>"Name"</c> switch case with a <c>Contains</c> comparer.
        /// </summary>
        [TestMethod]
        public void RoomDomRepository_ReadFilter_Name_Contains()
        {
                Helper.PopulateRooms();

            // "Main Server Room" and "Main Storage Area" both contain "Main".
            var nameFilter = RoomExposers.RoomProperties.Name.Contains("Main", StringComparison.OrdinalIgnoreCase);
            var expected = DemoData.Rooms
                .Where(r => r.Name.Contains("Main", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var roomsRetrieved = Helper.Rooms.Read(nameFilter);

            using (new AssertionScope())
            {
                roomsRetrieved.Should().NotBeNull();
                roomsRetrieved.Should().HaveCount(expected.Length);
                roomsRetrieved.Should().BeEquivalentTo(expected);
            }
        }

        // ---------------------------------------------------------------------------
        // Identifier (top-level exposer, not under RoomProperties — always worked)
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by the DOM object identifier returns exactly the one room that owns
        /// that identifier, and all its properties are preserved correctly.
        /// The <c>"Identifier"</c> switch case was correct before the fix; this test
        /// documents the baseline and confirms the fix did not regress it.
        /// </summary>
        [TestMethod]
        public void RoomDomRepository_ReadFilter_Identifier_Equal()
        {
                Helper.PopulateRooms();

            var roomIdentifier = DemoData.Rooms[3].Identifier;
            var filter = RoomExposers.Identifier.Equal(roomIdentifier);
            var expected = DemoData.Rooms.Single(filter.getLambda());

            var roomsRetrieved = Helper.Rooms.Read(filter);

            using (new AssertionScope())
            {
                roomsRetrieved.Should().NotBeNull();
                roomsRetrieved.Should().HaveCount(1);

                var room = roomsRetrieved.First();
                room.Identifier.Should().Be(expected.Identifier);
                room.Name.Should().Be(expected.Name);
                room.RoomId.Should().Be(expected.RoomId);
                room.Width.Should().Be(expected.Width);
                room.Depth.Should().Be(expected.Depth);
            }
        }

        // ---------------------------------------------------------------------------
        // RoomProperties.RoomId  ← regression for the "RoomProperties.RoomId" prefix bug
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by the business <c>RoomId</c> field returns exactly one room.
        /// Previously broken: the switch used <c>"RoomProperties.RoomId"</c> but the
        /// exposer delivers <c>"RoomId"</c>.
        /// </summary>
        [TestMethod]
        public void RoomDomRepository_ReadFilter_RoomId_Equal()
        {
                Helper.PopulateRooms();

            string roomId = "RM-005";
            var roomIdFilter = RoomExposers.RoomProperties.RoomId.Equal(roomId);
            var expected = DemoData.Rooms.Single(r => r.RoomId == roomId);

            var roomsRetrieved = Helper.Rooms.Read(roomIdFilter);

            using (new AssertionScope())
            {
                roomsRetrieved.Should().NotBeNull();
                roomsRetrieved.Should().HaveCount(1);

                var room = roomsRetrieved.First();
                room.RoomId.Should().Be(expected.RoomId);
                room.Name.Should().Be(expected.Name);
                room.Plan.Should().Be(expected.Plan);
            }
        }

        // ---------------------------------------------------------------------------
        // RoomProperties.Width  ← regression for the "RoomProperties.Width" prefix bug
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by a minimum room width (≥ threshold) returns all rooms at or above
        /// that width.  Previously broken: the switch used <c>"RoomProperties.Width"</c>
        /// but the exposer delivers <c>"Width"</c>.
        /// </summary>
        [TestMethod]
        public void RoomDomRepository_ReadFilter_Width_GreaterThanOrEqual()
        {
                Helper.PopulateRooms();

            // "Main Server Room" (1500), "Network Operations Center" (900),
            // "Cooling Equipment Room" (800) — three rooms at or above 800.
            long widthThreshold = 800L;
            var widthFilter = RoomExposers.RoomProperties.Width.GreaterThanOrEqual(widthThreshold);
            var expected = DemoData.Rooms.Where(r => r.Width >= widthThreshold).ToArray();

            var roomsRetrieved = Helper.Rooms.Read(widthFilter);

            using (new AssertionScope())
            {
                roomsRetrieved.Should().NotBeNull();
                roomsRetrieved.Should().HaveCount(expected.Length);
                roomsRetrieved.Should().BeEquivalentTo(expected);
            }
        }

        // ---------------------------------------------------------------------------
        // RoomProperties.Description  ← regression for the "RoomProperties.Description" prefix bug
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Filtering by a substring of the description returns all matching rooms.
        /// Previously broken: the switch used <c>"RoomProperties.Description"</c> but
        /// the exposer delivers <c>"Description"</c>.
        /// </summary>
        [TestMethod]
        public void RoomDomRepository_ReadFilter_Description_Contains()
        {
                Helper.PopulateRooms();

            // "Primary data center floor with raised flooring" and
            // "Precision cooling and HVAC units" do NOT share a keyword;
            // "NOC room with monitoring stations" is also unique.
            // Use "floor" which only appears in RM-001's description.
            var descriptionFilter = RoomExposers.RoomProperties.Description.Contains("flooring", StringComparison.OrdinalIgnoreCase);
            var expected = DemoData.Rooms
                .Where(r => r.Description.Contains("flooring", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            var roomsRetrieved = Helper.Rooms.Read(descriptionFilter);

            using (new AssertionScope())
            {
                roomsRetrieved.Should().NotBeNull();
                roomsRetrieved.Should().HaveCount(expected.Length);
                roomsRetrieved.Should().BeEquivalentTo(expected);
            }
        }

        // ---------------------------------------------------------------------------
        // Compound filter: Name AND Width
        // ---------------------------------------------------------------------------

        /// <summary>
        /// Combining a Name Contains filter with a Width upper bound using AND returns
        /// only rooms that satisfy both conditions simultaneously.
        /// </summary>
        [TestMethod]
        public void RoomDomRepository_ReadFilter_NameContainsAndWidthLessThan_Combined()
        {
                Helper.PopulateRooms();

            // "Main Server Room" (Width=1500) and "Main Storage Area" (Width=600).
            // Applying Width < 1000 should keep only "Main Storage Area".
            var combinedFilter = RoomExposers.RoomProperties.Name.Contains("Main", StringComparison.OrdinalIgnoreCase)
                .AND(RoomExposers.RoomProperties.Width.LessThan(1000L));

            var roomsRetrieved = Helper.Rooms.Read(combinedFilter);
            var expected = DemoData.Rooms
                .Where(r => r.Name.Contains("Main", StringComparison.OrdinalIgnoreCase) && r.Width < 1000L)
                .ToArray();

            using (new AssertionScope())
            {
                roomsRetrieved.Should().NotBeNull();
                roomsRetrieved.Should().HaveCount(expected.Length);
                roomsRetrieved.Should().BeEquivalentTo(expected);
            }
        }
    }
}
