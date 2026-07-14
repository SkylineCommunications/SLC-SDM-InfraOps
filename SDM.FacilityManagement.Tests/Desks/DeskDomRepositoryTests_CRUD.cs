namespace SDM.FacilityManagement.Tests.Desks
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using SDM.FacilityManagement.Tests.Setup;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    [TestClass]
    public partial class DeskDomRepositoryTests : BaseRepositoryTest
    {
        private Desk referenceDesk = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            referenceDesk = new Desk
            {
                Identifier = Guid.NewGuid().ToString(),
                DeskID = "DSK-REF-001",
                Name = "Desk T01",
                Description = "Reference desk for repository tests",
                Plan = "Level-T-T01",
            };
        }

        [TestMethod]
        public void DeskDomRepository_EmptyDOM_Create()
        {
            Helper.Desks.Create(referenceDesk);

            AssertCreated();
        }

        [TestMethod]
        public void DeskDomRepository_EmptyDOM_CreateOrUpdate_Create()
        {
            Helper.Desks.CreateOrUpdate([referenceDesk]);

            AssertCreated();
        }

        [TestMethod]
        public void DeskDomRepository_EmptyDOM_CreateOrUpdate_Update()
        {
            Helper.Desks.Create(referenceDesk);

            var updatedDesk = new Desk
            {
                Identifier = referenceDesk.Identifier,
                DeskID = "DSK-REF-002",
                Name = "Desk T99",
                Description = "Updated desk for repository tests",
                Plan = "Level-T-T99",
            };

            Helper.Desks.CreateOrUpdate([updatedDesk]);

            var persistedDesk = Helper.Desks.Read(DeskExposers.Identifier.Equal(referenceDesk.Identifier)).Single();
            AssertDeskUpdateDifferences(referenceDesk, persistedDesk);
        }

        [TestMethod]
        public void DeskDomRepository_ReadPaged()
        {
            const int pageSize = 2;
            Helper.PopulateDesks();

            var allFilter = new TRUEFilterElement<Desk>();
            var pagedResult = Helper.Desks.ReadPaged(allFilter, pageSize);
            var deskCount = Helper.Desks.Count(allFilter);

            using (new AssertionScope())
            {
                pagedResult.Should().NotBeNull();
                pagedResult.Should().HaveCount((int)(deskCount / pageSize));
                pagedResult.Should().AllSatisfy(page => page.Should().HaveCount(pageSize));
            }
        }

        [TestMethod]
        public void DeskDomRepository_DeleteBulk()
        {
            Helper.PopulateDesks();

            var filter = new ORFilterElement<Desk>(
                DeskExposers.DeskInformation.Name.Contains("A0", StringComparison.OrdinalIgnoreCase),
                DeskExposers.DeskInformation.DeskID.Equal("DSK-005"));
            var desksToDelete = Helper.Desks.Read(filter);

            Helper.Desks.Delete(desksToDelete);

            using (new AssertionScope())
            {
                Helper.Desks.Count(new TRUEFilterElement<Desk>()).Should().Be(DemoData.Desks.Count - 3);
                Helper.Desks.Count(DeskExposers.DeskInformation.Name.Contains("A0", StringComparison.OrdinalIgnoreCase)).Should().Be(0);
                Helper.Desks.Count(DeskExposers.DeskInformation.DeskID.Equal("DSK-005")).Should().Be(0);
            }
        }

        [TestMethod]
        public void DeskDomRepository_EmptyDOM_DeleteSingle()
        {
            Helper.PopulateDesks();

            var deskToDelete = Helper.Desks.Read(DeskExposers.DeskInformation.Name.Equal(DemoData.Desks[4].Name)).First();

            Helper.Desks.Delete(deskToDelete);

            using (new AssertionScope())
            {
                Helper.Desks.Count(new TRUEFilterElement<Desk>()).Should().Be(DemoData.Desks.Count - 1);
                Helper.Desks.Count(DeskExposers.Identifier.Equal(deskToDelete.Identifier)).Should().Be(0);
            }
        }

        private static void AssertDeskUpdateDifferences(Desk original, Desk updated)
        {
            using (new AssertionScope())
            {
                updated.DeskID.Should().Be("DSK-REF-002");
                updated.DeskID.Should().NotBe(original.DeskID);
                updated.Name.Should().Be("Desk T99");
                updated.Name.Should().NotBe(original.Name);
                updated.Description.Should().Be("Updated desk for repository tests");
                updated.Description.Should().NotBe(original.Description);
                updated.Plan.Should().Be("Level-T-T99");
                updated.Plan.Should().NotBe(original.Plan);
            }
        }

        private void AssertCreated()
        {
            using (new AssertionScope())
            {
                Helper.Desks.Count(new TRUEFilterElement<Desk>()).Should().Be(1);

                var createdDesk = Helper.Desks.Read(new TRUEFilterElement<Desk>()).First();
                createdDesk.DeskID.Should().Be(referenceDesk.DeskID);
                createdDesk.Name.Should().Be(referenceDesk.Name);
                createdDesk.Description.Should().Be(referenceDesk.Description);
                createdDesk.Plan.Should().Be(referenceDesk.Plan);
            }
        }
    }
}
