namespace SDM.FacilityManagement.Tests.Validation
{
    using System;

    using FluentAssertions;

    using SDM.FacilityManagement.Tests.Setup;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    [TestClass]
    public class DeskValidationTests : BaseRepositoryTest
    {
        [TestMethod]
        public void Desk_Create_WithEmptyId_ShouldThrow()
        {
            var entity = new Desk { Identifier = Guid.NewGuid().ToString(), DeskID = string.Empty };

            var action = () => Helper.Desks.Create(entity);

            action.Should().Throw<Exception>().WithMessage("*cannot be empty*");
        }

        [TestMethod]
        public void Desk_CreateOrUpdate_WithDuplicateIdInBatch_ShouldThrow()
        {
            var first = new Desk { Identifier = Guid.NewGuid().ToString(), DeskID = "DUP-1" };
            var second = new Desk { Identifier = Guid.NewGuid().ToString(), DeskID = "DUP-1" };

            var action = () => Helper.Desks.CreateOrUpdate(new[] { first, second });

            action.Should().Throw<Exception>().WithMessage("*duplicated within the batch*");
        }

        [TestMethod]
        public void Desk_Create_WithDuplicateIdInDatabase_ShouldThrow()
        {
            var existing = new Desk { Identifier = Guid.NewGuid().ToString(), DeskID = "EXIST-1" };
            Helper.Desks.Create(existing);

            var duplicate = new Desk { Identifier = Guid.NewGuid().ToString(), DeskID = "EXIST-1" };
            var action = () => Helper.Desks.Create(duplicate);

            action.Should().Throw<Exception>().WithMessage("*already in use*");
        }
    }
}
