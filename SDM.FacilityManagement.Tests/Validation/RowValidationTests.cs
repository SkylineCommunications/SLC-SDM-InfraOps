namespace SDM.FacilityManagement.Tests.Validation
{
    using System;
    using System.Linq;

    using FluentAssertions;

    using SDM.FacilityManagement.Tests.Setup;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.FacilityManagement.Services;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.FacilityManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

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

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void RowValidationHandler_WithEmptyOrWhitespaceRowId_ShouldReturnExactMessage(string id)
        {
            var entity = new Row { RowId = id };

            RowValidationHandler.IsRowIdValid(entity, out var result).Should().BeFalse();

            result.GetFailReason(RowValidationHandler.RowValidationField.RowId).Should().Be("Row Id cannot be empty or whitespace.");
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void RowValidator_IsRowIdValid_WithEmptyOrWhitespaceId_ShouldReturnExactMessage(string id)
        {
            var validator = CreateValidator();

            var result = validator.IsRowIdValid(id);

            result.IsValid.Should().BeFalse();
            result.GetFailReason(RowValidationHandler.RowValidationField.RowId).Should().Be("Row Id cannot be empty or whitespace.");
        }

        [TestMethod]
        public void RowValidator_IsRowIdValid_WithUniqueId_ShouldBeValid()
        {
            var validator = CreateValidator();

            var result = validator.IsRowIdValid("UNIQUE");

            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void RowValidator_IsRowIdValid_WithIdAlreadyInStore_ShouldReturnExactMessage()
        {
            Helper.Rows.Create(NewRow("EXISTING"));
            var validator = CreateValidator();

            var result = validator.IsRowIdValid("EXISTING");

            result.IsValid.Should().BeFalse();
            result.GetFailReason(RowValidationHandler.RowValidationField.RowId).Should().Be("Row Id 'EXISTING' is already in use.");
        }

        [TestMethod]
        public void RowValidator_ValidateBulk_WithIdInOtherChangedEntries_ShouldReturnExactMessage()
        {
            var validator = CreateValidator();

            var results = validator.ValidateBulk(new List<Row> { NewRow("BASE"), NewRow("DUP"), NewRow("DUP") }, RepositoryAction.Create);

            results[0].IsValid.Should().BeTrue(results[0].GetCombinedFailureReasons(";"));
            results[1].IsValid.Should().BeFalse();
            results[1].GetFailReason(RowValidationHandler.RowValidationField.RowId).Should().Be("Row Id 'DUP' is duplicated within the batch.");
            results[2].GetFailReason(RowValidationHandler.RowValidationField.RowId).Should().Be("Row Id 'DUP' is duplicated within the batch.");
        }

        [TestMethod]
        public void RowValidator_Validate_WithValidRow_ShouldBeValid()
        {
            var room = Helper.Rooms.Create(new Room { Identifier = Guid.NewGuid().ToString(), RoomId = "ROOM-VALID", Name = "Room VALID" });
            var entity = NewRow("VALID");
            entity.RoomFk.Room = new SdmObjectReference<Room>(room.Identifier);
            var validator = CreateValidator();

            var result = validator.Validate(entity, RepositoryAction.Create);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void RowValidator_Validate_SavedRowWithClearedId_ShouldBeInvalid()
        {
            var created = NewRow("VALID");
            Helper.Rows.Create(created);
            var existing = Helper.Rows.Read(RowExposers.Identifier.Equal(created.Identifier)).Single();
            existing.RowId = string.Empty;
            var validator = CreateValidator();

            var result = validator.Validate(existing, RepositoryAction.Update);

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(RowValidationHandler.RowValidationField.RowId, out _).Should().BeTrue();
        }

        private RowValidator CreateValidator()
        {
            return new RowValidator(new FacilityEntityLoader(Helper));
        }

        private static Row NewRow(string id)
        {
            return new Row { Identifier = Guid.NewGuid().ToString(), Name = "Row " + id, RowId = id };
        }
    }
}
