namespace SDM.FacilityManagement.Tests.State_Management
{
    using System;
    using System.Runtime.Serialization;

    using FluentAssertions;

    using SharedCommonLibrary.FacilityManagement.State_Management;
    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    [TestClass]
    public class FacilityStateMachineTests
    {
        [TestMethod]
        public void FacilityStateMachine_DraftToActive_IsAllowed()
        {
            FacilityStateMachine.IsTransitionAllowed(SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum.Draft, SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum.Active).Should().BeTrue();
            FacilityStateMachine.GetTransitionPath(SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum.Draft, SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum.Active).Should().ContainSingle().Which.Should().Be(SlcFacility_Management.Behaviors.Facility_Behaviour.TransitionsEnum.Draft_Active);
        }

        [TestMethod]
        public void FacilityStateMachine_ActiveToDeprecated_IsAllowed()
        {
            FacilityStateMachine.IsTransitionAllowed(SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum.Active, SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum.Deprecated).Should().BeTrue();
            FacilityStateMachine.GetTransitionPath(SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum.Active, SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum.Deprecated).Should().ContainSingle().Which.Should().Be(SlcFacility_Management.Behaviors.Facility_Behaviour.TransitionsEnum.Active_Deprecated);
        }

        [TestMethod]
        public void FacilityDomRepository_DeprecatedToActive_ThrowsNotAllowedException()
        {
            var repository = (FacilityDomRepository)FormatterServices.GetUninitializedObject(typeof(FacilityDomRepository));
            var item = new Facility
            {
                Identifier = Guid.NewGuid().ToString(),
                State = SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum.Deprecated,
            };

            Action transition = () => repository.TransitionTo(item, SlcFacility_Management.Behaviors.Facility_Behaviour.StatusesEnum.Active);

            transition.Should().Throw<InvalidOperationException>().WithMessage("State transition from Deprecated to Active is not allowed.");
        }
    }

    [TestClass]
    public class FloorStateMachineTests
    {
        [TestMethod]
        public void FloorStateMachine_DraftToActive_IsAllowed()
        {
            FloorStateMachine.IsTransitionAllowed(SlcFacility_Management.Behaviors.Floor_Behaviour.StatusesEnum.Draft, SlcFacility_Management.Behaviors.Floor_Behaviour.StatusesEnum.Active).Should().BeTrue();
            FloorStateMachine.GetTransitionPath(SlcFacility_Management.Behaviors.Floor_Behaviour.StatusesEnum.Draft, SlcFacility_Management.Behaviors.Floor_Behaviour.StatusesEnum.Active).Should().ContainSingle().Which.Should().Be(SlcFacility_Management.Behaviors.Floor_Behaviour.TransitionsEnum.Draft_Active);
        }

        [TestMethod]
        public void FloorStateMachine_ActiveToDeprecated_IsAllowed()
        {
            FloorStateMachine.IsTransitionAllowed(SlcFacility_Management.Behaviors.Floor_Behaviour.StatusesEnum.Active, SlcFacility_Management.Behaviors.Floor_Behaviour.StatusesEnum.Deprecated).Should().BeTrue();
            FloorStateMachine.GetTransitionPath(SlcFacility_Management.Behaviors.Floor_Behaviour.StatusesEnum.Active, SlcFacility_Management.Behaviors.Floor_Behaviour.StatusesEnum.Deprecated).Should().ContainSingle().Which.Should().Be(SlcFacility_Management.Behaviors.Floor_Behaviour.TransitionsEnum.Active_Deprecated);
        }

        [TestMethod]
        public void FloorDomRepository_DeprecatedToActive_ThrowsNotAllowedException()
        {
            var repository = (FloorDomRepository)FormatterServices.GetUninitializedObject(typeof(FloorDomRepository));
            var item = new Floor
            {
                Identifier = Guid.NewGuid().ToString(),
                State = SlcFacility_Management.Behaviors.Floor_Behaviour.StatusesEnum.Deprecated,
            };

            Action transition = () => repository.TransitionTo(item, SlcFacility_Management.Behaviors.Floor_Behaviour.StatusesEnum.Active);

            transition.Should().Throw<InvalidOperationException>().WithMessage("State transition from Deprecated to Active is not allowed.");
        }
    }

