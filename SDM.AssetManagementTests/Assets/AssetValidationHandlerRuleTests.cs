namespace SDM.AssetManagement.Tests.Assets
{
    using System;
    using System.Collections.Generic;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation.Reservations;
    using Skyline.DataMiner.SDM.Common.Services;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    [TestClass]
    public class AssetValidationHandlerRuleTests
    {
        [TestMethod]
        public void ParentAssetHolder_WithParentAssetButNoHolderNumber_ShouldFail()
        {
            var asset = new Asset
            {
                Location =
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
                DestinationLocation =
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
                DestinationLocation =
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
                DestinationLocation =
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
                DestinationLocation = { },
            };
            asset.IsNewInternal = false;
            asset.ResetChangeTracking();
            asset.DestinationLocation.RoomId = new SdmObjectReference<Room>(Guid.NewGuid().ToString());

            var isValid = AssetValidationHandler.IsDestinationLocationChangeAllowed(asset, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Cannot change Destination Location in current State"));
        }

        [TestMethod]
        public void ReservationPlacement_WithNullReservation_ShouldThrowArgumentNullException()
        {
            var validator = new InfraopsReservationValidator(new SdmEntityLoader());

            validator.Invoking(v => v.Validate(null, RepositoryAction.Create))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void ReservationPlacement_WithNoRack_ShouldFail()
        {
            var validator = new InfraopsReservationValidator(new SdmEntityLoader());

            var result = validator.Validate(new InfraopsReservation(), RepositoryAction.Create);

            result.IsValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Reservation must have a Rack specified."));
        }

        [TestMethod]
        public void ReservationPlacement_WithNoPositionRanges_ShouldFail()
        {
            var validator = new InfraopsReservationValidator(new SdmEntityLoader());
            var reservation = new InfraopsReservation
            {
                RackFk =
                {
                    Rack = new SdmObjectReference<Rack>(Guid.NewGuid().ToString()),
                },
                ReservedPositions = new List<InfraopsReservationBounderies>(),
            };

            var result = validator.Validate(reservation, RepositoryAction.Create);

            result.IsValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Reservation must have at least one position range."));
        }

        [TestMethod]
        public void ReservationPlacement_WithUnknownRack_ShouldFail()
        {
            var validator = new InfraopsReservationValidator(new SdmEntityLoader());
            var reservation = new InfraopsReservation
            {
                RackFk =
                {
                    Rack = new SdmObjectReference<Rack>(Guid.NewGuid().ToString()),
                },
                ReservedPositions =
                [
                    new InfraopsReservationBounderies { LowerBound = 1, UpperBound = 1 },
                ],
            };

            var result = validator.Validate(reservation, RepositoryAction.Create);

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

        #region Ported From Shared Tests

        private static readonly SdmObjectReference<Asset> PortAssetReference = new SdmObjectReference<Asset>(Guid.NewGuid().ToString());

        [TestMethod]
        public void RackPosition_WithRackPositionAndSide_ShouldBeValid()
        {
            var asset = new Asset
            {
                Location =
                {
                    RackId = new SdmObjectReference<Rack>(Guid.NewGuid().ToString()),
                    RackPosition = 5,
                    Side = SlcAsset_Management.Enums.SideEnum.Front,
                },
            };

            var isValid = AssetValidationHandler.IsRackPositionValid(asset, out var result);

            isValid.Should().BeTrue();
            result.IsValid.Should().BeTrue();
            result.FailureReasons.Should().BeEmpty();
        }

        [TestMethod]
        public void RackPosition_WithNoRackNoPositionNoSide_ShouldBeValid()
        {
            var asset = new Asset();

            var isValid = AssetValidationHandler.IsRackPositionValid(asset, out var result);

            isValid.Should().BeTrue();
            result.IsValid.Should().BeTrue();
            result.FailureReasons.Should().BeEmpty();
        }

        [TestMethod]
        public void InstallationInfo_WithUserAndDate_ShouldBeValid()
        {
            var asset = new Asset
            {
                InstallationUserId = new PnoObjectReference<Person>(Guid.NewGuid()),
                InstallationDate = DateTime.UtcNow,
            };

            var isValid = AssetValidationHandler.IsInstallationInfoValid(asset, out var result);

            isValid.Should().BeTrue();
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void ModificationInfo_WithUserAndDate_ShouldBeValid()
        {
            var asset = new Asset
            {
                ModificationUserId = new PnoObjectReference<Person>(Guid.NewGuid()),
                ModificationDate = DateTime.UtcNow,
            };

            var isValid = AssetValidationHandler.IsModificationInfoValid(asset, out var result);

            isValid.Should().BeTrue();
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void Ownership_WithPersonAndRole_ShouldBeValid()
        {
            var asset = new Asset
            {
                Ownership =
                {
                    ContactPerson = new PnoObjectReference<Person>(Guid.NewGuid()),
                    ContactPersonRole = new PnoObjectReference<Role>(Guid.NewGuid()),
                },
            };

            var isValid = AssetValidationHandler.IsOwnershipValid(asset, out var result);

            isValid.Should().BeTrue();
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void Custody_WithPersonAndRole_ShouldBeValid()
        {
            var asset = new Asset
            {
                Custody =
                {
                    ContactPerson = new PnoObjectReference<Person>(Guid.NewGuid()),
                    ContactPersonRole = new PnoObjectReference<Role>(Guid.NewGuid()),
                },
            };

            var isValid = AssetValidationHandler.IsCustodyValid(asset, out var result);

            isValid.Should().BeTrue();
            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void InstallationInfo_UserWithoutDate_ShouldFailOnDateOnly()
        {
            var asset = new Asset { InstallationUserId = new PnoObjectReference<Person>(Guid.NewGuid()) };

            var isValid = AssetValidationHandler.IsInstallationInfoValid(asset, out var result);

            isValid.Should().BeFalse();
            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(AssetValidationHandler.AssetValidationField.InstallationUserId, out _).Should().BeFalse();
            result.TryGetFailReason(AssetValidationHandler.AssetValidationField.InstallationDate, out _).Should().BeTrue();
        }

        [TestMethod]
        public void ModificationInfo_UserWithoutDate_ShouldFailOnDateOnly()
        {
            var asset = new Asset { ModificationUserId = new PnoObjectReference<Person>(Guid.NewGuid()) };

            var isValid = AssetValidationHandler.IsModificationInfoValid(asset, out var result);

            isValid.Should().BeFalse();
            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(AssetValidationHandler.AssetValidationField.ModificationUserId, out _).Should().BeFalse();
            result.TryGetFailReason(AssetValidationHandler.AssetValidationField.ModificationDate, out _).Should().BeTrue();
        }

        [TestMethod]
        public void Ownership_PersonWithoutRole_ShouldFailOnRoleOnly()
        {
            var asset = new Asset { Ownership = { ContactPerson = new PnoObjectReference<Person>(Guid.NewGuid()) } };

            var isValid = AssetValidationHandler.IsOwnershipValid(asset, out var result);

            isValid.Should().BeFalse();
            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(AssetValidationHandler.AssetValidationField.OwnerContactPerson, out _).Should().BeFalse();
            result.TryGetFailReason(AssetValidationHandler.AssetValidationField.OwnerContactPersonRole, out _).Should().BeTrue();
        }

        [TestMethod]
        public void Custody_PersonWithoutRole_ShouldFailOnRoleOnly()
        {
            var asset = new Asset { Custody = { ContactPerson = new PnoObjectReference<Person>(Guid.NewGuid()) } };

            var isValid = AssetValidationHandler.IsCustodyValid(asset, out var result);

            isValid.Should().BeFalse();
            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(AssetValidationHandler.AssetValidationField.CustodyContactPerson, out _).Should().BeFalse();
            result.TryGetFailReason(AssetValidationHandler.AssetValidationField.CustodyContactPersonRole, out _).Should().BeTrue();
        }

        [TestMethod]
        public void AssetHolders_WithDistinctHolders_ShouldBeValid()
        {
            var asset = new Asset
            {
                Holders = new List<AssetHolder>
                {
                    new AssetHolder { SlotNumber = 0, HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card },
                    new AssetHolder { SlotNumber = 1, HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card },
                    new AssetHolder { SlotNumber = 0, HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Fan },
                },
            };

            var result = AssetValidationHandler.ValidateAssetHolders(asset);

            result.IsValid.Should().BeTrue();
            result.FailureReasons.Should().BeEmpty();
        }

        [TestMethod]
        public void AssetHolders_WithNullAsset_ShouldFailOnAssetField()
        {
            var result = AssetValidationHandler.ValidateAssetHolders(null);

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(AssetValidationHandler.AssetValidationField.Asset, out var reason).Should().BeTrue();
            reason.Should().Contain("Asset cannot be null.");
        }

        [TestMethod]
        public void AssetHolders_WithAvailableCandidateSlot_ShouldBeValid()
        {
            var asset = new Asset
            {
                Holders = new List<AssetHolder>
                {
                    new AssetHolder { SlotNumber = 0, HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card },
                    new AssetHolder { SlotNumber = 99, HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card },
                },
            };

            var result = AssetValidationHandler.ValidateAssetHolders(asset);

            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void AssetHolders_WithNegativeSlotNumber_ShouldFailOnHolderField()
        {
            var asset = new Asset
            {
                Holders = new List<AssetHolder>
                {
                    new AssetHolder { SlotNumber = -1, HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card },
                },
            };

            var result = AssetValidationHandler.ValidateAssetHolders(asset);

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(AssetValidationHandler.AssetValidationField.Holder, out var reason).Should().BeTrue();
            reason.Should().Contain("cannot be negative");
        }

        [TestMethod]
        public void AssetHolders_WithDuplicateSlotAndRole_ShouldFailOnHolderField()
        {
            var asset = new Asset
            {
                Holders = new List<AssetHolder>
                {
                    new AssetHolder { SlotNumber = 2, HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card },
                    new AssetHolder { SlotNumber = 2, HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card },
                },
            };

            var result = AssetValidationHandler.ValidateAssetHolders(asset);

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(AssetValidationHandler.AssetValidationField.Holder, out var reason).Should().BeTrue();
            reason.Should().Contain("Duplicate Holder found");
        }

        [TestMethod]
        public void AssetElements_WithSinglePrimaryDistinctElements_ShouldBeValid()
        {
            var asset = new Asset
            {
                ElementLinks = new List<ElementLink>
                {
                    new ElementLink { ElementID = "1/1", IsPrimary = true },
                    new ElementLink { ElementID = "1/2", IsPrimary = false },
                },
            };

            var result = AssetValidationHandler.ValidateAssetElements(asset);

            result.IsValid.Should().BeTrue();
            result.FailureReasons.Should().BeEmpty();
        }

        [TestMethod]
        public void AssetElements_WithMultiplePrimary_ShouldFailOnElementField()
        {
            var asset = new Asset
            {
                ElementLinks = new List<ElementLink>
                {
                    new ElementLink { ElementID = "1/1", IsPrimary = true },
                    new ElementLink { ElementID = "1/2", IsPrimary = true },
                },
            };

            var result = AssetValidationHandler.ValidateAssetElements(asset);

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(AssetValidationHandler.AssetValidationField.Element, out var reason).Should().BeTrue();
            reason.Should().Contain("Only one Element can be marked as Primary.");
        }

        [TestMethod]
        public void DestinationRackPosition_WithNullAsset_ShouldFail()
        {
            var isValid = AssetValidationHandler.IsDestinationRackPositionValid(null, new AssetClass { HeightU = 1 }, out var result);

            isValid.Should().BeFalse();
            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(AssetValidationHandler.AssetValidationField.Asset, out _).Should().BeTrue();
        }

        [TestMethod]
        public void DestinationRackPosition_WithRackPositionAndSide_ShouldBeValid()
        {
            var asset = new Asset
            {
                DestinationLocation =
                {
                    RackId = new SdmObjectReference<Rack>(Guid.NewGuid().ToString()),
                    RackPosition = 5,
                    Side = SlcAsset_Management.Enums.SideEnum.Front,
                },
            };
            var assetClass = new AssetClass { HeightU = 1 };

            var isValid = AssetValidationHandler.IsDestinationRackPositionValid(asset, assetClass, out var result);

            isValid.Should().BeTrue();
            result.IsValid.Should().BeTrue();
            result.FailureReasons.Should().BeEmpty();
        }

        [TestMethod]
        public void DestinationRackPosition_WithPositionButNoRack_ShouldFail()
        {
            var asset = new Asset
            {
                DestinationLocation =
                {
                    RackPosition = 5,
                },
            };
            var assetClass = new AssetClass { HeightU = 1 };

            var isValid = AssetValidationHandler.IsDestinationRackPositionValid(asset, assetClass, out var result);

            isValid.Should().BeFalse();
            result.TryGetFailReason(AssetValidationHandler.AssetValidationField.DestinationRackPosition, out var reason).Should().BeTrue();
            reason.Should().Contain("Rack Position cannot be set when there is no Rack.");
        }

        [TestMethod]
        public void DestinationParentAssetHolder_WithNullAsset_ShouldFail()
        {
            var isValid = AssetValidationHandler.IsDestinationParentAssetHolderValid(null, out var result);

            isValid.Should().BeFalse();
            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(AssetValidationHandler.AssetValidationField.Asset, out _).Should().BeTrue();
        }

        [TestMethod]
        [DataRow(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available, true, DisplayName = "Available")]
        [DataRow(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InService, false, DisplayName = "In Service")]
        [DataRow(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit, false, DisplayName = "In Transit")]
        [DataRow(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InRepair, true, DisplayName = "In Repair")]
        public void CanEditLocation_ForExistingAssetInState_ShouldReturnExpected(
            SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum state,
            bool expected)
        {
            var asset = new Asset { State = state };
            asset.IsNewInternal = false;
            asset.ResetChangeTracking();

            AssetValidationHandler.CanEditLocation(asset).Should().Be(expected);
        }

        [TestMethod]
        public void LoadedDataPorts_WithDistinctPortsAndSinglePrimaries_ShouldBeValid()
        {
            var core = new AssetValidationCore(new SdmEntityLoader());

            var result = core.ValidateLoadedDataPorts(new List<DataPort>
            {
                CreateAssetDataPort(1, primaryIpv4: true),
                CreateAssetDataPort(2, primaryIpv6: true),
                CreateAssetDataPort(3),
            });

            result.IsValid.Should().BeTrue();
            result.FailureReasons.Should().BeEmpty();
        }

        [TestMethod]
        public void LoadedDataPorts_WithNegativePortNumber_ShouldFail()
        {
            var core = new AssetValidationCore(new SdmEntityLoader());

            var result = core.ValidateLoadedDataPorts(new List<DataPort> { CreateAssetDataPort(-1) });

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(DataPortValidationHandler.DataPortValidationField.PortNumber, out var reason).Should().BeTrue();
            reason.Should().Contain("-1");
        }

        [TestMethod]
        public void LoadedDataPorts_WithDuplicatePortNumber_ShouldFail()
        {
            var core = new AssetValidationCore(new SdmEntityLoader());

            var result = core.ValidateLoadedDataPorts(new List<DataPort> { CreateAssetDataPort(1), CreateAssetDataPort(1) });

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(DataPortValidationHandler.DataPortValidationField.PortNumber, out var reason).Should().BeTrue();
            reason.Should().Contain("Duplicate Data Port number found: 1");
        }

        [TestMethod]
        [DataRow(true, false, "IPv4", DisplayName = "Multiple primary IPv4")]
        [DataRow(false, true, "IPv6", DisplayName = "Multiple primary IPv6")]
        public void LoadedDataPorts_WithMultiplePrimaryOfSameFamily_ShouldFail(bool ipv4, bool ipv6, string family)
        {
            var core = new AssetValidationCore(new SdmEntityLoader());

            var result = core.ValidateLoadedDataPorts(new List<DataPort>
            {
                CreateAssetDataPort(1, ipv4, ipv6),
                CreateAssetDataPort(2, ipv4, ipv6),
            });

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(DataPortValidationHandler.DataPortValidationField.PrimaryPort, out var reason).Should().BeTrue();
            reason.Should().Contain($"Only one Data Port can be marked as Primary {family}.");
        }

        [TestMethod]
        public void LoadedPowerPorts_WithDistinctPortNumbers_ShouldBeValid()
        {
            var core = new AssetValidationCore(new SdmEntityLoader());

            var result = core.ValidateLoadedPowerPorts(new List<PowerPort> { CreateAssetPowerPort(1), CreateAssetPowerPort(2) });

            result.IsValid.Should().BeTrue();
            result.FailureReasons.Should().BeEmpty();
        }

        [TestMethod]
        public void LoadedPowerPorts_WithDuplicatePortNumber_ShouldFailOnPowerPortField()
        {
            var core = new AssetValidationCore(new SdmEntityLoader());

            var result = core.ValidateLoadedPowerPorts(new List<PowerPort> { CreateAssetPowerPort(1), CreateAssetPowerPort(1) });

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(AssetValidationHandler.AssetValidationField.PowerPort, out var reason).Should().BeTrue();
            reason.Should().Contain("1");
        }

        private static DataPort CreateAssetDataPort(long portNumber, bool primaryIpv4 = false, bool primaryIpv6 = false)
        {
            var dataPort = new DataPort
            {
                Identifier = Guid.NewGuid().ToString(),
                Asset = PortAssetReference,
                DataPortInfo =
                {
                    Name = $"ETH{portNumber}",
                    PortNumber = portNumber,
                    OutputType = SlcAsset_Management.Enums.Outputtype.IO,
                },
            };
            dataPort.PrimaryPortRelation.IsPrimaryIpv4 = primaryIpv4;
            dataPort.PrimaryPortRelation.IsPrimaryIpv6 = primaryIpv6;
            return dataPort;
        }

        private static PowerPort CreateAssetPowerPort(long portNumber)
        {
            return new PowerPort
            {
                Identifier = Guid.NewGuid().ToString(),
                Asset = PortAssetReference,
                PowerPortInfo =
                {
                    Name = $"PSU{portNumber}",
                    PortNumber = portNumber,
                    OutputType = SlcAsset_Management.Enums.Outputtype.IO,
                },
            };
        }

        #endregion

        private static Rack CreateRack()
        {
            var rack = new Rack
            {
                Identifier = Guid.NewGuid().ToString(),
                Position = SlcFacility_Management.Enums.RackpositionenumEnum.Bottom,
            };
            rack.Capacity.MaximumRackCapacity = 10;
            return rack;
        }
    }
}
