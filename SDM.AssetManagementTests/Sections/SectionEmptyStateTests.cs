namespace SDM.AssetManagement.Tests.Sections
{
    using System;
    using System.Collections.Generic;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    [TestClass]
    public class SectionEmptyStateTests
    {
        [TestMethod]
        public void AddressInfo_DefaultState_IsEmpty()
        {
            new AddressInfo().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void AddressInfo_AnyFieldSet_IsNotEmpty()
        {
            new AddressInfo().Also(x => x.Ipv4Address = "192.0.2.1").IsEmpty.Should().BeFalse();
            new AddressInfo().Also(x => x.Ipv6Address = "2001:db8::1").IsEmpty.Should().BeFalse();
            new AddressInfo().Also(x => x.Hostname = "asset.example.com").IsEmpty.Should().BeFalse();
            new AddressInfo().Also(x => x.DNS = true).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void AssetClassLifecycle_DefaultState_IsEmpty()
        {
            new AssetClassLifecycle().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void AssetClassLifecycle_AnyFieldSet_IsNotEmpty()
        {
            new AssetClassLifecycle().Also(x => x.EndOfLife = DateTime.UtcNow).IsEmpty.Should().BeFalse();
            new AssetClassLifecycle().Also(x => x.EndOfService = DateTime.UtcNow).IsEmpty.Should().BeFalse();
            new AssetClassLifecycle().Also(x => x.NominalLifetime = TimeSpan.FromDays(365)).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void AssetCustody_DefaultState_IsEmpty()
        {
            new AssetCustody().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void AssetCustody_AnyFieldSet_IsNotEmpty()
        {
            new AssetCustody().Also(x => x.From = DateTime.UtcNow).IsEmpty.Should().BeFalse();
            new AssetCustody().Also(x => x.Till = DateTime.UtcNow).IsEmpty.Should().BeFalse();
            new AssetCustody().Also(x => x.ContactPerson = Guid.NewGuid()).IsEmpty.Should().BeFalse();
            new AssetCustody().Also(x => x.Team = Guid.NewGuid()).IsEmpty.Should().BeFalse();
            new AssetCustody().Also(x => x.Organization = Guid.NewGuid()).IsEmpty.Should().BeFalse();
            new AssetCustody().Also(x => x.ContactPersonRole = Guid.NewGuid()).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void AssetHolder_DefaultState_IsEmpty()
        {
            new AssetHolder().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void AssetHolder_AnyFieldSet_IsNotEmpty()
        {
            new AssetHolder().Also(x => x.SlotNumber = 1).IsEmpty.Should().BeFalse();
            new AssetHolder().Also(x => x.Label = "slot").IsEmpty.Should().BeFalse();
            new AssetHolder().Also(x => x.HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void AssetLocation_DefaultState_IsEmpty()
        {
            new AssetLocation().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void AssetLocation_AnyFieldSet_IsNotEmpty()
        {
            new AssetLocation().Also(x => x.ParentAsset = Ref<Asset>()).IsEmpty.Should().BeFalse();
            new AssetLocation().Also(x => x.HolderNumber = 1).IsEmpty.Should().BeFalse();
            new AssetLocation().Also(x => x.RackId = Ref<Rack>()).IsEmpty.Should().BeFalse();
            new AssetLocation().Also(x => x.RackPosition = 1).IsEmpty.Should().BeFalse();
            new AssetLocation().Also(x => x.Side = SlcAsset_Management.Enums.SideEnum.Back).IsEmpty.Should().BeFalse();
            new AssetLocation().Also(x => x.DeskId = Guid.NewGuid()).IsEmpty.Should().BeFalse();
            new AssetLocation().Also(x => x.ContainerId = Ref<Facility>()).IsEmpty.Should().BeFalse();
            new AssetLocation().Also(x => x.RoomId = Ref<Room>()).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void AssetOwnership_DefaultState_IsEmpty()
        {
            new AssetOwnership().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void AssetOwnership_AnyFieldSet_IsNotEmpty()
        {
            new AssetOwnership().Also(x => x.Organization = Guid.NewGuid()).IsEmpty.Should().BeFalse();
            new AssetOwnership().Also(x => x.ContactPerson = Guid.NewGuid()).IsEmpty.Should().BeFalse();
            new AssetOwnership().Also(x => x.ContactPersonRole = Guid.NewGuid()).IsEmpty.Should().BeFalse();
            new AssetOwnership().Also(x => x.Team = Guid.NewGuid()).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void CableRelation_DefaultState_IsEmpty()
        {
            new CableRelation().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void CableRelation_CableTypeFksSet_IsNotEmpty()
        {
            new CableRelation().Also(x => x.CableTypeFks = new List<SdmObjectReference<CableType>> { Ref<CableType>() }).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void CategoryRelation_DefaultState_IsEmpty()
        {
            new CategoryRelation().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void CategoryRelation_CategoriesSet_IsNotEmpty()
        {
            new CategoryRelation().Also(x => x.Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Data }).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void DataPortInfo_DefaultState_IsEmpty()
        {
            new DataPortInfo().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void DataPortInfo_AnyFieldSet_IsNotEmpty()
        {
            new DataPortInfo().Also(x => x.Name = "data").IsEmpty.Should().BeFalse();
            new DataPortInfo().Also(x => x.PortNumber = 1).IsEmpty.Should().BeFalse();
            new DataPortInfo().Also(x => x.OutputType = SlcAsset_Management.Enums.Outputtype.In).IsEmpty.Should().BeFalse();
            new DataPortInfo().Also(x => x.PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Front).IsEmpty.Should().BeFalse();
            new DataPortInfo().Also(x => x.PortType = Ref<PortType>()).IsEmpty.Should().BeFalse();
            new DataPortInfo().Also(x => x.Label = "label").IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void DestinationInfo_DefaultState_IsEmpty()
        {
            new DestinationInfo().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void DestinationInfo_AnyFieldSet_IsNotEmpty()
        {
            new DestinationInfo().Also(x => x.CableTag = "cable").IsEmpty.Should().BeFalse();
            new DestinationInfo().Also(x => x.Port = Guid.NewGuid()).IsEmpty.Should().BeFalse();
            new DestinationInfo().Also(x => x.PortType = Ref<PortType>()).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void ElementLink_DefaultState_IsEmpty()
        {
            new ElementLink().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void ElementLink_AnyFieldSet_IsNotEmpty()
        {
            new ElementLink().Also(x => x.ElementID = "1/2").IsEmpty.Should().BeFalse();
            new ElementLink().Also(x => x.IsPrimary = true).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void HierarchyInfo_DefaultState_IsEmpty()
        {
            new HierarchyInfo().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void HierarchyInfo_HierarchyRoleSet_IsNotEmpty()
        {
            new HierarchyInfo().Also(x => x.HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Card).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void InfraopsReservationBounderies_DefaultState_IsEmpty()
        {
            new InfraopsReservationBounderies().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void InfraopsReservationBounderies_AnyFieldSet_IsNotEmpty()
        {
            new InfraopsReservationBounderies().Also(x => x.LowerBound = 1).IsEmpty.Should().BeFalse();
            new InfraopsReservationBounderies().Also(x => x.UpperBound = 1).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void PowerPortInfo_DefaultState_IsEmpty()
        {
            new PowerPortInfo().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void PowerPortInfo_AnyFieldSet_IsNotEmpty()
        {
            new PowerPortInfo().Also(x => x.Name = "power").IsEmpty.Should().BeFalse();
            new PowerPortInfo().Also(x => x.PortNumber = 1).IsEmpty.Should().BeFalse();
            new PowerPortInfo().Also(x => x.OutputType = SlcAsset_Management.Enums.Outputtype.In).IsEmpty.Should().BeFalse();
            new PowerPortInfo().Also(x => x.PortExposure = SlcAsset_Management.Enums.PortExposureEnum.Front).IsEmpty.Should().BeFalse();
            new PowerPortInfo().Also(x => x.PortType = Ref<PortType>()).IsEmpty.Should().BeFalse();
            new PowerPortInfo().Also(x => x.Label = "label").IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void PrimaryPortRelation_DefaultState_IsEmpty()
        {
            new PrimaryPortRelation().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void PrimaryPortRelation_AnyFieldSet_IsNotEmpty()
        {
            new PrimaryPortRelation().Also(x => x.IsPrimaryIpv6 = true).IsEmpty.Should().BeFalse();
            new PrimaryPortRelation().Also(x => x.IsPrimaryIpv4 = true).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void RackRelation_DefaultState_IsEmpty()
        {
            new RackRelation().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void RackRelation_RackSet_IsNotEmpty()
        {
            new RackRelation().Also(x => x.Rack = Ref<Rack>()).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void SourceInfo_DefaultState_IsEmpty()
        {
            new SourceInfo().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void SourceInfo_AnyFieldSet_IsNotEmpty()
        {
            new SourceInfo().Also(x => x.CableTag = "cable").IsEmpty.Should().BeFalse();
            new SourceInfo().Also(x => x.Port = Guid.NewGuid()).IsEmpty.Should().BeFalse();
            new SourceInfo().Also(x => x.PortType = Ref<PortType>()).IsEmpty.Should().BeFalse();
        }

        [TestMethod]
        public void TagsInfo_DefaultState_IsEmpty()
        {
            new TagsInfo().IsEmpty.Should().BeTrue();
        }

        [TestMethod]
        public void TagsInfo_TagsSet_IsNotEmpty()
        {
            new TagsInfo().Also(x => x.Tags = new List<SlcAsset_Management.Enums.TagOption> { SlcAsset_Management.Enums.TagOption.AcceptsDataConnection }).IsEmpty.Should().BeFalse();
        }

        private static SdmObjectReference<T> Ref<T>() where T : SdmObject<T>
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
