namespace Skyline.DataMiner.SDM.AssetManagement.Common.Validation
{
    using SharedCommonLibrary.AssetManagement.State_Management;
    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Validates all state-entry requirements for an asset transition path.
    /// </summary>
    public static class AssetTransitionValidator
    {
        /// <summary>
        /// Validates the asset against every state entered on the path to the target state.
        /// </summary>
        public static ValidationResult ValidatePath(
            Asset asset,
            SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum targetState)
        {
            var result = new ValidationResult();

            if (asset == null)
            {
                result.AddFailReason(AssetValidationHandler.AssetValidationField.Asset, "Asset cannot be null.");
                return result;
            }

            var transitions = StateMachine.GetTransitionPath(asset.State, targetState);
            if (transitions.Count == 0)
            {
                return result;
            }

            foreach (var enteredState in StateMachine.GetTransitionDestinationStates(asset.State, targetState))
            {
                result.AddFrom(AssetValidationHandler.ValidateDestinationLocation(asset, enteredState));

                if (enteredState == SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.Installed)
                {
                    AssetValidationHandler.IsReadyForInstall(asset, out var installResult);
                    result.AddFrom(installResult);
                }
            }

            return result;
        }
    }
}
