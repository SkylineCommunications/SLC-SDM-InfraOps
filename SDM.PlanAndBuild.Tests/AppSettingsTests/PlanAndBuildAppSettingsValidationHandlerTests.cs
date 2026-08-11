namespace SDM.PlanAndBuild.Tests.AppSettingsTests
{
	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

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
	}
}
