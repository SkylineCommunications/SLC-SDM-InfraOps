namespace SDM.PlanAndBuild.Tests.Setup
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Skyline.DataMiner.SDM.PlanAndBuild.Helpers;

	[TestClass]
	public abstract class BaseRepositoryTest
	{
		protected IPlanAndBuildApiHelper Helper { get; private set; } = null!;

		[TestInitialize]
		public void BaseTestInitialize()
		{
			// ✅ PRIMARY cleanup — fresh in-memory store even if a previous test crashed.
			Helper = RepositoryInitialize.InitializeEmptyRepositories();
		}

		[TestCleanup]
		public void BaseTestCleanup()
		{
			// ✅ OPTIONAL cleanup — releases the reference after each test.
			Helper = null!;
		}
	}
}
