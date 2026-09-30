namespace Skyline.DataMiner.SDM.AssetManagement.Common.Extensions
{
    using System;
    using System.Collections.Generic;
    using SharedCommonLibrary.AssetManagement.State_Management;
    using SharedMappers.DomIds;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Exceptions;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    public static partial class AssetExtensions
    {
        public static bool IsTransitionAllowed(this Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }
            return StateMachine.IsTransitionAllowed(asset.State, newState);
        }

        public static List<SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum> GetTransitionPathStates(this Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum newState)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            if (!StateMachine.IsTransitionAllowed(asset.State, newState))
            {
                throw new InvalidOperationException($"State transition from {asset.State} to {newState} is not allowed.");
            }

            return StateMachine.GetTransitionDestinationStates(asset.State, newState);
        }


        public static void ValidateTransitionPath(this Asset asset, SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum toState)
        {
            var validationResult = AssetTransitionValidator.ValidatePath(asset, toState);
            if (!validationResult.IsValid)
            {
                throw new AssetTransitionValidationException(validationResult);
            }
        }
    }
}