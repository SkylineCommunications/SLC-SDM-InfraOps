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
            var entity = new Row { Identifier = Guid.NewGuid().ToString(), Name = "Row", RowId = string.Empty };

            var action = () => Helper.Rows.Create(entity);

            action.Should().Throw<Exception>().WithMessage("*cannot be empty*");
        }

        [TestMethod]
        public void Row_Create_WithEmptyName_ShouldThrow()
        {
            var entity = new Row { Identifier = Guid.NewGuid().ToString(), Name = string.Empty, RowId = "ROW-1" };

            var action = () => Helper.Rows.Create(entity);

            action.Should().Throw<Exception>().WithMessage("*Row Name cannot be empty*");
        }

        [TestMethod]
        public void Row_CreateOrUpdate_WithDuplicateIdInBatch_ShouldThrow()
        {
            var first = new Row { Identifier = Guid.NewGuid().ToString(), Name = "Row 1", RowId = "DUP-1" };
            var second = new Row { Identifier = Guid.NewGuid().ToString(), Name = "Row 2", RowId = "DUP-1" };

            var action = () => Helper.Rows.CreateOrUpdate(new[] { first, second });

            action.Should().Throw<Exception>().WithMessage("*duplicated within the batch*");
        }

        [TestMethod]
        public void Row_Create_WithDuplicateIdInDatabase_ShouldThrow()
        {
            var existing = new Row { Identifier = Guid.NewGuid().ToString(), Name = "Existing Row", RowId = "EXIST-1" };
            Helper.Rows.Create(existing);

            var duplicate = new Row { Identifier = Guid.NewGuid().ToString(), Name = "Duplicate Row", RowId = "EXIST-1" };
            var action = () => Helper.Rows.Create(duplicate);

            action.Should().Throw<Exception>().WithMessage("*already in use*");
        }
    }
}
