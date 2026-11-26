namespace SDM.AssetManagement.Tests.Setup
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Transition validator whose transition rules always fail. Used to verify that repositories enforce
    /// attached <see cref="TransitionValidatorBase{T, TState}"/> rules.
    /// </summary>
    internal sealed class BlockingTransitionValidator<T, TState> : TransitionValidatorBase<T, TState>
        where T : class
        where TState : struct, Enum
    {
        internal const string Reason = "Blocked by transition rule.";

        private readonly Func<T, TState> _getState;

        internal BlockingTransitionValidator(Func<T, TState> getState)
        {
            _getState = getState;
        }

        protected override TState GetState(T entity) => _getState(entity);

        protected override ValidationResult ValidateTransitionPath(T entity, TState newState) => new ValidationResult();

        protected override ValidationResult ValidateTransitionRules(T entity, TState newState)
        {
            var result = new ValidationResult();
            result.AddFailReason("State", "State", Reason);
            return result;
        }

        protected override List<ValidationResult> ValidateBulkNoDatabaseBusinessRules(List<T> entities) => Valid(entities);

        protected override List<ValidationResult> ValidateBulkNoDatabaseBatchConflicts(List<T> entities) => Valid(entities);

        protected override List<ValidationResult> ValidateBulkWithDatabase(List<T> entities) => Valid(entities);

        private static List<ValidationResult> Valid(List<T> entities) => entities.Select(_ => new ValidationResult()).ToList();
    }
}
