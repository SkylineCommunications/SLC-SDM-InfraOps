namespace SDM.FacilityManagement.Tests.Validation
{
    using System;
    using System.Linq;

    using FluentAssertions;

    using SDM.FacilityManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Validation;

    [TestClass]
    public class RackValidationTests : BaseRepositoryTest
    {
        [TestMethod]
        public void Rack_Create_WithEmptyId_ShouldThrow()
        {
            var entity = new Rack { Identifier = Guid.NewGuid().ToString(), Name = "Rack", RackId = string.Empty };
            entity.Capacity.MaximumRackCapacity = 42;

            var action = () => Helper.Racks.Create(entity);

            action.Should().Throw<Exception>().WithMessage("*cannot be empty*");
        }

        [TestMethod]
        public void Rack_Create_WithEmptyName_ShouldThrow()
        {
            var entity = new Rack { Identifier = Guid.NewGuid().ToString(), Name = string.Empty, RackId = "RACK-1" };

            var action = () => Helper.Racks.Create(entity);

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
            Helper.Racks.Create(existing);

            var duplicate = new Rack { Identifier = Guid.NewGuid().ToString(), Name = "Duplicate Rack", RackId = "EXIST-1", Position = SlcFacility_Management.Enums.RackpositionenumEnum.Bottom };
            duplicate.Capacity.MaximumRackCapacity = 42;
            var action = () => Helper.Racks.Create(duplicate);

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

            var action = () => Helper.Racks.Create(entity);

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

            var action = () => Helper.Racks.Create(entity);

            action.Should().NotThrow();
        }

        [TestMethod]
        public void Rack_Update_ClearingPosition_ShouldThrow()
        {
            Helper.Racks.Create(NewRack("POS-5", SlcFacility_Management.Enums.RackpositionenumEnum.Top));
            var existing = Helper.Racks.Read(RackExposers.RackProperties.RackId.Equal("POS-5")).Single();
            existing.Position = null;

            var action = () => Helper.Racks.Update(existing);

            action.Should().Throw<Exception>().WithMessage("*Rack Position cannot be empty*");
        }

        [TestMethod]
        public void Rack_Update_WithoutTouchingPosition_ShouldSucceed()
        {
            Helper.Racks.Create(NewRack("POS-6", SlcFacility_Management.Enums.RackpositionenumEnum.Top));
            var existing = Helper.Racks.Read(RackExposers.RackProperties.RackId.Equal("POS-6")).Single();
            existing.Name = "Renamed Rack";

            var action = () => Helper.Racks.Update(existing);

            action.Should().NotThrow();
        }

        private static Rack NewRack(string rackId, SlcFacility_Management.Enums.RackpositionenumEnum? position)
        {
            var rack = new Rack { Identifier = Guid.NewGuid().ToString(), Name = $"Rack {rackId}", RackId = rackId, Position = position };
            rack.Capacity.MaximumRackCapacity = 42;
            return rack;
        }
    }
}