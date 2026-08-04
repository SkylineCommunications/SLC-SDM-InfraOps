namespace SDM.FacilityManagement.Tests.Validation
{
    using System;

    using FluentAssertions;

    using SDM.FacilityManagement.Tests.Setup;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    [TestClass]
    public class ZoneValidationTests : BaseRepositoryTest
    {
        [TestMethod]
        public void Zone_Create_WithEmptyId_ShouldThrow()
        {
            var entity = new Zone { Identifier = Guid.NewGuid().ToString(), ZoneId = string.Empty };

            var action = () => Helper.Zones.Create(entity);

            action.Should().Throw<Exception>().WithMessage("*cannot be empty*");
        }

        [TestMethod]
        public void Zone_CreateOrUpdate_WithDuplicateIdInBatch_ShouldThrow()
        {
            var first = new Zone { Identifier = Guid.NewGuid().ToString(), ZoneId = "DUP-1" };
            var second = new Zone { Identifier = Guid.NewGuid().ToString(), ZoneId = "DUP-1" };

            var action = () => Helper.Zones.CreateOrUpdate(new[] { first, second });

            action.Should().Throw<Exception>().WithMessage("*duplicated within the batch*");
        }

        [TestMethod]
        public void Zone_Create_WithDuplicateIdInDatabase_ShouldThrow()
        {
            var existing = new Zone { Identifier = Guid.NewGuid().ToString(), ZoneId = "EXIST-1" };
            Helper.Zones.Create(existing);

            var duplicate = new Zone { Identifier = Guid.NewGuid().ToString(), ZoneId = "EXIST-1" };
            var action = () => Helper.Zones.Create(duplicate);

            action.Should().Throw<Exception>().WithMessage("*already in use*");
        }
    }
}
