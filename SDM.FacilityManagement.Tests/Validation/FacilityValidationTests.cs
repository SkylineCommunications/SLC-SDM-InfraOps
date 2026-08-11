namespace SDM.FacilityManagement.Tests.Validation
{
    using System;

    using FluentAssertions;

    using SDM.FacilityManagement.Tests.Setup;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    [TestClass]
    public class FacilityValidationTests : BaseRepositoryTest
    {
        [TestMethod]
        public void Facility_Create_WithEmptyId_ShouldThrow()
        {
            var entity = new Facility { Identifier = Guid.NewGuid().ToString(), FacilityId = string.Empty };

            var action = () => Helper.Facilities.Create(entity);

            action.Should().Throw<Exception>().WithMessage("*cannot be empty*");
        }

        [TestMethod]
        public void Facility_CreateOrUpdate_WithDuplicateIdInBatch_ShouldThrow()
        {
            var first = new Facility { Identifier = Guid.NewGuid().ToString(), FacilityId = "DUP-1" };
            var second = new Facility { Identifier = Guid.NewGuid().ToString(), FacilityId = "DUP-1" };

            var action = () => Helper.Facilities.CreateOrUpdate(new[] { first, second });

            action.Should().Throw<Exception>().WithMessage("*duplicated within the batch*");
        }

        [TestMethod]
        public void Facility_Create_WithDuplicateIdInDatabase_ShouldThrow()
        {
            var existing = new Facility { Identifier = Guid.NewGuid().ToString(), FacilityId = "EXIST-1" };
            Helper.Facilities.Create(existing);

            var duplicate = new Facility { Identifier = Guid.NewGuid().ToString(), FacilityId = "EXIST-1" };
            var action = () => Helper.Facilities.Create(duplicate);

            action.Should().Throw<Exception>().WithMessage("*already in use*");
        }
    }
}
