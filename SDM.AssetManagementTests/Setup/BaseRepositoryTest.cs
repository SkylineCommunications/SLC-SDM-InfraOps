namespace SDM.AssetManagement.Tests.Setup
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public abstract class BaseRepositoryTest
    {
        protected ITestApiHelper Helper { get; private set; }

        [TestInitialize]
        public void BaseTestInitialize()
        {
            Helper = RepositoryInitialize.InitializeEmptyRepositories();

            // ✅ PRIMARY cleanup - runs even if previous test crashed
            Helper.CleanupAllTestData();
        }

        [TestCleanup]
        public void BaseTestCleanup()
        {
            // ✅ OPTIONAL cleanup - nice to have for manual test runs
            // Not critical since TestInitialize will clean up anyway
            Helper?.CleanupAllTestData();
        }
    }
}