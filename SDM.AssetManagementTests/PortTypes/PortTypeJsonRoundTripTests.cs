namespace SDM.AssetManagement.Tests.PortTypes
{
    using System;
    using System.Collections.Generic;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.Converters;

    [TestClass]
    public class PortTypeJsonRoundTripTests
    {
        [TestMethod]
        public void PortType_RoundTrip_PreservesAllPropertiesAndReferences()
        {
            var portType = new PortType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "RJ45",
                Description = "Ethernet port",
            };
            portType.CategoryLinks.Categories = new List<SlcAsset_Management.Enums.CategoriesEnum>
            {
                SlcAsset_Management.Enums.CategoriesEnum.Networking,
                SlcAsset_Management.Enums.CategoriesEnum.Data,
            };
            portType.CableFKs.CableTypeFks = new List<SdmObjectReference<CableType>>
            {
                new SdmObjectReference<CableType>(Guid.NewGuid().ToString()),
                new SdmObjectReference<CableType>(Guid.NewGuid().ToString()),
            };

            string json = InfraOpsJsonConvert.SerializeObject(portType);
            var restored = InfraOpsJsonConvert.DeserializeObject<PortType>(json);

            restored.Identifier.Should().Be(portType.Identifier);
            restored.CategoryLinks.Categories.Should().Equal(portType.CategoryLinks.Categories);
            restored.CableFKs.CableTypeFks.Should().Equal(portType.CableFKs.CableTypeFks);
            restored.Should().Be(portType);
        }

        [TestMethod]
        public void PortType_Serialize_WritesReferencesAsStringsAndEnumsAsNames()
        {
            var cableTypeId = Guid.NewGuid().ToString();
            var portType = new PortType();
            portType.CategoryLinks.Categories = new List<SlcAsset_Management.Enums.CategoriesEnum>
            {
                SlcAsset_Management.Enums.CategoriesEnum.Video,
            };
            portType.CableFKs.CableTypeFks = new List<SdmObjectReference<CableType>>
            {
                new SdmObjectReference<CableType>(cableTypeId),
            };

            string json = InfraOpsJsonConvert.SerializeObject(portType);

            json.Should().Contain("\"Categories\":[\"Video\"]");
            json.Should().Contain($"\"CableTypeFks\":[\"{cableTypeId}\"]");
        }

        [TestMethod]
        public void PortType_Deserialize_NullAndEmptyReferencesBecomeDefault()
        {
            var restored = InfraOpsJsonConvert.DeserializeObject<PortType>(
                "{\"CableFKs\":{\"CableTypeFks\":[null,\"\"]}}");

            restored.CableFKs.CableTypeFks.Should().HaveCount(2);
            restored.CableFKs.CableTypeFks.Should().OnlyContain(reference => string.IsNullOrEmpty(reference.Identifier));
        }
    }
}
