namespace SDM.FacilityManagement.Tests.Validation
{
    using System;

    using FluentAssertions;

    using SDM.FacilityManagement.Tests.Setup;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    [TestClass]
    public class RoomValidationTests : BaseRepositoryTest
    {
        [TestMethod]
        public void Room_Create_WithEmptyId_ShouldThrow()
        {
            var entity = new Room { Identifier = Guid.NewGuid().ToString(), RoomId = string.Empty };

            var action = () => Helper.Rooms.Create(entity);

            action.Should().Throw<Exception>().WithMessage("*cannot be empty*");
        }

        [TestMethod]
        public void Room_CreateOrUpdate_WithDuplicateIdInBatch_ShouldThrow()
        {
            var first = new Room { Identifier = Guid.NewGuid().ToString(), RoomId = "DUP-1" };
            var second = new Room { Identifier = Guid.NewGuid().ToString(), RoomId = "DUP-1" };

            var action = () => Helper.Rooms.CreateOrUpdate(new[] { first, second });

            action.Should().Throw<Exception>().WithMessage("*duplicated within the batch*");
        }

        [TestMethod]
        public void Room_Create_WithDuplicateIdInDatabase_ShouldThrow()
        {
            var existing = new Room { Identifier = Guid.NewGuid().ToString(), RoomId = "EXIST-1" };
            Helper.Rooms.Create(existing);

            var duplicate = new Room { Identifier = Guid.NewGuid().ToString(), RoomId = "EXIST-1" };
            var action = () => Helper.Rooms.Create(duplicate);

            action.Should().Throw<Exception>().WithMessage("*already in use*");
        }
    }
}
