namespace SDM.FacilityManagement.Tests.Zones
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.FacilityManagement.Tests.Setup;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    [TestClass]
	public class ZoneDomRepositoryTests : BaseRepositoryTest
	{
		[TestMethod]
		public void ZoneDomRepository_ReadFilter_ZonesWithoutRoom()
		{
			var zones = new[]
			{
				new Zone
				{
					Identifier = Guid.NewGuid().ToString(),
					ZoneId = "ZONE-001",
					Name = "Zone Without Room",
				},
				new Zone
				{
					Identifier = Guid.NewGuid().ToString(),
					ZoneId = "ZONE-002",
					Name = "Zone Without Room B",
				},
			};

			Helper.Zones.Create(zones);

			var filter = ZoneExposers.RoomFk.Room.HasNoValue();
			var expected = zones.Where(z => (!z.RoomFk?.Room.HasValue()) ?? false).ToArray();

			var zonesRetrieved = Helper.Zones.Read(filter);

			using (new AssertionScope())
			{
				zonesRetrieved.Should().NotBeNull();
				zonesRetrieved.Should().NotBeEmpty();
				zonesRetrieved.Should().HaveCount(expected.Length);
				zonesRetrieved.Should().BeEquivalentTo(expected);
			}
		}

		[TestMethod]
		public void ZoneDomRepository_ReadFilter_NonExistentRoom()
		{
			var filter = ZoneExposers.RoomFk.Room.Equal(new SdmObjectReference<Room>(Guid.NewGuid().ToString()));

			var zonesRetrieved = Helper.Zones.Read(filter);

			using (new AssertionScope())
			{
				zonesRetrieved.Should().NotBeNull();
				zonesRetrieved.Should().BeEmpty();
				zonesRetrieved.Should().HaveCount(0);
			}
		}

	}
}