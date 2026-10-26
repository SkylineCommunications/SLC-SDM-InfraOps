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
                SlcAsset_Management.Enums.TagOption.AcceptsDataConnection,
                SlcAsset_Management.Enums.TagOption.RackUnitConsumer,
            };
            deviceType.HierarchyInfo.HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Module;

            string json = InfraOpsJsonConvert.SerializeObject(deviceType);
            var restored = InfraOpsJsonConvert.DeserializeObject<DeviceType>(json);

            restored.Identifier.Should().Be(deviceType.Identifier);
            restored.TagsInfo.Tags.Should().Equal(deviceType.TagsInfo.Tags);
            restored.HierarchyInfo.HierarchyRole.Should().Be(SlcAsset_Management.Enums.HierarchyRoleEnum.Module);
            restored.Should().Be(deviceType);
            InfraOpsJsonConvert.SerializeObject(restored).Should().Be(json);
        }

        [TestMethod]
        public void DeviceType_AuditFields_ExportedButNotImported()
        {
            var deviceType = new DeviceType
            {
                CreatedAt = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                CreatedBy = "creator",
                LastModified = new DateTime(2021, 6, 7, 8, 9, 10, DateTimeKind.Utc),
                LastModifiedBy = "modifier",
            };

            string json = InfraOpsJsonConvert.SerializeObject(deviceType);
            var restored = InfraOpsJsonConvert.DeserializeObject<DeviceType>(json);

            json.Should().Contain("\"CreatedBy\":\"creator\"");
            json.Should().Contain("\"LastModifiedBy\":\"modifier\"");
            restored.CreatedAt.Should().Be(default(DateTime));
            restored.CreatedBy.Should().BeNull();
            restored.LastModified.Should().Be(default(DateTime));
            restored.LastModifiedBy.Should().BeNull();
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
