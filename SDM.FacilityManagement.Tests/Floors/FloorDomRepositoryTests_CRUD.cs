namespace SDM.FacilityManagement.Tests.Floors
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    public partial class FloorDomRepositoryTests
    {
        [TestMethod]
        public void FloorDomRepository_CreateWithFacilityReference_ReadBack_PersistsFloorIdAndFacility()
        {
            var facility = Helper.Facilities.Create(new Facility { Identifier = Guid.NewGuid().ToString(), FacilityId = "FAC-FLOOR", Name = "Facility FLOOR" });
            var floor = new Floor { Identifier = Guid.NewGuid().ToString(), Name = "Floor 1", FloorId = "FL-1" };
            floor.FacilityFk.Facility = new SdmObjectReference<Facility>(facility.Identifier);

            Helper.Floors.Create(floor);

            var reloaded = Helper.Floors.Read(FloorExposers.Identifier.Equal(floor.Identifier)).SingleOrDefault();

            using (new AssertionScope())
            {
                reloaded.Should().NotBeNull();
                reloaded!.FloorId.Should().Be("FL-1");
                reloaded.FacilityFk.Facility.Should().NotBeNull();
                reloaded.FacilityFk.Facility.Identifier.Should().Be(facility.Identifier);
            }
        }
    }
}
