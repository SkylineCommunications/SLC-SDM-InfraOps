namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations
{
    using System;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides generic validation methods for numeric values.
    /// </summary>
    public static class NumericValidators
    {
        /// <summary>
        /// Generic numeric validator that checks if a value satisfies a predicate.
        /// </summary>
        /// <typeparam name="TValue">The numeric type being validated.</typeparam>
        /// <typeparam name="TField">The enum type representing validation fields.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="predicate">The validation predicate.</param>
        /// <param name="field">The field identifier for error reporting.</param>
        /// <param name="errorMessage">The error message if validation fails.</param>
        /// <param name="result">The validation result output.</param>
        /// <returns>True if validation passes, false otherwise.</returns>
        public static bool ValidateNumericField<TValue, TField>(
            TValue value,
            Func<TValue, bool> predicate,
            TField field,
            string errorMessage,
            out ValidationResult result)
            where TValue : struct, IComparable<TValue>
            where TField : Enum
        {
            result = new ValidationResult();
            if (!predicate(value))
            {
                result.AddFailReason(field, errorMessage);
            }
            return result.IsValid;
        }


        //todo when nullables are supported, we can add overloads for nullable numeric types (e.g., int?, double?) that first check for null and then apply the same validation logic to the underlying value.
        /// <summary>
        /// Validates that a numeric value is not negative.
        /// </summary>
        /// <typeparam name="TValue">The numeric type being validated.</typeparam>
        /// <typeparam name="TField">The enum type representing validation fields.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="field">The field identifier for error reporting.</param>
        /// <param name="result">The validation result output.</param>
        /// <returns>True if validation passes, false otherwise.</returns>
        public static bool ValidateNonNegative<TValue, TField>(
            TValue value,
            TField field,
            out ValidationResult result)
            where TValue : struct, IComparable<TValue>
            where TField : Enum
        {
            var fieldName = FormatFieldName(field);
            return ValidateNumericField(
                value,
                v => v.CompareTo(default(TValue)) >= 0,
                field,
                $"The {fieldName} cannot be negative.",
                out result);
        }

        /// <summary>
        /// Validates that a numeric value is positive (greater than zero).
        /// </summary>
        /// <typeparam name="TValue">The numeric type being validated.</typeparam>
        /// <typeparam name="TField">The enum type representing validation fields.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="field">The field identifier for error reporting.</param>
        /// <param name="result">The validation result output.</param>
        /// <returns>True if validation passes, false otherwise.</returns>
        public static bool ValidatePositive<TValue, TField>(
            TValue value,
            TField field,
            out ValidationResult result)
            where TValue : struct, IComparable<TValue>
            where TField : Enum
        {
            var fieldName = FormatFieldName(field);
            return ValidateNumericField(
                value,
                v => v.CompareTo(default(TValue)) > 0,
                field,
                $"The {fieldName} must be greater than zero.",
                out result);
        }

        /// <summary>
        /// Validates that a numeric value falls within a specified range.
        /// </summary>
        /// <typeparam name="TValue">The numeric type being validated.</typeparam>
        /// <typeparam name="TField">The enum type representing validation fields.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="min">The minimum allowed value (inclusive).</param>
        /// <param name="max">The maximum allowed value (inclusive).</param>
        /// <param name="field">The field identifier for error reporting.</param>
        /// <param name="result">The validation result output.</param>
        /// <returns>True if validation passes, false otherwise.</returns>
        public static bool ValidateRange<TValue, TField>(
            TValue value,
            TValue min,
            TValue max,
            TField field,
            out ValidationResult result)
            where TValue : struct, IComparable<TValue>
            where TField : Enum
        {
            var fieldName = FormatFieldName(field);
            return ValidateNumericField(
                value,
                v => v.CompareTo(min) >= 0 && v.CompareTo(max) <= 0,
                field,
                $"The {fieldName} must be between {min} and {max}.",
                out result);
        }

        /// <summary>
        /// Validates that a numeric value is greater than a specified minimum.
        /// </summary>
        /// <typeparam name="TValue">The numeric type being validated.</typeparam>
        /// <typeparam name="TField">The enum type representing validation fields.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="min">The minimum allowed value (exclusive).</param>
        /// <param name="field">The field identifier for error reporting.</param>
        /// <param name="result">The validation result output.</param>
        /// <returns>True if validation passes, false otherwise.</returns>
        public static bool ValidateGreaterThan<TValue, TField>(
            TValue value,
            TValue min,
            TField field,
            out ValidationResult result)
            where TValue : struct, IComparable<TValue>
            where TField : Enum
        {
            var fieldName = FormatFieldName(field);
            return ValidateNumericField(
                value,
                v => v.CompareTo(min) > 0,
                field,
                $"The {fieldName} must be greater than {min}.",
                out result);
        }

        /// <summary>
        /// Validates that a numeric value is less than a specified maximum.
        /// </summary>
        /// <typeparam name="TValue">The numeric type being validated.</typeparam>
        /// <typeparam name="TField">The enum type representing validation fields.</typeparam>
        /// <param name="value">The value to validate.</param>
        /// <param name="max">The maximum allowed value (exclusive).</param>
        /// <param name="field">The field identifier for error reporting.</param>
        /// <param name="result">The validation result output.</param>
        /// <returns>True if validation passes, false otherwise.</returns>
        public static bool ValidateLessThan<TValue, TField>(
            TValue value,
            TValue max,
            TField field,
            out ValidationResult result)
            where TValue : struct, IComparable<TValue>
            where TField : Enum
        {
            var fieldName = FormatFieldName(field);
            return ValidateNumericField(
                value,
                v => v.CompareTo(max) < 0,
                field,
                $"The {fieldName} must be less than {max}.",
                out result);
        }

        /// <summary>
        /// Formats an enum field name into a human-readable lowercase string.
        /// Examples: "DeviceName" -> "device name", "HeightU" -> "height U", "MaxPowerConsumption" -> "max power consumption"
        /// </summary>
        private static string FormatFieldName<TField>(TField field) where TField : Enum
        {
            var name = field.ToString();

            // Insert spaces before capital letters (except the first one)
            // "DeviceName" -> "Device Name"
            var spacedName = Regex.Replace(name, "(?<!^)([A-Z])", " $1");

            // Convert to lowercase
            return spacedName.ToLower();
        }
    }
}