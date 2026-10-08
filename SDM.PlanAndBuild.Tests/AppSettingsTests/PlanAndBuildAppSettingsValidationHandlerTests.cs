namespace SDM.PlanAndBuild.Tests.AppSettingsTests
{
	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.SDM.PlanAndBuild.Models;
	using Skyline.DataMiner.SDM.PlanAndBuild.Validation;

	[TestClass]
	public class PlanAndBuildAppSettingsValidationHandlerTests
	{
		[TestMethod]
		public void IsValid_WithNullAppSettings_ShouldReturnInvalid()
		{
			var isValid = PlanAndBuildAppSettingsValidationHandler.IsValid(null!, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeFalse();
				result.TryGetFailReason(PlanAndBuildAppSettingsValidationHandler.PlanAndBuildAppSettingsValidationField.PlanAndBuildAppSettings, out var reason).Should().BeTrue();
				reason.Should().Be("PlanAndBuildAppSettings cannot be null.");
			}
		}

		[TestMethod]
		public void IsValid_WithDefaultAppSettings_ShouldReturnValid()
		{
			var isValid = PlanAndBuildAppSettingsValidationHandler.IsValid(new PlanAndBuildAppSettings(), out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeTrue();
				result.IsValid.Should().BeTrue();
				result.FailureReasons.Should().BeEmpty();
			}
		}

		[TestMethod]
		public void IsValid_WithFullyPopulatedAppSettings_ShouldReturnValid()
		{
			var appSettings = new PlanAndBuildAppSettings
			{
				JobIDPrefix = "TASK-",
				JobIDMinimumDigits = 3,
				JobIDStartingSeed = 10,
				JobIDIncrement = 2,
				JobIDNextSequence = 11,
			};

			var isValid = PlanAndBuildAppSettingsValidationHandler.IsValid(appSettings, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeTrue();
				result.FailureReasons.Should().BeEmpty();
			}
		}

		[TestMethod]
		public void Validator_Validate_WithDefaultAppSettings_ShouldReturnValid()
		{
			var result = new PlanAndBuildAppSettingsValidator().Validate(new PlanAndBuildAppSettings());

			using (new AssertionScope())
			{
				result.IsValid.Should().BeTrue();
				result.FailureReasons.Should().BeEmpty();
			}
		}
	}
}
