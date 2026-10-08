namespace SDM.FacilityManagement.Tests.Validation
{
    using FluentAssertions;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Validation;

    [TestClass]
    public class FacilityManagerAppSettingsValidationTests
    {
        [TestMethod]
        public void FacilityManagerAppSettingsValidationHandler_WithNullSettings_ShouldReturnExactMessage()
        {
            FacilityManagerAppSettingsValidationHandler.IsValid(null, out var result).Should().BeFalse();

            result.GetFailReason(FacilityManagerAppSettingsValidationHandler.FacilityManagerAppSettingsValidationField.FacilityManagerAppSettings)
                .Should().Be("FacilityManagerAppSettings cannot be null.");
        }

        [TestMethod]
        public void FacilityManagerAppSettingsValidationHandler_WithNonNullSettings_ShouldBeValid()
        {
            var settings = new FacilityManagerAppSettings { GoogleMapsAPIKey = "my-key" };

            FacilityManagerAppSettingsValidationHandler.IsValid(settings, out var result).Should().BeTrue();

            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void FacilityManagerAppSettingsValidator_WithNonNullSettings_ShouldBeValid()
        {
            var settings = new FacilityManagerAppSettings { GoogleMapsAPIKey = "my-key" };

            var result = new FacilityManagerAppSettingsValidator().Validate(settings);

            result.IsValid.Should().BeTrue();
        }
    }
}
