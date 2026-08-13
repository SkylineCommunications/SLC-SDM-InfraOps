namespace SDM.AssetManagement.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;

    [TestClass]
    public class DeleteValidationGuardTests : BaseRepositoryTest
    {
        [TestMethod]
        public void AssetClass_Delete_WhenActive_ShouldBeBlocked()
        {
            var assetClass = CreateAssetClass("Active Class", SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active);

            Action act = () => Helper.AssetManagement.AssetClasses.Delete(assetClass);

            act.Should().Throw<ValidationException>()
                .WithMessage("*Asset Class must not be in 'Active' State to Delete*");
        }

        [TestMethod]
        public void AssetClass_Delete_WhenAssetsReferenceIt_ShouldBeBlocked()
        {
            var assetClass = CreateAssetClass("Referenced Class", SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active);
            CreateAsset(assetClass, "AC-REF-ASSET");
            assetClass.State = SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Draft;

            Action act = () => Helper.AssetManagement.AssetClasses.Delete(assetClass);

            act.Should().Throw<ValidationException>()
                .WithMessage("*There are still Assets in this Asset Class. Please remove them first*");
        }

        [TestMethod]
        public void AssetClass_Delete_WhenDraftAndUnreferenced_ShouldBeAllowed()
        {
            var assetClass = CreateAssetClass("Unreferenced Draft Class", SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Draft);

            Action act = () => Helper.AssetManagement.AssetClasses.Delete(assetClass);

            act.Should().NotThrow();
        }

        [TestMethod]
        public void Asset_Delete_WhenStateIsNotAllowed_ShouldBeBlocked()
        {
            var assetClass = CreateAssetClass("Asset State Class", SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active);
            var asset = CreateAsset(assetClass, "ASSET-STATE-BLOCK");

            Action act = () => Helper.AssetManagement.Assets.Delete(asset);

            act.Should().Throw<ValidationException>()
                .WithMessage("*Asset must be in 'Not Available' or 'Disposed' State to Delete*");
        }

        [TestMethod]
        public void Asset_Delete_WhenAnyPortHasConnection_ShouldBeBlocked()
        {
            var assetClass = CreateAssetClass("Asset Connection Class", SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active, deviceTags: new List<SlcAsset_Management.Enums.TagOption> { SlcAsset_Management.Enums.TagOption.AcceptsDataConnection });
            var asset = CreateAsset(assetClass, "ASSET-CONNECTION-BLOCK");
            asset.State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable;
            var dataPort = CreateDataPort(asset, CreateDataPortType("Asset Connection Port Type"));
            CreateConnection(dataPort.Identifier, dataPort.DataPortInfo.Type);

            Action act = () => Helper.AssetManagement.Assets.Delete(asset);

            act.Should().Throw<ValidationException>()
                .WithMessage("*This asset has connections assigned. Please delete all of the connections first.*");
        }

        [TestMethod]
        public void Asset_Delete_WhenAllowedStateAndNoConnections_ShouldBeAllowed()
        {
            var assetClass = CreateAssetClass("Asset Allowed Class", SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active);
            var asset = CreateAsset(assetClass, "ASSET-DELETE-ALLOW");
            asset.State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable;

            Action act = () => Helper.AssetManagement.Assets.Delete(asset);

            act.Should().NotThrow();
        }

        [TestMethod]
        public void DataPort_Delete_WhenAssignedToConnection_ShouldBeBlocked()
        {
            var asset = CreateAsset(CreateAssetClass("Data Port Class", SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active, deviceTags: new List<SlcAsset_Management.Enums.TagOption> { SlcAsset_Management.Enums.TagOption.AcceptsDataConnection }), "DATA-PORT-ASSET");
            var dataPort = CreateDataPort(asset, CreateDataPortType("Data Port Connected Type"));
            CreateConnection(dataPort.Identifier, dataPort.DataPortInfo.Type);

            Action act = () => Helper.AssetManagement.DataPorts.Delete(dataPort);

            act.Should().Throw<ValidationException>()
                .WithMessage("*This port has connections assigned. Please delete all of the connections first.*");
        }

        [TestMethod]
        public void DataPort_Delete_WhenNotAssignedToConnection_ShouldBeAllowed()
        {
            var asset = CreateAsset(CreateAssetClass("Data Port Allowed Class", SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active), "DATA-PORT-ASSET-ALLOW");
            var dataPort = CreateDataPort(asset, CreateDataPortType("Data Port Allowed Type"));

            Action act = () => Helper.AssetManagement.DataPorts.Delete(dataPort);

            act.Should().NotThrow();
        }

        [TestMethod]
        public void PowerPort_Delete_WhenAssignedToConnection_ShouldBeBlocked()
        {
            var asset = CreateAsset(CreateAssetClass("Power Port Class", SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active, deviceTags: new List<SlcAsset_Management.Enums.TagOption> { SlcAsset_Management.Enums.TagOption.PowerProvider }, powerSupply: SlcAsset_Management.Enums.PowerSupplyEnum.AC), "POWER-PORT-ASSET");
            var powerPort = CreatePowerPort(asset, CreatePowerPortType("Power Port Connected Type"));
            CreateConnection(powerPort.Identifier, powerPort.PowerPortInfo.PortType);

            Action act = () => Helper.AssetManagement.PowerPorts.Delete(powerPort);

            act.Should().Throw<ValidationException>()
                .WithMessage("*This port has connections assigned. Please delete all of the connections first.*");
        }

        [TestMethod]
        public void PowerPort_Delete_WhenNotAssignedToConnection_ShouldBeAllowed()
        {
            var asset = CreateAsset(CreateAssetClass("Power Port Allowed Class", SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active), "POWER-PORT-ASSET-ALLOW");
            var powerPort = CreatePowerPort(asset, CreatePowerPortType("Power Port Allowed Type"));

            Action act = () => Helper.AssetManagement.PowerPorts.Delete(powerPort);

            act.Should().NotThrow();
        }

        [TestMethod]
        public void DeviceType_Delete_WhenAssetClassesReferenceIt_ShouldBeBlocked()
        {
            var deviceType = CreateDeviceType("Referenced Device Type");
            CreateAssetClass("Device Type Referencing Class", SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Draft, deviceType);

            Action act = () => Helper.AssetManagement.DeviceTypes.Delete(deviceType);

            act.Should().Throw<ValidationException>()
                .WithMessage("*There are still asset classes associated with this device type. Please remove them first.*");
        }

        [TestMethod]
        public void DeviceType_Delete_WhenNonDisposedAssetsReferenceIt_ShouldBeBlocked()
        {
            var deviceType = CreateDeviceType("Device Type With Assets");
            var assetClass = CreateAssetClass("Device Type Asset Class", SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active, deviceType);
            CreateAsset(assetClass, "DEVICE-TYPE-ASSET");

            Action act = () => Helper.AssetManagement.DeviceTypes.Delete(deviceType);

            act.Should().Throw<ValidationException>()
                .WithMessage("*There are already assets assigned to this device type not in the 'Disposed' State*");
        }

        [TestMethod]
        public void DeviceType_Delete_WhenUnreferenced_ShouldBeAllowed()
        {
            var deviceType = CreateDeviceType("Unreferenced Device Type");

            Action act = () => Helper.AssetManagement.DeviceTypes.Delete(deviceType);

            act.Should().NotThrow();
        }

        [TestMethod]
        public void PortType_Delete_WhenAssetPortsUseIt_ShouldBeBlocked()
        {
            var portType = CreateDataPortType("Used By Asset Port Type");
            var asset = CreateAsset(CreateAssetClass("Port Type Asset Class", SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active), "PORT-TYPE-ASSET");
            CreateDataPort(asset, portType);

            Action act = () => Helper.AssetManagement.PortTypes.Delete(portType);

            act.Should().Throw<ValidationException>()
                .WithMessage("*There are still asset with ports using this port type. Please remove them first.*");
        }

        [TestMethod]
        public void PortType_Delete_WhenAssetClassPortsUseIt_ShouldBeBlocked()
        {
            var portType = CreateDataPortType("Used By Asset Class Port Type");
            CreateAssetClass(
                "Port Type AssetClass Template",
                SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active,
                dataPorts: new List<DataPortInfo>
                {
                    new DataPortInfo
                    {
                        Name = "Template Data",
                        PortNumber = 1,
                        OutputType = SlcAsset_Management.Enums.Outputtype.IO,
                        PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Front,
                        Type = new SdmObjectReference<PortType>(portType.Identifier),
                    },
                });

            Action act = () => Helper.AssetManagement.PortTypes.Delete(portType);

            act.Should().Throw<ValidationException>()
                .WithMessage("*There are still asset classes with ports using this port type. Please remove them first.*");
        }

        [TestMethod]
        public void PortType_Delete_WhenUnreferenced_ShouldBeAllowed()
        {
            var portType = CreateDataPortType("Unreferenced Port Type");

            Action act = () => Helper.AssetManagement.PortTypes.Delete(portType);

            act.Should().NotThrow();
        }

        [TestMethod]
        public void CableType_Delete_WhenConnectionsUseIt_ShouldBeBlocked()
        {
            var cableType = CreateCableType("Connection Cable Type");
            var portType = CreateDataPortType("Cable Connection Port Type");
            var dataAcceptingDeviceType = CreateDeviceType("Cable Connection Device Type", new List<SlcAsset_Management.Enums.TagOption> { SlcAsset_Management.Enums.TagOption.AcceptsDataConnection });
            var dataPort = CreateDataPort(
                CreateAsset(CreateAssetClass("Cable Connection Class", SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum.Active, dataAcceptingDeviceType), "CABLE-CONNECTION-ASSET"),
                portType);
            CreateConnection(dataPort.Identifier, new SdmObjectReference<PortType>(portType.Identifier), cableType);

            Action act = () => Helper.AssetManagement.CableTypes.Delete(cableType);

            act.Should().Throw<ValidationException>()
                .WithMessage("*There are still connections using this cable type. Please remove them first.*");
        }

        [TestMethod]
        public void CableType_Delete_WhenPortTypesUseItAsCompatibility_ShouldBeBlocked()
        {
            var cableType = CreateCableType("Compatible Cable Type");
            CreateDataPortType("Cable Compatibility Port Type", cableType);

            Action act = () => Helper.AssetManagement.CableTypes.Delete(cableType);

            act.Should().Throw<ValidationException>()
                .WithMessage("*There are still port types using this cable type as compatibility. Please remove them first.*");
        }

        [TestMethod]
        public void CableType_Delete_WhenUnreferenced_ShouldBeAllowed()
        {
            var cableType = CreateCableType("Unreferenced Cable Type");

            Action act = () => Helper.AssetManagement.CableTypes.Delete(cableType);

            act.Should().NotThrow();
        }

        private AssetClass CreateAssetClass(
            string name,
            SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum state,
            DeviceType? deviceType = null,
            List<DataPortInfo>? dataPorts = null,
            List<SlcAsset_Management.Enums.TagOption>? deviceTags = null,
            SlcAsset_Management.Enums.PowerSupplyEnum? powerSupply = null)
        {
            deviceType = deviceType ?? CreateDeviceType($"{name} Device Type", deviceTags);
            var assetClass = new AssetClass
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = name,
                State = state,
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                PowerSupply = powerSupply,
                Depth = 10,
                Width = 20,
                Height = 30,
                HeightU = 1,
                Weight = 5,
                DataPorts = dataPorts ?? new List<DataPortInfo>(),
                PowerPorts = new List<PowerPortInfo>(),
                Holders = new List<AssetHolder>(),
            };

            return Helper.AssetManagement.AssetClasses.Create(assetClass);
        }

        private Asset CreateAsset(AssetClass assetClass, string assetId)
        {
            var asset = new Asset
            {
                Identifier = Guid.NewGuid().ToString(),
                AssetID = assetId,
                Name = $"{assetId} Name",
                AssetClassId = new SdmObjectReference<AssetClass>(assetClass.Identifier),
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Available,
            };

            return Helper.AssetManagement.Assets.Create(asset);
        }

        private DeviceType CreateDeviceType(string name, List<SlcAsset_Management.Enums.TagOption>? tags = null)
        {
            var deviceType = new DeviceType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = name,
                Description = $"{name} description",
                HierarchyInfo =
                {
                    HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.None,
                },
                TagsInfo =
                {
                    Tags = tags ?? new List<SlcAsset_Management.Enums.TagOption>(),
                },
            };

            return Helper.AssetManagement.DeviceTypes.Create(deviceType);
        }

        private PortType CreateDataPortType(string name, CableType? compatibleCableType = null)
        {
            return CreatePortType(name, SlcAsset_Management.Enums.CategoriesEnum.Data, compatibleCableType);
        }

        private PortType CreatePowerPortType(string name)
        {
            return CreatePortType(name, SlcAsset_Management.Enums.CategoriesEnum.Power);
        }

        private PortType CreatePortType(string name, SlcAsset_Management.Enums.CategoriesEnum category, CableType? compatibleCableType = null)
        {
            var cableRefs = compatibleCableType == null
                ? new List<SdmObjectReference<CableType>>()
                : new List<SdmObjectReference<CableType>> { new SdmObjectReference<CableType>(compatibleCableType.Identifier) };

            var portType = new PortType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = name,
                CategoryLinks =
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { category },
                },
                CableFKs =
                {
                    CableTypeFks = cableRefs,
                },
            };

            return Helper.AssetManagement.PortTypes.Create(portType);
        }

        private CableType CreateCableType(string name)
        {
            var cableType = new CableType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = name,
                CategoryLinks = new CategoryRelation
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Data },
                },
            };

            return Helper.AssetManagement.CableTypes.Create(cableType);
        }

        private DataPort CreateDataPort(Asset asset, PortType portType)
        {
            var dataPort = new DataPort
            {
                Identifier = Guid.NewGuid().ToString(),
                Asset = new SdmObjectReference<Asset>(asset.Identifier),
                DataPortInfo =
                {
                    Name = $"Data {Guid.NewGuid()}",
                    PortNumber = 1,
                    OutputType = SlcAsset_Management.Enums.Outputtype.IO,
                    PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Front,
                    Type = new SdmObjectReference<PortType>(portType.Identifier),
                },
            };

            return Helper.AssetManagement.DataPorts.Create(dataPort);
        }

        private PowerPort CreatePowerPort(Asset asset, PortType portType)
        {
            var powerPort = new PowerPort
            {
                Identifier = Guid.NewGuid().ToString(),
                Asset = new SdmObjectReference<Asset>(asset.Identifier),
                PowerPortInfo =
                {
                    Name = $"Power {Guid.NewGuid()}",
                    PortNumber = 1,
                    OutputType = SlcAsset_Management.Enums.Outputtype.IO,
                    PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Front,
                    PortType = new SdmObjectReference<PortType>(portType.Identifier),
                },
            };

            return Helper.AssetManagement.PowerPorts.Create(powerPort);
        }

        private void CreateConnection(string sourcePortId, SdmObjectReference<PortType> sourcePortType, CableType? cableType = null)
        {
            var connection = new Connection
            {
                Identifier = Guid.NewGuid().ToString(),
                CableType = cableType == null ? null : new SdmObjectReference<CableType>(cableType.Identifier),
                Source =
                {
                    Port = Guid.Parse(sourcePortId),
                    PortType = sourcePortType,
                },
                Destination =
                {
                    Port = Guid.Empty,
                    PortType = null,
                },
            };

            Helper.AssetManagement.Connections.Create(connection);
        }
    }
}
