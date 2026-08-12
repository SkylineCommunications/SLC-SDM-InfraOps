namespace SDM.PlanAndBuild.Tests.JobTests
{
	using System;
	using System.Collections.Generic;

	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.SDM.PlanAndBuild.Extensions;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;

	/// <summary>
	/// Unit tests for the Locations convenience methods on <see cref="PlanAndBuildJob"/>
	/// (AddLocation/RemoveLocation/SetLocations), mirroring InfraOpsShared's JobWrapper API.
	/// </summary>
	[TestClass]
	public class PlanAndBuildJobLocationsTests
	{
		[TestMethod]
		public void AddLocation_NewLocation_ShouldBeAdded()
		{
			var job = new PlanAndBuildJob();
			var location = Guid.NewGuid();

			job.AddLocation(location);

			job.Locations.Should().ContainSingle().Which.Should().Be(location);
		}

		[TestMethod]
		public void AddLocation_Duplicate_ShouldThrow()
		{
			var job = new PlanAndBuildJob();
			var location = Guid.NewGuid();
			job.AddLocation(location);

			Action act = () => job.AddLocation(location);

			act.Should().Throw<InvalidOperationException>();
		}

		[TestMethod]
		public void RemoveLocation_ExistingLocation_ShouldBeRemoved()
		{
			var job = new PlanAndBuildJob();
			var location = Guid.NewGuid();
			job.AddLocation(location);

			job.RemoveLocation(location);

			job.Locations.Should().BeEmpty();
		}

		[TestMethod]
		public void RemoveLocation_NotFound_ShouldThrow()
		{
			var job = new PlanAndBuildJob();

			Action act = () => job.RemoveLocation(Guid.NewGuid());

			act.Should().Throw<ArgumentException>();
		}

		[TestMethod]
		public void SetLocations_ShouldReplaceExistingList()
		{
			var job = new PlanAndBuildJob();
			job.AddLocation(Guid.NewGuid());

			var replacement = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
			job.SetLocations(replacement);

			job.Locations.Should().BeEquivalentTo(replacement);
		}

		[TestMethod]
		public void HasJobType_NoJobType_ShouldBeFalse()
		{
			var job = new PlanAndBuildJob();

			job.HasJobType().Should().BeFalse();
		}

		[TestMethod]
		public void IsAssignedToPerson_NoAssignedTo_ShouldBeFalse()
		{
			var job = new PlanAndBuildJob();

			job.Ownership.IsAssignedToPerson().Should().BeFalse();
		}

		[TestMethod]
		public void IsAssignedToPerson_WithAssignedTo_ShouldBeTrue()
		{
			var job = new PlanAndBuildJob();
			job.Ownership.AssignedTo = Guid.NewGuid();

			job.Ownership.IsAssignedToPerson().Should().BeTrue();
		}

		[TestMethod]
		public void HasAssignmentGroup_NoAssignmentGroup_ShouldBeFalse()
		{
			var job = new PlanAndBuildJob();

			job.Ownership.HasAssignmentGroup().Should().BeFalse();
		}

		[TestMethod]
		public void HasAssignmentGroup_WithAssignmentGroup_ShouldBeTrue()
		{
			var job = new PlanAndBuildJob();
			job.Ownership.AssignmentGroup = Guid.NewGuid();

			job.Ownership.HasAssignmentGroup().Should().BeTrue();
		}
	}
}
