namespace SDM.AssetManagement.Tests.AssetClasses
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;

    /// <summary>
    /// CRUD tests for AssetClass repository operations.
    /// </summary>
    [TestClass]
    public class AssetClassDomStorageProvider_CRUDTests : BaseRepositoryTest
    {
        private AssetClass referenceAssetClass = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            referenceAssetClass = new AssetClass
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Reference Class",
                DeviceTypeId = new SdmObjectReference<DeviceType>(Guid.NewGuid().ToString()), // Will be updated in tests
                Manufacturer = Guid.NewGuid(),
                Lifecycle =
                {
                    EndOfLife = DateTime.UtcNow.AddYears(5),
                    EndOfService = DateTime.UtcNow.AddYears(3),
                    NominalLifetime = TimeSpan.FromDays(365 * 7),
                },
                Description = "A dummy asset class for testing.",
                Height = 2.0,
                Depth = 0.5,
                Width = 0.4,
                HeightU = 1.0,
                Weight = 10.0,
                FrontImage = "front.png",
                BackImage = "back.png",
                MaximumPowerConsumption = 100.0,
                TypicalPowerConsumption = 80.0,
                PowerSupply = SlcAsset_Management.Enums.PowerSupplyEnum.AC,
                DataPorts = new List<DataPortInfo>
                {
                    new DataPortInfo
                    {
                        PortNumber = 1,
                        Name = "Port1",
                        PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Front,
                        OutputType = SlcAsset_Management.Enums.Outputtype.Out,
                        Label = "Label1",
                    },
                },
                PowerPorts = new List<PowerPortInfo>
                {
                    new PowerPortInfo
                    {
                        Name = "Power Port 1",
                        PortNumber = 1,
                        OutputType = SlcAsset_Management.Enums.Outputtype.Out,
                        PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Front,
                        Label = "Primary Power Port",
                    },
                    new PowerPortInfo
                    {
                        Name = "Power Port 2",
                        PortNumber = 2,
                        OutputType = SlcAsset_Management.Enums.Outputtype.In,
                        PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Back,
                        Label = "Backup Power Port",
                    },
                },
                Holders = new List<AssetHolder>
                {
                    new AssetHolder
                    {
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card,
                        SlotNumber = 1,
                    },
                    new AssetHolder
                    {
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Fan,
                        SlotNumber = 2,
                    },
                    new AssetHolder
                    {
                        HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis,
                        SlotNumber = 5,
                    },
                },
            };
        }

        #region Create Tests

        [TestMethod]
        public void Create_WithNonExistentDeviceTypeShouldFail_FromJson()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);

            string json = @"{
  ""Name"": ""My API Asset Class"",
  ""DeviceTypeId"": ""ca29a378-2ac2-d9b2-635a-94580d4691e8"",
  ""Description"": ""My First asset class API"",
  ""Manufacturer"": ""ca29a378-2ac2-d9b2-635a-94580d4691e8"",
  ""Depth"": 6814.415912601139,
  ""Height"": 1709.5826972307093,
  ""Width"": 7849.388501550755,
  ""HeightU"": 5037.701172850872,
  ""Weight"": 8761.514126576818,
  ""TypicalPowerConsumption"": 3494.876107564041,
  ""MaximumPowerConsumption"": 4408.18848273218,
  ""PowerSupply"": ""AC"",
  ""Lifecycle"": {
    ""EndOfLife"": ""2013-04-22T02:48:25.867Z"",
    ""EndOfService"": ""1997-11-29T06:35:16.473Z""
  }
}";
            var assetClassFromJson = Newtonsoft.Json.JsonConvert.DeserializeObject<AssetClass>(json);

            // Act
            
            var action = () => Helper.AssetManagement.AssetClasses.Create(assetClassFromJson);

            //Assert

            action.Should().Throw<Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions.ValidationException>()
                .WithMessage("*Device Type not found*");
        }

        [TestMethod]
        public void Create_WithNonExistentDeviceTypeShouldFail()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);

            var assetClass = new AssetClass
            {
                Name = "My API Asset Class",
                DeviceTypeId = new SdmObjectReference<DeviceType>("ca29a378-2ac2-d9b2-635a-94580d4691e8"),
                Description = "My Test Asset Class Description",
                Manufacturer = Guid.Parse("ca29a378-2ac2-d9b2-635a-94580d4691e8"),
                Depth = 6814.415912601139,
                Height = 1709.5826972307093,
                Width = 7849.388501550755,
                HeightU = 5037.701172850872,
                Weight = 8761.514126576818,
                TypicalPowerConsumption = 3494.876107564041,
                MaximumPowerConsumption = 4408.18848273218,
                PowerSupply = SlcAsset_Management.Enums.PowerSupplyEnum.AC,
                Lifecycle =
                {
                    EndOfLife = DateTime.Now,
                    EndOfService = DateTime.Now,
                }
            };

            // Act

            var action = () => Helper.AssetManagement.AssetClasses.Create(assetClass);

            //Assert

            action.Should().Throw<Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions.ValidationException>()
                .WithMessage("*Device Type not found*");
        }

        [TestMethod]
        public void Create_WithValidData_ShouldPersistAssetClass()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);
            var deviceType = Helper.TestData.DeviceTypes.First();
            referenceAssetClass.DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier);

            // Act
            Helper.AssetManagement.AssetClasses.Create(referenceAssetClass);

            // Assert
            AssertCreated();
        }

        [TestMethod]
        public void CreateOrUpdate_WithNewAssetClass_ShouldCreate()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);

            var deviceType = Helper.TestData.DeviceTypes.First();
            referenceAssetClass.DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier);

            // Act
            Helper.AssetManagement.AssetClasses.CreateOrUpdate([referenceAssetClass]);

            // Assert
            AssertCreated();
        }

        [TestMethod]
        public void CreateOrUpdate_WithExistingAssetClass_ShouldUpdate()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);

            var deviceType = Helper.TestData.DeviceTypes.First();
            referenceAssetClass.DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier);

            Helper.AssetManagement.AssetClasses.Create(referenceAssetClass);

            // Create updated version
            var updatedAssetClass = new AssetClass
            {
                Identifier = referenceAssetClass.Identifier,
                Name = "Updated Class Name",
                DeviceTypeId = referenceAssetClass.DeviceTypeId,
                Manufacturer = Guid.NewGuid(),
                Lifecycle =
                {
                    EndOfLife = DateTime.UtcNow.AddYears(10),
                    EndOfService = DateTime.UtcNow.AddYears(8),
                    NominalLifetime = TimeSpan.FromDays(365 * 10),
                },
                Description = "Updated asset class description.",
                Height = 30.0,
                Depth = 70.0,
                Width = 60.0,
                HeightU = 2.0,
                Weight = 20.0,
                FrontImage = "front2.png",
                BackImage = "back2.png",
                MaximumPowerConsumption = 200.0,
                TypicalPowerConsumption = 150.0,
                PowerSupply = SlcAsset_Management.Enums.PowerSupplyEnum.DC,
                DataPorts = new List<DataPortInfo>
                {
                    new DataPortInfo
                    {
                        PortNumber = 2,
                        Name = "Port2",
                        PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Back,
                        OutputType = SlcAsset_Management.Enums.Outputtype.In,
                        Label = "Label2",
                    },
                },
                PowerPorts = new List<PowerPortInfo>(),
                Holders = new List<AssetHolder>(),
            };

            // Act
            var persistedAssetClass = Helper.AssetManagement.AssetClasses.CreateOrUpdate([updatedAssetClass]).First();

            // Assert
            AssertAssetClassUpdateDifferences(referenceAssetClass, persistedAssetClass);
        }

        #endregion

        #region Read Tests

        [TestMethod]
        public void ReadPaged_WithValidFilter_ShouldReturnPages()
        {
            // Arrange
            const int pageSize = 2;
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.AssetClasses);

            var allFilter = new TRUEFilterElement<AssetClass>();
            var totalCount = Helper.TestData.AssetClasses.Count;

            // Act
            var pagedResult = Helper.AssetManagement.AssetClasses.ReadPaged(allFilter, pageSize);

            // Assert
            using (new AssertionScope())
            {
                pagedResult.Should().NotBeNull();
                pagedResult.Should().HaveCount((int)(totalCount / pageSize), "should have correct number of pages");
                pagedResult.Should().AllSatisfy(page => page.Should().HaveCount(pageSize), "each page should have correct size");
            }
        }

        #endregion

        #region Delete Tests

        [TestMethod]
        public void Delete_Single_ShouldRemoveAssetClass()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.AssetClasses);

            var initialCount = Helper.TestData.AssetClasses.Count;
            var assetClassToDelete = Helper.AssetManagement.AssetClasses
                .Read(AssetClassExposers.DeviceName.Equal("Router"))
                .First();
            assetClassToDelete.State = SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Draft;

            // Act
            Helper.AssetManagement.AssetClasses.Delete(assetClassToDelete);

            // Assert
            using (new AssertionScope())
            {
                Helper.AssetManagement.AssetClasses.Count(new TRUEFilterElement<AssetClass>())
                    .Should().Be(initialCount - 1, "one asset class should be deleted");

                Helper.AssetManagement.AssetClasses.Count(AssetClassExposers.Identifier.Equal(assetClassToDelete.Identifier))
                    .Should().Be(0, "deleted asset class should not exist");
            }
        }

        [TestMethod]
        public void Delete_Bulk_ShouldRemoveMultipleAssetClasses()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.AssetClasses);

            var initialCount = Helper.TestData.AssetClasses.Count;

            var filter = new ORFilterElement<AssetClass>(
                AssetClassExposers.DeviceName.Equal("UPS"),
                AssetClassExposers.DeviceName.Equal("Firewall"),
                AssetClassExposers.DeviceDescription.Contains("Ethernet", StringComparison.OrdinalIgnoreCase));

            var assetClassesToDelete = Helper.AssetManagement.AssetClasses.Read(filter).ToList();
            assetClassesToDelete.ForEach(assetClass => assetClass.State = SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Draft);
            var deleteCount = assetClassesToDelete.Count;

            // Act
            Helper.AssetManagement.AssetClasses.Delete(assetClassesToDelete);

            // Assert
            using (new AssertionScope())
            {
                Helper.AssetManagement.AssetClasses.Count(new TRUEFilterElement<AssetClass>())
                    .Should().Be(initialCount - deleteCount, $"{deleteCount} asset classes should be deleted");

                Helper.AssetManagement.AssetClasses.Count(AssetClassExposers.DeviceName.Equal("UPS"))
                    .Should().Be(0, "UPS should be deleted");

                Helper.AssetManagement.AssetClasses.Count(AssetClassExposers.DeviceName.Equal("Firewall"))
                    .Should().Be(0, "Firewall should be deleted");
            }
        }

        #endregion

        #region Assertion Helpers

        private static void AssertAssetClassUpdateDifferences(AssetClass original, AssetClass updated)
        {
            using (new AssertionScope())
            {
                // Basic properties
                updated.Name.Should().Be("Updated Class Name");
                updated.Description.Should().Be("Updated asset class description.");

                updated.Manufacturer.Should().NotBe(Guid.Empty);
                original.Manufacturer.Should().NotBe(Guid.Empty, "original Manufacturer must be set");
                updated.Manufacturer.Should().NotBe(original.Manufacturer);

                // Physical dimensions
                updated.Height.Should().Be(30.0);
                updated.Depth.Should().Be(70.0);
                updated.Width.Should().Be(60.0);
                updated.HeightU.Should().Be(2.0);
                updated.Weight.Should().Be(20.0);

                // Images
                updated.FrontImage.Should().Be("front2.png");
                updated.BackImage.Should().Be("back2.png");

                // Power
                updated.MaximumPowerConsumption.Should().Be(200.0);
                updated.TypicalPowerConsumption.Should().Be(150.0);
                updated.PowerSupply.Should().Be(SlcAsset_Management.Enums.PowerSupplyEnum.DC);

                // Lifecycle
                updated.Lifecycle.Should().NotBeNull();

                updated.Lifecycle.EndOfLife.Should().NotBeNull();
                original.Lifecycle.EndOfLife.Should().HaveValue("original Lifecycle must be set");
                updated.Lifecycle.EndOfLife.Should().BeAfter(original.Lifecycle.EndOfLife!.Value);

                original.Lifecycle.EndOfService.Should().HaveValue("original EndOfService must be set");
                updated.Lifecycle.EndOfService.Should().BeAfter(original.Lifecycle.EndOfService!.Value);

                original.Lifecycle.NominalLifetime.Should().HaveValue("original NominalLifetime must be set");
                updated.Lifecycle.NominalLifetime.Should().BeGreaterThan(original.Lifecycle.NominalLifetime!.Value);

                // Data Ports
                updated.DataPorts.Should().HaveCount(1);
                updated.DataPorts[0].PortNumber.Should().Be(2);
                updated.DataPorts[0].Name.Should().Be("Port2");
                updated.DataPorts[0].Label.Should().Be("Label2");
                updated.DataPorts[0].Type.Should().NotBeNull();
                updated.DataPorts[0].Type.HasValue().Should().BeFalse();
                updated.DataPorts[0].PortExposure.Should().Be(SlcAsset_Management.Enums.PortExposureEnum.Back);
                updated.DataPorts[0].OutputType.Should().Be(SlcAsset_Management.Enums.Outputtype.In);

                // Collections cleared
                updated.PowerPorts.Should().BeEmpty();
                updated.Holders.Should().BeEmpty();
            }
        }

        private void AssertCreated()
        {
            using (new AssertionScope())
            {
                Helper.AssetManagement.AssetClasses.Count(new TRUEFilterElement<AssetClass>()).Should().Be(1);

                var created = Helper.AssetManagement.AssetClasses.Read(new TRUEFilterElement<AssetClass>()).First();

                // Basic properties
                created.Should().NotBeNull();
                created.Name.Should().Be("Reference Class");
                created.Manufacturer.Should().NotBe(Guid.Empty);
                created.Description.Should().Be("A dummy asset class for testing.");

                // Physical dimensions
                created.Height.Should().Be(2.0);
                created.Depth.Should().Be(0.5);
                created.Width.Should().Be(0.4);
                created.HeightU.Should().Be(1.0);
                created.Weight.Should().Be(10.0);

                // Images
                created.FrontImage.Should().Be("front.png");
                created.BackImage.Should().Be("back.png");

                // Power
                created.MaximumPowerConsumption.Should().Be(100.0);
                created.TypicalPowerConsumption.Should().Be(80.0);
                created.PowerSupply.Should().Be(SlcAsset_Management.Enums.PowerSupplyEnum.AC);

                // Lifecycle
                created.Lifecycle.Should().NotBeNull();
                created.Lifecycle.EndOfLife.Should().BeAfter(DateTime.UtcNow);
                created.Lifecycle.EndOfService.Should().BeAfter(DateTime.UtcNow);
                created.Lifecycle.NominalLifetime.Should().Be(TimeSpan.FromDays(365 * 7));

                // Data Ports
                created.DataPorts.Should().HaveCount(1);
                created.DataPorts[0].PortNumber.Should().Be(1);
                created.DataPorts[0].Name.Should().Be("Port1");
                created.DataPorts[0].Label.Should().Be("Label1");
                created.DataPorts[0].Type.Should().NotBeNull();
                created.DataPorts[0].Type.HasValue().Should().BeFalse();
                created.DataPorts[0].PortExposure.Should().Be(SlcAsset_Management.Enums.PortExposureEnum.Front);
                created.DataPorts[0].OutputType.Should().Be(SlcAsset_Management.Enums.Outputtype.Out);

                // Power Ports
                created.PowerPorts.Should().HaveCount(2);
                created.PowerPorts[0].PortNumber.Should().Be(1);
                created.PowerPorts[0].PortExposure.Should().Be(SlcAsset_Management.Enums.PortExposureEnum.Front);
                created.PowerPorts[0].OutputType.Should().Be(SlcAsset_Management.Enums.Outputtype.Out);
                created.PowerPorts[1].PortNumber.Should().Be(2);
                created.PowerPorts[1].PortExposure.Should().Be(SlcAsset_Management.Enums.PortExposureEnum.Back);
                created.PowerPorts[1].OutputType.Should().Be(SlcAsset_Management.Enums.Outputtype.In);

                // Holders
                created.Holders.Should().HaveCount(3);
                created.Holders[0].HierarchyRole.Should().Be(SlcAsset_Management.Enums.HierarchyRoleEnum.Card);
                created.Holders[0].SlotNumber.Should().Be(1);
                created.Holders[1].HierarchyRole.Should().Be(SlcAsset_Management.Enums.HierarchyRoleEnum.Fan);
                created.Holders[1].SlotNumber.Should().Be(2);
                created.Holders[2].HierarchyRole.Should().Be(SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis);
                created.Holders[2].SlotNumber.Should().Be(5);
            }
        }

        #endregion
    }
}