namespace Skyline.DataMiner.SDM.AssetManagement.Validation
{
    using System;
    using System.Collections.Generic;

    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Generic port number validator — enforces that port numbers are non-negative and unique within a collection.
    /// Applicable to any port type (DataPort, PowerPort, AssetClass DataPortInfo, AssetClass PowerPortInfo).
    /// </summary>
    internal static class PortNumberValidator
    {
        /// <summary>
        /// Validates that all port numbers in <paramref name="ports"/> are non-negative and unique.
        /// Fails fast on the first violation.
        /// </summary>
        /// <typeparam name="TPort">The port element type.</typeparam>
        /// <typeparam name="TField">The validation field enum type.</typeparam>
        /// <param name="ports">The collection of ports to validate.</param>
        /// <param name="getPortNumber">Selector that extracts the port number from a port element.</param>
        /// <param name="field">The validation field to report failures against.</param>
        /// <param name="portTypeName">Human-readable port type name used in error messages (e.g. "Data Port", "Power Port").</param>
        public static ValidationResult ValidateCollection<TPort, TField>(
            IEnumerable<TPort> ports,
            Func<TPort, long?> getPortNumber,
            TField field,
            string portTypeName = "Port") where TField : Enum
        {
            var result = new ValidationResult();
            var seen = new HashSet<long>();

            foreach (var port in ports)
            {
                var nullable = getPortNumber(port);

                if (!nullable.HasValue)
                {
                    result.AddFailReason(field, $"{portTypeName} number must have a value.");
                    return result;
                }

                var number = nullable.Value;

                if (number < 0)
                {
                    result.AddFailReason(field, $"{portTypeName} number cannot be negative. Found: {number}");
                    return result;
                }

                if (!seen.Add(number))
                {
                    result.AddFailReason(field, $"Duplicate {portTypeName} number found: {number}");
                    return result;
                }
            }

            return result;
        }

        public static ValidationResult ValidateCollection<TPort, TField>(
            IEnumerable<TPort> ports,
            Func<TPort, long> getPortNumber,
            TField field,
            string portTypeName = "Port") where TField : Enum
        {
            var result = new ValidationResult();
            var seen = new HashSet<long>();

            foreach (var port in ports)
            {
                var number = getPortNumber(port);

                if (number < 0)
                {
                    result.AddFailReason(field, $"{portTypeName} number cannot be negative. Found: {number}");
                    return result;
                }

                if (!seen.Add(number))
                {
                    result.AddFailReason(field, $"Duplicate {portTypeName} number found: {number}");
                    return result;
                }
            }

            return result;
        }
    }
}
