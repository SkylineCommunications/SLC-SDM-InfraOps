namespace SDM.AssetManagement.Tests.Setup
{
    using System.Collections.Generic;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Status;

    internal static class AssetClassBehaviorFixture
    {
        internal const string ModuleId = "(slc)asset_management";

        internal static DomDefinition BuildAssetClassDefinition()
        {
            return new DomDefinition("AssetClass")
            {
                ID = SlcAsset_Management.Definitions.AssetClass,
                DomBehaviorDefinitionId = SlcAsset_Management.Behaviors.Asset_Class_Behavior.Id,
            };
        }

        internal static DomBehaviorDefinition BuildAssetClassBehaviorDefinition()
        {
            return new DomBehaviorDefinition("Asset_Class_Behavior")
            {
                ID = SlcAsset_Management.Behaviors.Asset_Class_Behavior.Id,
                InitialStatusId = SlcAsset_Management.Behaviors.Asset_Class_Behavior.Statuses.Draft,
                Statuses = new List<DomStatus>
                {
                    new DomStatus(SlcAsset_Management.Behaviors.Asset_Class_Behavior.Statuses.Draft, "Draft"),
                    new DomStatus(SlcAsset_Management.Behaviors.Asset_Class_Behavior.Statuses.Active, "Active"),
                    new DomStatus(SlcAsset_Management.Behaviors.Asset_Class_Behavior.Statuses.Deprecated, "Deprecated"),
                },
                StatusTransitions = new List<DomStatusTransition>
                {
                    new DomStatusTransition(
                        SlcAsset_Management.Behaviors.Asset_Class_Behavior.Transitions.Draft_Active,
                        SlcAsset_Management.Behaviors.Asset_Class_Behavior.Statuses.Draft,
                        SlcAsset_Management.Behaviors.Asset_Class_Behavior.Statuses.Active),
                    new DomStatusTransition(
                        SlcAsset_Management.Behaviors.Asset_Class_Behavior.Transitions.Active_Deprecated,
                        SlcAsset_Management.Behaviors.Asset_Class_Behavior.Statuses.Active,
                        SlcAsset_Management.Behaviors.Asset_Class_Behavior.Statuses.Deprecated),
                },
            };
        }
    }
}