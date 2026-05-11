namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;

    /// <summary>
    /// Exception thrown when bulk validation fails.
    /// Contains validation results for all failed entities.
    /// </summary>
    public class BulkValidationException : InfraOpsException
    {
        /// <summary>
        /// Gets the validation results for all failed entities.
        /// Key: Entity identifier, Value: ValidationResult with errors.
        /// </summary>
        public Dictionary<string, ValidationResult> FailedResults { get; }

        /// <summary>
        /// Gets the total number of entities that failed validation.
        /// </summary>
        public int FailedCount => FailedResults.Count;

       /// <summary>
       /// Initializes a new instance of the BulkValidationException class with the specified validation results.   
       /// </summary>
       /// <remarks>Use this constructor to create an exception that aggregates multiple validation
       /// failures, allowing callers to inspect all failed results in detail.</remarks>
       /// <param name="results">A dictionary containing the validation results for each failed field or property. The key is the field or
       /// property name, and the value is the corresponding ValidationResult. Cannot be null.</param>
        public BulkValidationException(Dictionary<string, ValidationResult> results)
            : base(BuildErrorMessage(results))
        {
            FailedResults = results ?? new Dictionary<string, ValidationResult>();
        }

        private static string BuildErrorMessage(Dictionary<string, ValidationResult> failedResults)
        {
            if (failedResults == null || !failedResults.Any())
            {
                return "Bulk validation failed.";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Bulk validation failed for {failedResults.Count} item(s):");
            sb.AppendLine();

            foreach (var kvp in failedResults.Take(5)) // Show first 5 failures
            {
                sb.AppendLine($"Entity '{kvp.Key}':");
                foreach (var error in kvp.Value.FailureReasons.Take(3)) // Show first 3 errors per entity
                {
                    sb.AppendLine($"  - [{error.Key}] {error.Value}");
                }
                if (kvp.Value.FailureReasons.Count > 3)
                {
                    sb.AppendLine($"  ... and {kvp.Value.FailureReasons.Count - 3} more error(s)");
                }
                sb.AppendLine();
            }

            if (failedResults.Count > 5)
            {
                sb.AppendLine($"... and {failedResults.Count - 5} more failed item(s)");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets a summary of all validation errors.
        /// </summary>
        public string GetDetailedErrorSummary()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Bulk Validation Failed - {FailedCount} item(s) with errors:");
            sb.AppendLine();

            foreach (var kvp in FailedResults)
            {
                sb.AppendLine($"Entity '{kvp.Key}':");
                foreach (var error in kvp.Value.FailureReasons)
                {
                    sb.AppendLine($"  - [{error.Key}] {error.Value}");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}