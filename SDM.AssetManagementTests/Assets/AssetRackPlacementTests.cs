namespace SDM.AssetManagement.Tests.Assets
{
    using System;
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
    /// Rack placement boundary and occupancy tests, exercised through asset create/update validation.
    /// Ported from the SLC-S-InfraOps RackValidationHandler.ValidateRackSpace and RackHelper.AssignAssetToRack tests.
    /// </summary>
    [TestClass]
    public class AssetRackPlacementTests : BaseRepositoryTest
    {
        private const bool Top = true;
        private const bool Bottom = false;

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.PopulateWithDemoData(DemoDataLayer.DeviceTypes);
        }

        #region Rack Space Boundaries

        [DataTestMethod]
        [DataRow(Bottom, 1, 1, DisplayName = "Bottom_1U_AtMinPosition")]
        [DataRow(Bottom, 10, 1, DisplayName = "Bottom_1U_AtMaxPosition")]
        [DataRow(Bottom, 9, 2, DisplayName = "Bottom_2U_AtLastFittingPosition")]
        [DataRow(Bottom, 1, 3, DisplayName = "Bottom_3U_AtMinPosition")]
        [DataRow(Bottom, 8, 3, DisplayName = "Bottom_3U_AtLastFittingPosition")]
        [DataRow(Bottom, 1, 10, DisplayName = "Bottom_MaxU_AtPosition1_FillsEntireRack")]
        [DataRow(Top, 1, 1, DisplayName = "Top_1U_AtMinPosition")]
        [DataRow(Top, 10, 1, DisplayName = "Top_1U_AtMaxPosition")]
        [DataRow(Top, 2, 2, DisplayName = "Top_2U_AtFirstValidPosition")]
        [DataRow(Top, 10, 2, DisplayName = "Top_2U_AtMaxPosition")]
        [DataRow(Top, 3, 3, DisplayName = "Top_3U_AtFirstValidPosition")]
        [DataRow(Top, 10, 3, DisplayName = "Top_3U_AtMaxPosition")]
        [DataRow(Top, 10, 10, DisplayName = "Top_MaxU_AtMaxPosition_FillsEntireRack")]
        public void Create_InTenUnitRack_WithinBoundaries_ShouldSucceed(bool topNumbering, int position, int heightU)
        {
            var rack = CreateRack(10, topNumbering);
            var assetClass = CreateRackAssetClass(heightU);

            var created = Helper.AssetManagement.Assets.Create(NewRackAsset("FIT", assetClass, rack, position));

            var reloaded = Helper.AssetManagement.Assets.Read(AssetExposers.Identifier.Equal(created.Identifier)).Single();
            using (new AssertionScope())
            {
                reloaded.Location.RackId.Identifier.Should().Be(rack.Identifier);
                reloaded.Location.RackPosition.Should().Be(position);
            }
        }

        [DataTestMethod]
        [DataRow(Bottom, 9, 5, DisplayName = "HeightGoesOutOfRack")]
        [DataRow(Bottom, 10, 2, DisplayName = "Bottom_2U_AtMaxPosition")]
        [DataRow(Bottom, 9, 3, DisplayName = "Bottom_3U_ExceedsAtPosition9")]
        [DataRow(Bottom, 2, 10, DisplayName = "Bottom_MaxU_AtPosition2")]
        [DataRow(Top, 1, 2, DisplayName = "Top_2U_AtMinPosition")]
        [DataRow(Top, 1, 3, DisplayName = "Top_3U_AtMinPosition")]
        [DataRow(Top, 1, 10, DisplayName = "Top_MaxU_AtMinPosition")]
        public void Create_InTenUnitRack_ExtendingBeyondBoundaries_ShouldFail(bool topNumbering, int position, int heightU)
        {
            var rack = CreateRack(10, topNumbering);
            var assetClass = CreateRackAssetClass(heightU);

            Action act = () => Helper.AssetManagement.Assets.Create(NewRackAsset("OVERFLOW", assetClass, rack, position));

            act.Should().Throw<Exception>()
                .WithMessage($"*Invalid Position {position}. Extends beyond rack boundaries (Rack has 10 units)*");
        }

        [TestMethod]
        public void Create_WithRackPositionZero_And2UAsset_ShouldFail()
        {
            var rack = CreateRack(42, Bottom);
            var assetClass = CreateRackAssetClass(2);

            Action act = () => Helper.AssetManagement.Assets.Create(NewRackAsset("ZERO", assetClass, rack, 0));

            act.Should().Throw<Exception>()
                .WithMessage("*Rack Position must be greater than 0*");
        }

        #endregion

        #region Rack Space Occupancy

        [TestMethod]
        public void Create_2UAssetAtPosition1InEmpty42URack_ShouldAttachAssetToRack()
        {
            var rack = CreateRack(42, Bottom);
            var assetClass = CreateRackAssetClass(2);

            var created = Helper.AssetManagement.Assets.Create(NewRackAsset("ASSIGN", assetClass, rack, 1));

            var reloaded = Helper.AssetManagement.Assets.Read(AssetExposers.Identifier.Equal(created.Identifier)).Single();
            using (new AssertionScope())
            {
                reloaded.Location.RackId.Identifier.Should().Be(rack.Identifier);
                reloaded.Location.RackPosition.Should().Be(1);
                reloaded.Location.Side.Should().Be(SlcAsset_Management.Enums.SideEnum.Front);
            }
        }

        [DataTestMethod]
        [DataRow(1, DisplayName = "AssignAssetToRack_PositionOccupied")]
        [DataRow(5, DisplayName = "ValidateRackSpace_PositionOccupied")]
        public void Create_2UAssetAtPositionOccupiedBy2UAsset_ShouldFail(int position)
        {
            var rack = CreateRack(42, Bottom);
            var assetClass = CreateRackAssetClass(2);
            Helper.AssetManagement.Assets.Create(NewRackAsset("EXISTING", assetClass, rack, position));

            Action act = () => Helper.AssetManagement.Assets.Create(NewRackAsset("NEW", assetClass, rack, position));

            act.Should().Throw<Exception>()
                .WithMessage($"*Rack space is already occupied by asset 'Rack Asset EXISTING' at position {position}*");
        }

        [TestMethod]
        public void Create_5UAssetAtPosition8In10URack_ShouldFail()
        {
            var rack = CreateRack(10, Bottom);
            var assetClass = CreateRackAssetClass(5);

            Action act = () => Helper.AssetManagement.Assets.Create(NewRackAsset("OUT", assetClass, rack, 8));

            act.Should().Throw<Exception>()
                .WithMessage("*Invalid Position 8. Extends beyond rack boundaries (Rack has 10 units)*");
        }

        [TestMethod]
        public void Update_AssetKeepingItsOwnRackPosition_ShouldExcludeItselfFromOccupancy()
        {
            var rack = CreateRack(42, Bottom);
            var assetClass = CreateRackAssetClass(2);
            var created = Helper.AssetManagement.Assets.Create(NewRackAsset("SELF", assetClass, rack, 5));

            var update = Helper.AssetManagement.Assets.Read(AssetExposers.Identifier.Equal(created.Identifier)).Single();
            update.Description = "Updated in place";

            Action act = () => Helper.AssetManagement.Assets.CreateOrUpdate([update]);

            act.Should().NotThrow();
            var reloaded = Helper.AssetManagement.Assets.Read(AssetExposers.Identifier.Equal(created.Identifier)).Single();
            using (new AssertionScope())
            {
                reloaded.Description.Should().Be("Updated in place");
                reloaded.Location.RackPosition.Should().Be(5);
            }
        }

        #endregion

        #region Destination Rack Placement

        [TestMethod]
        public void Create_InTransitWithDestinationRackPositionOutOfRange_ShouldFail()
        {
            var rack = CreateRack(42, Bottom);
            var assetClass = CreateRackAssetClass(1);
            var asset = new Asset
            {
                AssetID = "RACK-ASSET-DEST",
                Name = "Rack Asset DEST",
                AssetClassId = new SdmObjectReference<AssetClass>(assetClass.Identifier),
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit,
                DestinationLocation =
                {
                    RackId = new SdmObjectReference<Rack>(rack.Identifier),
                    RackPosition = 100000,
                    Side = SlcAsset_Management.Enums.SideEnum.Front,
                },
            };

            Action act = () => Helper.AssetManagement.Assets.Create(asset);

            act.Should().Throw<Exception>();
        }

        #endregion

        #region Helpers

        private Rack CreateRack(int capacity, bool topNumbering)
        {
            var rack = new Rack
            {
                Identifier = Guid.NewGuid().ToString(),
                RackId = $"RACK-{Guid.NewGuid():N}",
                Name = $"Placement Rack {Guid.NewGuid():N}",
                Position = topNumbering
                    ? SlcFacility_Management.Enums.RackpositionenumEnum.Top
                    : SlcFacility_Management.Enums.RackpositionenumEnum.Bottom,
            };
            rack.Capacity.MaximumRackCapacity = capacity;

            return Helper.FacilityManagement.Racks.Create(rack);
        }

        private AssetClass CreateRackAssetClass(int heightU)
        {
            var deviceType = Helper.TestData.RackMountableDeviceType();

            return Helper.AssetManagement.AssetClasses.Create(new AssetClass
            {
                Manufacturer = PeopleApiMock.NewManufacturer(),
                Name = $"Rack asset class {heightU}U {Guid.NewGuid()}",
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                State = SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active,
                Height = heightU * 4.45,
                Width = 10,
                Depth = 1,
                HeightU = heightU,
                Weight = 1,
            });
        }

        private static Asset NewRackAsset(string suffix, AssetClass assetClass, Rack rack, int position)
        {
            return new Asset
            {
                AssetID = $"RACK-ASSET-{suffix}",
                Name = $"Rack Asset {suffix}",
                AssetClassId = new SdmObjectReference<AssetClass>(assetClass.Identifier),
                Location =
                {
                    RackId = new SdmObjectReference<Rack>(rack.Identifier),
                    RackPosition = position,
                    Side = SlcAsset_Management.Enums.SideEnum.Front,
                },
            };
        }

        #endregion
    }
}
