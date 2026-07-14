using System.Collections.Generic;
using System.Linq;

namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations
{
    public static class ValidationResultExtensions
    {
        /// <summary>
        /// Combines two ValidationResult instances into a single result.
        /// Includes both failures and warnings from both results.
        /// </summary>
        public static ValidationResult CombineWith(this ValidationResult first, ValidationResult second)
        {
            if (first == null) return second ?? new ValidationResult();
            if (second == null) return first;

            var combined = new ValidationResult();
            combined.AddFrom(first);
            combined.AddFrom(second);
            return combined;
        }

        /// <summary>
        /// Merges a collection of ValidationResult objects into a single result.
        /// Combines all failures and warnings from all results.
        /// Returns an empty ValidationResult if the collection is null or empty.
        /// </summary>
        /// <param name="results">The collection of validation results to merge.</param>
        /// <returns>A single ValidationResult containing all failures and warnings.</returns>
        public static ValidationResult MergeAll(this IEnumerable<ValidationResult> results)
        {
            if (results == null || !results.Any())
            {
                return new ValidationResult();
            }

            var merged = new ValidationResult();
            foreach (var result in results)
            {
                if (result != null)
                {
                    merged.AddFrom(result);
                }
            }

            return merged;
        }

        /// <summary>
        /// Merges validation results at specific indices into a target list.
        /// Useful for combining results from multiple validation phases.
        /// </summary>
        /// <param name="target">The target list to merge results into.</param>
        /// <param name="source">The source list of results to merge from.</param>
        public static void MergeFrom(this List<ValidationResult> target, List<ValidationResult> source)
        {
            if (target == null || source == null)
            {
                return;
            }

            int count = System.Math.Min(target.Count, source.Count);
            for (int i = 0; i < count; i++)
            {
                target[i].AddFrom(source[i]);
            }
        }

        /// <summary>
        /// Checks if any validation result in the collection has failures.
        /// </summary>
        public static bool AnyInvalid(this IEnumerable<ValidationResult> results)
        {
            return results?.Any(r => r != null && !r.IsValid) ?? false;
        }

        /// <summary>
        /// Checks if all validation results in the collection are valid.
        /// </summary>
        public static bool AllValid(this IEnumerable<ValidationResult> results)
        {
            return results?.All(r => r == null || r.IsValid) ?? true;
        }

        /// <summary>
        /// Gets the count of invalid results in the collection.
        /// </summary>
        public static int CountInvalid(this IEnumerable<ValidationResult> results)
        {
            return results?.Count(r => r != null && !r.IsValid) ?? 0;
        }
    }
}
