using System.Collections.Generic;
using SharedMappers.DomIds;
using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Status;
namespace SDM.AssetManagement.Tests.Setup
{
    internal static class AssetBehaviorFixture
    {
        internal const string ModuleId = "(slc)asset_management";
        internal static DomDefinition BuildAssetDefinition() => new DomDefinition("Asset") { ID = SlcAsset_Management.Definitions.Asset, DomBehaviorDefinitionId = SlcAsset_Management.Behaviors.Asset_Behavior.Id };
        internal static DomBehaviorDefinition BuildAssetBehaviorDefinition()
        {
            return new DomBehaviorDefinition("Asset_Behavior")
            {
                ID = SlcAsset_Management.Behaviors.Asset_Behavior.Id,
                InitialStatusId = SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.NotAvailable,
                Statuses = new List<DomStatus>
                {
                    new DomStatus(SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.NotAvailable, "Not Available"),
                    new DomStatus(SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Available, "Available"),
                    new DomStatus(SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.BuildPlanReady, "Build Plan Ready"),
                    new DomStatus(SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Installed, "Installed"),
                    new DomStatus(SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InService, "In Service"),
                    new DomStatus(SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Disposed, "Disposed"),
                    new DomStatus(SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning, "In Planning"),
                    new DomStatus(SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit, "In Transit"),
                    new DomStatus(SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair, "In Repair"),
                },
                StatusTransitions = new List<DomStatusTransition>
                {
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Notavailable_To_Available, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.NotAvailable, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Available),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Notavailable_To_Disposed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.NotAvailable, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Disposed),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Available_To_Notavailable, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Available, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.NotAvailable),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Buildplanready_To_Installed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.BuildPlanReady, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Installed),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Installed_To_Inservice, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Installed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InService),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inservice_To_Notavailable, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InService, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.NotAvailable),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inservice_To_Buildplanready, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InService, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.BuildPlanReady),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inservice_To_Available, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InService, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Available),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inservice_To_Installed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InService, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Installed),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inplanning_To_Available, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Available),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inplanning_To_Buildplanready, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.BuildPlanReady),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Buildplanready_To_Inplanning, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.BuildPlanReady, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inservice_To_Inplanning, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InService, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Available_To_Inplanning, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Available, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Installed_To_Inplanning, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Installed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Notavailable_To_Intransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.NotAvailable, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Available_To_Intransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Available, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Buildplanready_To_Intransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.BuildPlanReady, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Installed_To_Intransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Installed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inservice_To_Intransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InService, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inplanning_To_Intransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inrepair_To_Intransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Notavailable_To_Inrepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.NotAvailable, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Available_To_Inrepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Available, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Buildplanready_To_Inrepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.BuildPlanReady, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Installed_To_Inrepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Installed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inservice_To_Inrepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InService, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inplanning_To_Inrepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Intransit_To_Inrepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Intransit_To_Notavailable, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.NotAvailable),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Intransit_To_Available, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Available),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Intransit_To_Buildplanready, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.BuildPlanReady),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Intransit_To_Installed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Installed),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Intransit_To_Disposed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Disposed),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Intransit_To_Inplanning, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inrepair_To_Notavailable, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.NotAvailable),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inrepair_To_Available, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Available),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inrepair_To_Buildplanready, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.BuildPlanReady),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inrepair_To_Installed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Installed),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inrepair_To_Disposed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Disposed),
                    new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inrepair_To_Inplanning, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning),
                },
            };
        }
    }
}
