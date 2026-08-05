namespace SDM.PlanAndBuild.Tests.JobTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.PlanAndBuild.Tests.Setup;

	using Skyline.DataMiner.SDM.FacilityManagement.Helpers;
	using Skyline.DataMiner.SDM.FacilityManagement.Models;
	using Skyline.DataMiner.SDM.PlanAndBuild.Extensions;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;

	/// <summary>
	/// Unit tests for <see cref="PlanAndBuildJobModelExtensions.ResolveLocations(PlanAndBuildJob, IFacilityManagementApiHelper)"/>
	/// and its bulk overload, which resolve <see cref="PlanAndBuildJob.Locations"/> to their concrete
	/// Facility Management DOM instances (Facility, Floor, Room, Zone, Row, Desk, Rack).
	/// </summary>
	[TestClass]
	public class PlanAndBuildJobLocationResolverTests : BaseRepositoryTest
	{
		private IFacilityManagementApiHelper facilityHelper = null!;

		private Facility facility = null!;
		private Floor floor = null!;
		private Room room = null!;
		private Zone zone = null!;
		private Row row = null!;
		private Desk desk = null!;
		private Rack rack = null!;

		[TestInitialize]
		public void Initialize()
		{
			facilityHelper = Helper.Connection.GetMockedFacilityManagementHelper();

			facility = facilityHelper.Facilities.Create(new Facility { Identifier = Guid.NewGuid().ToString(), Name = "Facility 1", FacilityId = "FAC-1" });
			floor = facilityHelper.Floors.Create(new Floor { Identifier = Guid.NewGuid().ToString(), Name = "Floor 1", FloorId = "FLR-1" });
			room = facilityHelper.Rooms.Create(new Room { Identifier = Guid.NewGuid().ToString(), Name = "Room 1", RoomId = "ROM-1" });
			zone = facilityHelper.Zones.Create(new Zone { Identifier = Guid.NewGuid().ToString(), Name = "Zone 1", ZoneId = "ZON-1" });
			row = facilityHelper.Rows.Create(new Row { Identifier = Guid.NewGuid().ToString(), Name = "Row 1", RowId = "ROW-1" });
			desk = facilityHelper.Desks.Create(new Desk { Identifier = Guid.NewGuid().ToString(), Name = "Desk 1", DeskID = "DSK-1" });
			rack = facilityHelper.Racks.Create(new Rack { Identifier = Guid.NewGuid().ToString(), Name = "Rack 1", RackId = "RCK-1", Capacity = new RackCapacity { MaximumRackCapacity = 42 } });
		}

		[TestMethod]
		public void ResolveLocations_AllSevenTypes_ShouldResolveWithCorrectTypings()
		{
			var job = new PlanAndBuildJob();
			job.SetLocations(new[]
			{
				Guid.Parse(facility.Identifier),
				Guid.Parse(floor.Identifier),
				Guid.Parse(room.Identifier),
				Guid.Parse(zone.Identifier),
				Guid.Parse(row.Identifier),
				Guid.Parse(desk.Identifier),
				Guid.Parse(rack.Identifier),
			});

			var resolved = job.ResolveLocations(facilityHelper);

			using (new AssertionScope())
			{
				resolved.Should().HaveCount(7);
				resolved.Should().NotContain(jl => jl.Kind == FacilityLocationKind.Unknown);

				resolved.Single(jl => jl.Id == Guid.Parse(facility.Identifier)).Should().Match<JobLocation>(jl =>
					jl.Kind == FacilityLocationKind.Facility && jl.Facility != null && jl.Value == jl.Facility);
				resolved.Single(jl => jl.Id == Guid.Parse(floor.Identifier)).Should().Match<JobLocation>(jl =>
					jl.Kind == FacilityLocationKind.Floor && jl.Floor != null && jl.Value == jl.Floor);
				resolved.Single(jl => jl.Id == Guid.Parse(room.Identifier)).Should().Match<JobLocation>(jl =>
					jl.Kind == FacilityLocationKind.Room && jl.Room != null && jl.Value == jl.Room);
				resolved.Single(jl => jl.Id == Guid.Parse(zone.Identifier)).Should().Match<JobLocation>(jl =>
					jl.Kind == FacilityLocationKind.Zone && jl.Zone != null && jl.Value == jl.Zone);
				resolved.Single(jl => jl.Id == Guid.Parse(row.Identifier)).Should().Match<JobLocation>(jl =>
					jl.Kind == FacilityLocationKind.Row && jl.Row != null && jl.Value == jl.Row);
				resolved.Single(jl => jl.Id == Guid.Parse(desk.Identifier)).Should().Match<JobLocation>(jl =>
					jl.Kind == FacilityLocationKind.Desk && jl.Desk != null && jl.Value == jl.Desk);
				resolved.Single(jl => jl.Id == Guid.Parse(rack.Identifier)).Should().Match<JobLocation>(jl =>
					jl.Kind == FacilityLocationKind.Rack && jl.Rack != null && jl.Value == jl.Rack);
			}
		}

		[TestMethod]
		public void ResolveLocations_UnknownGuid_ShouldResolveAsUnknown()
		{
			var job = new PlanAndBuildJob();
			var unknownId = Guid.NewGuid();
			job.SetLocations(new[] { unknownId });

			var resolved = job.ResolveLocations(facilityHelper);

			using (new AssertionScope())
			{
				resolved.Should().ContainSingle();
				var jobLocation = resolved.Single();
				jobLocation.Id.Should().Be(unknownId);
				jobLocation.Kind.Should().Be(FacilityLocationKind.Unknown);
				jobLocation.Value.Should().BeNull();
			}
		}

		[TestMethod]
		public void ResolveLocations_EmptyLocations_ShouldReturnEmptyWithoutQuerying()
		{
			var job = new PlanAndBuildJob();

			var resolved = job.ResolveLocations(facilityHelper);

			resolved.Should().BeEmpty();
		}

		[TestMethod]
		public void ResolveLocations_NullFacilityHelper_ShouldThrow()
		{
			var job = new PlanAndBuildJob();
			job.SetLocations(new[] { Guid.NewGuid() });

			Action act = () => job.ResolveLocations(null!);

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void ResolveLocations_Bulk_ShouldResolveAcrossMultipleJobs()
		{
			var jobA = new PlanAndBuildJob();
			jobA.SetLocations(new[] { Guid.Parse(facility.Identifier), Guid.Parse(room.Identifier) });

			var jobB = new PlanAndBuildJob();
			jobB.SetLocations(new[] { Guid.Parse(rack.Identifier) });

			var resolved = new[] { jobA, jobB }.ResolveLocations(facilityHelper);

			using (new AssertionScope())
			{
				resolved.Should().HaveCount(2);

				var jobALocations = resolved.Single(kvp => ReferenceEquals(kvp.Key, jobA)).Value;
				var jobBLocations = resolved.Single(kvp => ReferenceEquals(kvp.Key, jobB)).Value;

				jobALocations.Should().HaveCount(2);
				jobALocations.Should().Contain(jl => jl.Kind == FacilityLocationKind.Facility);
				jobALocations.Should().Contain(jl => jl.Kind == FacilityLocationKind.Room);

				jobBLocations.Should().ContainSingle();
				jobBLocations.Single().Kind.Should().Be(FacilityLocationKind.Rack);
			}
		}

		[TestMethod]
		public void ResolveLocations_Bulk_NullJobs_ShouldThrow()
		{
			Action act = () => ((IEnumerable<PlanAndBuildJob>)null!).ResolveLocations(facilityHelper);

			act.Should().Throw<ArgumentNullException>();
		}
	}
}
