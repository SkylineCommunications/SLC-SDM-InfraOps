namespace SDM.PlanAndBuild.Tests.JobTests
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using SDM.PlanAndBuild.Tests.Setup;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.Net.Sections;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.AssetManagement.Models;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;

	[TestClass]
	public class PlanAndBuildJobCableTypeStringTests : BaseRepositoryTest
	{
		[TestMethod]
		public void RemovedConnectionSnapshot_CableType_WritesAndReadsString()
		{
			var identifier = Guid.NewGuid().ToString();
			var job = new PlanAndBuildJob
			{
				ConnectionsOnJob = new List<JobConnection>
				{
					new JobConnection { CableType = new SdmObjectReference<CableType>(identifier), Status = "Removed" },
				},
			};
			var repository = typeof(PlanAndBuildJob).Assembly.GetType("Skyline.DataMiner.SDM.PlanAndBuild.Models.PlanAndBuildJobDomRepository", true)!;
			var mapper = Activator.CreateInstance(repository, ConnectionHelper.CreateConnection())!;
			var instance = (DomInstance)repository.GetMethod("ToInstance", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(mapper, new object[] { job })!;
			var field = instance.Sections.SelectMany(section => section.FieldValues)
				.Single(value => value.FieldDescriptorID.Id == Guid.Parse("42d731c8-e875-4b51-b4ad-2453cc613eac"));
			Assert.IsInstanceOfType(field.Value, typeof(ValueWrapper<string>));
			var restored = (PlanAndBuildJob)repository.GetMethod("FromInstance", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(mapper, new object[] { instance })!;
			Assert.AreEqual(identifier, restored.ConnectionsOnJob.Single().CableType.Identifier);
		}

		[TestMethod]
		public void UpdateRemovedSnapshot_CableTypeFilter_MatchesStringIdentifier()
		{
			Helper.PopulateAppSettings();
			var jobType = Helper.JobTypes.Create(new JobType { Name = "Snapshot test" });
			var identifier = Guid.NewGuid().ToString();
			var job = Helper.Jobs.Create(new PlanAndBuildJob
			{
				JobName = "Cable snapshot",
				Type = new SdmObjectReference<JobType>(jobType.Identifier),
				ConnectionsOnJob = new List<JobConnection>
				{
					new JobConnection { CableType = new SdmObjectReference<CableType>(identifier), Status = "Removed" },
				},
			});
			job.ConnectionsOnJob.Single().Source = "Removed endpoint snapshot";
			Helper.Jobs.Update(job);
			var results = Helper.Jobs.Read(PlanAndBuildJobExposers.ConnectionsOnJob.CableType.Equal(new SdmObjectReference<CableType>(identifier))).ToList();
			Assert.AreEqual(1, results.Count);
			Assert.AreEqual("Removed", results.Single().ConnectionsOnJob.Single().Status);
		}
	}
}
