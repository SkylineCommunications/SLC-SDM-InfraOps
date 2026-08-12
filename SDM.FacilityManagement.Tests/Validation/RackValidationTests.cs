namespace SDM.FacilityManagement.Tests.Validation
{
    using System;

    using FluentAssertions;

    using SDM.FacilityManagement.Tests.Setup;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Validation;

    [TestClass]
    public class RackValidationTests : BaseRepositoryTest
    {
        [TestMethod]
        public void Rack_Create_WithEmptyId_ShouldThrow()
        {
            var entity = new Rack { Identifier = Guid.NewGuid().ToString(), RackId = string.Empty };
            entity.Capacity.MaximumRackCapacity = 42;

            var action = () => Helper.Racks.Create(entity);

            action.Should().Throw<Exception>().WithMessage("*cannot be empty*");
        }

        [TestMethod]
        public void Rack_CreateOrUpdate_WithDuplicateIdInBatch_ShouldThrow()
        {
            var first = new Rack { Identifier = Guid.NewGuid().ToString(), RackId = "DUP-1" };
            first.Capacity.MaximumRackCapacity = 42;
            var second = new Rack { Identifier = Guid.NewGuid().ToString(), RackId = "DUP-1" };
            second.Capacity.MaximumRackCapacity = 42;

            var action = () => Helper.Racks.CreateOrUpdate(new[] { first, second });

            action.Should().Throw<Exception>().WithMessage("*duplicated within the batch*");
        }

        [TestMethod]
        public void Rack_Create_WithDuplicateIdInDatabase_ShouldThrow()
        {
            var existing = new Rack { Identifier = Guid.NewGuid().ToString(), RackId = "EXIST-1" };
            existing.Capacity.MaximumRackCapacity = 42;
            Helper.Racks.Create(existing);

            var duplicate = new Rack { Identifier = Guid.NewGuid().ToString(), RackId = "EXIST-1" };
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
    }
}
