namespace SDM.AssetManagement.Tests.Setup
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public abstract class BaseRepositoryTest
    {
        protected ITestApiHelper Helper { get; private set; } = null!;

        [TestInitialize]
        public void BaseTestInitialize()
        {
            Helper = RepositoryInitialize.InitializeEmptyRepositories();
            Helper.CleanupAllTestData();
        }

        [TestCleanup]
        public void BaseTestCleanup()
        {
            Helper?.CleanupAllTestData();
        }
    }
}