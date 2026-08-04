namespace SDM.FacilityManagement.Tests.Validation
{
    using System;

    using FluentAssertions;

    using SDM.FacilityManagement.Tests.Setup;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    [TestClass]
    public class RowValidationTests : BaseRepositoryTest
    {
        [TestMethod]
        public void Row_Create_WithEmptyId_ShouldThrow()
        {
            var entity = new Row { Identifier = Guid.NewGuid().ToString(), RowId = string.Empty };

            var action = () => Helper.Rows.Create(entity);

            action.Should().Throw<Exception>().WithMessage("*cannot be empty*");
        }

        [TestMethod]
        public void Row_CreateOrUpdate_WithDuplicateIdInBatch_ShouldThrow()
        {
            var first = new Row { Identifier = Guid.NewGuid().ToString(), RowId = "DUP-1" };
            var second = new Row { Identifier = Guid.NewGuid().ToString(), RowId = "DUP-1" };

            var action = () => Helper.Rows.CreateOrUpdate(new[] { first, second });

            action.Should().Throw<Exception>().WithMessage("*duplicated within the batch*");
        }

        [TestMethod]
        public void Row_Create_WithDuplicateIdInDatabase_ShouldThrow()
        {
            var existing = new Row { Identifier = Guid.NewGuid().ToString(), RowId = "EXIST-1" };
            Helper.Rows.Create(existing);

            var duplicate = new Row { Identifier = Guid.NewGuid().ToString(), RowId = "EXIST-1" };
            var action = () => Helper.Rows.Create(duplicate);

            action.Should().Throw<Exception>().WithMessage("*already in use*");
        }
    }
}
