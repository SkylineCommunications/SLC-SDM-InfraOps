namespace SDM.AssetManagement.Tests.Assets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    /// <summary>
    /// Regression tests guaranteeing that DOM Section IDs are reused (kept stable) across updates
    /// instead of being regenerated on every save. Mirrors the SectionId stability pattern ported
    /// from SLC-SDM-IPAM (ISectionTrackable).
    /// </summary>
    [TestClass]
    public class AssetSectionIdStabilityTests : BaseRepositoryTest
    {
        private Asset BuildReferenceAsset()
        {
            Helper.PopulateWithDemoData(DemoDataLayer.AssetClasses);
            var assetClass = Helper.TestData.AssetClasses.First();

            var originRoom = Helper.FacilityManagement.Rooms.Create(new Room
            {
                Identifier = Guid.NewGuid().ToString(),
                RoomId = $"ROOM-ORIGIN-{Guid.NewGuid()}",
                Name = "Origin Room",
            });

            var destinationRoom = Helper.FacilityManagement.Rooms.Create(new Room
            {
                Identifier = Guid.NewGuid().ToString(),
                RoomId = $"ROOM-DEST-{Guid.NewGuid()}",
                Name = "Destination Room",
            });

            return new Asset
            {
                AssetID = Guid.NewGuid().ToString(),
                Name = "Stability Asset",
                AssetClassId = new SdmObjectReference<AssetClass>(assetClass.Identifier),
                Description = "Original description",
                HardwareVersion = "HW1.0",
                MacAddress = "00-14-22-01-23-45",
                PurchaseDate = DateTime.UtcNow.AddYears(-1),
                FirstUseDate = DateTime.UtcNow.AddMonths(-11),
                EndOfWarrantyDate = DateTime.UtcNow.AddYears(1),
                InstallationDate = DateTime.UtcNow.AddMonths(-10),
                InstallationUserId = Guid.NewGuid(),
                EndOfLifeDate = DateTime.UtcNow.AddYears(5),
                State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.InTransit,
                Location = new AssetLocation
                {
                    RoomId = new SdmObjectReference<Room>(originRoom.Identifier),
                },
                DestinationLocation = new AssetLocation
                {
                    RoomId = new SdmObjectReference<Room>(destinationRoom.Identifier),
                },
                Ownership = new AssetOwnership
                {
                    Organization = Guid.NewGuid(),
                    ContactPerson = Guid.NewGuid(),
                    ContactPersonRole = Guid.NewGuid(),
                },
                Custody = new AssetCustody
                {
                    From = DateTime.UtcNow.AddMonths(-6),
                    Till = DateTime.UtcNow.AddMonths(6),
                    ContactPerson = Guid.NewGuid(),
                    ContactPersonRole = Guid.NewGuid(),
                },
                Holders = new List<AssetHolder>
                {
                    new AssetHolder { SlotNumber = 4, HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis },
                    new AssetHolder { SlotNumber = 1, HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card },
                },
                ElementLinks = new List<ElementLink>
                {
                    new ElementLink { ElementID = "123/456", IsPrimary = true },
                    new ElementLink { ElementID = "1845/2" },
                },
            };
        }

        private static SectionIdSnapshot Snapshot(Asset asset)
        {
            return new SectionIdSnapshot
            {
                AssetProperties = asset.AssetPropertiesSectionId,
                NetworkDetails = asset.NetworkDetailsSectionId,
                Lifecycle = asset.LifecycleSectionId,
                Location = asset.Location == null ? null : ((ISectionTrackable)asset.Location).SectionId,
                DestinationLocation = asset.DestinationLocation == null ? null : ((ISectionTrackable)asset.DestinationLocation).SectionId,
                Ownership = ((ISectionTrackable)asset.Ownership).SectionId,
                Custody = ((ISectionTrackable)asset.Custody).SectionId,
                Holders = asset.Holders.Select(h => ((ISectionTrackable)h).SectionId).ToList(),
                ElementLinks = asset.ElementLinks.Select(e => ((ISectionTrackable)e).SectionId).ToList(),
            };
        }

        [TestMethod]
        public void Update_ShouldReuseSectionIds_ForAllSectionShapes()
        {
            // Arrange — create an asset covering all three section shapes (inline, sub-model, multiple).
            var reference = BuildReferenceAsset();
            Helper.AssetManagement.Assets.Create(reference);

            var created = Helper.AssetManagement.Assets
                .Read(AssetExposers.AssetName.Equal(reference.Name))
                .Single();

            var before = Snapshot(created);

            // Every section should have been assigned a Section ID after the first save.
            using (new AssertionScope("captured section ids"))
            {
                before.AssetProperties.Should().NotBeNull();
                before.NetworkDetails.Should().NotBeNull();
                before.Lifecycle.Should().NotBeNull();
                before.Location.Should().NotBeNull();
                before.DestinationLocation.Should().NotBeNull();
                before.Ownership.Should().NotBeNull();
                before.Custody.Should().NotBeNull();
                before.Holders.Should().OnlyContain(id => id.HasValue).And.HaveCount(2);
                before.ElementLinks.Should().OnlyContain(id => id.HasValue).And.HaveCount(2);
            }

            // Act — mutate an unrelated scalar on the read-back object and persist the update.
            // The in-memory DOM test engine does not carry an existing instance's StatusId across an
            // isNew==false update the way the real DOM engine does, so mark the object as new to force
            // a valid StatusId to be written. This does not affect Section ID tracking: the Section IDs
            // captured on the read-back object must still be reused by ToInstance.
            created.IsNewInternal = true;
            created.Description = "Updated description";
            Helper.AssetManagement.Assets.CreateOrUpdate([created]);

            var updated = Helper.AssetManagement.Assets
                .Read(AssetExposers.AssetName.Equal(reference.Name))
                .Single();

            var after = Snapshot(updated);

            // Assert — the mutation was applied and every Section ID was reused, not regenerated.
            using (new AssertionScope("section ids reused after update"))
            {
                updated.Description.Should().Be("Updated description");

                after.AssetProperties.Should().Be(before.AssetProperties);
                after.NetworkDetails.Should().Be(before.NetworkDetails);
                after.Lifecycle.Should().Be(before.Lifecycle);
                after.Location.Should().Be(before.Location);
                after.DestinationLocation.Should().Be(before.DestinationLocation);
                after.Ownership.Should().Be(before.Ownership);
                after.Custody.Should().Be(before.Custody);
                after.Holders.Should().Equal(before.Holders);
                after.ElementLinks.Should().Equal(before.ElementLinks);
            }
        }

        [TestMethod]
        public void Update_WithMutatedHoldersList_ShouldReusePerElementSectionIds()
        {
            // Arrange — multiple-section shape: an Asset can have several AssetHolders,
            // each stored as its own Section instance with its own Section ID.
            var reference = BuildReferenceAsset();
            Helper.AssetManagement.Assets.Create(reference);

            var created = Helper.AssetManagement.Assets
                .Read(AssetExposers.AssetName.Equal(reference.Name))
                .Single();

            var chassisHolder = created.Holders.Single(h => h.HierarchyRole == SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis);
            var cardHolder = created.Holders.Single(h => h.HierarchyRole == SlcAsset_Management.Enums.HierarchyRoleEnum.Card);
            var chassisSectionId = ((ISectionTrackable)chassisHolder).SectionId;
            var cardSectionId = ((ISectionTrackable)cardHolder).SectionId;
            chassisSectionId.Should().NotBeNull();
            cardSectionId.Should().NotBeNull();
            chassisSectionId.Should().NotBe(cardSectionId.Value);

            // Act — remove the card holder, keep the chassis holder (mutated), add a brand-new fan holder.
            created.IsNewInternal = true;
            chassisHolder.SlotNumber = 8;
            created.Holders = new List<AssetHolder>
            {
                chassisHolder,
                new AssetHolder { SlotNumber = 2, HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Fan },
            };
            Helper.AssetManagement.Assets.CreateOrUpdate([created]);

            var updated = Helper.AssetManagement.Assets
                .Read(AssetExposers.AssetName.Equal(reference.Name))
                .Single();

            // Assert — the surviving holder kept its Section ID, the removed one is gone,
            // and the new holder received a fresh Section ID.
            using (new AssertionScope("per-element section ids"))
            {
                updated.Holders.Should().HaveCount(2);

                var survivingChassis = updated.Holders.Single(h => h.HierarchyRole == SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis);
                survivingChassis.SlotNumber.Should().Be(8);
                ((ISectionTrackable)survivingChassis).SectionId.Should().Be(chassisSectionId);

                var newFan = updated.Holders.Single(h => h.HierarchyRole == SlcAsset_Management.Enums.HierarchyRoleEnum.Fan);
                var newFanSectionId = ((ISectionTrackable)newFan).SectionId;
                newFanSectionId.Should().NotBeNull();
                newFanSectionId.Should().NotBe(chassisSectionId.Value);
                newFanSectionId.Should().NotBe(cardSectionId.Value);
            }
        }

        private sealed class SectionIdSnapshot
        {
            public Guid? AssetProperties { get; set; }

            public Guid? NetworkDetails { get; set; }

            public Guid? Lifecycle { get; set; }

            public Guid? Location { get; set; }

            public Guid? DestinationLocation { get; set; }

            public Guid? Ownership { get; set; }

            public Guid? Custody { get; set; }

            public List<Guid?> Holders { get; set; } = new List<Guid?>();

            public List<Guid?> ElementLinks { get; set; } = new List<Guid?>();
        }
    }
}
