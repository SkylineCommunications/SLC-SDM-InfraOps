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
    public class FloorValidationTests : BaseRepositoryTest
    {
        [TestMethod]
        public void Floor_Create_WithEmptyId_ShouldThrow()
        {
            var entity = new Floor { Identifier = Guid.NewGuid().ToString(), FloorId = string.Empty };

            var action = () => Helper.Floors.Create(Helper.AttachFacility(entity));

            action.Should().Throw<Exception>().WithMessage("*cannot be empty*");
        }

        [TestMethod]
        public void Floor_CreateOrUpdate_WithDuplicateIdInBatch_ShouldThrow()
        {
            var first = new Floor { Identifier = Guid.NewGuid().ToString(), FloorId = "DUP-1" };
            var second = new Floor { Identifier = Guid.NewGuid().ToString(), FloorId = "DUP-1" };

            var action = () => Helper.Floors.CreateOrUpdate(new[] { first, second });

            action.Should().Throw<Exception>().WithMessage("*duplicated within the batch*");
        }

        [TestMethod]
        public void Floor_Create_WithDuplicateIdInDatabase_ShouldThrow()
        {
            var existing = new Floor { Identifier = Guid.NewGuid().ToString(), FloorId = "EXIST-1", Name = "Floor EXIST-1" };
            Helper.Floors.Create(Helper.AttachFacility(existing));

            var duplicate = new Floor { Identifier = Guid.NewGuid().ToString(), FloorId = "EXIST-1", Name = "Floor EXIST-1" };
            var action = () => Helper.Floors.Create(Helper.AttachFacility(duplicate));

            action.Should().Throw<Exception>().WithMessage("*already in use*");
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void FloorValidationHandler_WithEmptyOrWhitespaceFloorId_ShouldReturnExactMessage(string id)
        {
            var entity = new Floor { FloorId = id };

            FloorValidationHandler.IsFloorIdValid(entity, out var result).Should().BeFalse();

            result.GetFailReason(FloorValidationHandler.FloorValidationField.FloorId).Should().Be("Floor Id cannot be empty or whitespace.");
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void FloorValidator_IsFloorIdValid_WithEmptyOrWhitespaceId_ShouldReturnExactMessage(string id)
        {
            var validator = CreateValidator();

            var result = validator.IsFloorIdValid(id);

            result.IsValid.Should().BeFalse();
            result.GetFailReason(FloorValidationHandler.FloorValidationField.FloorId).Should().Be("Floor Id cannot be empty or whitespace.");
        }

        [TestMethod]
        public void FloorValidator_IsFloorIdValid_WithUniqueId_ShouldBeValid()
        {
            var validator = CreateValidator();

            var result = validator.IsFloorIdValid("UNIQUE");

            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void FloorValidator_IsFloorIdValid_WithIdAlreadyInStore_ShouldReturnExactMessage()
        {
            Helper.Floors.Create(Helper.AttachFacility(NewFloor("EXISTING")));
            var validator = CreateValidator();

            var result = validator.IsFloorIdValid("EXISTING");

            result.IsValid.Should().BeFalse();
            result.GetFailReason(FloorValidationHandler.FloorValidationField.FloorId).Should().Be("Floor Id 'EXISTING' is already in use.");
        }

        [TestMethod]
        public void FloorValidator_ValidateBulk_WithIdInOtherChangedEntries_ShouldReturnExactMessage()
        {
            var validator = CreateValidator();

            var results = validator.ValidateBulk(new List<Floor> { NewFloor("BASE"), NewFloor("DUP"), NewFloor("DUP") }, RepositoryAction.Create);

            results[0].IsValid.Should().BeTrue(results[0].GetCombinedFailureReasons(";"));
            results[1].IsValid.Should().BeFalse();
            results[1].GetFailReason(FloorValidationHandler.FloorValidationField.FloorId).Should().Be("Floor Id 'DUP' is duplicated within the batch.");
            results[2].GetFailReason(FloorValidationHandler.FloorValidationField.FloorId).Should().Be("Floor Id 'DUP' is duplicated within the batch.");
        }

        [TestMethod]
        public void FloorValidator_Validate_WithValidFloor_ShouldBeValid()
        {
            var facility = Helper.Facilities.Create(new Facility { Identifier = Guid.NewGuid().ToString(), FacilityId = "FAC-VALID", Name = "Facility VALID" });
            var entity = NewFloor("VALID");
            entity.FacilityFk.Facility = new SdmObjectReference<Facility>(facility.Identifier);
            var validator = CreateValidator();

            var result = validator.Validate(entity, RepositoryAction.Create);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void FloorValidator_Validate_SavedFloorWithClearedId_ShouldBeInvalid()
        {
            var created = NewFloor("VALID");
            Helper.Floors.Create(Helper.AttachFacility(created));
            var existing = Helper.Floors.Read(FloorExposers.Identifier.Equal(created.Identifier)).Single();
            existing.FloorId = string.Empty;
            var validator = CreateValidator();

            var result = validator.Validate(existing, RepositoryAction.Update);

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(FloorValidationHandler.FloorValidationField.FloorId, out _).Should().BeTrue();
        }

        private FloorValidator CreateValidator()
        {
            return new FloorValidator(new FacilityEntityLoader(Helper));
        }

        private static Floor NewFloor(string id)
        {
            return new Floor { Identifier = Guid.NewGuid().ToString(), Name = "Floor " + id, FloorId = id };
        }
    }
}
