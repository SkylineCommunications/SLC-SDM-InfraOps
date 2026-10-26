namespace SDM.AssetManagement.Tests.Assets
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.SDM.InfraOps.Core.Converters;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

    [TestClass]
    public class AssetJsonRoundTripTests
    {
        [TestMethod]
        public void Asset_RoundTrip_PreservesSdmAndPnoReferences()
        {
            var asset = new Asset
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Asset",
                SerialNumber = "SN-1",
                AssetClassId = new SdmObjectReference<AssetClass>(Guid.NewGuid().ToString()),
                InstallationUserId = new PnoObjectReference<Person>(Guid.NewGuid()),
                InstallationDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            };
            asset.Location.RackId = new SdmObjectReference<Rack>(Guid.NewGuid().ToString());
            asset.Location.RackPosition = 5;
            asset.Ownership.Team = new PnoObjectReference<Team>(Guid.NewGuid());
            asset.Holders.Add(new AssetHolder { SlotNumber = 2, Label = "L" });
            asset.ElementLinks.Add(new ElementLink { ElementID = "1/2", IsPrimary = true });
            asset.OperationalFlags.Add(SlcAsset_Management.Enums.Operationalflagsenum.Faulty);

            string json = InfraOpsJsonConvert.SerializeObject(asset);
            var restored = InfraOpsJsonConvert.DeserializeObject<Asset>(json);

            restored.Identifier.Should().Be(asset.Identifier);
            restored.AssetClassId.Should().Be(asset.AssetClassId);
            restored.InstallationUserId.Should().Be(asset.InstallationUserId);
            restored.Location.RackId.Should().Be(asset.Location.RackId);
            restored.Ownership.Team.Should().Be(asset.Ownership.Team);
            restored.Should().Be(asset);
        }

        [TestMethod]
        public void Asset_Serialize_WritesReferencesAsStringsAndEnumsAsNames()
        {
            var userId = Guid.NewGuid();
            var asset = new Asset
            {
                InstallationUserId = new PnoObjectReference<Person>(userId),
            };
            asset.OperationalFlags.Add(SlcAsset_Management.Enums.Operationalflagsenum.Faulty);

            string json = InfraOpsJsonConvert.SerializeObject(asset);

            json.Should().Contain($"\"InstallationUserId\":\"{userId}\"");
            json.Should().Contain("\"OperationalFlags\":[\"Faulty\"]");
        }

        [TestMethod]
        public void Asset_Serialize_EmptyReferencesAreNull()
        {
            string json = InfraOpsJsonConvert.SerializeObject(new Asset());

            json.Should().Contain("\"InstallationUserId\":null");
            json.Should().Contain("\"AssetClassId\":null");
        }

        [TestMethod]
        public void Asset_Deserialize_NullAndEmptyReferencesBecomeDefault()
        {
            var restored = InfraOpsJsonConvert.DeserializeObject<Asset>(
                "{\"InstallationUserId\":null,\"AssetClassId\":\"\",\"Ownership\":{\"Team\":null}}");

            restored.InstallationUserId.Identifier.Should().Be(Guid.Empty);
            restored.AssetClassId.Identifier.Should().BeNullOrEmpty();
            restored.Ownership.Team.Identifier.Should().Be(Guid.Empty);
        }
    }
}
