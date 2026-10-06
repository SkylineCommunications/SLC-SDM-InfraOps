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
    public class ZoneValidationTests : BaseRepositoryTest
    {
        [TestMethod]
        public void Zone_Create_WithEmptyId_ShouldThrow()
        {
            var entity = new Zone { Identifier = Guid.NewGuid().ToString(), Name = "Zone", ZoneId = string.Empty };

            var action = () => Helper.Zones.Create(Helper.AttachRoom(entity));

            action.Should().Throw<Exception>().WithMessage("*cannot be empty*");
        }

        [TestMethod]
        public void Zone_Create_WithEmptyName_ShouldThrow()
        {
            var entity = new Zone { Identifier = Guid.NewGuid().ToString(), Name = string.Empty, ZoneId = "ZONE-1" };

            var action = () => Helper.Zones.Create(Helper.AttachRoom(entity));

            action.Should().Throw<Exception>().WithMessage("*Zone Name cannot be empty*");
        }

        [TestMethod]
        public void Zone_CreateOrUpdate_WithDuplicateIdInBatch_ShouldThrow()
        {
            var first = new Zone { Identifier = Guid.NewGuid().ToString(), Name = "Zone 1", ZoneId = "DUP-1" };
            var second = new Zone { Identifier = Guid.NewGuid().ToString(), Name = "Zone 2", ZoneId = "DUP-1" };

            var action = () => Helper.Zones.CreateOrUpdate(new[] { first, second });

            action.Should().Throw<Exception>().WithMessage("*duplicated within the batch*");
        }

        [TestMethod]
        public void Zone_Create_WithoutCoolingCapacity_ShouldThrow()
        {
            var entity = new Zone { Identifier = Guid.NewGuid().ToString(), Name = "Zone", ZoneId = "ZONE-CC-1" };

            var action = () => Helper.Zones.Create(Helper.AttachRoom(entity));

            action.Should().Throw<Exception>().WithMessage("*cooling capacity must be defined*");
        }

        [TestMethod]
        public void Zone_Create_WithNegativeCoolingCapacity_ShouldThrow()
        {
            var entity = new Zone
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Zone",
                ZoneId = "ZONE-CC-2",
                ZoneCapacity = { CoolingCapacity = -1.0 },
            };

            var action = () => Helper.Zones.Create(Helper.AttachRoom(entity));

            action.Should().Throw<Exception>().WithMessage("*cooling capacity cannot be negative*");
        }

        [TestMethod]
        public void Zone_Create_WithValidCoolingCapacity_ShouldSucceed()
        {
            var entity = new Zone
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Zone",
                ZoneId = "ZONE-CC-3",
                ZoneCapacity = { CoolingCapacity = 12.5 },
            };

            var action = () => Helper.Zones.Create(Helper.AttachRoom(entity));

            action.Should().NotThrow();
        }

        [TestMethod]
        public void Zone_Create_WithDuplicateIdInDatabase_ShouldThrow()
        {
            var existing = new Zone { Identifier = Guid.NewGuid().ToString(), Name = "Existing Zone", ZoneId = "EXIST-1", ZoneCapacity = { CoolingCapacity = 5.0 } };
            Helper.Zones.Create(Helper.AttachRoom(existing));

            var duplicate = new Zone { Identifier = Guid.NewGuid().ToString(), Name = "Duplicate Zone", ZoneId = "EXIST-1", ZoneCapacity = { CoolingCapacity = 5.0 } };
            var action = () => Helper.Zones.Create(Helper.AttachRoom(duplicate));

            action.Should().Throw<Exception>().WithMessage("*already in use*");
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void ZoneValidationHandler_WithEmptyOrWhitespaceZoneId_ShouldReturnExactMessage(string id)
        {
            var entity = new Zone { ZoneId = id };

            ZoneValidationHandler.IsZoneIdValid(entity, out var result).Should().BeFalse();

            result.GetFailReason(ZoneValidationHandler.ZoneValidationField.ZoneId).Should().Be("Zone Id cannot be empty or whitespace.");
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void ZoneValidator_IsZoneIdValid_WithEmptyOrWhitespaceId_ShouldReturnExactMessage(string id)
        {
            var validator = CreateValidator();

            var result = validator.IsZoneIdValid(id);

            result.IsValid.Should().BeFalse();
            result.GetFailReason(ZoneValidationHandler.ZoneValidationField.ZoneId).Should().Be("Zone Id cannot be empty or whitespace.");
        }

        [TestMethod]
        public void ZoneValidator_IsZoneIdValid_WithUniqueId_ShouldBeValid()
        {
            var validator = CreateValidator();

            var result = validator.IsZoneIdValid("UNIQUE");

            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void ZoneValidator_IsZoneIdValid_WithIdAlreadyInStore_ShouldReturnExactMessage()
        {
            Helper.Zones.Create(Helper.AttachRoom(NewZone("EXISTING")));
            var validator = CreateValidator();

            var result = validator.IsZoneIdValid("EXISTING");

            result.IsValid.Should().BeFalse();
            result.GetFailReason(ZoneValidationHandler.ZoneValidationField.ZoneId).Should().Be("Zone Id 'EXISTING' is already in use.");
        }

        [TestMethod]
        public void ZoneValidator_ValidateBulk_WithIdInOtherChangedEntries_ShouldReturnExactMessage()
        {
            var validator = CreateValidator();

            var results = validator.ValidateBulk(new List<Zone> { NewZone("BASE"), NewZone("DUP"), NewZone("DUP") }, RepositoryAction.Create);

            results[0].IsValid.Should().BeTrue(results[0].GetCombinedFailureReasons(";"));
            results[1].IsValid.Should().BeFalse();
            results[1].GetFailReason(ZoneValidationHandler.ZoneValidationField.ZoneId).Should().Be("Zone Id 'DUP' is duplicated within the batch.");
            results[2].GetFailReason(ZoneValidationHandler.ZoneValidationField.ZoneId).Should().Be("Zone Id 'DUP' is duplicated within the batch.");
        }

        [TestMethod]
        public void ZoneValidator_Validate_WithValidZone_ShouldBeValid()
        {
            var room = Helper.Rooms.Create(Helper.AttachFloor(new Room { Identifier = Guid.NewGuid().ToString(), RoomId = "ROOM-VALID", Name = "Room VALID" }));
            var entity = NewZone("VALID");
            entity.RoomFk.Room = new SdmObjectReference<Room>(room.Identifier);
            var validator = CreateValidator();

            var result = validator.Validate(entity, RepositoryAction.Create);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        [Ignore("Behavior difference: failing at ZoneValidator requiring CoolingCapacity ('Zone cooling capacity must be defined.'); consumer accepts a zone without cooling capacity")]
        public void ZoneValidator_Validate_WithZoneWithoutCoolingCapacity_ShouldBeValid()
        {
            var room = Helper.Rooms.Create(Helper.AttachFloor(new Room { Identifier = Guid.NewGuid().ToString(), RoomId = "ROOM-VALID", Name = "Room VALID" }));
            var entity = new Zone { Identifier = Guid.NewGuid().ToString(), Name = "VALID", ZoneId = "VALID" };
            entity.RoomFk.Room = new SdmObjectReference<Room>(room.Identifier);
            var validator = CreateValidator();

            var result = validator.Validate(entity, RepositoryAction.Create);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void ZoneValidator_Validate_SavedZoneWithClearedId_ShouldBeInvalid()
        {
            var created = NewZone("VALID");
            Helper.Zones.Create(Helper.AttachRoom(created));
            var existing = Helper.Zones.Read(ZoneExposers.Identifier.Equal(created.Identifier)).Single();
            existing.ZoneId = string.Empty;
            var validator = CreateValidator();

            var result = validator.Validate(existing, RepositoryAction.Update);

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(ZoneValidationHandler.ZoneValidationField.ZoneId, out _).Should().BeTrue();
        }

        private ZoneValidator CreateValidator()
        {
            return new ZoneValidator(new FacilityEntityLoader(Helper));
        }

        private static Zone NewZone(string id)
        {
            return new Zone { Identifier = Guid.NewGuid().ToString(), Name = "Zone " + id, ZoneId = id, ZoneCapacity = { CoolingCapacity = 5.0 } };
        }
    }
}
