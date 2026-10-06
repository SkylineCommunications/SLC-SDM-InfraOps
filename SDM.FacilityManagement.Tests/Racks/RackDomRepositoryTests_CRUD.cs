namespace SDM.FacilityManagement.Tests.Racks
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    public partial class RackDomRepositoryTests
    {
        [TestMethod]
        public void RackDomRepository_Create_ReadBack_PersistsRackIdAndRackCapacity()
        {
            var rack = new Rack
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Rack 1",
                RackId = "RK-1",
                Position = SlcFacility_Management.Enums.RackpositionenumEnum.Bottom,
            };
            rack.Capacity.MaximumRackCapacity = 42;

            Helper.Racks.Create(Helper.AttachRow(rack));

            var reloaded = Helper.Racks.Read(RackExposers.Identifier.Equal(rack.Identifier)).SingleOrDefault();

            using (new AssertionScope())
            {
                reloaded.Should().NotBeNull();
                reloaded!.RackId.Should().Be("RK-1");
                reloaded.Capacity.MaximumRackCapacity.Should().Be(42);
            }
        }
    }
}
