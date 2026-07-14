namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations
{
    using System;

    /// <summary>
    /// Composable validator with error handling capabilities.
    /// </summary>
    public class Validator<T>
    {
        private readonly Func<T, ValidationResult> _validationFunc;
        private readonly bool _stopOnFailure;

        private Validator(Func<T, ValidationResult> validationFunc, bool stopOnFailure = false)
        {
            _validationFunc = validationFunc ?? throw new ArgumentNullException(nameof(validationFunc));
            _stopOnFailure = stopOnFailure;
        }

        /// <summary>
        /// Creates a validator from a validation function.
        /// </summary>
        public static Validator<T> Create(Func<T, ValidationResult> validationFunc)
        {
            return new Validator<T>(validationFunc);
        }

        /// <summary>
        /// Creates a validator that always succeeds.
        /// </summary>
        public static Validator<T> Success()
        {
            return new Validator<T>(_ => new ValidationResult());
        }

        /// <summary>
        /// Chains another validator to execute after this one.
        /// If this validator fails and StopOnFailure was set, next validator is skipped.
        /// </summary>
        public Validator<T> AndThen(Validator<T> next)
        {
            if (next == null)
            {
                throw new ArgumentNullException(nameof(next));
            }

            return new Validator<T>(obj =>
            {
                var result1 = _validationFunc(obj);

                // Stop if this validator marked as critical and failed
                if (_stopOnFailure && !result1.IsValid)
                {
                    return result1;
                }

                var result2 = next._validationFunc(obj);
                return result1.AddFailuresFrom(result2);
            });
        }

        /// <summary>
        /// Chains another validation function to execute after this one.
        /// </summary>
        public Validator<T> AndThen(Func<T, ValidationResult> validationFunc)
        {
            return AndThen(Create(validationFunc));
        }

        /// <summary>
        /// Marks this validator as critical - stops pipeline on failure.
        /// </summary>
        public Validator<T> StopOnFailure()
        {
            return new Validator<T>(_validationFunc, stopOnFailure: true);
        }

        /// <summary>
        /// Executes the validation pipeline.
        /// </summary>
        public ValidationResult Validate(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return _validationFunc(obj);
        }

        /// <summary>
        /// Executes validation and throws exception if invalid.
        /// </summary>
        public void ValidateAndThrow(T obj)
        {
            var result = Validate(obj);
            if (!result.IsValid)
            {
                throw result.ToException();
            }
        }

        /// <summary>
        /// Executes validation with custom error handler.
        /// </summary>
        public ValidationResult ValidateWithHandler(T obj, Action<ValidationResult> onError)
        {
            var result = Validate(obj);
            if (!result.IsValid && onError != null)
            {
                onError(result);
            }
            return result;
        }

        /// <summary>
        /// Conditionally executes this validator based on a predicate.
        /// </summary>
        public Validator<T> When(Func<T, bool> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return new Validator<T>(obj =>
            {
                if (predicate(obj))
                {
                    return _validationFunc(obj);
                }
                return new ValidationResult();
            });
        }

        /// <summary>
        /// Executes validator with try-catch and returns validation result with exception details.
        /// </summary>
        public Validator<T> WithExceptionHandling<TField>(TField errorField, string errorMessage = null) where TField : Enum
        {
            return new Validator<T>(obj =>
            {
                try
                {
                    return _validationFunc(obj);
                }
                catch (Exception ex)
                {
                    var result = new ValidationResult();
                    var message = errorMessage ?? $"Validation error: {ex.Message}";
                    result.AddFailReason(errorField, message);
                    return result;
                }
            });
        }

        /// <summary>
        /// Combines multiple validators with OR logic - succeeds if ANY validator passes.
        /// </summary>
        public static Validator<T> Any(params Validator<T>[] validators)
        {
            if (validators == null || validators.Length == 0)
            {
                return Success();
            }

            return new Validator<T>(obj =>
            {
                var combinedResult = new ValidationResult();
                foreach (var validator in validators)
                {
                    var result = validator.Validate(obj);
                    if (result.IsValid)
                    {
                        return new ValidationResult(); // Short-circuit on first success
                    }
                    combinedResult.AddFailuresFrom(result);
                }
                return combinedResult;
            });
        }

        /// <summary>
        /// Combines multiple validators with AND logic - all must pass.
        /// </summary>
        public static Validator<T> All(params Validator<T>[] validators)
        {
            if (validators == null || validators.Length == 0)
            {
                return Success();
            }

            return new Validator<T>(obj =>
            {
                var combinedResult = new ValidationResult();
                foreach (var validator in validators)
                {
                    var result = validator.Validate(obj);
                    combinedResult.AddFailuresFrom(result);
                }
                return combinedResult;
            });
        }
    }
}