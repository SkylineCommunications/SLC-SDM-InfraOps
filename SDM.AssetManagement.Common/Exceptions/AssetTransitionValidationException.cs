namespace Skyline.DataMiner.SDM.AssetManagement.Common.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Represents validation failures for an asset transition path.
    /// </summary>
    [Serializable]
    public class AssetTransitionValidationException : InfraOpsException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetTransitionValidationException"/> class.
        /// </summary>
        public AssetTransitionValidationException(ValidationResult validationResult)
            : base(GetMessage(validationResult))
        {
            ValidationResult = validationResult;
        }

        /// <summary>
        /// Gets the aggregated transition validation result.
        /// </summary>
        public ValidationResult ValidationResult { get; }

        /// <summary>
        /// Gets the aggregated transition failure reasons.
        /// </summary>
        public IReadOnlyDictionary<string, string> FailureReasons => ValidationResult.FailureReasons;

        protected AssetTransitionValidationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        private static string GetMessage(ValidationResult validationResult)
        {
            if (validationResult == null)
            {
                throw new ArgumentNullException(nameof(validationResult));
            }

            return validationResult.GetCombinedFailureReasons(Environment.NewLine);
        }
    }
}
