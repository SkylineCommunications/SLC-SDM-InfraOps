using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Skyline.DataMiner.SDM;
using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;
using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations
{
    public static class ValidationResultExtensions
    {
        public static ValidationResult CombineWith(this ValidationResult first, ValidationResult second)
        {
            var combined = new ValidationResult();
            combined.AddFrom(first);
            combined.AddFrom(second);
            return combined;
        }

        /// <summary>
        /// Converts bulk validation results to a BulkValidationException.
        /// Includes all validation failures for multiple entities.
        /// </summary>
        public static BulkValidationException ToException(
            this Dictionary<string, ValidationResult> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            var failedResults = results
                .Where(kvp => !kvp.Value.IsValid)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return new BulkValidationException(failedResults);
        }
    }
}