    [TestClass]
    public class RoomStateMachineTests
    {
        [TestMethod]
        public void RoomStateMachine_DraftToActive_IsAllowed()
        {
            RoomStateMachine.IsTransitionAllowed(SlcFacility_Management.Behaviors.Room_Behaviour.StatusesEnum.Draft, SlcFacility_Management.Behaviors.Room_Behaviour.StatusesEnum.Active).Should().BeTrue();
            RoomStateMachine.GetTransitionPath(SlcFacility_Management.Behaviors.Room_Behaviour.StatusesEnum.Draft, SlcFacility_Management.Behaviors.Room_Behaviour.StatusesEnum.Active).Should().ContainSingle().Which.Should().Be(SlcFacility_Management.Behaviors.Room_Behaviour.TransitionsEnum.Draft_Active);
        }

        [TestMethod]
        public void RoomStateMachine_ActiveToDeprecated_IsAllowed()
        {
            RoomStateMachine.IsTransitionAllowed(SlcFacility_Management.Behaviors.Room_Behaviour.StatusesEnum.Active, SlcFacility_Management.Behaviors.Room_Behaviour.StatusesEnum.Deprecated).Should().BeTrue();
            RoomStateMachine.GetTransitionPath(SlcFacility_Management.Behaviors.Room_Behaviour.StatusesEnum.Active, SlcFacility_Management.Behaviors.Room_Behaviour.StatusesEnum.Deprecated).Should().ContainSingle().Which.Should().Be(SlcFacility_Management.Behaviors.Room_Behaviour.TransitionsEnum.Active_Deprecated);
        }

        [TestMethod]
        public void RoomDomRepository_DeprecatedToActive_ThrowsNotAllowedException()
        {
            var repository = (RoomDomRepository)FormatterServices.GetUninitializedObject(typeof(RoomDomRepository));
            var item = new Room
            {
                Identifier = Guid.NewGuid().ToString(),
                State = SlcFacility_Management.Behaviors.Room_Behaviour.StatusesEnum.Deprecated,
            };

            Action transition = () => repository.TransitionTo(item, SlcFacility_Management.Behaviors.Room_Behaviour.StatusesEnum.Active);

            transition.Should().Throw<InvalidOperationException>().WithMessage("State transition from Deprecated to Active is not allowed.");
        }
    }

    [TestClass]
    public class RowStateMachineTests
    {
        [TestMethod]
        public void RowStateMachine_DraftToActive_IsAllowed()
        {
            RowStateMachine.IsTransitionAllowed(SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum.Draft, SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum.Active).Should().BeTrue();
            RowStateMachine.GetTransitionPath(SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum.Draft, SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum.Active).Should().ContainSingle().Which.Should().Be(SlcFacility_Management.Behaviors.Row_Behaviour.TransitionsEnum.Draft_Active);
        }

        [TestMethod]
        public void RowStateMachine_ActiveToDeprecated_IsAllowed()
        {
            RowStateMachine.IsTransitionAllowed(SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum.Active, SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum.Deprecated).Should().BeTrue();
            RowStateMachine.GetTransitionPath(SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum.Active, SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum.Deprecated).Should().ContainSingle().Which.Should().Be(SlcFacility_Management.Behaviors.Row_Behaviour.TransitionsEnum.Active_Deprecated);
        }

        [TestMethod]
        public void RowDomRepository_DeprecatedToActive_ThrowsNotAllowedException()
        {
            var repository = (RowDomRepository)FormatterServices.GetUninitializedObject(typeof(RowDomRepository));
            var item = new Row
            {
                Identifier = Guid.NewGuid().ToString(),
                State = SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum.Deprecated,
            };

            Action transition = () => repository.TransitionTo(item, SlcFacility_Management.Behaviors.Row_Behaviour.StatusesEnum.Active);

            transition.Should().Throw<InvalidOperationException>().WithMessage("State transition from Deprecated to Active is not allowed.");
        }
    }

    [TestClass]
    public class RackStateMachineTests
    {
        [TestMethod]
        public void RackStateMachine_DraftToActive_IsAllowed()
        {
            RackStateMachine.IsTransitionAllowed(SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum.Draft, SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum.Active).Should().BeTrue();
            RackStateMachine.GetTransitionPath(SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum.Draft, SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum.Active).Should().ContainSingle().Which.Should().Be(SlcFacility_Management.Behaviors.Rack_Behaviour.TransitionsEnum.Draft_Active);
        }

        [TestMethod]
        public void RackStateMachine_ActiveToDeprecated_IsAllowed()
        {
            RackStateMachine.IsTransitionAllowed(SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum.Active, SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum.Deprecated).Should().BeTrue();
            RackStateMachine.GetTransitionPath(SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum.Active, SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum.Deprecated).Should().ContainSingle().Which.Should().Be(SlcFacility_Management.Behaviors.Rack_Behaviour.TransitionsEnum.Active_Deprecated);
        }

