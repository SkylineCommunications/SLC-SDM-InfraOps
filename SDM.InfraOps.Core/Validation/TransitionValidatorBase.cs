namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Base class for validators that also validate status transitions.
    /// Status transitions bypass the middleware, so the repository enforces the rules itself once a validator is
    /// assigned to its <c>TransitionValidator</c> property.
    /// Override <see cref="ValidateTransitionRules(T, TState)"/> to add rules.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    /// <typeparam name="TState">The entity's status enum.</typeparam>
    public abstract class TransitionValidatorBase<T, TState> : ValidatorBase<T>
        where T : class
        where TState : struct, Enum
    {
        /// <summary>
        /// Validates a state transition (path and business rules) without executing it. Never throws.
        /// </summary>
        public ValidationResult ValidateTransition(T entity, TState newState)
        {
            if (entity == null)
            {
                return CreateNullEntityResult();
            }

            if (EqualityComparer<TState>.Default.Equals(GetState(entity), newState))
            {
                return new ValidationResult();
            }

            var result = ValidateTransitionPath(entity, newState);
            if (!result.IsValid)
            {
                return result;
            }

            result.AddFrom(ValidateTransitionRules(entity, newState));
            return result;
        }

        /// <summary>
        /// Transition guard invoked by the repository after its own path check. Runs the business rules only
        /// and throws when they fail.
        /// </summary>
        public void EnsureTransitionAllowed(T entity, TState newState)
        {
            var result = ValidateTransitionRules(entity, newState);
            if (!result.IsValid)
            {
                throw CreateTransitionException(result);
            }
        }

        /// <summary>
        /// Gets the current state of the entity.
        /// </summary>
        protected abstract TState GetState(T entity);

        /// <summary>
        /// Validates that a path exists from the current state to <paramref name="newState"/>.
        /// Only called when the state changes and the entity is not <c>null</c>.
        /// </summary>
        protected abstract ValidationResult ValidateTransitionPath(T entity, TState newState);

        /// <summary>
        /// Business rules for the transition. No rules by default.
        /// </summary>
        /// <remarks>
        /// Receives the entity exactly as the repository passes it, which may differ from the caller's in-memory
        /// copy (e.g. Asset's <c>TransitionTo</c> reverts pending edits first, so rules see the persisted values).
        /// </remarks>
        protected virtual ValidationResult ValidateTransitionRules(T entity, TState newState)
        {
            return new ValidationResult();
        }

        /// <summary>
        /// Creates the exception thrown by <see cref="EnsureTransitionAllowed(T, TState)"/>.
        /// Defaults to <see cref="ValidationException"/>.
        /// </summary>
        protected virtual Exception CreateTransitionException(ValidationResult result)
        {
            return result.ToException();
        }
    }
}
