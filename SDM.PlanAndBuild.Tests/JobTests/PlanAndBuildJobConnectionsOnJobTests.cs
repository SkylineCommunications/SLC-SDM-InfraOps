namespace SDM.PlanAndBuild.Tests.JobTests
{
	using System;
	using System.Collections.Generic;

	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.AssetManagement.Models;
	using Skyline.DataMiner.SDM.PlanAndBuild.Extensions;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;

	/// <summary>
	/// Unit tests for the ConnectionsOnJob convenience methods on <see cref="PlanAndBuildJob"/>
	/// (AddConnectionsOnJobItem/RemoveItemFromConnectionsOnJob/SetConnectionsOnJob/ClearConnectionsOnJob),
	/// mirroring InfraOpsShared's JobWrapper API.
	/// </summary>
	[TestClass]
	public class PlanAndBuildJobConnectionsOnJobTests
	{
		[TestMethod]
		public void AddConnectionsOnJobItem_NewConnection_ShouldBeAdded()
		{
			var job = new PlanAndBuildJob();
			var connection = new JobConnection { ConnectionId = new SdmObjectReference<Connection>(Guid.NewGuid().ToString()) };

			job.AddConnectionsOnJobItem(connection);

			job.ConnectionsOnJob.Should().ContainSingle().Which.Should().Be(connection);
		}

		[TestMethod]
		public void AddConnectionsOnJobItem_Null_ShouldThrow()
		{
			var job = new PlanAndBuildJob();

			Action act = () => job.AddConnectionsOnJobItem(null);

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void AddConnectionsOnJobItem_DuplicateConnectionId_ShouldThrow()
		{
			var job = new PlanAndBuildJob();
			var connectionId = new SdmObjectReference<Connection>(Guid.NewGuid().ToString());
			job.AddConnectionsOnJobItem(new JobConnection { ConnectionId = connectionId, Source = "A" });

			Action act = () => job.AddConnectionsOnJobItem(new JobConnection { ConnectionId = connectionId, Source = "B" });

			act.Should().Throw<InvalidOperationException>();
		}

		[TestMethod]
		public void RemoveItemFromConnectionsOnJob_ExistingConnection_ShouldBeRemoved()
		{
			var job = new PlanAndBuildJob();
			var connection = new JobConnection { ConnectionId = new SdmObjectReference<Connection>(Guid.NewGuid().ToString()) };
			job.AddConnectionsOnJobItem(connection);

			job.RemoveItemFromConnectionsOnJob(connection);

			job.ConnectionsOnJob.Should().BeEmpty();
		}

		[TestMethod]
		public void RemoveItemFromConnectionsOnJob_Null_ShouldThrow()
		{
			var job = new PlanAndBuildJob();

			Action act = () => job.RemoveItemFromConnectionsOnJob(null);

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void RemoveItemFromConnectionsOnJob_NotFound_ShouldThrow()
		{
			var job = new PlanAndBuildJob();
			var connection = new JobConnection { ConnectionId = new SdmObjectReference<Connection>(Guid.NewGuid().ToString()) };

			Action act = () => job.RemoveItemFromConnectionsOnJob(connection);

			act.Should().Throw<ArgumentException>();
		}

		[TestMethod]
		public void SetConnectionsOnJob_ShouldReplaceExistingList()
		{
			var job = new PlanAndBuildJob();
			job.AddConnectionsOnJobItem(new JobConnection { ConnectionId = new SdmObjectReference<Connection>(Guid.NewGuid().ToString()) });

			var replacement = new List<JobConnection>
			{
				new JobConnection { ConnectionId = new SdmObjectReference<Connection>(Guid.NewGuid().ToString()) },
				new JobConnection { ConnectionId = new SdmObjectReference<Connection>(Guid.NewGuid().ToString()) },
			};
			job.SetConnectionsOnJob(replacement);

			job.ConnectionsOnJob.Should().BeEquivalentTo(replacement);
		}

		[TestMethod]
		public void ClearConnectionsOnJob_ShouldEmptyList()
		{
			var job = new PlanAndBuildJob();
			job.AddConnectionsOnJobItem(new JobConnection { ConnectionId = new SdmObjectReference<Connection>(Guid.NewGuid().ToString()) });

			job.ClearConnectionsOnJob();

			job.ConnectionsOnJob.Should().BeEmpty();
		}
	}
}
