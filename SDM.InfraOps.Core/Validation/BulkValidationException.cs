namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;

    /// <summary>
    /// Exception thrown when bulk validation fails.
    /// Contains validation results for all failed entities with their corresponding entities.
    /// </summary>
    /// <typeparam name="T">The type of entity being validated.</typeparam>
    internal class BulkValidationException<T> : InfraOpsException
    {
        /// <summary>
        /// Gets the list of failed entities with their validation results.
        /// </summary>
        public List<(T Entity, ValidationResult Result)> FailedItems { get; }

        /// <summary>
        /// Gets the total number of entities that failed validation.
        /// </summary>
        public int FailedCount => FailedItems.Count;

        /// <summary>
        /// Initializes a new instance of the BulkValidationException class with the specified entities and validation results.
        /// </summary>
        /// <param name="entities">The list of entities that were validated.</param>
        /// <param name="results">The validation results corresponding to each entity (same order and count).</param>
        /// <param name="getDisplayName">Optional function to extract a display name from an entity. If null, uses index-based naming.</param>
        public BulkValidationException(
            List<T> entities, 
            List<ValidationResult> results, 
            Func<T, string> getDisplayName = null)
            : base(BuildErrorMessage(entities, results, getDisplayName))
        {
            if (entities == null) throw new ArgumentNullException(nameof(entities));
            if (results == null) throw new ArgumentNullException(nameof(results));
            if (entities.Count != results.Count)
            {
                throw new ArgumentException("Entities and results lists must have the same count.");
            }

            FailedItems = BuildFailedItems(entities, results);
        }

        private static List<(T Entity, ValidationResult Result)> BuildFailedItems(
            List<T> entities, 
            List<ValidationResult> results)
        {
            var failed = new List<(T Entity, ValidationResult Result)>();
            
            for (int i = 0; i < entities.Count; i++)
            {
                if (!results[i].IsValid)
                {
                    failed.Add((entities[i], results[i]));
                }
            }

            return failed;
        }

        private static string BuildErrorMessage(
            List<T> entities, 
            List<ValidationResult> results, 
            Func<T, string> getDisplayName)
        {
            if (entities == null || results == null || entities.Count == 0)
                return "Bulk validation failed.";

            var failedCount = results.Count(r => !r.IsValid);

            var sb = new StringBuilder();
            sb.AppendLine($"Bulk validation failed for {failedCount} item(s):");
            sb.AppendLine();

            var displayed = 0;
            for (int i = 0; i < entities.Count && displayed < 5; i++)
            {
                if (results[i].IsValid) continue;
                AppendEntityErrors(sb, entities[i], results[i], i, getDisplayName);
                displayed++;
            }

            if (failedCount > 5)
                sb.AppendLine($"... and {failedCount - 5} more failed item(s)");

            return sb.ToString();
        }

        private static void AppendEntityErrors(
            StringBuilder sb,
            T entity,
            ValidationResult result,
            int index,
            Func<T, string> getDisplayName)
        {
            const int maxErrors = 3;
            var entityName = getDisplayName?.Invoke(entity) ?? $"Item at index {index}";
            sb.AppendLine($"{entityName}:");

            var shown = 0;
            foreach (var error in result.FailureReasons)
            {
                if (shown++ >= maxErrors) break;
                sb.AppendLine($"  - [{error.Key}] {error.Value}");
            }

            if (result.FailureReasons.Count > maxErrors)
                sb.AppendLine($"  ... and {result.FailureReasons.Count - maxErrors} more error(s)");

            sb.AppendLine();
        }

        /// <summary>
        /// Gets a summary of all validation errors.
        /// </summary>
        /// <param name="getDisplayName">Optional function to extract a display name from an entity.</param>
        public string GetDetailedErrorSummary(Func<T, string> getDisplayName = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Bulk Validation Failed - {FailedCount} item(s) with errors:");
            sb.AppendLine();

            for (int i = 0; i < FailedItems.Count; i++)
            {
                var item = FailedItems[i];
                var entityName = getDisplayName?.Invoke(item.Entity) ?? $"Item {i}";
                
                sb.AppendLine($"{entityName}:");
                foreach (var error in item.Result.FailureReasons)
                {
                    sb.AppendLine($"  - [{error.Key}] {error.Value}");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}