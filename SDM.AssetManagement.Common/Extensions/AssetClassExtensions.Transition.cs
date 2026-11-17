namespace Skyline.DataMiner.SDM.AssetManagement.Common.Extensions
{
    using System;
    using SharedCommonLibrary.AssetManagement.State_Management;
    using SharedMappers.DomIds;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    public static partial class AssetClassExtensions
    {
        public static bool IsTransitionAllowed(this AssetClass assetClass, SlcAsset_Management.Behaviors.Asset_Class_Behavior.StatusesEnum newState)
        {
            if (assetClass == null)
            {
                throw new ArgumentNullException(nameof(assetClass));
            }
            return StateMachine.IsTransitionAllowed(assetClass.State, newState);
        }
    }
}