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
    public class DeskValidationTests : BaseRepositoryTest
    {
        [TestMethod]
        public void Desk_Create_WithEmptyId_ShouldThrow()
        {
            var entity = new Desk { Identifier = Guid.NewGuid().ToString(), Name = "Desk", DeskID = string.Empty };

            var action = () => Helper.Desks.Create(Helper.AttachRoom(entity));

            action.Should().Throw<Exception>().WithMessage("*cannot be empty*");
        }

        [TestMethod]
        public void Desk_Create_WithEmptyName_ShouldThrow()
        {
            var entity = new Desk { Identifier = Guid.NewGuid().ToString(), Name = string.Empty, DeskID = "DSK-1" };

            var action = () => Helper.Desks.Create(Helper.AttachRoom(entity));

            action.Should().Throw<Exception>().WithMessage("*Desk Name cannot be empty*");
        }

        [TestMethod]
        public void Desk_CreateOrUpdate_WithDuplicateIdInBatch_ShouldThrow()
        {
            var first = new Desk { Identifier = Guid.NewGuid().ToString(), Name = "Desk 1", DeskID = "DUP-1" };
            var second = new Desk { Identifier = Guid.NewGuid().ToString(), Name = "Desk 2", DeskID = "DUP-1" };

            var action = () => Helper.Desks.CreateOrUpdate(new[] { first, second });

            action.Should().Throw<Exception>().WithMessage("*duplicated within the batch*");
        }

        [TestMethod]
        public void Desk_Create_WithDuplicateIdInDatabase_ShouldThrow()
        {
            var existing = new Desk { Identifier = Guid.NewGuid().ToString(), Name = "Existing Desk", DeskID = "EXIST-1" };
            Helper.Desks.Create(Helper.AttachRoom(existing));

            var duplicate = new Desk { Identifier = Guid.NewGuid().ToString(), Name = "Duplicate Desk", DeskID = "EXIST-1" };
            var action = () => Helper.Desks.Create(Helper.AttachRoom(duplicate));

            action.Should().Throw<Exception>().WithMessage("*already in use*");
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void DeskValidationHandler_WithEmptyOrWhitespaceDeskId_ShouldReturnExactMessage(string deskId)
        {
            var desk = new Desk { DeskID = deskId };

            DeskValidationHandler.IsDeskIdValid(desk, out var result).Should().BeFalse();

            result.GetFailReason(DeskValidationHandler.DeskValidationField.DeskId).Should().Be("Desk Id cannot be empty or whitespace.");
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void DeskValidator_IsDeskIdValid_WithEmptyOrWhitespaceId_ShouldReturnExactMessage(string deskId)
        {
            var validator = CreateValidator();

            var result = validator.IsDeskIdValid(deskId);

            result.IsValid.Should().BeFalse();
            result.GetFailReason(DeskValidationHandler.DeskValidationField.DeskId).Should().Be("Desk Id cannot be empty or whitespace.");
        }

        [TestMethod]
        public void DeskValidator_IsDeskIdValid_WithUniqueId_ShouldBeValid()
        {
            var validator = CreateValidator();

            var result = validator.IsDeskIdValid("UNIQUE");

            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void DeskValidator_IsDeskIdValid_WithIdAlreadyInStore_ShouldReturnExactMessage()
        {
            Helper.Desks.Create(Helper.AttachRoom(NewDesk("EXISTING")));
            var validator = CreateValidator();

            var result = validator.IsDeskIdValid("EXISTING");

            result.IsValid.Should().BeFalse();
            result.GetFailReason(DeskValidationHandler.DeskValidationField.DeskId).Should().Be("Desk Id 'EXISTING' is already in use.");
        }

        [TestMethod]
        public void DeskValidator_ValidateBulk_WithIdInOtherChangedEntries_ShouldReturnExactMessage()
        {
            var validator = CreateValidator();

            var results = validator.ValidateBulk(new List<Desk> { NewDesk("BASE"), NewDesk("DUP"), NewDesk("DUP") }, RepositoryAction.Create);

            results[0].IsValid.Should().BeTrue();
            results[1].IsValid.Should().BeFalse();
            results[1].GetFailReason(DeskValidationHandler.DeskValidationField.DeskId).Should().Be("Desk Id 'DUP' is duplicated within the batch.");
            results[2].GetFailReason(DeskValidationHandler.DeskValidationField.DeskId).Should().Be("Desk Id 'DUP' is duplicated within the batch.");
        }

        [TestMethod]
        public void DeskValidator_Validate_WithValidDesk_ShouldBeValid()
        {
            var room = Helper.Rooms.Create(Helper.AttachFloor(new Room { Identifier = Guid.NewGuid().ToString(), RoomId = "ROOM-VALID", Name = "Room VALID" }));
            var desk = NewDesk("VALID");
            desk.RoomFk.Room = new SdmObjectReference<Room>(room.Identifier);
            var validator = CreateValidator();

            var result = validator.Validate(desk, RepositoryAction.Create);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void DeskValidator_Validate_SavedDeskWithClearedId_ShouldBeInvalid()
        {
            Helper.Desks.Create(Helper.AttachRoom(NewDesk("VALID")));
            var existing = Helper.Desks.Read(DeskExposers.DeskInformation.DeskID.Equal("VALID")).Single();
            existing.DeskID = string.Empty;
            var validator = CreateValidator();

            var result = validator.Validate(existing, RepositoryAction.Update);

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(DeskValidationHandler.DeskValidationField.DeskId, out _).Should().BeTrue();
        }

        private DeskValidator CreateValidator()
        {
            return new DeskValidator(new FacilityEntityLoader(Helper));
        }

        private static Desk NewDesk(string deskId)
        {
            return new Desk { Identifier = Guid.NewGuid().ToString(), Name = $"Desk {deskId}", DeskID = deskId };
        }
    }
}
