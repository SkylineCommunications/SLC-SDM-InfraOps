namespace SDM.PlanAndBuild.Tests.JobTests
{
	using System;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.PlanAndBuild.Tests.Setup;

	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;
	using Skyline.DataMiner.SDM.PlanAndBuild.Validation;
	using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
	using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

	using Statuses = SharedMappers.DomIds.SlcPlan_And_Build.Behaviors.Job_Behavior.StatusesEnum;

	[TestClass]
	public class PlanAndBuildJobRequiredReferenceTests : BaseRepositoryTest
	{
		private PlanAndBuildJobValidator _validator = null!;
		private JobType _jobType = null!;

		[TestInitialize]
		public void Setup()
		{
			Helper.PopulateAppSettings();
			_validator = new PlanAndBuildJobValidator(Helper, ConnectionHelper.CreateDefaultPeopleApiMock());
			_jobType = Helper.JobTypes.Create(new JobType { Name = "Installation" });
		}

		[TestMethod]
		public void Validate_WithoutJobType_ShouldReturnJobTypeRequired()
		{
			var job = new PlanAndBuildJob { JobName = "Job without type" };

			var result = _validator.Validate(job, RepositoryAction.Create);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PlanAndBuildJobValidationHandler.PlanAndBuildJobValidationField.JobType, out var reason).Should().BeTrue();
				reason.Should().Contain("A Job Type must be selected.");
			}
		}

		[TestMethod]
		public void Validate_InNewState_WithoutAssignment_ShouldReturnValid()
		{
			var job = new PlanAndBuildJob
			{
				JobName = "New job",
				Type = new SdmObjectReference<JobType>(_jobType.Identifier),
			};

			var result = _validator.Validate(job, RepositoryAction.Create);

			result.IsValid.Should().BeTrue();
		}

		[DataTestMethod]
		[DataRow(Statuses.Active)]
		[DataRow(Statuses.Assigned)]
		[DataRow(Statuses.Review)]
		[DataRow(Statuses.Resolved)]
		public void Validate_InStateRequiringAssignment_WithoutAssignment_ShouldReturnRequired(Statuses state)
		{
			var job = new PlanAndBuildJob
			{
				JobName = "Unassigned job",
				Type = new SdmObjectReference<JobType>(_jobType.Identifier),
				State = state,
			};

			var result = _validator.Validate(job, RepositoryAction.Create);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PlanAndBuildJobValidationHandler.PlanAndBuildJobValidationField.AssignedTo, out var assignedTo).Should().BeTrue();
				assignedTo.Should().Contain("Assigned To is required.");
				result.TryGetFailReason(PlanAndBuildJobValidationHandler.PlanAndBuildJobValidationField.AssignmentGroup, out var group).Should().BeTrue();
				group.Should().Contain("Assignment Group is required.");
			}
		}

		[TestMethod]
		public void Validate_Canceled_WithoutAssignment_ShouldNotRequireAssignment()
		{
			var job = new PlanAndBuildJob
			{
				JobName = "Canceled job",
				Type = new SdmObjectReference<JobType>(_jobType.Identifier),
				State = Statuses.Canceled,
			};

			var result = _validator.Validate(job, RepositoryAction.Create);

			result.TryGetFailReason(PlanAndBuildJobValidationHandler.PlanAndBuildJobValidationField.AssignedTo, out _).Should().BeFalse();
		}
	}
}
