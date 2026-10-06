namespace SDM.FacilityManagement.Tests.Validation
{
    using System;
    using System.Linq;

    using FluentAssertions;

    using SDM.FacilityManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Services;
    using Skyline.DataMiner.SDM.FacilityManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    [TestClass]
    public class RackValidationTests : BaseRepositoryTest
    {
        [TestMethod]
        public void Rack_Create_WithEmptyId_ShouldThrow()
        {
            var entity = new Rack { Identifier = Guid.NewGuid().ToString(), Name = "Rack", RackId = string.Empty };
            entity.Capacity.MaximumRackCapacity = 42;

            var action = () => Helper.Racks.Create(Helper.AttachRow(entity));

            action.Should().Throw<Exception>().WithMessage("*cannot be empty*");
        }

        [TestMethod]
        public void Rack_Create_WithEmptyName_ShouldThrow()
        {
            var entity = new Rack { Identifier = Guid.NewGuid().ToString(), Name = string.Empty, RackId = "RACK-1" };

            var action = () => Helper.Racks.Create(Helper.AttachRow(entity));

            action.Should().Throw<Exception>().WithMessage("*Rack Name cannot be empty*");
        }

        [TestMethod]
        public void Rack_CreateOrUpdate_WithDuplicateIdInBatch_ShouldThrow()
        {
            var first = new Rack { Identifier = Guid.NewGuid().ToString(), Name = "Rack 1", RackId = "DUP-1", Position = SlcFacility_Management.Enums.RackpositionenumEnum.Bottom };
            first.Capacity.MaximumRackCapacity = 42;
            var second = new Rack { Identifier = Guid.NewGuid().ToString(), Name = "Rack 2", RackId = "DUP-1", Position = SlcFacility_Management.Enums.RackpositionenumEnum.Bottom };
            second.Capacity.MaximumRackCapacity = 42;

            var action = () => Helper.Racks.CreateOrUpdate(new[] { first, second });

            action.Should().Throw<Exception>().WithMessage("*duplicated within the batch*");
        }

        [TestMethod]
        public void Rack_Create_WithDuplicateIdInDatabase_ShouldThrow()
        {
            var existing = new Rack { Identifier = Guid.NewGuid().ToString(), Name = "Existing Rack", RackId = "EXIST-1", Position = SlcFacility_Management.Enums.RackpositionenumEnum.Bottom };
            existing.Capacity.MaximumRackCapacity = 42;
            Helper.Racks.Create(Helper.AttachRow(existing));

            var duplicate = new Rack { Identifier = Guid.NewGuid().ToString(), Name = "Duplicate Rack", RackId = "EXIST-1", Position = SlcFacility_Management.Enums.RackpositionenumEnum.Bottom };
            duplicate.Capacity.MaximumRackCapacity = 42;
            var action = () => Helper.Racks.Create(Helper.AttachRow(duplicate));

            action.Should().Throw<Exception>().WithMessage("*already in use*");
        }

        [TestMethod]
        public void RackValidationHandler_WithNullRack_ShouldReturnExactMessage()
        {
            RackValidationHandler.IsRackHeightValid(null, out var result).Should().BeFalse();

            result.GetFailReason(RackValidationHandler.RackValidationField.Rack).Should().Be("Rack cannot be null.");
        }

        [TestMethod]
        public void RackValidationHandler_WithInvalidHeight_ShouldReturnExactMessage()
        {
            var rack = new Rack { Height = 321 };

            RackValidationHandler.IsRackHeightValid(rack, out var result).Should().BeFalse();

            result.GetFailReason(RackValidationHandler.RackValidationField.Height).Should().Be("Rack Height must be between 0 and 320 cm.");
        }

        [TestMethod]
        public void RackValidationHandler_WithInvalidDepth_ShouldReturnExactMessage()
        {
            var rack = new Rack { Depth = 121 };

            RackValidationHandler.IsRackDepthValid(rack, out var result).Should().BeFalse();

            result.GetFailReason(RackValidationHandler.RackValidationField.Depth).Should().Be("Rack Depth must be between 0 and 120 cm.");
        }

        [TestMethod]
        public void RackValidationHandler_WithInvalidWidth_ShouldReturnExactMessage()
        {
            var rack = new Rack { Width = -1 };

            RackValidationHandler.IsRackWidthValid(rack, out var result).Should().BeFalse();

            result.GetFailReason(RackValidationHandler.RackValidationField.Width).Should().Be("Rack Width must be between 0 and 120 cm.");
        }

        [TestMethod]
        public void RackValidationHandler_WithMissingRackUnits_ShouldReturnExactMessage()
        {
            var rack = new Rack();

            RackValidationHandler.IsRackUnitCapacityValid(rack, out var result).Should().BeFalse();

            result.GetFailReason(RackValidationHandler.RackValidationField.RackUnits).Should().Be("Rack Units cannot be empty.");
        }

        [TestMethod]
        public void RackValidationHandler_WithInvalidRackUnits_ShouldReturnExactMessage()
        {
            var rack = new Rack();
            rack.Capacity.MaximumRackCapacity = 71;

            RackValidationHandler.IsRackUnitCapacityValid(rack, out var result).Should().BeFalse();

            result.GetFailReason(RackValidationHandler.RackValidationField.RackUnits).Should().Be("Rack Units must be between 1 and 70.");
        }

        [TestMethod]
        public void RackValidationHandler_WithNegativePowerCapacity_ShouldReturnExactMessage()
        {
            var rack = new Rack();
            rack.Capacity.MaximumPowerCapacity = -1;

            RackValidationHandler.IsRackPowerCapacityValid(rack, out var result).Should().BeFalse();

            result.GetFailReason(RackValidationHandler.RackValidationField.PowerCapacity).Should().Be("Rack Power Capacity cannot be negative.");
        }

        [TestMethod]
        public void RackValidationHandler_WithMissingPosition_ShouldReturnExactMessage()
        {
            var rack = new Rack();

            RackValidationHandler.IsRackPositionValid(rack, out var result).Should().BeFalse();

            result.GetFailReason(RackValidationHandler.RackValidationField.RackPosition).Should().Be("Rack Position cannot be empty.");
        }

        [TestMethod]
        public void RackValidationHandler_WithNullRack_Position_ShouldReturnExactMessage()
        {
            RackValidationHandler.IsRackPositionValid(null, out var result).Should().BeFalse();

            result.GetFailReason(RackValidationHandler.RackValidationField.Rack).Should().Be("Rack cannot be null.");
        }

        [TestMethod]
        [DataRow(SlcFacility_Management.Enums.RackpositionenumEnum.Top)]
        [DataRow(SlcFacility_Management.Enums.RackpositionenumEnum.Bottom)]
        public void RackValidationHandler_WithPosition_ShouldBeValid(SlcFacility_Management.Enums.RackpositionenumEnum position)
        {
            var rack = new Rack { Position = position };

            RackValidationHandler.IsRackPositionValid(rack, out _).Should().BeTrue();
        }

        [TestMethod]
        public void Rack_Create_WithoutPosition_ShouldThrow()
        {
            var entity = NewRack("POS-1", null);

            var action = () => Helper.Racks.Create(Helper.AttachRow(entity));

            action.Should().Throw<Exception>().WithMessage("*Rack Position cannot be empty*");
        }

        [TestMethod]
        public void Rack_CreateOrUpdate_WithoutPositionInBatch_ShouldThrow()
        {
            var valid = NewRack("POS-2", SlcFacility_Management.Enums.RackpositionenumEnum.Top);
            var invalid = NewRack("POS-3", null);

            var action = () => Helper.Racks.CreateOrUpdate(new[] { valid, invalid });

            action.Should().Throw<Exception>().WithMessage("*Rack Position cannot be empty*");
        }

        [TestMethod]
        [DataRow(SlcFacility_Management.Enums.RackpositionenumEnum.Top)]
        [DataRow(SlcFacility_Management.Enums.RackpositionenumEnum.Bottom)]
        public void Rack_Create_WithPosition_ShouldSucceed(SlcFacility_Management.Enums.RackpositionenumEnum position)
        {
            var entity = NewRack("POS-4", position);

            var action = () => Helper.Racks.Create(Helper.AttachRow(entity));

            action.Should().NotThrow();
        }

        [TestMethod]
        public void Rack_Update_ClearingPosition_ShouldThrow()
        {
            Helper.Racks.Create(Helper.AttachRow(NewRack("POS-5", SlcFacility_Management.Enums.RackpositionenumEnum.Top)));
            var existing = Helper.Racks.Read(RackExposers.RackProperties.RackId.Equal("POS-5")).Single();
            existing.Position = null;

            var action = () => Helper.Racks.Update(existing);

            action.Should().Throw<Exception>().WithMessage("*Rack Position cannot be empty*");
        }

        [TestMethod]
        public void Rack_Update_WithoutTouchingPosition_ShouldSucceed()
        {
            Helper.Racks.Create(Helper.AttachRow(NewRack("POS-6", SlcFacility_Management.Enums.RackpositionenumEnum.Top)));
            var existing = Helper.Racks.Read(RackExposers.RackProperties.RackId.Equal("POS-6")).Single();
            existing.Name = "Renamed Rack";

            var action = () => Helper.Racks.Update(existing);

            action.Should().NotThrow();
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void RackValidationHandler_WithEmptyOrWhitespaceRackId_ShouldReturnExactMessage(string rackId)
        {
            var rack = new Rack { RackId = rackId };

            RackValidationHandler.IsRackIdValid(rack, out var result).Should().BeFalse();

            result.GetFailReason(RackValidationHandler.RackValidationField.RackId).Should().Be("Rack Id cannot be empty or whitespace.");
        }

        [TestMethod]
        public void RackValidator_IsRackIdValid_WithUniqueId_ShouldBeValid()
        {
            var validator = CreateValidator();

            var result = validator.IsRackIdValid("UNIQUE");

            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void RackValidator_IsRackIdValid_WithIdAlreadyInStore_ShouldReturnExactMessage()
        {
            Helper.Racks.Create(Helper.AttachRow(NewRack("EXISTING", SlcFacility_Management.Enums.RackpositionenumEnum.Bottom)));
            var validator = CreateValidator();

            var result = validator.IsRackIdValid("EXISTING");

            result.IsValid.Should().BeFalse();
            result.GetFailReason(RackValidationHandler.RackValidationField.RackId).Should().Be("Rack Id 'EXISTING' is already in use.");
        }

        [TestMethod]
        public void RackValidator_ValidateBulk_WithIdInOtherChangedEntries_ShouldReturnExactMessage()
        {
            var baseEntry = NewRack("BASE", SlcFacility_Management.Enums.RackpositionenumEnum.Bottom);
            var first = NewRack("DUP", SlcFacility_Management.Enums.RackpositionenumEnum.Bottom);
            var second = NewRack("DUP", SlcFacility_Management.Enums.RackpositionenumEnum.Bottom);
            var validator = CreateValidator();

            var results = validator.ValidateBulk(new List<Rack> { baseEntry, first, second }, RepositoryAction.Create);

            results[0].IsValid.Should().BeTrue();
            results[1].IsValid.Should().BeFalse();
            results[1].GetFailReason(RackValidationHandler.RackValidationField.RackId).Should().Be("Rack Id 'DUP' is duplicated within the batch.");
            results[2].GetFailReason(RackValidationHandler.RackValidationField.RackId).Should().Be("Rack Id 'DUP' is duplicated within the batch.");
        }

        [TestMethod]
        public void RackValidator_Validate_WithValidRack_ShouldBeValid()
        {
            var rack = Helper.AttachRow(NewRack("VALID", SlcFacility_Management.Enums.RackpositionenumEnum.Bottom));
            rack.Height = 200.0;
            rack.Width = 60.0;
            rack.Depth = 80.0;
            rack.Capacity.MaximumPowerCapacity = 10.0;
            var validator = CreateValidator();

            var result = validator.Validate(rack, RepositoryAction.Create);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void Rack_Update_ClearingRackId_ShouldThrow()
        {
            Helper.Racks.Create(Helper.AttachRow(NewRack("VALID-UPD", SlcFacility_Management.Enums.RackpositionenumEnum.Bottom)));
            var existing = Helper.Racks.Read(RackExposers.RackProperties.RackId.Equal("VALID-UPD")).Single();
            existing.RackId = string.Empty;

            var action = () => Helper.Racks.Update(existing);

            action.Should().Throw<Exception>().WithMessage("*Rack Id cannot be empty or whitespace*");
        }

        [TestMethod]
        public void RackValidationHandler_WithAllDimensionsAtMaximum_ShouldBeValid()
        {
            var rack = new Rack { Width = 120, Depth = 120, Height = 320 };
            rack.Capacity.MaximumRackCapacity = 70;

            RackValidationHandler.IsRackUnitCapacityValid(rack, out _).Should().BeTrue();
            RackValidationHandler.IsRackWidthValid(rack, out _).Should().BeTrue();
            RackValidationHandler.IsRackDepthValid(rack, out _).Should().BeTrue();
            RackValidationHandler.IsRackHeightValid(rack, out _).Should().BeTrue();
        }

        [TestMethod]
        [DataRow(1d)]
        [DataRow(42d)]
        [DataRow(70d)]
        public void RackValidationHandler_WithRackUnitsWithinRange_ShouldBeValid(double rackUnits)
        {
            var rack = new Rack();
            rack.Capacity.MaximumRackCapacity = rackUnits;

            RackValidationHandler.IsRackUnitCapacityValid(rack, out var result).Should().BeTrue();

            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        [Ignore("Behavior difference: failing at IsRackUnitCapacityValid rejecting 0 rack units (SDM requires 1..70); consumer IsRackSizeValid accepts 0")]
        public void RackValidationHandler_WithZeroRackUnits_ShouldBeValid()
        {
            var rack = new Rack();
            rack.Capacity.MaximumRackCapacity = 0;

            RackValidationHandler.IsRackUnitCapacityValid(rack, out var result).Should().BeTrue();

            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        [DataRow(-1d)]
        [DataRow(0d)]
        public void RackValidationHandler_WithRackUnitsBelowMinimum_ShouldReturnExactMessage(double rackUnits)
        {
            var rack = new Rack();
            rack.Capacity.MaximumPowerCapacity = 10.0;
            rack.Capacity.MaximumRackCapacity = rackUnits;

            RackValidationHandler.IsRackUnitCapacityValid(rack, out var result).Should().BeFalse();

            result.GetFailReason(RackValidationHandler.RackValidationField.RackUnits).Should().Be("Rack Units must be between 1 and 70.");
        }

        [TestMethod]
        public void Rack_Update_WithRackUnitsWithinRange_ShouldSucceed()
        {
            Helper.Racks.Create(Helper.AttachRow(NewRack("CAP-1", SlcFacility_Management.Enums.RackpositionenumEnum.Bottom)));
            var existing = Helper.Racks.Read(RackExposers.RackProperties.RackId.Equal("CAP-1")).Single();
            existing.Capacity.MaximumRackCapacity = 42;

            var action = () => Helper.Racks.Update(existing);

            action.Should().NotThrow();
        }

        [TestMethod]
        public void Rack_Update_WithRackUnitsBelowMinimum_ShouldThrow()
        {
            var rack = NewRack("CAP-2", SlcFacility_Management.Enums.RackpositionenumEnum.Bottom);
            rack.Capacity.MaximumPowerCapacity = 10.0;
            Helper.Racks.Create(Helper.AttachRow(rack));
            var existing = Helper.Racks.Read(RackExposers.RackProperties.RackId.Equal("CAP-2")).Single();
            existing.Capacity.MaximumRackCapacity = 0;

            var action = () => Helper.Racks.Update(existing);

            action.Should().Throw<Exception>().WithMessage("*Rack Units must be between 1 and 70*");
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow(0d)]
        [DataRow(200d)]
        [DataRow(320d)]
        public void RackValidationHandler_WithHeightWithinRange_ShouldBeValid(double? height)
        {
            var rack = new Rack { Height = height };

            RackValidationHandler.IsRackHeightValid(rack, out var result).Should().BeTrue();

            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        [DataRow(-1d)]
        [DataRow(321d)]
        public void RackValidationHandler_WithHeightOutOfRange_ShouldReturnExactMessage(double height)
        {
            var rack = new Rack { Height = height };

            RackValidationHandler.IsRackHeightValid(rack, out var result).Should().BeFalse();

            result.GetFailReason(RackValidationHandler.RackValidationField.Height).Should().Be("Rack Height must be between 0 and 320 cm.");
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow(0d)]
        [DataRow(60d)]
        [DataRow(120d)]
        public void RackValidationHandler_WithDepthWithinRange_ShouldBeValid(double? depth)
        {
            var rack = new Rack { Depth = depth };

            RackValidationHandler.IsRackDepthValid(rack, out var result).Should().BeTrue();

            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        [DataRow(-0.1d)]
        [DataRow(-1d)]
        [DataRow(121d)]
        public void RackValidationHandler_WithDepthOutOfRange_ShouldReturnExactMessage(double depth)
        {
            var rack = new Rack { Depth = depth };

            RackValidationHandler.IsRackDepthValid(rack, out var result).Should().BeFalse();

            result.GetFailReason(RackValidationHandler.RackValidationField.Depth).Should().Be("Rack Depth must be between 0 and 120 cm.");
        }

        [TestMethod]
        [DataRow(null)]
        [DataRow(0d)]
        [DataRow(50d)]
        [DataRow(120d)]
        public void RackValidationHandler_WithWidthWithinRange_ShouldBeValid(double? width)
        {
            var rack = new Rack { Width = width };

            RackValidationHandler.IsRackWidthValid(rack, out var result).Should().BeTrue();

            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        [DataRow(-0.1d)]
        [DataRow(121d)]
        public void RackValidationHandler_WithWidthOutOfRange_ShouldReturnExactMessage(double width)
        {
            var rack = new Rack { Width = width };

            RackValidationHandler.IsRackWidthValid(rack, out var result).Should().BeFalse();

            result.GetFailReason(RackValidationHandler.RackValidationField.Width).Should().Be("Rack Width must be between 0 and 120 cm.");
        }

        [TestMethod]
        public void RackValidationHandler_WithUnsetPowerCapacity_ShouldBeValid()
        {
            var rack = new Rack();

            RackValidationHandler.IsRackPowerCapacityValid(rack, out var result).Should().BeTrue();

            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        [DataRow(0d)]
        [DataRow(10d)]
        [DataRow(10000d)]
        public void RackValidationHandler_WithNonNegativePowerCapacity_ShouldBeValid(double powerCapacity)
        {
            var rack = new Rack();
            rack.Capacity.MaximumPowerCapacity = powerCapacity;

            RackValidationHandler.IsRackPowerCapacityValid(rack, out var result).Should().BeTrue();

            result.IsValid.Should().BeTrue();
        }

        private RackValidator CreateValidator()
        {
            return new RackValidator(new FacilityEntityLoader(Helper));
        }

        private static Rack NewRack(string rackId, SlcFacility_Management.Enums.RackpositionenumEnum? position)
        {
            var rack = new Rack { Identifier = Guid.NewGuid().ToString(), Name = $"Rack {rackId}", RackId = rackId, Position = position };
            rack.Capacity.MaximumRackCapacity = 42;
            return rack;
        }
    }
}