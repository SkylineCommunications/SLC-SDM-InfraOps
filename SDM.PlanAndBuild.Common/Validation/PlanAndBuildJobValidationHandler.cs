namespace Skyline.DataMiner.SDM.PlanAndBuild.Validation
{
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for PlanAndBuildJob business rules.
    /// Contains pure validation logic without data access.
    /// Ported from InfraOpsShared.DOM_Classes.DOM.Applications.Plan_And_Build.Validation.JobValidationHandler
    /// and the additional UI-layer checks in InfraOpsInteractiveAutomationCommon.Dialogs.PlanAndBuild.JobInfo.DataModelBase.
    /// </summary>
    public static class PlanAndBuildJobValidationHandler
    {
        public enum PlanAndBuildJobValidationField
        {
            Job,
            JobName,
            JobType,
            End,
            AssignedTo,
            AssignmentGroup,
            Attachments,
        }

        #region Info Validation

        /// <summary>
        /// Validates that the JobName is not empty or whitespace.
        /// </summary>
        public static bool IsJobNameValid(PlanAndBuildJob job, out ValidationResult result)
        {
            result = new ValidationResult();

            if (job == null)
            {
                result.AddFailReason(PlanAndBuildJobValidationField.Job, "Job cannot be null.");
                return result.IsValid;
            }

            if (string.IsNullOrWhiteSpace(job.JobName))
            {
                result.AddFailReason(PlanAndBuildJobValidationField.JobName, "Job Name cannot be empty or whitespace.");
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates that a JobType has been selected.
        /// </summary>
        public static bool IsJobTypeValid(PlanAndBuildJob job, out ValidationResult result)
        {
            result = new ValidationResult();

            if (job == null)
            {
                result.AddFailReason(PlanAndBuildJobValidationField.Job, "Job cannot be null.");
                return result.IsValid;
            }

            if (job.JobType == null)
            {
                result.AddFailReason(PlanAndBuildJobValidationField.JobType, "A Job Type must be selected.");
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates that, when an End time is set, it is strictly greater than the Start time.
        /// </summary>
        public static bool IsEndTimeValid(PlanAndBuildJob job, out ValidationResult result)
        {
            result = new ValidationResult();

            if (job == null)
            {
                result.AddFailReason(PlanAndBuildJobValidationField.Job, "Job cannot be null.");
                return result.IsValid;
            }

            if (job.End.HasValue && job.Start.HasValue && job.Start.Value >= job.End.Value)
            {
                result.AddFailReason(PlanAndBuildJobValidationField.End, "End time must be higher than Start time.");
            }

            return result.IsValid;
        }

        #endregion
    }
}
