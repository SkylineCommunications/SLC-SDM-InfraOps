namespace SDM.FacilityManagement.Tests.Validation
{
    using System;

    using FluentAssertions;

    using SDM.FacilityManagement.Tests.Setup;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    [TestClass]
    public class RackValidationTests : BaseRepositoryTest
    {
        [TestMethod]
        public void Rack_Create_WithEmptyId_ShouldThrow()
        {
            var entity = new Rack { Identifier = Guid.NewGuid().ToString(), RackId = string.Empty };

            var action = () => Helper.Racks.Create(entity);

            action.Should().Throw<Exception>().WithMessage("*cannot be empty*");
        }

        [TestMethod]
        public void Rack_CreateOrUpdate_WithDuplicateIdInBatch_ShouldThrow()
        {
            var first = new Rack { Identifier = Guid.NewGuid().ToString(), RackId = "DUP-1" };
            var second = new Rack { Identifier = Guid.NewGuid().ToString(), RackId = "DUP-1" };

            var action = () => Helper.Racks.CreateOrUpdate(new[] { first, second });

            action.Should().Throw<Exception>().WithMessage("*duplicated within the batch*");
        }

        [TestMethod]
        public void Rack_Create_WithDuplicateIdInDatabase_ShouldThrow()
        {
            var existing = new Rack { Identifier = Guid.NewGuid().ToString(), RackId = "EXIST-1" };
            Helper.Racks.Create(existing);

            var duplicate = new Rack { Identifier = Guid.NewGuid().ToString(), RackId = "EXIST-1" };
            var action = () => Helper.Racks.Create(duplicate);

            action.Should().Throw<Exception>().WithMessage("*already in use*");
        }
    }
}
