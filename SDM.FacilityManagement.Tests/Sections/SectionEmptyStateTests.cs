namespace SDM.FacilityManagement.Tests.Sections
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

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
            new RackCapacity().Also(x => x.MaximumPowerCapacity = 10).IsEmpty.Should().BeFalse();
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
            new RoomOwnership().Also(x => x.Team = Guid.NewGuid()).IsEmpty.Should().BeFalse();
            new RoomOwnership().Also(x => x.Owner = Guid.NewGuid()).IsEmpty.Should().BeFalse();
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
