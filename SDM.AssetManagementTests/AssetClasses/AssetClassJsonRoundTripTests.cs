namespace SDM.AssetManagement.Tests.AssetClasses
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.SDM.InfraOps.Core.Converters;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

    [TestClass]
    public class AssetClassJsonRoundTripTests
    {
        [TestMethod]
        public void AssetClass_RoundTrip_PreservesAllPropertiesAndReferences()
        {
            var assetClass = new AssetClass
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Class",
                Description = "Description",
                DeviceTypeId = new SdmObjectReference<DeviceType>(Guid.NewGuid().ToString()),
                Manufacturer = new PnoObjectReference<Organization>(Guid.NewGuid()),
                Depth = 1.5,
                Height = 2.5,
                Width = 3.5,
                HeightU = 2,
                Weight = 10.25,
                Plan = "plan.png",
                FrontImage = "front.png",
                BackImage = "back.png",
                TypicalPowerConsumption = 100.5,
                MaximumPowerConsumption = 200.5,
                PowerSupply = SlcAsset_Management.Enums.PowerSupplyEnum.DC,
                CIType = "CI",
                IsBookable = true,
            };
            assetClass.Lifecycle.EndOfLife = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            assetClass.Lifecycle.EndOfService = new DateTime(2029, 6, 1, 0, 0, 0, DateTimeKind.Utc);
            assetClass.Lifecycle.NominalLifetime = TimeSpan.FromDays(365);
            assetClass.DataPorts.Add(new DataPortInfo
            {
                Name = "Data1",
                PortNumber = 1,
                OutputType = SlcAsset_Management.Enums.Outputtype.IO,
                PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Front,
                PortType = new SdmObjectReference<PortType>(Guid.NewGuid().ToString()),
                Label = "D1",
            });
            assetClass.PowerPorts.Add(new PowerPortInfo
            {
                Name = "Power1",
                PortNumber = 2,
                OutputType = SlcAsset_Management.Enums.Outputtype.In,
                PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Back,
                PortType = new SdmObjectReference<PortType>(Guid.NewGuid().ToString()),
                Label = "P1",
            });
            assetClass.Holders.Add(new AssetHolder
            {
                SlotNumber = 3,
                Label = "Slot",
                HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card,
            });
            assetClass.ProtocolLink.Protocol = "Protocol";
            assetClass.Attachments.Add(new Attachment
            {
                FilePath = "file.pdf",
                AttachedAt = new DateTime(2021, 2, 3, 0, 0, 0, DateTimeKind.Utc),
                AttachedBy = "user",
            });

            string json = InfraOpsJsonConvert.SerializeObject(assetClass);
            var restored = InfraOpsJsonConvert.DeserializeObject<AssetClass>(json);

            restored.Identifier.Should().Be(assetClass.Identifier);
            restored.DeviceTypeId.Should().Be(assetClass.DeviceTypeId);
            restored.Manufacturer.Should().Be(assetClass.Manufacturer);
            restored.PowerSupply.Should().Be(SlcAsset_Management.Enums.PowerSupplyEnum.DC);
            restored.Lifecycle.NominalLifetime.Should().Be(TimeSpan.FromDays(365));
            restored.DataPorts.Should().ContainSingle().Which.PortType.Should().Be(assetClass.DataPorts[0].PortType);
            restored.PowerPorts.Should().ContainSingle().Which.PortType.Should().Be(assetClass.PowerPorts[0].PortType);
            restored.Should().Be(assetClass);
        }

        [TestMethod]
        public void AssetClass_Serialize_WritesReferencesAsStringsAndEnumsAsNames()
        {
            var manufacturerId = Guid.NewGuid();
            var deviceTypeId = Guid.NewGuid().ToString();
            var assetClass = new AssetClass
            {
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceTypeId),
                Manufacturer = new PnoObjectReference<Organization>(manufacturerId),
                PowerSupply = SlcAsset_Management.Enums.PowerSupplyEnum.DC,
            };
            assetClass.Holders.Add(new AssetHolder { HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.SubCard });

            string json = InfraOpsJsonConvert.SerializeObject(assetClass);

            json.Should().Contain($"\"DeviceTypeId\":\"{deviceTypeId}\"");
            json.Should().Contain($"\"Manufacturer\":\"{manufacturerId}\"");
            json.Should().Contain("\"PowerSupply\":\"DC\"");
            json.Should().Contain("\"HierarchyRole\":\"SubCard\"");
        }

        [TestMethod]
        public void AssetClass_Serialize_EmptyReferencesAreNull()
        {
            string json = InfraOpsJsonConvert.SerializeObject(new AssetClass());

            json.Should().Contain("\"DeviceTypeId\":null");
            json.Should().Contain("\"Manufacturer\":null");
        }

        [TestMethod]
        public void AssetClass_Deserialize_NullAndEmptyReferencesBecomeDefault()
        {
            var restored = InfraOpsJsonConvert.DeserializeObject<AssetClass>(
                "{\"DeviceTypeId\":\"\",\"Manufacturer\":null,\"DataPorts\":[{\"PortType\":\"\"}],\"PowerPorts\":[{\"PortType\":null}]}");

            restored.DeviceTypeId.Identifier.Should().BeNullOrEmpty();
            restored.Manufacturer.Identifier.Should().Be(Guid.Empty);
            restored.DataPorts.Should().ContainSingle().Which.PortType.Identifier.Should().BeNullOrEmpty();
            restored.PowerPorts.Should().ContainSingle().Which.PortType.Identifier.Should().BeNullOrEmpty();
        }
    }
}
