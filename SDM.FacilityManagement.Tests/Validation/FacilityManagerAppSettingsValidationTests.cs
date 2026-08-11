namespace SDM.FacilityManagement.Tests.Validation
{
    using FluentAssertions;

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
    }
}
