namespace SDM.PlanAndBuild.Tests.JobTypeTests
{
	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.SDM.PlanAndBuild.Models;
	using Skyline.DataMiner.SDM.PlanAndBuild.Validation;

	/// <summary>
	/// Unit tests for JobType validation business rules.
	/// Tests the static validation methods in JobTypeValidationHandler.
	/// </summary>
	[TestClass]
	public class JobTypeValidationHandlerTests
	{
		#region Name Validation

		[TestMethod]
		public void IsNameValid_WithNullJobType_ShouldBeInvalid()
		{
			var isValid = JobTypeValidationHandler.IsNameValid(null!, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeFalse();
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(JobTypeValidationHandler.JobTypeValidationField.JobType, out var reason).Should().BeTrue();
				reason.Should().Contain("cannot be null");
			}
		}

		[TestMethod]
		[DataRow("", DisplayName = "Empty Name")]
		[DataRow("   ", DisplayName = "Whitespace Name")]
		[DataRow(null, DisplayName = "Null Name")]
		public void IsNameValid_WithInvalidName_ShouldBeInvalid(string name)
		{
			var jobType = new JobType { Name = name };

			var isValid = JobTypeValidationHandler.IsNameValid(jobType, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeFalse();
				result.TryGetFailReason(JobTypeValidationHandler.JobTypeValidationField.Name, out var reason).Should().BeTrue();
				reason.Should().Contain("cannot be empty or whitespace");
			}
		}

		[TestMethod]
		public void IsNameValid_WithValidName_ShouldBeValid()
		{
			var jobType = new JobType { Name = "Installation" };

			var isValid = JobTypeValidationHandler.IsNameValid(jobType, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeTrue();
				result.IsValid.Should().BeTrue();
			}
		}

		#endregion
	}
}
