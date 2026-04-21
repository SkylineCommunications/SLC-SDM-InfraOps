namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;

    public class ValidationResult
    {
        private readonly Dictionary<string, string> _failReasons;
        private readonly Dictionary<string, string> _displayKey;

        private bool _isValid;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class.
        /// </summary>
        public ValidationResult()
        {
            _failReasons = new Dictionary<string, string>();
            _displayKey = new Dictionary<string, string>();
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
                throw new InvalidOperationException($"Fail Reason for field '{field}' already exists. Cannot add multiple reasons for the same field.");
            }

            _failReasons[field] = reason;
            _displayKey[field] = displayFieldName;
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
            /// Merges failures from another ValidationResult.
            /// </summary>
            public Builder AddFailuresFrom(ValidationResult otherResult)
            {
                _result.AddFailuresFrom(otherResult);
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