namespace Skyline.DataMiner.SDM.InfraOps.Orchestration.AssetDeletion
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Skyline.DataMiner.SDM.AssetManagement.Deletion;

    /// <summary>
    /// Reports DOM deletion and dependency-cleanup outcomes for each Asset in a deletion request.
    /// </summary>
    public sealed class AssetDeletionCascadeException : Exception
    {
        internal AssetDeletionCascadeException(
            IReadOnlyCollection<AssetDeletionOutcome> outcomes,
            Exception domException)
            : base(BuildMessage(outcomes), domException)
        {
            Outcomes = outcomes ?? throw new ArgumentNullException(nameof(outcomes));
            DomException = domException;
        }

        /// <summary>
        /// Gets the per-Asset outcomes, including the failed cleanup stage where applicable.
        /// </summary>
        public IReadOnlyCollection<AssetDeletionOutcome> Outcomes { get; }

        /// <summary>
        /// Gets the original DOM deletion exception, if the DOM operation failed or partially failed.
        /// </summary>
        public Exception DomException { get; }

        private static string BuildMessage(IReadOnlyCollection<AssetDeletionOutcome> outcomes)
        {
            return "Asset deletion completed with DOM or dependency-cleanup failures: " +
                string.Join("; ", outcomes.Select(outcome => outcome.ToString()));
        }
    }

    /// <summary>
    /// Describes the result for one Asset in a deletion request.
    /// </summary>
    public sealed class AssetDeletionOutcome
    {
        internal AssetDeletionOutcome(
            string assetIdentifier,
            bool domDeletionSucceeded,
            string failedStage,
            Exception failure,
            AssetDeletionRecoveryContext recoveryContext = null)
        {
            AssetIdentifier = assetIdentifier;
            DomDeletionSucceeded = domDeletionSucceeded;
            FailedStage = failedStage;
            Failure = failure;
            RecoveryContext = recoveryContext;
        }

        /// <summary>
        /// Gets the Asset DOM identifier.
        /// </summary>
        public string AssetIdentifier { get; }

        /// <summary>
        /// Gets whether the Asset DOM instance was deleted.
        /// </summary>
        public bool DomDeletionSucceeded { get; }

        /// <summary>
        /// Gets the failed cleanup stage, or null when all work completed.
        /// </summary>
        public string FailedStage { get; }

        /// <summary>
        /// Gets the DOM or cleanup exception associated with this outcome.
        /// </summary>
        public Exception Failure { get; }

        /// <summary>
        /// Gets the caller-retained context for retrying incomplete cleanup, or null for a DOM failure.
        /// </summary>
        public AssetDeletionRecoveryContext RecoveryContext { get; }

        public override string ToString()
        {
            var status = DomDeletionSucceeded ? "DOM deleted" : "DOM delete failed";
            return string.IsNullOrEmpty(FailedStage)
                ? $"{AssetIdentifier}: {status}"
                : $"{AssetIdentifier}: {status}; stage '{FailedStage}' failed: {Failure?.Message}";
        }
    }
}
