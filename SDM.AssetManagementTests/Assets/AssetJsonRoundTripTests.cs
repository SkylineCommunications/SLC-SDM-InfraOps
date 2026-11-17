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
                AssetID = "A-100",
                SerialNumber = "SN-1",
                Description = "Description",
                FW_OS = "FW 1.2.3",
                HardwareVersion = "HW 4",
                MacAddress = "AA:BB:CC:DD:EE:FF",
                AssetClassId = new SdmObjectReference<AssetClass>(Guid.NewGuid().ToString()),
                InstallationUserId = new PnoObjectReference<Person>(Guid.NewGuid()),
                ModificationUserId = new PnoObjectReference<Person>(Guid.NewGuid()),
                InstallationDate = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                FirstUseDate = new DateTime(2020, 2, 2, 0, 0, 0, DateTimeKind.Utc),
                PurchaseDate = new DateTime(2019, 3, 3, 0, 0, 0, DateTimeKind.Utc),
                ModificationDate = new DateTime(2021, 4, 4, 0, 0, 0, DateTimeKind.Utc),
                EndOfLifeDate = new DateTime(2030, 5, 5, 0, 0, 0, DateTimeKind.Utc),
                EndOfWarrantyDate = new DateTime(2025, 6, 6, 0, 0, 0, DateTimeKind.Utc),
            };
            asset.Location.ParentAsset = new SdmObjectReference<Asset>(Guid.NewGuid().ToString());
            asset.Location.HolderNumber = 7;
            asset.Location.RackId = new SdmObjectReference<Rack>(Guid.NewGuid().ToString());
            asset.Location.RackPosition = 5;
            asset.Location.Side = SlcAsset_Management.Enums.SideEnum.Right;
            asset.Location.DeskId = new SdmObjectReference<Desk>(Guid.NewGuid().ToString());
            asset.Location.ContainerId = new SdmObjectReference<Facility>(Guid.NewGuid().ToString());
            asset.Location.RoomId = new SdmObjectReference<Room>(Guid.NewGuid().ToString());
            asset.DestinationLocation.ParentAsset = new SdmObjectReference<Asset>(Guid.NewGuid().ToString());
            asset.DestinationLocation.HolderNumber = 8;
            asset.DestinationLocation.RackId = new SdmObjectReference<Rack>(Guid.NewGuid().ToString());
            asset.DestinationLocation.RackPosition = 9;
            asset.DestinationLocation.Side = SlcAsset_Management.Enums.SideEnum.Left;
            asset.DestinationLocation.DeskId = new SdmObjectReference<Desk>(Guid.NewGuid().ToString());
            asset.DestinationLocation.ContainerId = new SdmObjectReference<Facility>(Guid.NewGuid().ToString());
            asset.DestinationLocation.RoomId = new SdmObjectReference<Room>(Guid.NewGuid().ToString());
            asset.Ownership.Organization = new PnoObjectReference<Organization>(Guid.NewGuid());
            asset.Ownership.ContactPerson = new PnoObjectReference<Person>(Guid.NewGuid());
            asset.Ownership.ContactPersonRole = new PnoObjectReference<Role>(Guid.NewGuid());
            asset.Ownership.Team = new PnoObjectReference<Team>(Guid.NewGuid());
            asset.Custody.From = new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            asset.Custody.Till = new DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            asset.Custody.Organization = new PnoObjectReference<Organization>(Guid.NewGuid());
            asset.Custody.ContactPerson = new PnoObjectReference<Person>(Guid.NewGuid());
            asset.Custody.ContactPersonRole = new PnoObjectReference<Role>(Guid.NewGuid());
            asset.Custody.Team = new PnoObjectReference<Team>(Guid.NewGuid());
            asset.Holders.Add(new AssetHolder { SlotNumber = 2, Label = "L", HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card });
            asset.Holders.Add(new AssetHolder { SlotNumber = 3, Label = "M", HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Fan });
            asset.ElementLinks.Add(new ElementLink { ElementID = "1/2", IsPrimary = true });
            asset.ElementLinks.Add(new ElementLink { ElementID = "3/4", IsPrimary = true });
            asset.Attachments.Add(new Attachment
            {
                FilePath = "file.pdf",
                AttachedAt = new DateTime(2021, 2, 3, 0, 0, 0, DateTimeKind.Utc),
                AttachedBy = "user",
            });
            asset.Attachments.Add(new Attachment
            {
                FilePath = "other.png",
                AttachedAt = new DateTime(2022, 5, 6, 0, 0, 0, DateTimeKind.Utc),
                AttachedBy = "user2",
            });
            asset.OperationalFlags.Add(SlcAsset_Management.Enums.Operationalflagsenum.Faulty);

            string json = InfraOpsJsonConvert.SerializeObject(asset);
            var restored = InfraOpsJsonConvert.DeserializeObject<Asset>(json);

            restored.Identifier.Should().Be(asset.Identifier);
            restored.AssetClassId.Should().Be(asset.AssetClassId);
            restored.InstallationUserId.Should().Be(asset.InstallationUserId);
            restored.ModificationUserId.Should().Be(asset.ModificationUserId);
            restored.Location.RackId.Should().Be(asset.Location.RackId);
            restored.Location.ParentAsset.Should().Be(asset.Location.ParentAsset);
            restored.Location.DeskId.Should().Be(asset.Location.DeskId);
            restored.Location.ContainerId.Should().Be(asset.Location.ContainerId);
            restored.Location.RoomId.Should().Be(asset.Location.RoomId);
            restored.DestinationLocation.RackId.Should().Be(asset.DestinationLocation.RackId);
            restored.Ownership.Organization.Should().Be(asset.Ownership.Organization);
            restored.Ownership.ContactPerson.Should().Be(asset.Ownership.ContactPerson);
            restored.Ownership.ContactPersonRole.Should().Be(asset.Ownership.ContactPersonRole);
            restored.Ownership.Team.Should().Be(asset.Ownership.Team);
            restored.Custody.Organization.Should().Be(asset.Custody.Organization);
            restored.Custody.ContactPerson.Should().Be(asset.Custody.ContactPerson);
            restored.Custody.ContactPersonRole.Should().Be(asset.Custody.ContactPersonRole);
            restored.Custody.Team.Should().Be(asset.Custody.Team);
            restored.Holders.Should().HaveCount(2);
            restored.ElementLinks.Should().HaveCount(2);
            restored.Attachments.Should().HaveCount(2);
            restored.Should().Be(asset);
            InfraOpsJsonConvert.SerializeObject(restored).Should().Be(json);
        }

        [TestMethod]
        public void Asset_AuditFields_ExportedButNotImported()
        {
            var asset = new Asset
            {
                CreatedAt = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                CreatedBy = "creator",
                LastModified = new DateTime(2021, 6, 7, 8, 9, 10, DateTimeKind.Utc),
                LastModifiedBy = "modifier",
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InService,
            };

            string json = InfraOpsJsonConvert.SerializeObject(asset);
            var restored = InfraOpsJsonConvert.DeserializeObject<Asset>(json);

            json.Should().Contain("\"CreatedBy\":\"creator\"");
            json.Should().Contain("\"LastModifiedBy\":\"modifier\"");
            json.Should().Contain("\"State\":\"InService\"");
            restored.CreatedAt.Should().Be(default(DateTime));
            restored.CreatedBy.Should().BeNull();
            restored.LastModified.Should().Be(default(DateTime));
            restored.LastModifiedBy.Should().BeNull();
            restored.State.Should().Be(SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable);
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
