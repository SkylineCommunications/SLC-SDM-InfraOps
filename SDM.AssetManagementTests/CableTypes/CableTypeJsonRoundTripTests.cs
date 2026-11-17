namespace SDM.AssetManagement.Tests.CableTypes
{
    using System;
    using System.Collections.Generic;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.Converters;

    [TestClass]
    public class CableTypeJsonRoundTripTests
    {
        [TestMethod]
        public void CableType_RoundTrip_PreservesAllProperties()
        {
            var cableType = new CableType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Cat6",
                Description = "Ethernet cable",
            };
            cableType.CategoryLinks.Categories = new List<SlcAsset_Management.Enums.CategoriesEnum>
            {
                SlcAsset_Management.Enums.CategoriesEnum.Audio,
                SlcAsset_Management.Enums.CategoriesEnum.Power,
            };

            string json = InfraOpsJsonConvert.SerializeObject(cableType);
            var restored = InfraOpsJsonConvert.DeserializeObject<CableType>(json);

            restored.Identifier.Should().Be(cableType.Identifier);
            restored.CategoryLinks.Categories.Should().Equal(cableType.CategoryLinks.Categories);
            restored.Should().Be(cableType);
            InfraOpsJsonConvert.SerializeObject(restored).Should().Be(json);
        }

        [TestMethod]
        public void CableType_AuditFields_ExportedButNotImported()
        {
            var cableType = new CableType
            {
                CreatedAt = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                CreatedBy = "creator",
                LastModified = new DateTime(2021, 6, 7, 8, 9, 10, DateTimeKind.Utc),
                LastModifiedBy = "modifier",
            };

            string json = InfraOpsJsonConvert.SerializeObject(cableType);
            var restored = InfraOpsJsonConvert.DeserializeObject<CableType>(json);

            json.Should().Contain("\"CreatedBy\":\"creator\"");
            json.Should().Contain("\"LastModifiedBy\":\"modifier\"");
            restored.CreatedAt.Should().Be(default(DateTime));
            restored.CreatedBy.Should().BeNull();
            restored.LastModified.Should().Be(default(DateTime));
            restored.LastModifiedBy.Should().BeNull();
        }

        [TestMethod]
        public void CableType_Serialize_WritesEnumsAsNames()
        {
            var cableType = new CableType();
            cableType.CategoryLinks.Categories = new List<SlcAsset_Management.Enums.CategoriesEnum>
            {
                SlcAsset_Management.Enums.CategoriesEnum.Broadcast,
            };

            string json = InfraOpsJsonConvert.SerializeObject(cableType);

            json.Should().Contain("\"Categories\":[\"Broadcast\"]");
        }
    }
}
