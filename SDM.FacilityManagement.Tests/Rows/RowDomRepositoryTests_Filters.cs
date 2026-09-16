namespace SDM.FacilityManagement.Tests.Rows
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
	public class RowDomRepositoryTests : BaseRepositoryTest
	{
		[TestMethod]
		public void RowDomRepository_ReadFilter_RowsWithoutRoom()
		{
			var rows = new[]
			{
				new Row
				{
					Identifier = Guid.NewGuid().ToString(),
					RowId = "ROW-001",
					Name = "Row Without Room",
				},
				new Row
				{
					Identifier = Guid.NewGuid().ToString(),
					RowId = "ROW-002",
					Name = "Row Without Room B",
				},
			};

			Helper.Rows.Create(rows);

			var filter = RowExposers.RoomFk.Room.HasNoValue();
			var expected = rows.Where(r => (!r.RoomFk?.Room.HasValue()) ?? false).ToArray();

			var rowsRetrieved = Helper.Rows.Read(filter);

			using (new AssertionScope())
			{
				rowsRetrieved.Should().NotBeNull();
				rowsRetrieved.Should().NotBeEmpty();
				rowsRetrieved.Should().HaveCount(expected.Length);
				rowsRetrieved.Should().BeEquivalentTo(expected);
			}
		}

		[TestMethod]
		public void RowDomRepository_ReadFilter_NonExistentRoom()
		{
			var filter = RowExposers.RoomFk.Room.Equal(new SdmObjectReference<Room>(Guid.NewGuid().ToString()));

			var rowsRetrieved = Helper.Rows.Read(filter);

			using (new AssertionScope())
			{
				rowsRetrieved.Should().NotBeNull();
				rowsRetrieved.Should().BeEmpty();
				rowsRetrieved.Should().HaveCount(0);
			}
		}

	}
}