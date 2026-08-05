namespace SDM.AssetManagement.Tests.Assets
{
    using System;
    using System.Collections.Generic;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedCommonLibrary.AssetManagement.Models;
    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    [TestClass]
    public class AssetValidationHandlerRuleTests
    {
        [TestMethod]
        public void ParentAssetHolder_WithParentAssetButNoHolderNumber_ShouldFail()
        {
            var asset = new Asset
            {
                Location = new AssetLocation
                {
                    ParentAsset = new SdmObjectReference<Asset>(Guid.NewGuid().ToString()),
                },
            };

            var isValid = AssetValidationHandler.IsParentAssetHolderValid(asset, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Holder Number must be set when Parent Asset is provided."));
        }

        [TestMethod]
        public void DestinationParentAssetHolder_WithHolderNumberButNoParentAsset_ShouldFail()
        {
            var asset = new Asset
            {
                DestinationLocation = new AssetLocation
                {
                    HolderNumber = 1,
                },
            };

            var isValid = AssetValidationHandler.IsDestinationParentAssetHolderValid(asset, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Holder Number cannot be set when there is no Parent Asset."));
        }

        [TestMethod]
        public void DestinationRackPosition_WithNullAssetClass_ShouldFail()
        {
            var asset = new Asset
            {
                DestinationLocation = new AssetLocation
                {
                    RackId = new SdmObjectReference<Rack>(Guid.NewGuid().ToString()),
                    RackPosition = 1,
                    Side = SlcAsset_Management.Enums.SideEnum.Front,
                },
            };

            var isValid = AssetValidationHandler.IsDestinationRackPositionValid(asset, null, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Asset Class cannot be null."));
        }

        [TestMethod]
        public void DestinationRackPosition_WithNonRackUnitAssetClass_ShouldFail()
        {
            var asset = new Asset
            {
                DestinationLocation = new AssetLocation
                {
                    RackId = new SdmObjectReference<Rack>(Guid.NewGuid().ToString()),
                    RackPosition = 1,
                    Side = SlcAsset_Management.Enums.SideEnum.Front,
                },
            };
            var assetClass = new AssetClass { HeightU = 0 };

            var isValid = AssetValidationHandler.IsDestinationRackPositionValid(asset, assetClass, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Asset Class must have a Height (U) greater than 0 to be attached to a Rack."));
        }

        [TestMethod]
        public void DestinationLocationChange_WithNonTransitState_ShouldFail()
        {
            var asset = new Asset
            {
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available,
                DestinationLocation = new AssetLocation(),
            };
            asset.IsNewInternal = false;
            asset.ResetChangeTracking();
            asset.DestinationLocation.RoomId = new SdmObjectReference<Room>(Guid.NewGuid().ToString());

            var isValid = AssetValidationHandler.IsDestinationLocationChangeAllowed(asset, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Cannot change Destination Location in current State"));
        }

        [TestMethod]
        public void ReservationPlacement_WithNullReservation_ShouldFail()
        {
            var validator = new AssetValidator(new SdmEntityLoader());

            var result = validator.ValidateReservationPlacement(null);

            result.IsValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Reservation cannot be null."));
        }

        [TestMethod]
        public void ReservationPlacement_WithNoRack_ShouldFail()
        {
            var validator = new AssetValidator(new SdmEntityLoader());

            var result = validator.ValidateReservationPlacement(new InfraopsReservation());

            result.IsValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Reservation must have a Rack specified."));
        }

        [TestMethod]
        public void ReservationPlacement_WithNoPositionRanges_ShouldFail()
        {
            var validator = new AssetValidator(new SdmEntityLoader());
            var reservation = new InfraopsReservation
            {
                RackFk = new RackRelation
                {
                    Rack = new SdmObjectReference<Rack>(Guid.NewGuid().ToString()),
                },
                ReservedPositions = new List<InfraopsReservationBounderies>(),
            };

            var result = validator.ValidateReservationPlacement(reservation);

            result.IsValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Reservation must have at least one position range."));
        }

        [TestMethod]
        public void ReservationPlacement_WithUnknownRack_ShouldFail()
        {
            var validator = new AssetValidator(new SdmEntityLoader());
            var reservation = new InfraopsReservation
            {
                RackFk = new RackRelation
                {
                    Rack = new SdmObjectReference<Rack>(Guid.NewGuid().ToString()),
                },
                ReservedPositions =
                [
                    new InfraopsReservationBounderies { LowerBound = 1, UpperBound = 1 },
                ],
            };

            var result = validator.ValidateReservationPlacement(reservation);

            result.IsValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Rack not found."));
        }

        [TestMethod]
        public void RackPlacement_WithPositionLessThanOne_ShouldFail()
        {
            var rack = CreateRack();

            var isValid = RackPlacementValidation.IsAssetPlacementValid(rack, 0, 1, null, null, null, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Invalid Position. Position must be greater than 0."));
        }

        [TestMethod]
        public void RackPlacement_WithHeightLessThanOne_ShouldFail()
        {
            var rack = CreateRack();

            var isValid = RackPlacementValidation.IsAssetPlacementValid(rack, 1, 0, null, null, null, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Invalid Height. Height (U) must be greater than 0."));
        }

        [TestMethod]
        public void RackPlacement_WithReservationConflict_ShouldFail()
        {
            var rack = CreateRack();
            var reservations = new List<(InfraopsReservation Reservation, List<(long LowerBound, long UpperBound)> Ranges)>
            {
                (new InfraopsReservation { Identifier = Guid.NewGuid().ToString() }, new List<(long LowerBound, long UpperBound)> { (1, 2) }),
            };

            var isValid = RackPlacementValidation.IsAssetPlacementValid(rack, 1, 1, null, null, reservations, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Invalid Position. Rack space is already reserved"));
        }

        private static Rack CreateRack()
        {
            return new Rack
            {
                Identifier = Guid.NewGuid().ToString(),
                Capacity = new RackCapacity
                {
                    MaximumRackCapacity = 10,
                },
                Position = SlcFacility_Management.Enums.RackpositionenumEnum.Bottom,
            };
        }
    }
}
