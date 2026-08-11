namespace SDM.FacilityManagement.Tests.Validation
{
    using System;

    using FluentAssertions;

    using SDM.FacilityManagement.Tests.Setup;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    [TestClass]
    public class SiteValidationTests : BaseRepositoryTest
    {
        [TestMethod]
        public void Site_Create_WithEmptyId_ShouldThrow()
        {
            var entity = new Site { Identifier = Guid.NewGuid().ToString(), SiteId = string.Empty };

            var action = () => Helper.Sites.Create(entity);

            action.Should().Throw<Exception>().WithMessage("*cannot be empty*");
        }

        [TestMethod]
        public void Site_CreateOrUpdate_WithDuplicateIdInBatch_ShouldThrow()
        {
            var first = new Site { Identifier = Guid.NewGuid().ToString(), SiteId = "DUP-1" };
            var second = new Site { Identifier = Guid.NewGuid().ToString(), SiteId = "DUP-1" };

            var action = () => Helper.Sites.CreateOrUpdate(new[] { first, second });

            action.Should().Throw<Exception>().WithMessage("*duplicated within the batch*");
        }

        [TestMethod]
        public void Site_Create_WithDuplicateIdInDatabase_ShouldThrow()
        {
            var existing = new Site { Identifier = Guid.NewGuid().ToString(), SiteId = "EXIST-1" };
            Helper.Sites.Create(existing);

            var duplicate = new Site { Identifier = Guid.NewGuid().ToString(), SiteId = "EXIST-1" };
            var action = () => Helper.Sites.Create(duplicate);

            action.Should().Throw<Exception>().WithMessage("*already in use*");
        }
    }
}
