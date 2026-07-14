namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;

    public class ValidationResult
    {
        private readonly Dictionary<string, string> _failReasons;
        private readonly Dictionary<string, string> _displayKey;
        private readonly Dictionary<string, string> _warnings;
        private readonly Dictionary<string, string> _warningsDisplayKey;

        private bool _isValid;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class.
        /// </summary>
        public ValidationResult()
        {
            _failReasons = new Dictionary<string, string>();
            _displayKey = new Dictionary<string, string>();
            _warnings = new Dictionary<string, string>(); 
            _warningsDisplayKey = new Dictionary<string, string>();
            _isValid = true;
        }

        public bool IsValid
        {
            get
            {
                return _isValid;
            }
        }

        public IReadOnlyDictionary<string, string> FailureReasons => _failReasons;

        /// <summary>
        /// Gets the collection of warnings (non-blocking validation notices).
        /// </summary>
        public IReadOnlyDictionary<string, string> Warnings => _warnings;

        /// <summary>
        /// Indicates whether the result has any warnings.
        /// </summary>
        public bool HasWarnings => _warnings.Any();

        public bool TryGetFailReason<T>(T field, out string reason) where T : Enum
        {
            return _failReasons.TryGetValue(GetFieldToId(field), out reason);
        }

        public string GetFailReason<T>(T field) where T : Enum
        {
            if (!TryGetFailReason(field, out string reason))
            {
                reason = string.Empty;
            }

            return reason;
        }

        public void AddFailReason<T>(T field, string reason) where T : Enum
        {
            AddFailReason(GetFieldToId(field), Convert.ToString(field), reason);
        }

        public void AddFailReason(string field, string displayFieldName, string reason)
        {
            _isValid = false;
            if (_failReasons.ContainsKey(field))
            {
                return; // ignore - field already has an error
            }

            _failReasons[field] = reason;
            _displayKey[field] = displayFieldName;
        }

        /// <summary>
        /// Adds a warning to the validation result.
        /// Warnings don't affect IsValid, but provide notices to the user.
        /// </summary>
        public void AddWarning<T>(T field, string reason) where T : Enum
        {
            AddWarning(GetFieldToId(field), Convert.ToString(field), reason);
        }

        /// <summary>
        /// Adds a warning to the validation result.
        /// </summary>
        public void AddWarning(string field, string displayFieldName, string reason)
        {
            if (_warnings.ContainsKey(field))
            {
                // For warnings, we can append instead of throwing
                _warnings[field] = _warnings[field] + "; " + reason;
            }
            else
            {
                _warnings[field] = reason;
                _warningsDisplayKey[field] = displayFieldName;
            }
        }

        /// <summary>
        /// Gets a warning message for a specific field.
        /// </summary>
        public bool TryGetWarning<T>(T field, out string warning) where T : Enum
        {
            return _warnings.TryGetValue(GetFieldToId(field), out warning);
        }

        /// <summary>
        /// Gets the combined warning messages.
        /// </summary>
        public string GetCombinedWarnings(string separator)
        {
            List<string> warnings = new List<string>();
            foreach (var entry in _warnings)
            {
                warnings.Add($"{_warningsDisplayKey[entry.Key]}: {entry.Value}");
            }

            return string.Join(separator, warnings);
        }

        /// <summary>
        /// Adds all failures from another ValidationResult into this instance.
        /// Skips duplicate fields to prevent exceptions.
        /// </summary>
        /// <param name="otherResult">The validation result to merge into this instance.</param>
        /// <returns>This instance for fluent chaining.</returns>
        public ValidationResult AddFailuresFrom(ValidationResult otherResult)
        {
            if (otherResult == null)
            {
                return this;
            }

            foreach (var entry in otherResult.FailureReasons)
            {
                if (!_failReasons.ContainsKey(entry.Key))
                {
                    AddFailReason(entry.Key, otherResult._displayKey[entry.Key], entry.Value);
                }
            }

            return this;
        }

        /// <summary>
        /// Adds all warnings from another ValidationResult into this instance.
        /// </summary>
        public ValidationResult AddWarningsFrom(ValidationResult otherResult)
        {
            if (otherResult == null)
            {
                return this;
            }

            foreach (var entry in otherResult.Warnings)
            {
                if (!_warnings.ContainsKey(entry.Key))
                {
                    AddWarning(entry.Key, otherResult._warningsDisplayKey[entry.Key], entry.Value);
                }
            }

            return this;
        }

        /// <summary>
        /// Adds all failures and warnings from another ValidationResult into this instance.
        /// Convenience method for merging both failures and warnings in one call.
        /// </summary>
        /// <param name="otherResult">The validation result to merge.</param>
        /// <returns>This instance for fluent chaining.</returns>
        public ValidationResult AddFrom(ValidationResult otherResult)
        {
            AddFailuresFrom(otherResult);
            AddWarningsFrom(otherResult);
            return this;
        }

        /// <summary>
        /// Adds multiple failures from an enumerable collection.
        /// </summary>
        public ValidationResult AddRange<T>(IEnumerable<KeyValuePair<T, string>> failures) where T : Enum
        {
            if (failures == null)
            {
                return this;
            }

            foreach (var failure in failures)
            {
                var fieldId = GetFieldToId(failure.Key);
                if (!_failReasons.ContainsKey(fieldId))
                {
                    AddFailReason(failure.Key, failure.Value);
                }
            }

            return this;
        }

        /// <summary>
        /// Combines multiple ValidationResults into a single result.
        /// </summary>
        public static ValidationResult Combine(params ValidationResult[] results)
        {
            var combined = new ValidationResult();

            foreach (var result in results)
            {
                if (result != null)
                {
                    combined.AddFailuresFrom(result);
                    combined.AddWarningsFrom(result);
                }
            }

            return combined;
        }

        /// <summary>
        /// Converts validation failures to a ValidationException.
        /// </summary>
        public Exception ToException()
        {
            var exceptionMessage = GetCombinedFailureReasons(Environment.NewLine);
            return new ValidationException(exceptionMessage);
        }

        public string GetCombinedFailureReasons(string separator)
        {
            List<string> reasons = new List<string>();
            foreach (var entry in _failReasons)
            {
                reasons.Add($"{_displayKey[entry.Key]}: {entry.Value}");
            }

            return string.Join(separator, reasons);
        }

        private string GetFieldToId<T>(T field) where T : Enum
        {
            return $"{typeof(T).Name}_{Convert.ToString(field)}";
        }

        /// <summary>
        /// Builder for constructing ValidationResult with fluent API.
        /// </summary>
        public class Builder
        {
            private readonly ValidationResult _result;

            public Builder()
            {
                _result = new ValidationResult();
            }

            /// <summary>
            /// Adds a failure reason for a specific field.
            /// </summary>
            public Builder AddFailReason<T>(T field, string reason) where T : Enum
            {
                if (!_result._failReasons.ContainsKey(_result.GetFieldToId(field)))
                {
                    _result.AddFailReason(field, reason);
                }
                return this;
            }

            /// <summary>
            /// Adds a warning for a specific field.
            /// </summary>
            public Builder AddWarning<T>(T field, string reason) where T : Enum
            {
                _result.AddWarning(field, reason);
                return this;
            }

            /// <summary>
            /// Merges failures from another ValidationResult.
            /// </summary>
            public Builder AddFailuresFrom(ValidationResult otherResult)
            {
                _result.AddFailuresFrom(otherResult);
                return this;
            }

            /// <summary>
            /// Merges warnings from another ValidationResult.
            /// </summary>
            public Builder AddWarningsFrom(ValidationResult otherResult)
            {
                _result.AddWarningsFrom(otherResult);
                return this;
            }

            /// <summary>
            /// Adds multiple failures at once.
            /// </summary>
            public Builder AddRange<T>(IEnumerable<KeyValuePair<T, string>> failures) where T : Enum
            {
                _result.AddRange(failures);
                return this;
            }

            /// <summary>
            /// Builds the final ValidationResult.
            /// </summary>
            public ValidationResult Build()
            {
                return _result;
            }
        }
    }
}