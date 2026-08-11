namespace Skyline.DataMiner.SDM.PlanAndBuild.Validation
{
    using SharedMappers.DomIds;

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
            Locations,
            AssetsUsed,
            Connections,
            Start,
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

            if (job.Type == null)
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

        #region State-Gated Edit Validation

        public static bool AreStateGatedChangesAllowed(PlanAndBuildJob job, out ValidationResult result)
        {
            result = new ValidationResult();

            if (job == null)
            {
                result.AddFailReason(PlanAndBuildJobValidationField.Job, "Job cannot be null.");
                return result.IsValid;
            }

            if (job.IsNew)
            {
                return result.IsValid;
            }

            var originalState = job.StateField.OriginalValue;

            if (job.LocationsField.Changed && !CanEditLocations(originalState))
            {
                result.AddFailReason(PlanAndBuildJobValidationField.Locations, "Cannot edit job locations. This action is only available for jobs in 'New' or 'Assigned' state.");
            }

            if (job.AssetsUsedField.Changed && IsTerminal(originalState))
            {
                result.AddFailReason(PlanAndBuildJobValidationField.AssetsUsed, "Cannot edit job assets used. This action is not available for 'Resolved' or 'Cancelled' jobs.");
            }

            if (job.ConnectionsOnJobField.Changed && IsTerminal(originalState))
            {
                result.AddFailReason(PlanAndBuildJobValidationField.Connections, "Cannot edit job connections. This action is not available for 'Resolved' or 'Cancelled' jobs.");
            }

            if (job.JobNameField.Changed && originalState != SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.New)
            {
                result.AddFailReason(PlanAndBuildJobValidationField.JobName, "Cannot edit the Job Name unless the Job is in 'New' state.");
            }

            if (job.StartField.Changed && originalState != SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.New)
            {
                result.AddFailReason(PlanAndBuildJobValidationField.Start, "Cannot edit the Start time unless the Job is in 'New' state.");
            }

            if (job.TypeField.Changed && originalState != SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.New)
            {
                result.AddFailReason(PlanAndBuildJobValidationField.JobType, "Cannot edit the Job Type unless the Job is in 'New' state.");
            }

            return result.IsValid;
        }

        private static bool CanEditLocations(SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum state)
        {
            return state == SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.New ||
                state == SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Assigned;
        }

        private static bool IsTerminal(SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum state)
        {
            return state == SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Resolved ||
                state == SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum.Canceled;
        }

        #endregion
    }
}