        [TestMethod]
        public void RackDomRepository_DeprecatedToActive_ThrowsNotAllowedException()
        {
            var repository = (RackDomRepository)FormatterServices.GetUninitializedObject(typeof(RackDomRepository));
            var item = new Rack
            {
                Identifier = Guid.NewGuid().ToString(),
                State = SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum.Deprecated,
            };

            Action transition = () => repository.TransitionTo(item, SlcFacility_Management.Behaviors.Rack_Behaviour.StatusesEnum.Active);

            transition.Should().Throw<InvalidOperationException>().WithMessage("State transition from Deprecated to Active is not allowed.");
        }
    }

    [TestClass]
    public class ZoneStateMachineTests
    {
        [TestMethod]
        public void ZoneStateMachine_DraftToActive_IsAllowed()
        {
            ZoneStateMachine.IsTransitionAllowed(SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum.Draft, SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum.Active).Should().BeTrue();
            ZoneStateMachine.GetTransitionPath(SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum.Draft, SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum.Active).Should().ContainSingle().Which.Should().Be(SlcFacility_Management.Behaviors.Zone_Behaviour.TransitionsEnum.Draft_Active);
        }

        [TestMethod]
        public void ZoneStateMachine_ActiveToDeprecated_IsAllowed()
        {
            ZoneStateMachine.IsTransitionAllowed(SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum.Active, SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum.Deprecated).Should().BeTrue();
            ZoneStateMachine.GetTransitionPath(SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum.Active, SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum.Deprecated).Should().ContainSingle().Which.Should().Be(SlcFacility_Management.Behaviors.Zone_Behaviour.TransitionsEnum.Active_Deprecated);
        }

        [TestMethod]
        public void ZoneDomRepository_DeprecatedToActive_ThrowsNotAllowedException()
        {
            var repository = (ZoneDomRepository)FormatterServices.GetUninitializedObject(typeof(ZoneDomRepository));
            var item = new Zone
            {
                Identifier = Guid.NewGuid().ToString(),
                State = SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum.Deprecated,
            };

            Action transition = () => repository.TransitionTo(item, SlcFacility_Management.Behaviors.Zone_Behaviour.StatusesEnum.Active);

            transition.Should().Throw<InvalidOperationException>().WithMessage("State transition from Deprecated to Active is not allowed.");
        }
    }

    [TestClass]
    public class SiteStateMachineTests
    {
        [TestMethod]
        public void SiteStateMachine_DraftToActive_IsAllowed()
        {
            SiteStateMachine.IsTransitionAllowed(SlcFacility_Management.Behaviors.Site_Behaviour.StatusesEnum.Draft, SlcFacility_Management.Behaviors.Site_Behaviour.StatusesEnum.Active).Should().BeTrue();
            SiteStateMachine.GetTransitionPath(SlcFacility_Management.Behaviors.Site_Behaviour.StatusesEnum.Draft, SlcFacility_Management.Behaviors.Site_Behaviour.StatusesEnum.Active).Should().ContainSingle().Which.Should().Be(SlcFacility_Management.Behaviors.Site_Behaviour.TransitionsEnum.Draft_Active);
        }

        [TestMethod]
        public void SiteStateMachine_ActiveToDeprecated_IsAllowed()
        {
            SiteStateMachine.IsTransitionAllowed(SlcFacility_Management.Behaviors.Site_Behaviour.StatusesEnum.Active, SlcFacility_Management.Behaviors.Site_Behaviour.StatusesEnum.Deprecated).Should().BeTrue();
            SiteStateMachine.GetTransitionPath(SlcFacility_Management.Behaviors.Site_Behaviour.StatusesEnum.Active, SlcFacility_Management.Behaviors.Site_Behaviour.StatusesEnum.Deprecated).Should().ContainSingle().Which.Should().Be(SlcFacility_Management.Behaviors.Site_Behaviour.TransitionsEnum.Active_Deprecated);
        }

        [TestMethod]
        public void SiteDomRepository_DeprecatedToActive_ThrowsNotAllowedException()
        {
            var repository = (SiteDomRepository)FormatterServices.GetUninitializedObject(typeof(SiteDomRepository));
            var item = new Site
            {
                Identifier = Guid.NewGuid().ToString(),
                State = SlcFacility_Management.Behaviors.Site_Behaviour.StatusesEnum.Deprecated,
            };

            Action transition = () => repository.TransitionTo(item, SlcFacility_Management.Behaviors.Site_Behaviour.StatusesEnum.Active);

            transition.Should().Throw<InvalidOperationException>().WithMessage("State transition from Deprecated to Active is not allowed.");
        }
    }

}