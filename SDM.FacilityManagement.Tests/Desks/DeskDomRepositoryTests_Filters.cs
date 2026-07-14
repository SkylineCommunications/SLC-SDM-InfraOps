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

    public partial class DeskDomRepositoryTests : BaseRepositoryTest
    {
        [TestMethod]
        public void DeskDomRepository_ReadFilter_Name_Equals()
        {
            Helper.PopulateDesks();

            const string deskName = "Desk A01";
            var filter = DeskExposers.DeskInformation.Name.Equal(deskName);
            var expected = DemoData.Desks.Single(d => d.Name == deskName);

            var desksRetrieved = Helper.Desks.Read(filter);

            using (new AssertionScope())
            {
                desksRetrieved.Should().HaveCount(1);
                desksRetrieved.First().Should().BeEquivalentTo(expected);
            }
        }

        [TestMethod]
        public void DeskDomRepository_ReadFilter_Name_Contains()
        {
            Helper.PopulateDesks();

            const string searchTerm = "Desk A";
            var filter = DeskExposers.DeskInformation.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
            var expected = DemoData.Desks.Where(d => d.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToArray();

            var desksRetrieved = Helper.Desks.Read(filter);

            using (new AssertionScope())
            {
                desksRetrieved.Should().HaveCount(expected.Length);
                desksRetrieved.Should().BeEquivalentTo(expected);
            }
        }

        [TestMethod]
        public void DeskDomRepository_ReadFilter_DeskID_Equal()
        {
            Helper.PopulateDesks();

            const string deskId = "DSK-003";
            var filter = DeskExposers.DeskInformation.DeskID.Equal(deskId);
            var expected = DemoData.Desks.Single(d => d.DeskID == deskId);

            var desksRetrieved = Helper.Desks.Read(filter);

            using (new AssertionScope())
            {
                desksRetrieved.Should().HaveCount(1);
                desksRetrieved.First().Should().BeEquivalentTo(expected);
            }
        }

        [TestMethod]
        public void DeskDomRepository_ReadFilter_Description_Contains()
        {
            Helper.PopulateDesks();

            const string searchTerm = "collaboration";
            var filter = DeskExposers.DeskInformation.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
            var expected = DemoData.Desks.Where(d => d.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToArray();

            var desksRetrieved = Helper.Desks.Read(filter);

            using (new AssertionScope())
            {
                desksRetrieved.Should().HaveCount(expected.Length);
                desksRetrieved.Should().BeEquivalentTo(expected);
            }
        }

        [TestMethod]
        public void DeskDomRepository_ReadFilter_Identifier_Equal()
        {
            Helper.PopulateDesks();

            var identifier = DemoData.Desks[3].Identifier;
            var filter = DeskExposers.Identifier.Equal(identifier);
            var expected = DemoData.Desks.Single(d => d.Identifier == identifier);

            var desksRetrieved = Helper.Desks.Read(filter);

            using (new AssertionScope())
            {
                desksRetrieved.Should().HaveCount(1);
                desksRetrieved.First().Should().BeEquivalentTo(expected);
            }
        }

        [TestMethod]
        public void DeskDomRepository_ReadFilter_NameAndDeskID_Combined()
        {
            Helper.PopulateDesks();

            var filter = DeskExposers.DeskInformation.Name.Contains("Desk C", StringComparison.OrdinalIgnoreCase)
                .AND(DeskExposers.DeskInformation.DeskID.Equal("DSK-006"));
            var expected = DemoData.Desks
                .Where(d => d.Name.Contains("Desk C", StringComparison.OrdinalIgnoreCase) && d.DeskID == "DSK-006")
                .ToArray();

            var desksRetrieved = Helper.Desks.Read(filter);

            using (new AssertionScope())
            {
                desksRetrieved.Should().HaveCount(expected.Length);
                desksRetrieved.Should().BeEquivalentTo(expected);
            }
        }
    }
}
