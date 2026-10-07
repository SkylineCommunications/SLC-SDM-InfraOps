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
    public class FacilityValidationTests : BaseRepositoryTest
    {
        [TestMethod]
        public void Facility_Create_WithEmptyId_ShouldThrow()
        {
            var entity = new Facility { Identifier = Guid.NewGuid().ToString(), Name = "Facility", FacilityId = string.Empty };

            var action = () => Helper.Facilities.Create(entity);

            action.Should().Throw<Exception>().WithMessage("*cannot be empty*");
        }

        [TestMethod]
        public void Facility_Create_WithEmptyName_ShouldThrow()
        {
            var entity = new Facility { Identifier = Guid.NewGuid().ToString(), Name = string.Empty, FacilityId = "FAC-1" };

            var action = () => Helper.Facilities.Create(entity);

            action.Should().Throw<Exception>().WithMessage("*Facility Name cannot be empty*");
        }

        [TestMethod]
        [DataRow(-90.1)]
        [DataRow(90.1)]
        public void Facility_Create_WithLatitudeOutOfRange_ShouldThrow(double latitude)
        {
            var entity = new Facility { Identifier = Guid.NewGuid().ToString(), Name = "Facility", FacilityId = "FAC-1", Latitude = latitude };

            var action = () => Helper.Facilities.Create(entity);

            action.Should().Throw<Exception>().WithMessage("*latitude must be between -90 and 90*");
        }

        [TestMethod]
        [DataRow(-180.1)]
        [DataRow(180.1)]
        public void Facility_Create_WithLongitudeOutOfRange_ShouldThrow(double longitude)
        {
            var entity = new Facility { Identifier = Guid.NewGuid().ToString(), Name = "Facility", FacilityId = "FAC-1", Longitude = longitude };

            var action = () => Helper.Facilities.Create(entity);

            action.Should().Throw<Exception>().WithMessage("*longitude must be between -180 and 180*");
        }

        [TestMethod]
        public void Facility_CreateOrUpdate_WithLatitudeOutOfRangeInBatch_ShouldThrow()
        {
            var first = new Facility { Identifier = Guid.NewGuid().ToString(), Name = "Facility 1", FacilityId = "FAC-1", Latitude = 91 };
            var second = new Facility { Identifier = Guid.NewGuid().ToString(), Name = "Facility 2", FacilityId = "FAC-2" };

            var action = () => Helper.Facilities.CreateOrUpdate(new[] { first, second });

            action.Should().Throw<Exception>().WithMessage("*latitude must be between -90 and 90*");
        }

        [TestMethod]
        [DataRow(null, null)]
        [DataRow(-90.0, -180.0)]
        [DataRow(0.0, 0.0)]
        [DataRow(90.0, 180.0)]
        public void FacilityValidationHandler_WithNullOrInRangeCoordinates_ShouldBeValid(double? latitude, double? longitude)
        {
            var entity = new Facility { Latitude = latitude, Longitude = longitude };

            FacilityValidationHandler.IsFacilityLatitudeValid(entity, out _).Should().BeTrue();
            FacilityValidationHandler.IsFacilityLongitudeValid(entity, out _).Should().BeTrue();
        }

        [TestMethod]
        [DataRow(double.NaN)]
        [DataRow(double.PositiveInfinity)]
        [DataRow(double.NegativeInfinity)]
        [DataRow(-91.0)]
        [DataRow(91.0)]
        public void FacilityValidationHandler_WithInvalidLatitude_ShouldReturnExactMessage(double latitude)
        {
            var entity = new Facility { Latitude = latitude };

            FacilityValidationHandler.IsFacilityLatitudeValid(entity, out var result).Should().BeFalse();

            result.GetFailReason(FacilityValidationHandler.FacilityValidationField.Latitude).Should().Be("The latitude must be between -90 and 90.");
        }

        [TestMethod]
        [DataRow(double.NaN)]
        [DataRow(double.PositiveInfinity)]
        [DataRow(double.NegativeInfinity)]
        [DataRow(-181.0)]
        [DataRow(181.0)]
        public void FacilityValidationHandler_WithInvalidLongitude_ShouldReturnExactMessage(double longitude)
        {
            var entity = new Facility { Longitude = longitude };

            FacilityValidationHandler.IsFacilityLongitudeValid(entity, out var result).Should().BeFalse();

            result.GetFailReason(FacilityValidationHandler.FacilityValidationField.Longitude).Should().Be("The longitude must be between -180 and 180.");
        }

        [TestMethod]
        public void Facility_CreateOrUpdate_WithDuplicateIdInBatch_ShouldThrow()
        {
            var first = new Facility { Identifier = Guid.NewGuid().ToString(), Name = "Facility 1", FacilityId = "DUP-1" };
            var second = new Facility { Identifier = Guid.NewGuid().ToString(), Name = "Facility 2", FacilityId = "DUP-1" };

            var action = () => Helper.Facilities.CreateOrUpdate(new[] { first, second });

            action.Should().Throw<Exception>().WithMessage("*duplicated within the batch*");
        }

        [TestMethod]
        public void Facility_Create_WithDuplicateIdInDatabase_ShouldThrow()
        {
            var existing = new Facility { Identifier = Guid.NewGuid().ToString(), Name = "Existing Facility", FacilityId = "EXIST-1" };
            Helper.Facilities.Create(existing);

            var duplicate = new Facility { Identifier = Guid.NewGuid().ToString(), Name = "Duplicate Facility", FacilityId = "EXIST-1" };
            var action = () => Helper.Facilities.Create(duplicate);

            action.Should().Throw<Exception>().WithMessage("*already in use*");
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void FacilityValidationHandler_WithEmptyOrWhitespaceFacilityId_ShouldReturnExactMessage(string id)
        {
            var entity = new Facility { FacilityId = id };

            FacilityValidationHandler.IsFacilityIdValid(entity, out var result).Should().BeFalse();

            result.GetFailReason(FacilityValidationHandler.FacilityValidationField.FacilityId).Should().Be("Facility Id cannot be empty or whitespace.");
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void FacilityValidator_IsFacilityIdValid_WithEmptyOrWhitespaceId_ShouldReturnExactMessage(string id)
        {
            var validator = CreateValidator();

            var result = validator.IsFacilityIdValid(id);

            result.IsValid.Should().BeFalse();
            result.GetFailReason(FacilityValidationHandler.FacilityValidationField.FacilityId).Should().Be("Facility Id cannot be empty or whitespace.");
        }

        [TestMethod]
        public void FacilityValidator_IsFacilityIdValid_WithUniqueId_ShouldBeValid()
        {
            var validator = CreateValidator();

            var result = validator.IsFacilityIdValid("UNIQUE");

            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void FacilityValidator_IsFacilityIdValid_WithIdAlreadyInStore_ShouldReturnExactMessage()
        {
            Helper.Facilities.Create(NewFacility("EXISTING"));
            var validator = CreateValidator();

            var result = validator.IsFacilityIdValid("EXISTING");

            result.IsValid.Should().BeFalse();
            result.GetFailReason(FacilityValidationHandler.FacilityValidationField.FacilityId).Should().Be("Facility Id 'EXISTING' is already in use.");
        }

        [TestMethod]
        public void FacilityValidator_ValidateBulk_WithIdInOtherChangedEntries_ShouldReturnExactMessage()
        {
            var validator = CreateValidator();

            var results = validator.ValidateBulk(new List<Facility> { NewFacility("BASE"), NewFacility("DUP"), NewFacility("DUP") }, RepositoryAction.Create);

            results[0].IsValid.Should().BeTrue(results[0].GetCombinedFailureReasons(";"));
            results[1].IsValid.Should().BeFalse();
            results[1].GetFailReason(FacilityValidationHandler.FacilityValidationField.FacilityId).Should().Be("Facility Id 'DUP' is duplicated within the batch.");
            results[2].GetFailReason(FacilityValidationHandler.FacilityValidationField.FacilityId).Should().Be("Facility Id 'DUP' is duplicated within the batch.");
        }

        [TestMethod]
        public void FacilityValidator_Validate_WithValidFacility_ShouldBeValid()
        {
            var site = Helper.Sites.Create(new Site { Identifier = Guid.NewGuid().ToString(), SiteId = "SITE-VALID", Name = "Site VALID" });
            var entity = NewFacility("VALID");
            entity.SiteFk.Site = new SdmObjectReference<Site>(site.Identifier);
            var validator = CreateValidator();

            var result = validator.Validate(entity, RepositoryAction.Create);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void FacilityValidator_Validate_SavedFacilityWithClearedId_ShouldBeInvalid()
        {
            var created = NewFacility("VALID");
            Helper.Facilities.Create(created);
            var existing = Helper.Facilities.Read(FacilityExposers.Identifier.Equal(created.Identifier)).Single();
            existing.FacilityId = string.Empty;
            var validator = CreateValidator();

            var result = validator.Validate(existing, RepositoryAction.Update);

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(FacilityValidationHandler.FacilityValidationField.FacilityId, out _).Should().BeTrue();
        }

        [TestMethod]
        public void FacilityValidator_Validate_SavedFacilityWithClearedName_ShouldBeInvalid()
        {
            var created = NewFacility("VALID");
            Helper.Facilities.Create(created);
            var existing = Helper.Facilities.Read(FacilityExposers.Identifier.Equal(created.Identifier)).Single();
            existing.Name = string.Empty;
            var validator = CreateValidator();

            var result = validator.Validate(existing, RepositoryAction.Update);

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(FacilityValidationHandler.FacilityValidationField.Name, out _).Should().BeTrue();
        }

        private FacilityValidator CreateValidator()
        {
            return new FacilityValidator(new FacilityEntityLoader(Helper));
        }

        private static Facility NewFacility(string id)
        {
            return new Facility { Identifier = Guid.NewGuid().ToString(), Name = "Facility " + id, FacilityId = id };
        }
    }
}
