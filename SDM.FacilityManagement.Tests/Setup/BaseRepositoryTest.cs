namespace SDM.FacilityManagement.Tests.Setup
{
	using Microsoft.VisualStudio.TestTools.UnitTesting;
	using Skyline.DataMiner.SDM.FacilityManagement.Helpers;

	[TestClass]
	public abstract class BaseRepositoryTest
	{
		protected IFacilityManagementApiHelper Helper { get; private set; } = null!;

		[TestInitialize]
		public void BaseTestInitialize()
		{
			Helper = RepositoryInitialize.InitializeEmptyRepositories();
		}

		[TestCleanup]
		public void BaseTestCleanup()
		{
			Helper = null!;
		}
	}
}
