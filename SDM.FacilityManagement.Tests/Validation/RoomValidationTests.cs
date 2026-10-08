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
    public class RoomValidationTests : BaseRepositoryTest
    {
        [TestMethod]
        public void Room_Create_WithEmptyId_ShouldThrow()
        {
            var entity = new Room { Identifier = Guid.NewGuid().ToString(), Name = "Room", RoomId = string.Empty };

            var action = () => Helper.Rooms.Create(Helper.AttachFloor(entity));

            action.Should().Throw<Exception>().WithMessage("*cannot be empty*");
        }

        [TestMethod]
        public void Room_Create_WithEmptyName_ShouldThrow()
        {
            var entity = new Room { Identifier = Guid.NewGuid().ToString(), Name = string.Empty, RoomId = "ROOM-1" };

            var action = () => Helper.Rooms.Create(Helper.AttachFloor(entity));

            action.Should().Throw<Exception>().WithMessage("*Room Name cannot be empty*");
        }

        [TestMethod]
        public void Room_Create_WithNegativeWidth_ShouldThrow()
        {
            var entity = new Room { Identifier = Guid.NewGuid().ToString(), Name = "Room", RoomId = "ROOM-1", Width = -1 };

            var action = () => Helper.Rooms.Create(Helper.AttachFloor(entity));

            action.Should().Throw<Exception>().WithMessage("*width cannot be negative*");
        }

        [TestMethod]
        public void Room_Create_WithNegativeDepth_ShouldThrow()
        {
            var entity = new Room { Identifier = Guid.NewGuid().ToString(), Name = "Room", RoomId = "ROOM-1", Depth = -1 };

            var action = () => Helper.Rooms.Create(Helper.AttachFloor(entity));

            action.Should().Throw<Exception>().WithMessage("*depth cannot be negative*");
        }

        [TestMethod]
        public void Room_CreateOrUpdate_WithNegativeWidthInBatch_ShouldThrow()
        {
            var first = new Room { Identifier = Guid.NewGuid().ToString(), Name = "Room 1", RoomId = "ROOM-1", Width = -5 };
            var second = new Room { Identifier = Guid.NewGuid().ToString(), Name = "Room 2", RoomId = "ROOM-2" };

            var action = () => Helper.Rooms.CreateOrUpdate(new[] { first, second });

            action.Should().Throw<Exception>().WithMessage("*width cannot be negative*");
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow(0L)]
        [DataRow(10L)]
        public void RoomValidationHandler_WithNullOrNonNegativeWidthAndDepth_ShouldBeValid(long? value)
        {
            var entity = new Room { Width = value, Depth = value };

            RoomValidationHandler.IsRoomWidthValid(entity, out _).Should().BeTrue();
            RoomValidationHandler.IsRoomDepthValid(entity, out _).Should().BeTrue();
        }

        [TestMethod]
        public void RoomValidationHandler_WithNegativeWidthAndDepth_ShouldReturnExactMessage()
        {
            var entity = new Room { Width = -1, Depth = -2 };

            RoomValidationHandler.IsRoomWidthValid(entity, out var widthResult).Should().BeFalse();
            RoomValidationHandler.IsRoomDepthValid(entity, out var depthResult).Should().BeFalse();

            widthResult.GetFailReason(RoomValidationHandler.RoomValidationField.Width).Should().Be("The width cannot be negative.");
            depthResult.GetFailReason(RoomValidationHandler.RoomValidationField.Depth).Should().Be("The depth cannot be negative.");
        }

        [TestMethod]
        public void Room_CreateOrUpdate_WithDuplicateIdInBatch_ShouldThrow()
        {
            var first = new Room { Identifier = Guid.NewGuid().ToString(), Name = "Room 1", RoomId = "DUP-1" };
            var second = new Room { Identifier = Guid.NewGuid().ToString(), Name = "Room 2", RoomId = "DUP-1" };

            var action = () => Helper.Rooms.CreateOrUpdate(new[] { first, second });

            action.Should().Throw<Exception>().WithMessage("*duplicated within the batch*");
        }

        [TestMethod]
        public void Room_Create_WithDuplicateIdInDatabase_ShouldThrow()
        {
            var existing = new Room { Identifier = Guid.NewGuid().ToString(), Name = "Existing Room", RoomId = "EXIST-1" };
            Helper.Rooms.Create(Helper.AttachFloor(existing));

            var duplicate = new Room { Identifier = Guid.NewGuid().ToString(), Name = "Duplicate Room", RoomId = "EXIST-1" };
            var action = () => Helper.Rooms.Create(Helper.AttachFloor(duplicate));

            action.Should().Throw<Exception>().WithMessage("*already in use*");
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void RoomValidationHandler_WithEmptyOrWhitespaceRoomId_ShouldReturnExactMessage(string id)
        {
            var entity = new Room { RoomId = id };

            RoomValidationHandler.IsRoomIdValid(entity, out var result).Should().BeFalse();

            result.GetFailReason(RoomValidationHandler.RoomValidationField.RoomId).Should().Be("Room Id cannot be empty or whitespace.");
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void RoomValidator_IsRoomIdValid_WithEmptyOrWhitespaceId_ShouldReturnExactMessage(string id)
        {
            var validator = CreateValidator();

            var result = validator.IsRoomIdValid(id);

            result.IsValid.Should().BeFalse();
            result.GetFailReason(RoomValidationHandler.RoomValidationField.RoomId).Should().Be("Room Id cannot be empty or whitespace.");
        }

        [TestMethod]
        public void RoomValidator_IsRoomIdValid_WithUniqueId_ShouldBeValid()
        {
            var validator = CreateValidator();

            var result = validator.IsRoomIdValid("UNIQUE");

            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void RoomValidator_IsRoomIdValid_WithIdAlreadyInStore_ShouldReturnExactMessage()
        {
            Helper.Rooms.Create(Helper.AttachFloor(NewRoom("EXISTING")));
            var validator = CreateValidator();

            var result = validator.IsRoomIdValid("EXISTING");

            result.IsValid.Should().BeFalse();
            result.GetFailReason(RoomValidationHandler.RoomValidationField.RoomId).Should().Be("Room Id 'EXISTING' is already in use.");
        }

        [TestMethod]
        public void RoomValidator_ValidateBulk_WithIdInOtherChangedEntries_ShouldReturnExactMessage()
        {
            var validator = CreateValidator();

            var results = validator.ValidateBulk(new List<Room> { NewRoom("BASE"), NewRoom("DUP"), NewRoom("DUP") }, RepositoryAction.Create);

            results[0].IsValid.Should().BeTrue(results[0].GetCombinedFailureReasons(";"));
            results[1].IsValid.Should().BeFalse();
            results[1].GetFailReason(RoomValidationHandler.RoomValidationField.RoomId).Should().Be("Room Id 'DUP' is duplicated within the batch.");
            results[2].GetFailReason(RoomValidationHandler.RoomValidationField.RoomId).Should().Be("Room Id 'DUP' is duplicated within the batch.");
        }

        [TestMethod]
        public void RoomValidator_Validate_WithValidRoom_ShouldBeValid()
        {
            var floor = Helper.Floors.Create(Helper.AttachFacility(new Floor { Identifier = Guid.NewGuid().ToString(), FloorId = "FLR-VALID", Name = "Floor VALID" }));
            var entity = NewRoom("VALID");
            entity.FloorFk.Floor = new SdmObjectReference<Floor>(floor.Identifier);
            var validator = CreateValidator();

            var result = validator.Validate(entity, RepositoryAction.Create);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void RoomValidator_Validate_SavedRoomWithClearedId_ShouldBeInvalid()
        {
            var created = NewRoom("VALID");
            Helper.Rooms.Create(Helper.AttachFloor(created));
            var existing = Helper.Rooms.Read(RoomExposers.Identifier.Equal(created.Identifier)).Single();
            existing.RoomId = string.Empty;
            var validator = CreateValidator();

            var result = validator.Validate(existing, RepositoryAction.Update);

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(RoomValidationHandler.RoomValidationField.RoomId, out _).Should().BeTrue();
        }

        [TestMethod]
        public void RoomValidator_Validate_SavedRoomWithClearedName_ShouldBeInvalid()
        {
            var created = NewRoom("VALID");
            Helper.Rooms.Create(Helper.AttachFloor(created));
            var existing = Helper.Rooms.Read(RoomExposers.Identifier.Equal(created.Identifier)).Single();
            existing.Name = string.Empty;
            var validator = CreateValidator();

            var result = validator.Validate(existing, RepositoryAction.Update);

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(RoomValidationHandler.RoomValidationField.Name, out _).Should().BeTrue();
        }

        private RoomValidator CreateValidator()
        {
            return new RoomValidator(new FacilityEntityLoader(Helper));
        }

        private static Room NewRoom(string id)
        {
            return new Room { Identifier = Guid.NewGuid().ToString(), Name = "Room " + id, RoomId = id };
        }
    }
}
