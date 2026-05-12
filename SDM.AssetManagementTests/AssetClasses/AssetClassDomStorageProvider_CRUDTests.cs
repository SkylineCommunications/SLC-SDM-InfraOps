namespace SDM.AssetManagement.Tests
{
    using System;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement;
    using Skyline.DataMiner.SDM.AssetManagement.Helpers;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Repositories;

    [TestClass]
    public partial class AssetClassDomStorageProvider
    {
        private AssetClass referenceAssetClass;

        [TestInitialize]
        public void TestInitialize()
        {
            var id = Guid.NewGuid();
            referenceAssetClass = new AssetClass
            {
                Identifier = id.ToString(),
                Id = id,
                Name = "Reference Class",
                DeviceTypeId = new SdmObjectReference<DeviceType>(Guid.NewGuid().ToString()),
                Manufacturer = Guid.NewGuid(),
                Lifecycle = new AssetClassLifecycle
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
                        Identifier = Guid.NewGuid().ToString(),
                        Name = "Power Port 1",
                        PortNumber = 1,
                        OutputType = SlcAsset_Management.Enums.Outputtype.Out,
                        PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Front,
                        Label = "Primary Power Port",
                    },
                   new PowerPortInfo
                    {
                        Identifier = Guid.NewGuid().ToString(),
                        Name = "Power Port 2",
                        PortNumber = 2,
                        OutputType = SlcAsset_Management.Enums.Outputtype.In,
                        PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Back,
                        Label = "Backup Power Port",
                    } ,
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

        [TestMethod]
        public void AssetClassDomStorageProvider_EmptyDOM_Create()
        {
            var helper = RepositoryInitialize.InitializeEmptyRepositories();
            helper.AssetClasses.Create(referenceAssetClass);

            AssertCreated(helper);
        }

        [TestMethod]
        public void AssetClassDomStorageProvider_EmptyDOM_CreateOrUpdate_Create()
        {
            var helper = RepositoryInitialize.InitializeEmptyRepositories();
            helper.AssetClasses.CreateOrUpdate([referenceAssetClass]);

            AssertCreated(helper);
        }

        [TestMethod]
        public void AssetClassDomStorageProvider_EmptyDOM_CreateOrUpdate_Update()
        {
            var helper = RepositoryInitialize.InitializeEmptyRepositories();
            helper.AssetClasses.Create(referenceAssetClass);

            // Change more things here
            var updatedAssetClass = new AssetClass
            {
                Identifier = referenceAssetClass.Identifier,
                Id = referenceAssetClass.Id,
                Name = "Updated Class Name",
                DeviceTypeId = new SdmObjectReference<DeviceType>(Guid.NewGuid().ToString()),
                Manufacturer = Guid.NewGuid(),
                Lifecycle = new AssetClassLifecycle
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
                    } ,
                },
                PowerPorts = new List<PowerPortInfo>(),
                Holders = new List<AssetHolder>(),
            };

            helper.AssetClasses.CreateOrUpdate([updatedAssetClass]);
            AssertAssetClassUpdateDifferences(referenceAssetClass, updatedAssetClass);
        }

        [TestMethod]
        public void AssetClassDomStorageProvider_ReadPaged()
        {
            const int pageCount = 2;
            var helper = RepositoryInitialize.InitializeEmptyRepositories();
            helper.PopulateAssetClasses();

            FilterElement<AssetClass> allFilter = new TRUEFilterElement<AssetClass>();
            var pagedResult = helper.AssetClasses.ReadPaged(allFilter, pageCount);
            var assetClassCount = helper.AssetClasses.Count(allFilter);

            using (new AssertionScope())
            {
                pagedResult.Should().NotBeNull();
                pagedResult.Should().HaveCount((int)(assetClassCount / pageCount));
                pagedResult.Should().AllSatisfy(page => page.Should().HaveCount(pageCount));
            }
        }

        [TestMethod]
        public void AssetClassDomStorageProvider_DeleteBulk()
        {
            var helper = RepositoryInitialize.InitializeEmptyRepositories();
            helper.PopulateAssetClasses();

            var filter = new ORFilterElement<AssetClass>(
                AssetClassExposers.DeviceName.Equal("UPS"),
                AssetClassExposers.DeviceName.Equal("Firewall"),
                AssetClassExposers.DeviceDescription.Contains("Ethernet", StringComparison.OrdinalIgnoreCase));
            var assetClassesToDelete = helper.AssetClasses.Read(filter);

            helper.AssetClasses.Delete(assetClassesToDelete);

            using (new AssertionScope())
            {
                helper.AssetClasses.Count(new TRUEFilterElement<AssetClass>()).Should().Be(DemoData.AssetClasses.Count - 3);
                helper.AssetClasses.Count(AssetClassExposers.DeviceName.Equal("UPS")).Should().Be(0);
                helper.AssetClasses.Count(AssetClassExposers.DeviceName.Equal("Firewall")).Should().Be(0);
                helper.AssetClasses.Count(AssetClassExposers.DeviceDescription.Contains("Fiber")).Should().Be(0);
            }
        }

        [TestMethod]
        public void AssetClassDomStorageProvider_EmptyDOM_DeleteSingle()
        {
            var helper = RepositoryInitialize.InitializeEmptyRepositories();
            helper.PopulateAssetClasses();

            var assetClassToDelete = helper.AssetClasses.Read(AssetClassExposers.DeviceName.Equal("Router")).First();

            helper.AssetClasses.Delete(assetClassToDelete);

            helper.AssetClasses.Count(new TRUEFilterElement<AssetClass>()).Should().Be(DemoData.AssetClasses.Count - 1);
            helper.AssetClasses.Count(AssetClassExposers.Identifier.Equal(assetClassToDelete.Id.ToString())).Should().Be(0);
        }

        private static void AssertAssetClassUpdateDifferences(AssetClass original, AssetClass updated)
        {
            using (new AssertionScope())
            {
                updated.Id.Should().Be(original.Id);
                updated.Name.Should().NotBe(original.Name);
                updated.Name.Should().Be("Updated Class Name");
                updated.Description.Should().NotBe(original.Description);
                updated.Description.Should().Be("Updated asset class description.");
                updated.Manufacturer.Should().NotBe(original.Manufacturer);
                updated.Height.Should().Be(30.0);
                updated.Depth.Should().Be(70.0);
                updated.Width.Should().Be(60.0);
                updated.HeightU.Should().Be(2.0);
                updated.Weight.Should().Be(20.0);
                updated.FrontImage.Should().Be("front2.png");
                updated.BackImage.Should().Be("back2.png");
                updated.MaximumPowerConsumption.Should().Be(200.0);
                updated.TypicalPowerConsumption.Should().Be(150.0);
                updated.PowerSupply.Should().Be(SlcAsset_Management.Enums.PowerSupplyEnum.DC);
                updated.Lifecycle.Should().NotBeNull();
                updated.Lifecycle.EndOfLife.Should().BeAfter(original.Lifecycle.EndOfLife);
                updated.Lifecycle.EndOfService.Should().BeAfter(original.Lifecycle.EndOfService);
                updated.Lifecycle.NominalLifetime.Should().BeGreaterThan(original.Lifecycle.NominalLifetime);
                updated.DataPorts.Should().HaveCount(1);
                updated.DataPorts[0].PortNumber.Should().Be(2);
                updated.DataPorts[0].Name.Should().Be("Port2");
                updated.DataPorts[0].Label.Should().Be("Label2");
                updated.DataPorts[0].Type.Should().Be(default);
                updated.DataPorts[0].PortExposure.Should().Be(SlcAsset_Management.Enums.PortExposureEnum.Back);
                updated.DataPorts[0].OutputType.Should().Be(SlcAsset_Management.Enums.Outputtype.In);
                updated.PowerPorts.Should().BeEmpty();
                updated.Holders.Should().BeEmpty();
            }
        }

        private void AssertCreated(IAssetManagementApiHelper helper)
        {
            using (new AssertionScope())
            {
                helper.AssetClasses.Count(new TRUEFilterElement<AssetClass>()).Should().Be(1);

                var createdClass = helper.AssetClasses.Read(new TRUEFilterElement<AssetClass>()).First();
                createdClass.Should().NotBeNull();
                createdClass.Name.Should().Be("Reference Class");
                createdClass.Manufacturer.Should().NotBe(Guid.Empty);
                createdClass.Description.Should().Be("A dummy asset class for testing.");
                createdClass.Height.Should().Be(2.0);
                createdClass.Depth.Should().Be(0.5);
                createdClass.Width.Should().Be(0.4);
                createdClass.HeightU.Should().Be(1.0);
                createdClass.Weight.Should().Be(10.0);
                createdClass.FrontImage.Should().Be("front.png");
                createdClass.BackImage.Should().Be("back.png");
                createdClass.MaximumPowerConsumption.Should().Be(100.0);
                createdClass.TypicalPowerConsumption.Should().Be(80.0);
                createdClass.PowerSupply.Should().Be(SlcAsset_Management.Enums.PowerSupplyEnum.AC);
                createdClass.Lifecycle.Should().NotBeNull();
                createdClass.Lifecycle.EndOfLife.Should().BeAfter(DateTime.UtcNow);
                createdClass.Lifecycle.EndOfService.Should().BeAfter(DateTime.UtcNow);
                createdClass.Lifecycle.NominalLifetime.Should().Be(TimeSpan.FromDays(365 * 7));
                createdClass.DataPorts.Should().HaveCount(1);
                createdClass.DataPorts[0].PortNumber.Should().Be(1);
                createdClass.DataPorts[0].Name.Should().Be("Port1");
                createdClass.DataPorts[0].Type.Should().Be(default);
                createdClass.DataPorts[0].Label.Should().Be("Label1");
                createdClass.DataPorts[0].PortExposure.Should().Be(SlcAsset_Management.Enums.PortExposureEnum.Front);
                createdClass.DataPorts[0].OutputType.Should().Be(SlcAsset_Management.Enums.Outputtype.Out);
                createdClass.PowerPorts.Should().HaveCount(2);
                createdClass.PowerPorts[0].PortNumber.Should().Be(1);
                createdClass.PowerPorts[0].PortExposure.Should().Be(SlcAsset_Management.Enums.PortExposureEnum.Front);
                createdClass.PowerPorts[0].OutputType.Should().Be(SlcAsset_Management.Enums.Outputtype.Out);
                createdClass.PowerPorts[1].PortNumber.Should().Be(2);
                createdClass.PowerPorts[1].PortExposure.Should().Be(SlcAsset_Management.Enums.PortExposureEnum.Back);
                createdClass.PowerPorts[1].OutputType.Should().Be(SlcAsset_Management.Enums.Outputtype.In);
                createdClass.Holders.Should().HaveCount(3);
                createdClass.Holders[0].HierarchyRole.Should().Be(SlcAsset_Management.Enums.HierarchyRoleEnum.Card);
                createdClass.Holders[0].SlotNumber.Should().Be(1);
                createdClass.Holders[1].HierarchyRole.Should().Be(SlcAsset_Management.Enums.HierarchyRoleEnum.Fan);
                createdClass.Holders[1].SlotNumber.Should().Be(2);
                createdClass.Holders[2].HierarchyRole.Should().Be(SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis);
                createdClass.Holders[2].SlotNumber.Should().Be(5);
            }
        }
    }
}