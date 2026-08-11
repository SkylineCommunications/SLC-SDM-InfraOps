namespace SDM.FacilityManagement.Tests.Validation
{
    using System;

    using FluentAssertions;

    using SDM.FacilityManagement.Tests.Setup;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    [TestClass]
    public class FloorValidationTests : BaseRepositoryTest
    {
        [TestMethod]
        public void Floor_Create_WithEmptyId_ShouldThrow()
        {
            var entity = new Floor { Identifier = Guid.NewGuid().ToString(), FloorId = string.Empty };

            var action = () => Helper.Floors.Create(entity);

            action.Should().Throw<Exception>().WithMessage("*cannot be empty*");
        }

        [TestMethod]
        public void Floor_CreateOrUpdate_WithDuplicateIdInBatch_ShouldThrow()
        {
            var first = new Floor { Identifier = Guid.NewGuid().ToString(), FloorId = "DUP-1" };
            var second = new Floor { Identifier = Guid.NewGuid().ToString(), FloorId = "DUP-1" };

            var action = () => Helper.Floors.CreateOrUpdate(new[] { first, second });

            action.Should().Throw<Exception>().WithMessage("*duplicated within the batch*");
        }

        [TestMethod]
        public void Floor_Create_WithDuplicateIdInDatabase_ShouldThrow()
        {
            var existing = new Floor { Identifier = Guid.NewGuid().ToString(), FloorId = "EXIST-1" };
            Helper.Floors.Create(existing);

            var duplicate = new Floor { Identifier = Guid.NewGuid().ToString(), FloorId = "EXIST-1" };
            var action = () => Helper.Floors.Create(duplicate);

            action.Should().Throw<Exception>().WithMessage("*already in use*");
        }
    }
}
