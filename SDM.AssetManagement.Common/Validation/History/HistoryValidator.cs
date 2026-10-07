namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Public validator service for History validation.
    /// No business rules are currently defined for this entity; kept as a placeholder so this
    /// module follows the same Validator/Middleware pairing as the other repositories and can be
    /// wired with <see cref="Skyline.DataMiner.SDM.AssetManagement.Common.Middleware.HistoryValidationMiddleware"/>.
    /// </summary>
    public class HistoryValidator : ValidatorBase<History>
    {
        protected override ValidationResult Validate(History history)
        {
            return ValidateInfo(history);
        }

        protected override List<ValidationResult> ValidateBulk(List<History> histories)
        {
            return histories == null
                ? new List<ValidationResult>()
                : histories.Select(ValidateInfo).ToList();
        }

        private static ValidationResult ValidateInfo(History history)
        {
            var result = new ValidationResult();

            if (!HistoryValidationHandler.IsValid(history, out var infoResult))
            {
                result.AddFailuresFrom(infoResult);
            }

            return result;
        }
    }
}