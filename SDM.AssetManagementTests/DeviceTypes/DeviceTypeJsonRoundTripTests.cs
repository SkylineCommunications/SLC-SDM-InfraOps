namespace SDM.AssetManagement.Tests.DeviceTypes
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.Converters;

    [TestClass]
    public class DeviceTypeJsonRoundTripTests
    {
        [TestMethod]
        public void DeviceType_RoundTrip_PreservesAllProperties()
        {
            var deviceType = new DeviceType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Switch",
                Description = "Network switch",
            };
            deviceType.TagsInfo.Tags = new System.Collections.Generic.List<SlcAsset_Management.Enums.TagOption>
            {
                SlcAsset_Management.Enums.TagOption.PowerProvider,
                SlcAsset_Management.Enums.TagOption.RackUnitConsumer,
            };
            deviceType.HierarchyInfo.HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis;

            string json = InfraOpsJsonConvert.SerializeObject(deviceType);
            var restored = InfraOpsJsonConvert.DeserializeObject<DeviceType>(json);

            restored.Identifier.Should().Be(deviceType.Identifier);
            restored.TagsInfo.Tags.Should().Equal(deviceType.TagsInfo.Tags);
            restored.HierarchyInfo.HierarchyRole.Should().Be(SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis);
            restored.Should().Be(deviceType);
        }

        [TestMethod]
        public void DeviceType_Serialize_WritesEnumsAsNames()
        {
            var deviceType = new DeviceType();
            deviceType.TagsInfo.Tags = new System.Collections.Generic.List<SlcAsset_Management.Enums.TagOption>
            {
                SlcAsset_Management.Enums.TagOption.AcceptsDataConnection,
            };
            deviceType.HierarchyInfo.HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Fan;

            string json = InfraOpsJsonConvert.SerializeObject(deviceType);

            json.Should().Contain("\"Tags\":[\"AcceptsDataConnection\"]");
            json.Should().Contain("\"HierarchyRole\":\"Fan\"");
        }
    }
}
