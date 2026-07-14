namespace Skyline.DataMiner.SDM.PlanAndBuild.Validation
{
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for JobType business rules.
    /// Contains pure validation logic without data access.
    /// Ported from InfraOpsShared.DOM_Classes.DOM.Applications.Plan_And_Build.Validation.JobTypeValidationHandler.
    /// </summary>
    public static class JobTypeValidationHandler
    {
        public enum JobTypeValidationField
        {
            JobType,
            Name,
        }

        #region Info Validation

        /// <summary>
        /// Validates that the Name is not empty or whitespace.
        /// </summary>
        public static bool IsNameValid(JobType jobType, out ValidationResult result)
        {
            result = new ValidationResult();

            if (jobType == null)
            {
                result.AddFailReason(JobTypeValidationField.JobType, "Job Type cannot be null.");
                return result.IsValid;
            }

            if (string.IsNullOrWhiteSpace(jobType.Name))
            {
                result.AddFailReason(JobTypeValidationField.Name, "Job Type Name cannot be empty or whitespace.");
            }

            return result.IsValid;
        }

        #endregion
    }
}
