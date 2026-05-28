namespace SDM.AssetManagement.Tests.Assets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    /// <summary>
    /// Tests that validate all Asset validation rules by intentionally violating them.
    /// Each test targets a specific validation rule to ensure proper error detection.
    /// </summary>
    [TestClass]
    public class AssetValidatorTests : BaseRepositoryTest
    {
        private Asset baseValidAsset = null!;
        private AssetClass testAssetClass = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            // Setup: Create a valid asset class first
            Helper.PopulateWithDemoData(DemoDataLayer.AssetClasses);
            testAssetClass = Helper.TestData.AssetClasses.First();

            // Create a base valid asset for modification in tests
            baseValidAsset = new Asset
            {
                AssetID = "TEST-ASSET-001",
                Name = "Valid Test Asset",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Description = "Base valid asset for testing",
            };
        }

        #region Asset Class Validation Tests

        [TestMethod]
        public void Create_WithNullAssetClass_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-001",
                Name = "Asset Without Class",
                AssetClassId = null,
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Asset Class cannot be empty*");
        }

        [TestMethod]
        public void Create_WithEmptyAssetClassReference_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-002",
                Name = "Asset With Empty Class Reference",
                AssetClassId = new SdmObjectReference<AssetClass>(),
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Asset Class cannot be empty*");
        }

        #endregion

        #region Name Uniqueness Validation Tests

        [TestMethod]
        public void Create_WithEmptyName_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-003",
                Name = "",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Asset Name cannot be empty*");
        }

        [TestMethod]
        public void Create_WithDuplicateName_ShouldFail()
        {
            // Arrange
            var firstAsset = new Asset
            {
                AssetID = "TEST-004",
                Name = "Duplicate Name Asset",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
            };
            Helper.AssetManagement.Assets.Create(firstAsset);

            var duplicateAsset = new Asset
            {
                AssetID = "TEST-005",
                Name = "Duplicate Name Asset",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(duplicateAsset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Asset Name*already in use*");
        }

        #endregion

        #region Asset ID Uniqueness Validation Tests

        [TestMethod]
        public void Create_WithEmptyAssetID_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "",
                Name = "Asset Without ID",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Asset ID cannot be empty*");
        }

        [TestMethod]
        public void Create_WithDuplicateAssetID_ShouldFail()
        {
            // Arrange
            var firstAsset = new Asset
            {
                AssetID = "DUPLICATE-ID-001",
                Name = "First Asset",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
            };
            Helper.AssetManagement.Assets.Create(firstAsset);

            var duplicateAsset = new Asset
            {
                AssetID = "DUPLICATE-ID-001",
                Name = "Second Asset",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(duplicateAsset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Asset ID*already in use*");
        }

        #endregion

        #region Serial Number Uniqueness Validation Tests

        [TestMethod]
        public void Create_WithDuplicateSerialNumberSameClass_ShouldFail()
        {
            // Arrange
            var firstAsset = new Asset
            {
                AssetID = "TEST-SN-001",
                Name = "First Asset With Serial",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                SerialNumber = "SN-12345",
            };
            Helper.AssetManagement.Assets.Create(firstAsset);

            var duplicateAsset = new Asset
            {
                AssetID = "TEST-SN-002",
                Name = "Second Asset With Same Serial",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                SerialNumber = "SN-12345",
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(duplicateAsset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Serial Number is already in use for this Asset Class*");
        }

        #endregion

        #region Location Validation Tests - Single Location Type

        [TestMethod]
        public void Create_WithMultipleLocationTypes_ShouldFail()
        {
            // Arrange
            Helper.PopulateWithDemoData(DemoDataLayer.Racks);
            var rack = Helper.TestData.Racks.First();

            var asset = new Asset
            {
                AssetID = "TEST-LOC-001",
                Name = "Asset With Multiple Locations",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Location = new AssetLocation
                {
                    RackId = new SdmObjectReference<Rack>(rack.Identifier),
                    RackPosition = 10,
                    Side = SlcAsset_Management.Enums.SideEnum.Front,
                    DeskId = Guid.NewGuid(), // Multiple location types
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*multiple Locations attached*");
        }

        #endregion

        #region Parent Asset Holder Validation Tests

        [TestMethod]
        public void Create_WithHolderNumberButNoParentAsset_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-HOLDER-001",
                Name = "Asset With Holder But No Parent",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Location = new AssetLocation
                {
                    HolderNumber = 5,
                    // ParentAsset not set
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Holder Number cannot be set when there is no Parent Asset*");
        }

        [TestMethod]
        public void Create_WithNegativeHolderNumber_ShouldFail()
        {
            // Arrange
            var parentAsset = Helper.AssetManagement.Assets.Create(baseValidAsset);

            var childAsset = new Asset
            {
                AssetID = "TEST-HOLDER-003",
                Name = "Asset With Negative Holder",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Location = new AssetLocation
                {
                    ParentAsset = new SdmObjectReference<Asset>(parentAsset.Identifier),
                    HolderNumber = -5,
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(childAsset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Holder Number cannot be negative*");
        }

        #endregion

        #region Rack Position Validation Tests

        [TestMethod]
        public void Create_WithRackPositionButNoRack_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-RACK-001",
                Name = "Asset With Position But No Rack",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Location = new AssetLocation
                {
                    RackPosition = 10,
                    // RackId not set
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Rack Position cannot be set when there is no Rack*");
        }

        [TestMethod]
        [Ignore("Waiting for nullable Side support")]
        public void Create_WithRackSideButNoRack_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-RACK-002",
                Name = "Asset With Side But No Rack",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Location = new AssetLocation
                {
                    Side = SlcAsset_Management.Enums.SideEnum.Front,
                    // RackId not set
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Rack Side cannot be set when there is no Rack*");
        }

        [TestMethod]
        [Ignore("Waiting for nullable Position support")]
        public void Create_WithRackButNoPosition_ShouldFail()
        {
            // Arrange
            Helper.PopulateWithDemoData(DemoDataLayer.Racks);
            var rack = Helper.TestData.Racks.First();

            var asset = new Asset
            {
                AssetID = "TEST-RACK-003",
                Name = "Asset With Rack But No Position",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Location = new AssetLocation
                {
                    RackId = new SdmObjectReference<Rack>(rack.Identifier),
                    // RackPosition not set
                    Side = SlcAsset_Management.Enums.SideEnum.Front,
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Rack Position must be set when Rack is provided*");
        }

        [TestMethod]
        [Ignore("Waiting for nullable Side support")]
        public void Create_WithRackButNoSide_ShouldFail()
        {
            // Arrange
            Helper.PopulateWithDemoData(DemoDataLayer.Racks);
            var rack = Helper.TestData.Racks.First();

            var asset = new Asset
            {
                AssetID = "TEST-RACK-004",
                Name = "Asset With Rack But No Side",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Location = new AssetLocation
                {
                    RackId = new SdmObjectReference<Rack>(rack.Identifier),
                    RackPosition = 10,
                    // Side not set
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Rack Side must be set when Rack is provided*");
        }

        [TestMethod]
        public void Create_WithZeroRackPosition_ShouldFail()
        {
            // Arrange
            Helper.PopulateWithDemoData(DemoDataLayer.Racks);
            var rack = Helper.TestData.Racks.First();

            var asset = new Asset
            {
                AssetID = "TEST-RACK-005",
                Name = "Asset With Zero Position",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Location = new AssetLocation
                {
                    RackId = new SdmObjectReference<Rack>(rack.Identifier),
                    RackPosition = 0,
                    Side = SlcAsset_Management.Enums.SideEnum.Front,
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Rack Position must be greater than 0*");
        }

        [TestMethod]
        public void Create_WithNegativeRackPosition_ShouldFail()
        {
            // Arrange
            Helper.PopulateWithDemoData(DemoDataLayer.Racks);
            var rack = Helper.TestData.Racks.First();

            var asset = new Asset
            {
                AssetID = "TEST-RACK-006",
                Name = "Asset With Negative Position",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Location = new AssetLocation
                {
                    RackId = new SdmObjectReference<Rack>(rack.Identifier),
                    RackPosition = -5,
                    Side = SlcAsset_Management.Enums.SideEnum.Front,
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Rack Position must be greater than 0*");
        }

        #endregion

        #region Destination Location Validation Tests

        [TestMethod]
        public void Create_WithDestinationLocationButNotInTransit_ShouldWarn()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-DEST-001",
                Name = "Asset With Destination But Not In Transit",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available,
                DestinationLocation = new AssetLocation
                {
                    RoomId = new SdmObjectReference<Room>(Guid.NewGuid().ToString()),
                },
            };

            // Act
            var created = Helper.AssetManagement.Assets.Create(asset);

            // Assert - Destination location should be discarded
            created.DestinationLocation.Should().Be(default);
        }

        [TestMethod]
        public void Create_InTransitWithoutDestinationLocation_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-DEST-002",
                Name = "In Transit Asset Without Destination",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit,
                // DestinationLocation not set
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Destination Location is mandatory when Asset is in 'In Transit' state*");
        }

        [TestMethod]
        public void Create_WithMultipleDestinationLocationTypes_ShouldFail()
        {
            // Arrange
            Helper.PopulateWithDemoData(DemoDataLayer.Racks);
            var rack = Helper.TestData.Racks.First();

            var asset = new Asset
            {
                AssetID = "TEST-DEST-003",
                Name = "Asset With Multiple Destination Locations",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit,
                DestinationLocation = new AssetLocation
                {
                    RackId = new SdmObjectReference<Rack>(rack.Identifier),
                    RackPosition = 10,
                    Side = SlcAsset_Management.Enums.SideEnum.Front,
                    DeskId = Guid.NewGuid(), // Multiple destination types
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*multiple Destination Locations attached*");
        }

        #endregion

        #region Destination Parent Asset Holder Validation Tests

        [TestMethod]
        [Ignore("Waiting for nullable HolderNumber support")]
        public void Create_WithDestinationHolderButNoParentAsset_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-DEST-HOLDER-001",
                Name = "In Transit With Holder But No Parent",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit,
                DestinationLocation = new AssetLocation
                {
                    HolderNumber = 5,
                    // ParentAsset not set
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Holder Number cannot be set when there is no Parent Asset*");
        }

        [TestMethod]
        public void Create_WithNegativeDestinationHolderNumber_ShouldFail()
        {
            // Arrange
            var parentAsset = Helper.AssetManagement.Assets.Create(baseValidAsset);

            var asset = new Asset
            {
                AssetID = "TEST-DEST-HOLDER-003",
                Name = "In Transit With Negative Holder",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit,
                DestinationLocation = new AssetLocation
                {
                    ParentAsset = new SdmObjectReference<Asset>(parentAsset.Identifier),
                    HolderNumber = -3,
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Holder Number cannot be negative*");
        }

        #endregion

        #region Destination Rack Position Validation Tests

        [TestMethod]
        [Ignore("Waiting for nullable RackSide support")]
        public void Create_WithDestinationRackPositionButNoRack_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-DEST-RACK-001",
                Name = "In Transit With Position But No Rack",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit,
                DestinationLocation = new AssetLocation
                {
                    RackPosition = 10,
                    // RackId not set
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Rack Position cannot be set when there is no Rack*");
        }

        [TestMethod]
        [Ignore("Waiting for nullable RackPosition support")]
        public void Create_WithDestinationRackButNoPosition_ShouldFail()
        {
            // Arrange
            Helper.PopulateWithDemoData(DemoDataLayer.Racks);
            var rack = Helper.TestData.Racks.First();

            var asset = new Asset
            {
                AssetID = "TEST-DEST-RACK-002",
                Name = "In Transit With Rack But No Position",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit,
                DestinationLocation = new AssetLocation
                {
                    RackId = new SdmObjectReference<Rack>(rack.Identifier),
                    // RackPosition not set
                    Side = SlcAsset_Management.Enums.SideEnum.Front,
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Rack Position must be set when Rack is provided*");
        }

        [TestMethod]
        [Ignore("Waiting for nullable RackSide support")]
        public void Create_WithDestinationRackButNoSide_ShouldFail()
        {
            // Arrange
            Helper.PopulateWithDemoData(DemoDataLayer.Racks);
            var rack = Helper.TestData.Racks.First();

            var asset = new Asset
            {
                AssetID = "TEST-DEST-RACK-003",
                Name = "In Transit With Rack But No Side",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit,
                DestinationLocation = new AssetLocation
                {
                    RackId = new SdmObjectReference<Rack>(rack.Identifier),
                    RackPosition = 10,
                    // Side not set
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Rack Side must be set when Rack is provided*");
        }

        [TestMethod]
        public void Create_WithZeroDestinationRackPosition_ShouldFail()
        {
            // Arrange
            Helper.PopulateWithDemoData(DemoDataLayer.Racks);
            var rack = Helper.TestData.Racks.First();

            var asset = new Asset
            {
                AssetID = "TEST-DEST-RACK-004",
                Name = "In Transit With Zero Position",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit,
                DestinationLocation = new AssetLocation
                {
                    RackId = new SdmObjectReference<Rack>(rack.Identifier),
                    RackPosition = 0,
                    Side = SlcAsset_Management.Enums.SideEnum.Front,
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Rack Position must be greater than 0*");
        }

        #endregion

        #region Lifecycle Validation Tests

        [TestMethod]
        public void Create_WithInstallationUserButNoDate_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-LIFECYCLE-001",
                Name = "Asset With Installation User But No Date",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                InstallationUserId = Guid.NewGuid(),
                // InstallationDate not set
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Installation Date must be set when Installation User is provided*");
        }

        [TestMethod]
        public void Create_WithInstallationDateButNoUser_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-LIFECYCLE-002",
                Name = "Asset With Installation Date But No User",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                InstallationDate = DateTime.UtcNow,
                // InstallationUserId not set
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Installation User must be set when Installation Date is provided*");
        }

        [TestMethod]
        public void Create_WithModificationUserButNoDate_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-LIFECYCLE-003",
                Name = "Asset With Modification User But No Date",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                ModificationUserId = Guid.NewGuid(),
                // ModificationDate not set
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Modification Date must be set when Modification User is provided*");
        }

        [TestMethod]
        public void Create_WithModificationDateButNoUser_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-LIFECYCLE-004",
                Name = "Asset With Modification Date But No User",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                ModificationDate = DateTime.UtcNow,
                // ModificationUserId not set
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Modification User must be set when Modification Date is provided*");
        }

        #endregion

        #region Ownership Validation Tests

        [TestMethod]
        public void Create_WithOwnerContactPersonButNoRole_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-OWNERSHIP-001",
                Name = "Asset With Owner Person But No Role",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Ownership = new AssetOwnership
                {
                    ContactPerson = Guid.NewGuid(),
                    // ContactPersonRole not set
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Owner Contact Person Role must be set when Contact Person is provided*");
        }

        [TestMethod]
        public void Create_WithOwnerRoleButNoPerson_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-OWNERSHIP-002",
                Name = "Asset With Owner Role But No Person",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Ownership = new AssetOwnership
                {
                    ContactPersonRole = Guid.NewGuid(),
                    // ContactPerson not set
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Owner Contact Person must be set when Contact Person Role is provided*");
        }

        #endregion

        #region Custody Validation Tests

        [TestMethod]
        public void Create_WithCustodyContactPersonButNoRole_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-CUSTODY-001",
                Name = "Asset With Custody Person But No Role",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Custody = new AssetCustody
                {
                    ContactPerson = Guid.NewGuid(),
                    // ContactPersonRole not set
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Custody Contact Person Role must be set when Contact Person is provided*");
        }

        [TestMethod]
        public void Create_WithCustodyRoleButNoPerson_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-CUSTODY-002",
                Name = "Asset With Custody Role But No Person",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Custody = new AssetCustody
                {
                    ContactPersonRole = Guid.NewGuid(),
                    // ContactPerson not set
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Custody Contact Person must be set when Contact Person Role is provided*");
        }

        #endregion

        #region Holder Collection Validation Tests

        [TestMethod]
        public void Create_WithHolderWithNegativeSlotNumber_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-HOLDER-COL-002",
                Name = "Asset With Negative Holder Slot",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Holders = new List<AssetHolder>
                {
                    new AssetHolder
                    {
                        SlotNumber = -5,
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card,
                    },
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Holder Slot number cannot be negative*");
        }

        [TestMethod]
        public void Create_WithDuplicateHolders_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-HOLDER-COL-003",
                Name = "Asset With Duplicate Holders",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Holders = new List<AssetHolder>
                {
                    new AssetHolder
                    {
                        SlotNumber = 1,
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card,
                    },
                    new AssetHolder
                    {
                        SlotNumber = 1, // Duplicate slot + role
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card,
                    },
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Duplicate Holder found*");
        }

        #endregion

        #region Element Collection Validation Tests

        [TestMethod]
        public void Create_WithMultiplePrimaryElements_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-ELEMENT-001",
                Name = "Asset With Multiple Primary Elements",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                ElementLinks = new List<ElementLink>
                {
                    new ElementLink
                    {
                        ElementID = "100/1",
                        IsPrimary = true,
                    },
                    new ElementLink
                    {
                        ElementID = "100/2",
                        IsPrimary = true, // Duplicate primary
                    },
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Only one Element can be marked as Primary*");
        }

        [TestMethod]
        public void Create_WithDuplicateElementIDs_ShouldFail()
        {
            // Arrange
            var asset = new Asset
            {
                AssetID = "TEST-ELEMENT-002",
                Name = "Asset With Duplicate Element IDs",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                ElementLinks = new List<ElementLink>
                {
                    new ElementLink
                    {
                        ElementID = "100/1",
                    },
                    new ElementLink
                    {
                        ElementID = "100/1", // Duplicate
                    },
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Duplicate Element ID found*");
        }

        #endregion

        #region Bulk Validation - Batch Conflicts

        [TestMethod]
        public void CreateOrUpdate_WithDuplicateNamesInBatch_ShouldFail()
        {
            // Arrange
            var assets = new List<Asset>
            {
                new Asset
                {
                    AssetID = "BATCH-001",
                    Name = "Duplicate Name In Batch",
                    AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                },
                new Asset
                {
                    AssetID = "BATCH-002",
                    Name = "Duplicate Name In Batch", // Duplicate in batch
                    AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.CreateOrUpdate(assets);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*duplicated within the validation batch*");
        }

        [TestMethod]
        public void CreateOrUpdate_WithDuplicateAssetIDsInBatch_ShouldFail()
        {
            // Arrange
            var assets = new List<Asset>
            {
                new Asset
                {
                    AssetID = "DUPLICATE-BATCH-ID",
                    Name = "First Asset",
                    AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                },
                new Asset
                {
                    AssetID = "DUPLICATE-BATCH-ID", // Duplicate in batch
                    Name = "Second Asset",
                    AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.CreateOrUpdate(assets);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Asset ID*duplicated within the validation batch*");
        }

        [TestMethod]
        public void CreateOrUpdate_WithDuplicateSerialNumbersInBatch_ShouldFail()
        {
            // Arrange
            var assets = new List<Asset>
            {
                new Asset
                {
                    AssetID = "BATCH-SN-001",
                    Name = "First Asset With Serial",
                    AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                    SerialNumber = "DUPLICATE-SERIAL-BATCH",
                },
                new Asset
                {
                    AssetID = "BATCH-SN-002",
                    Name = "Second Asset With Serial",
                    AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                    SerialNumber = "DUPLICATE-SERIAL-BATCH", // Duplicate in batch
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.CreateOrUpdate(assets);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Serial Number*duplicated within the validation batch*");
        }

        #endregion

        #region Rack Space Availability Tests

        [TestMethod]
        [Ignore("Waiting for nullable Location fields support")]
        public void Create_WithRackPositionExceedingCapacity_ShouldFail()
        {
            // Arrange
            Helper.PopulateWithDemoData(DemoDataLayer.Racks);
            var rack = Helper.TestData.Racks.First();
            var maxCapacity = rack.Capacity.MaximumRackCapacity;

            var asset = new Asset
            {
                AssetID = "TEST-RACK-CAPACITY-001",
                Name = "Asset Exceeding Rack Capacity",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Location = new AssetLocation
                {
                    RackId = new SdmObjectReference<Rack>(rack.Identifier),
                    RackPosition = (long)maxCapacity + 10, // Exceeds capacity
                    Side = SlcAsset_Management.Enums.SideEnum.Front,
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage($"*Must be within Rack*max {maxCapacity}*");
        }

        [TestMethod]
        public void Create_WithRackPositionConflictingWithExistingAsset_ShouldFail()
        {
            // Arrange
            Helper.PopulateWithDemoData(DemoDataLayer.Assets);
            
            // Find an existing asset that's in a rack
            var existingAsset = Helper.TestData.Assets
                .FirstOrDefault(a => a.Location?.RackId != null && a.Location.RackPosition > 0);
            
            if (existingAsset == null)
            {
                Assert.Inconclusive("No test data available with rack placement.");
                return;
            }

            // Try to create a new asset at the same position
            var conflictingAsset = new Asset
            {
                AssetID = "TEST-RACK-CONFLICT-001",
                Name = "Conflicting Rack Position Asset",
                AssetClassId = existingAsset.AssetClassId,
                Location = new AssetLocation
                {
                    RackId = existingAsset.Location.RackId,
                    RackPosition = existingAsset.Location.RackPosition, // Same position
                    Side = existingAsset.Location.Side,
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(conflictingAsset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*occupied*");
        }

        [TestMethod]
        [Ignore("Waiting for nullable Location fields support")]
        public void Create_WithRackPositionCausingOverlapWithExistingAsset_ShouldFail()
        {
            // Arrange
            Helper.PopulateWithDemoData(DemoDataLayer.Racks);
            var rack = Helper.TestData.Racks.First();

            // Create first asset at position 10 with height 3U (occupies 10-12)
            var assetClass = Helper.TestData.AssetClasses
                .FirstOrDefault(ac => ac.HeightU > 1);
            
            if (assetClass == null)
            {
                Assert.Inconclusive("No asset class with height > 1U found.");
                return;
            }

            var firstAsset = new Asset
            {
                AssetID = "TEST-OVERLAP-FIRST",
                Name = "First Asset Occupying Space",
                AssetClassId = new SdmObjectReference<AssetClass>(assetClass.Identifier),
                Location = new AssetLocation
                {
                    RackId = new SdmObjectReference<Rack>(rack.Identifier),
                    RackPosition = 10,
                    Side = SlcAsset_Management.Enums.SideEnum.Front,
                },
            };
            Helper.AssetManagement.Assets.Create(firstAsset);

            // Try to create second asset at position 11 (overlaps with first asset)
            var overlappingAsset = new Asset
            {
                AssetID = "TEST-OVERLAP-SECOND",
                Name = "Overlapping Asset",
                AssetClassId = new SdmObjectReference<AssetClass>(assetClass.Identifier),
                Location = new AssetLocation
                {
                    RackId = new SdmObjectReference<Rack>(rack.Identifier),
                    RackPosition = 11, // Overlaps with first asset (10-12)
                    Side = SlcAsset_Management.Enums.SideEnum.Front,
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(overlappingAsset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*occupied*");
        }

        [TestMethod]
        [Ignore("TODO SDM-1234: RackPosition is a non-nullable long with default value 0, causing incorrect validation behavior for rack capacity checks. Skip until nullable types are implemented.")]
        public void Create_WithAssetHeightExceedingRackCapacity_BottomToTop_ShouldFail()
        {
            // Arrange
            Helper.PopulateWithDemoData(DemoDataLayer.AssetClasses);
            var bottomRack = Helper.TestData.Racks.FirstOrDefault(r => r.Position == SlcFacility_Management.Enums.RackpositionenumEnum.Bottom);

            if(bottomRack == null)
            {
                Assert.Inconclusive("No bottom-to-top rack found in test data.");
            }

            var maxCapacity = bottomRack.Capacity.MaximumRackCapacity;

            // Find asset class with significant height
            var tallAssetClass = Helper.TestData.AssetClasses
                .FirstOrDefault(ac => ac.HeightU > 1);
            
            if (tallAssetClass == null)
            {
                Assert.Inconclusive("No asset class with height > 1U and RackUnitConsumer tag found.");
                return;
            }

            // Calculate position that will cause overflow
            // For bottom-up racks: endPos = (position - 1) + heightU
            // To exceed capacity: endPos > maxCapacity
            // Therefore: position > maxCapacity - heightU + 1
            // Use: position = maxCapacity - heightU + 2 to guarantee overflow
            var assetHeight = (long)tallAssetClass.HeightU;
            var overflowPosition = (long)maxCapacity - assetHeight + 2;

            // Try to place asset where position + height exceeds capacity
            var asset = new Asset
            {
                AssetID = "TEST-RACK-OVERFLOW-001",
                Name = "Asset Causing Rack Overflow",
                AssetClassId = new SdmObjectReference<AssetClass>(tallAssetClass.Identifier),
                Location = new AssetLocation
                {
                    RackId = new SdmObjectReference<Rack>(bottomRack.Identifier),
                    RackPosition = overflowPosition, // Calculated to exceed capacity
                    Side = SlcAsset_Management.Enums.SideEnum.Front,
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Invalid Position. Extends beyond rack boundaries*");
        }

        [TestMethod]
        [Ignore("TODO SDM-1234: RackPosition is a non-nullable long with default value 0, causing incorrect validation behavior for rack capacity checks. Skip until nullable types are implemented.")]
        public void Create_WithAssetHeightExceedingRackCapacity_TopToBottom_ShouldFail()
        {
            // Arrange
            Helper.PopulateWithDemoData(DemoDataLayer.AssetClasses);

            // Create a top-to-bottom rack
            var topRack = Helper.TestData.Racks.FirstOrDefault(r => r.Position == SlcFacility_Management.Enums.RackpositionenumEnum.Top);

            if (topRack == null)
            {
                Assert.Inconclusive("No top-to-bottom rack found in test data.");
            }

            var tallAssetClass = Helper.TestData.AssetClasses
                .FirstOrDefault(ac => ac.HeightU > 1);

            if (tallAssetClass == null)
            {
                Assert.Inconclusive("No asset class with height > 1U found.");
                return;
            }

            // For top-down: to exceed at bottom, position must be < heightU
            // Position 1 with 2U asset → occupies positions 0-1 → exceeds at bottom (position 0)
            var assetHeight = (long)tallAssetClass.HeightU;
            var overflowPosition = assetHeight - 1; // Will cause negative startPos

            var asset = new Asset
            {
                AssetID = "TEST-TOP-RACK-OVERFLOW-001",
                Name = "Asset Causing Top Rack Overflow",
                AssetClassId = new SdmObjectReference<AssetClass>(tallAssetClass.Identifier),
                Location = new AssetLocation
                {
                    RackId = new SdmObjectReference<Rack>(topRack.Identifier),
                    RackPosition = overflowPosition,
                    Side = SlcAsset_Management.Enums.SideEnum.Front,
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Invalid Position. Extends beyond rack boundaries*");
        }

        [TestMethod]
        [Ignore("Waiting for nullable Location fields support")]
        public void CreateOrUpdate_WithOverlappingRackPositionsInBatch_ShouldFail()
        {
            // Arrange
            Helper.PopulateWithDemoData(DemoDataLayer.Racks);
            var rack = Helper.TestData.Racks.First();

            // Find asset class with height > 1U
            var assetClass = Helper.TestData.AssetClasses
                .FirstOrDefault(ac => ac.HeightU > 1);
            
            if (assetClass == null)
            {
                Assert.Inconclusive("No asset class with height > 1U found.");
                return;
            }

            var assets = new List<Asset>
            {
                new Asset
                {
                    AssetID = "BATCH-RACK-001",
                    Name = "First Batch Asset",
                    AssetClassId = new SdmObjectReference<AssetClass>(assetClass.Identifier),
                    Location = new AssetLocation
                    {
                        RackId = new SdmObjectReference<Rack>(rack.Identifier),
                        RackPosition = 10,
                        Side = SlcAsset_Management.Enums.SideEnum.Front,
                    },
                },
                new Asset
                {
                    AssetID = "BATCH-RACK-002",
                    Name = "Second Batch Asset",
                    AssetClassId = new SdmObjectReference<AssetClass>(assetClass.Identifier),
                    Location = new AssetLocation
                    {
                        RackId = new SdmObjectReference<Rack>(rack.Identifier),
                        RackPosition = 11,
                        Side = SlcAsset_Management.Enums.SideEnum.Front,
                    },
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.CreateOrUpdate(assets);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*conflicts with another asset in the validation batch*");
        }

        #endregion

        #region Parent Asset Holder Availability Tests

        [TestMethod]
        [Ignore("Waiting for nullable Location fields support")]
        public void Create_WithHolderAlreadyOccupiedByAnotherAsset_ShouldFail()
        {

            var deviceType = Helper.TestData.DeviceTypes.First(dt => dt.Identifier == testAssetClass.DeviceTypeId.Identifier);

            // Arrange
            var parentAsset = new Asset
            {
                AssetID = "PARENT-WITH-HOLDERS",
                Name = "Parent Asset With Holders",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Holders = new List<AssetHolder>
                {
                    new AssetHolder
                    {
                        SlotNumber = 1,
                        HierarchyRole = deviceType.HierarchyInfo.HierarchyRole,
                    },
                    new AssetHolder
                    {
                        SlotNumber = 2,
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card,
                    },
                },
            };
            var createdParent = Helper.AssetManagement.Assets.Create(parentAsset);

            // Create first child occupying holder slot 1
            var firstChild = new Asset
            {
                AssetID = "CHILD-IN-HOLDER-1",
                Name = "First Child Asset",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Location = new AssetLocation
                {
                    ParentAsset = new SdmObjectReference<Asset>(createdParent.Identifier),
                    HolderNumber = 1,
                },
            };
            Helper.AssetManagement.Assets.Create(firstChild);

            // Try to create second child in the same holder slot
            var secondChild = new Asset
            {
                AssetID = "CHILD-CONFLICT-HOLDER-1",
                Name = "Conflicting Child Asset",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Location = new AssetLocation
                {
                    ParentAsset = new SdmObjectReference<Asset>(createdParent.Identifier),
                    HolderNumber = 1, // Same holder as first child
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(secondChild);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*Holder*already occupied*");
        }

        [TestMethod]
        [Ignore("Waiting for nullable Location fields support")]
        public void Create_WithInvalidHolderSlotOnParent_ShouldFail()
        {
            // Arrange
            var parentAsset = new Asset
            {
                AssetID = "PARENT-LIMITED-HOLDERS",
                Name = "Parent With Limited Holders",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Holders = new List<AssetHolder>
                {
                    new AssetHolder
                    {
                        SlotNumber = 1,
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card,
                    },
                    new AssetHolder
                    {
                        SlotNumber = 2,
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Fan,
                    },
                },
            };
            var createdParent = Helper.AssetManagement.Assets.Create(parentAsset);

            // Try to attach child to non-existent holder slot
            var childAsset = new Asset
            {
                AssetID = "CHILD-INVALID-HOLDER",
                Name = "Child With Invalid Holder",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Location = new AssetLocation
                {
                    ParentAsset = new SdmObjectReference<Asset>(createdParent.Identifier),
                    HolderNumber = 5, // Slot 5 doesn't exist on parent
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(childAsset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*does not have a holder slot*");
        }

        [TestMethod]
        [Ignore("Waiting for nullable Location fields support")]
        public void Create_WithMismatchedHierarchyRoleOnParent_ShouldFail()
        {
            // Arrange
            var parentAsset = new Asset
            {
                AssetID = "PARENT-MIXED-HOLDERS",
                Name = "Parent With Mixed Holder Types",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Holders = new List<AssetHolder>
                {
                    new AssetHolder
                    {
                        SlotNumber = 1,
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card,
                    },
                    new AssetHolder
                    {
                        SlotNumber = 2,
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Fan,
                    },
                },
            };
            var createdParent = Helper.AssetManagement.Assets.Create(parentAsset);

            // Get asset class with Fan hierarchy role
            var fanAssetClass = Helper.TestData.AssetClasses
                .FirstOrDefault(ac => ac.DeviceTypeId != null); // Assume device type determines hierarchy role
            
            if (fanAssetClass == null)
            {
                fanAssetClass = testAssetClass; // Fallback
            }

            // Try to attach child with Fan role to Card slot
            var childAsset = new Asset
            {
                AssetID = "CHILD-WRONG-ROLE",
                Name = "Child With Mismatched Role",
                AssetClassId = new SdmObjectReference<AssetClass>(fanAssetClass.Identifier),
                Location = new AssetLocation
                {
                    ParentAsset = new SdmObjectReference<Asset>(createdParent.Identifier),
                    HolderNumber = 1, // Slot 1 expects Card, but child is Fan
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.Create(childAsset);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*does not have a holder slot*Hierarchy Role*");
        }

        [TestMethod]
        [Ignore("Waiting for nullable Location fields support")]
        public void CreateOrUpdate_WithSameHolderInBatch_ShouldFail()
        {
            // Arrange
            var parentAsset = new Asset
            {
                AssetID = "PARENT-FOR-BATCH",
                Name = "Parent For Batch Test",
                AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                Holders = new List<AssetHolder>
                {
                    new AssetHolder
                    {
                        SlotNumber = 1,
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card,
                    },
                },
            };
            var createdParent = Helper.AssetManagement.Assets.Create(parentAsset);

            var assets = new List<Asset>
            {
                new Asset
                {
                    AssetID = "BATCH-CHILD-001",
                    Name = "First Batch Child",
                    AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                    Location = new AssetLocation
                    {
                        ParentAsset = new SdmObjectReference<Asset>(createdParent.Identifier),
                        HolderNumber = 1,
                    },
                },
                new Asset
                {
                    AssetID = "BATCH-CHILD-002",
                    Name = "Second Batch Child",
                    AssetClassId = new SdmObjectReference<AssetClass>(testAssetClass.Identifier),
                    Location = new AssetLocation
                    {
                        ParentAsset = new SdmObjectReference<Asset>(createdParent.Identifier),
                        HolderNumber = 1, // Same holder as first child
                    },
                },
            };

            // Act
            Action act = () => Helper.AssetManagement.Assets.CreateOrUpdate(assets);

            // Assert
            act.Should().Throw<Exception>()
                .WithMessage("*already claimed by another asset in the validation batch*");
        }

        #endregion
    }
}