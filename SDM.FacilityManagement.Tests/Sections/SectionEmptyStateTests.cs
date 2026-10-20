namespace SDM.FacilityManagement.Tests.Sections
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    [TestClass]
    public class SectionEmptyStateTests
    {
        [TestMethod]
        public void FacilityRelation_DefaultState_IsEmpty()
        {
            new FacilityRelation().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void FacilityRelation_FacilitySet_IsNotEmpty()
        {
            new FacilityRelation().Also(x => x.Facility = NewReference<Facility>()).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void FloorRelation_DefaultState_IsEmpty()
        {
            new FloorRelation().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void FloorRelation_FloorSet_IsNotEmpty()
        {
            new FloorRelation().Also(x => x.Floor = NewReference<Floor>()).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void ImageInfo_DefaultState_IsEmpty()
        {
            new ImageInfo().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void ImageInfo_AnyFieldSet_IsNotEmpty()
        {
            new ImageInfo().Also(x => x.ImageFilePath = "rack.png").IsEmpty.Should().BeFalse();
            new ImageInfo().Also(x => x.UploadTimestamp = DateTime.UtcNow).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void RackCapacity_DefaultState_IsEmpty()
        {
            new RackCapacity().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void RackCapacity_AnyFieldSet_IsNotEmpty()
        {
            new RackCapacity().Also(x => x.MaximumRackCapacity = 42).IsEmpty.Should().BeFalse();
            new RackCapacity().Also(x => x.MaximumRackCapacity = 0).IsEmpty.Should().BeFalse();
            new RackCapacity().Also(x => x.MaximumPowerCapacity = 10).IsEmpty.Should().BeFalse();
            new RackCapacity().Also(x => x.MaximumPowerCapacity = 0).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void ResourceLink_DefaultState_IsEmpty()
        {
            new ResourceLink().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void ResourceLink_ResourceIdSet_IsNotEmpty()
        {
            new ResourceLink().Also(x => x.ResourceId = Guid.NewGuid()).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void RoomOwnership_DefaultState_IsEmpty()
        {
            new RoomOwnership().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void RoomOwnership_AnyFieldSet_IsNotEmpty()
        {
            new RoomOwnership().Also(x => x.Team = new PnoObjectReference<Team>(Guid.NewGuid())).IsEmpty.Should().BeFalse();
            new RoomOwnership().Also(x => x.Owner = new PnoObjectReference<Person>(Guid.NewGuid())).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void RoomRelation_DefaultState_IsEmpty()
        {
            new RoomRelation().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void RoomRelation_RoomSet_IsNotEmpty()
        {
            new RoomRelation().Also(x => x.Room = NewReference<Room>()).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void RowRelation_DefaultState_IsEmpty()
        {
            new RowRelation().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void RowRelation_RowSet_IsNotEmpty()
        {
            new RowRelation().Also(x => x.Row = NewReference<Row>()).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void SiteRelation_DefaultState_IsEmpty()
        {
            new SiteRelation().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void SiteRelation_SiteSet_IsNotEmpty()
        {
            new SiteRelation().Also(x => x.Site = NewReference<Site>()).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void ZoneCapacity_DefaultState_IsEmpty()
        {
            new ZoneCapacity().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void ZoneCapacity_CoolingCapacitySet_IsNotEmpty()
        {
            new ZoneCapacity().Also(x => x.CoolingCapacity = 12.5).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void ZoneRelation_DefaultState_IsEmpty()
        {
            new ZoneRelation().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void ZoneRelation_ZoneSet_IsNotEmpty()
        {
            new ZoneRelation().Also(x => x.Zone = NewReference<Zone>()).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void Rack_RoundTrip_PreservesZeroCapacityValues()
        {
            var rack = new Rack
            {
                Identifier = Guid.NewGuid().ToString(),
                Capacity =
                {
                    MaximumRackCapacity = 0,
                    MaximumPowerCapacity = 0,
                },
            };

            var repo = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(RackDomRepository));
            var toInstance = typeof(RackDomRepository).GetMethod("ToInstance", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var fromInstance = typeof(RackDomRepository).GetMethod("FromInstance", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var roundTripped = (Rack)fromInstance.Invoke(repo, new[] { toInstance.Invoke(repo, new object[] { rack }) });
            roundTripped.Capacity.IsEmpty.Should().BeFalse();
            roundTripped.Capacity.MaximumRackCapacity.Should().Be(0);
            roundTripped.Capacity.MaximumPowerCapacity.Should().Be(0);

            var secondRoundTrip = (Rack)fromInstance.Invoke(repo, new[] { toInstance.Invoke(repo, new object[] { roundTripped }) });
            secondRoundTrip.Capacity.IsEmpty.Should().BeFalse();
            secondRoundTrip.Capacity.MaximumRackCapacity.Should().Be(0);
            secondRoundTrip.Capacity.MaximumPowerCapacity.Should().Be(0);
        }

        [TestMethod]
        public void Rack_RevertFieldChanges_WhenLoadedWithZeroCapacity_KeepsZeroAndWritesSection()
        {
            var rack = new Rack
            {
                Identifier = Guid.NewGuid().ToString(),
                Capacity =
                {
                    MaximumRackCapacity = 0,
                    MaximumPowerCapacity = 0,
                },
            };

            var repo = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(RackDomRepository));
            var toInstance = typeof(RackDomRepository).GetMethod("ToInstance", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var fromInstance = typeof(RackDomRepository).GetMethod("FromInstance", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            var loaded = (Rack)fromInstance.Invoke(repo, new[] { toInstance.Invoke(repo, new object[] { rack }) });
            loaded.Capacity.MaximumRackCapacity = 42;

            var reverted = SdmChangeTracking.RevertFieldChanges(loaded);

            reverted.Should().ContainSingle(c => c.Section == nameof(Rack.Capacity) && c.FieldName == nameof(RackCapacity.MaximumRackCapacity));
            loaded.Capacity.MaximumRackCapacity.Should().Be(0);
            loaded.Capacity.IsEmpty.Should().BeFalse();

            var savedAgain = (Rack)fromInstance.Invoke(repo, new[] { toInstance.Invoke(repo, new object[] { loaded }) });
            savedAgain.Capacity.IsEmpty.Should().BeFalse();
            savedAgain.Capacity.MaximumRackCapacity.Should().Be(0);
        }

        [TestMethod]
        public void Rack_RevertFieldChanges_WhenUnsetCapacitySetToZero_RevertsToEmptySection()
        {
            var rack = new Rack
            {
                Identifier = Guid.NewGuid().ToString(),
            };
            rack.ResetChangeTracking();

            rack.Capacity.IsEmpty.Should().BeTrue();
            rack.Capacity.MaximumRackCapacity = 0;
            rack.Capacity.IsEmpty.Should().BeFalse();
            rack.Capacity.Changed.Should().BeTrue();

            var reverted = SdmChangeTracking.RevertFieldChanges(rack);

            reverted.Should().ContainSingle(c => c.Section == nameof(Rack.Capacity) && c.FieldName == nameof(RackCapacity.MaximumRackCapacity));
            rack.Capacity.IsEmpty.Should().BeTrue();
            rack.Capacity.Changed.Should().BeFalse();
        }

        private static SdmObjectReference<T> NewReference<T>() where T : SdmObject<T>
        {
            return new SdmObjectReference<T>(Guid.NewGuid().ToString());
        }
    }

    internal static class ObjectExtensions
    {
        public static T Also<T>(this T obj, Action<T> action)
        {
            action(obj);
            return obj;
        }
    }
}
