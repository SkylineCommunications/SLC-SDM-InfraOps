namespace SDM.FacilityManagement.Tests
{
	using SDM.FacilityManagement.Tests.Setup;
	using Skyline.DataMiner.SDM.FacilityManagement.Helpers;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.FacilityManagement.Models;
	using System.Linq;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;

	public static class RepositoryInitialize
	{
		public static IFacilityManagementApiHelper Initialize()
		{
			return ConnectionHelper.CreateConnection().GetMockedHelper();
		}

		/// <summary>
		/// Populates the Facilities repository with the provided <paramref name="facilities"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <param name="facilities">Predefined collection of <see cref="Facility"/> objects to create.</param>
		/// <returns><see cref="IFacilityManagementApiHelper"/> API helper interface with populated data.</returns>
		public static IFacilityManagementApiHelper PopulateFacilities(this IFacilityManagementApiHelper helper, IEnumerable<Facility> facilities)
		{
			if (facilities is null || !facilities.Any())
			{
				return helper.PopulateFacilities();
			}

			helper.Facilities.Create(facilities);

			return helper;
		}

		/// <summary>
		/// Populates the Facilities repository with default <seealso cref="Asset"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <returns><see cref="IAssetManagementApiHelper"/> API helper interface with populated data.</returns>
		public static IFacilityManagementApiHelper PopulateFacilities(this IFacilityManagementApiHelper helper)
		{
			helper.Facilities.Create(DemoData.Facilities);

			return helper;
		}

		/// <summary>
		/// Populates the Racks repository with default <see cref="Rack"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <returns><see cref="IFacilityManagementApiHelper"/> API helper interface with populated data.</returns>
		public static IFacilityManagementApiHelper PopulateRacks(this IFacilityManagementApiHelper helper)
		{
			helper.EnsureRowChain();
			helper.Racks.Create(DemoData.Racks);

			return helper;
		}

		/// <summary>
		/// Populates the Desks repository with default <see cref="Desk"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <returns><see cref="IFacilityManagementApiHelper"/> API helper interface with populated data.</returns>
		public static IFacilityManagementApiHelper PopulateDesks(this IFacilityManagementApiHelper helper)
		{
			helper.EnsureRoomChain();
			helper.Desks.Create(DemoData.Desks);

			return helper;
		}

		/// <summary>
		/// Populates the AppSettings repository with default <see cref="FacilityManagerAppSettings"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <returns><see cref="IFacilityManagementApiHelper"/> API helper interface with populated data.</returns>
		public static IFacilityManagementApiHelper PopulateFacilityManagerAppSettings(this IFacilityManagementApiHelper helper)
		{
			helper.AppSettings.Create(DemoData.FacilityManagerAppSettings);

			return helper;
		}

		/// <summary>
		/// Populates the Rooms repository with default <see cref="Room"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <returns><see cref="IFacilityManagementApiHelper"/> API helper interface with populated data.</returns>
		public static IFacilityManagementApiHelper PopulateRooms(this IFacilityManagementApiHelper helper)
		{
			helper.EnsureFloorChain();
			helper.Rooms.Create(DemoData.Rooms);

			return helper;
		}

		/// <summary>
		/// Populates the Floors repository with default <see cref="Floor"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <returns><see cref="IFacilityManagementApiHelper"/> API helper interface with populated data.</returns>
		public static IFacilityManagementApiHelper PopulateFloors(this IFacilityManagementApiHelper helper)
		{
			helper.EnsureFacility();
			helper.Floors.Create(DemoData.Floors);

			return helper;
		}

		/// <summary>
		/// Populates the Zones repository with default <see cref="Zone"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <returns><see cref="IFacilityManagementApiHelper"/> API helper interface with populated data.</returns>
		public static IFacilityManagementApiHelper PopulateZones(this IFacilityManagementApiHelper helper)
		{
			helper.EnsureRoomChain();
			helper.Zones.Create(DemoData.Zones);

			return helper;
		}

		/// <summary>
		/// Populates the Rows repository with default <see cref="Row"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <returns><see cref="IFacilityManagementApiHelper"/> API helper interface with populated data.</returns>
		public static IFacilityManagementApiHelper PopulateRows(this IFacilityManagementApiHelper helper)
		{
			helper.EnsureRoomChain();
			helper.Rows.Create(DemoData.Rows);

			return helper;
		}

		/// <summary>
		/// Populates the Sites repository with default <see cref="Site"/> test data.
		/// </summary>
		/// <param name="helper">Mocked API helper.</param>
		/// <returns><see cref="IFacilityManagementApiHelper"/> API helper interface with populated data.</returns>
		public static IFacilityManagementApiHelper PopulateSites(this IFacilityManagementApiHelper helper)
		{
			helper.Sites.Create(DemoData.Sites);

			return helper;
		}

		// Parent chain helpers: create only the first demo entity of each level, and only when missing.
		public static void EnsureFacility(this IFacilityManagementApiHelper helper)
		{
			var facility = DemoData.Facilities[0];
			if (!helper.Facilities.Read(FacilityExposers.Identifier.Equal(facility.Identifier)).Any())
			{
				helper.Facilities.Create(facility);
			}
		}

		public static void EnsureFloorChain(this IFacilityManagementApiHelper helper)
		{
			helper.EnsureFacility();
			var floor = DemoData.Floors[0];
			if (!helper.Floors.Read(FloorExposers.Identifier.Equal(floor.Identifier)).Any())
			{
				helper.Floors.Create(floor);
			}
		}

		public static void EnsureRoomChain(this IFacilityManagementApiHelper helper)
		{
			helper.EnsureFloorChain();
			var room = DemoData.Rooms[0];
			if (!helper.Rooms.Read(RoomExposers.Identifier.Equal(room.Identifier)).Any())
			{
				helper.Rooms.Create(room);
			}
		}

		public static void EnsureRowChain(this IFacilityManagementApiHelper helper)
		{
			helper.EnsureRoomChain();
			var row = DemoData.Rows[0];
			if (!helper.Rows.Read(RowExposers.Identifier.Equal(row.Identifier)).Any())
			{
				helper.Rows.Create(row);
			}
		}

		// Attach helpers for tests that build their own entities: ensure the demo parent chain exists and reference it.
		public static Floor AttachFacility(this IFacilityManagementApiHelper helper, Floor floor)
		{
			if (floor.FacilityFk.IsEmpty)
			{
				helper.EnsureFacility();
				floor.FacilityFk.Facility = new SdmObjectReference<Facility>(DemoData.Facilities[0].Identifier);
			}

			return floor;
		}
		public static Room AttachFloor(this IFacilityManagementApiHelper helper, Room room)
		{
			if (room.FloorFk.IsEmpty)
			{
				helper.EnsureFloorChain();
				room.FloorFk.Floor = new SdmObjectReference<Floor>(DemoData.Floors[0].Identifier);
			}

			return room;
		}
		public static Row AttachRoom(this IFacilityManagementApiHelper helper, Row row)
		{
			if (row.RoomFk.IsEmpty)
			{
				helper.EnsureRoomChain();
				row.RoomFk.Room = new SdmObjectReference<Room>(DemoData.Rooms[0].Identifier);
			}

			return row;
		}
		public static Zone AttachRoom(this IFacilityManagementApiHelper helper, Zone zone)
		{
			if (zone.RoomFk.IsEmpty)
			{
				helper.EnsureRoomChain();
				zone.RoomFk.Room = new SdmObjectReference<Room>(DemoData.Rooms[0].Identifier);
			}

			return zone;
		}
		public static Desk AttachRoom(this IFacilityManagementApiHelper helper, Desk desk)
		{
			if (desk.RoomFk.IsEmpty)
			{
				helper.EnsureRoomChain();
				desk.RoomFk.Room = new SdmObjectReference<Room>(DemoData.Rooms[0].Identifier);
			}

			return desk;
		}
		public static Rack AttachRow(this IFacilityManagementApiHelper helper, Rack rack)
		{
			if (rack.RowFk.IsEmpty)
			{
				helper.EnsureRowChain();
				rack.RowFk.Row = new SdmObjectReference<Row>(DemoData.Rows[0].Identifier);
			}

			return rack;
		}
	}
}