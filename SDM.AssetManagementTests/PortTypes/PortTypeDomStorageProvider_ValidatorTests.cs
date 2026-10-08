namespace SDM.AssetManagement.Tests.PortTypes
{
    using System.Collections.Generic;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using static Skyline.DataMiner.SDM.AssetManagement.Validation.PortTypeValidator;

    /// <summary>
    /// Validator-result tests for PortType.
    /// </summary>
    public partial class PortTypeDomStorageProviderTests
    {
        [TestMethod]
        public void PortTypeValidator_Validate_WithValidPortType_ShouldReturnValid()
        {
            var result = Helper.AssetManagement.PortTypeValidator.Validate(referencePortType, RepositoryAction.Create);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void PortTypeValidator_Validate_WithEmptyCategories_ShouldReturnInvalid()
        {
            referencePortType.CategoryLinks.Categories = new List<SlcAsset_Management.Enums.CategoriesEnum>();

            var result = Helper.AssetManagement.PortTypeValidator.Validate(referencePortType, RepositoryAction.Create);

            result.IsValid.Should().BeFalse();
            result.GetFailReason(PortTypeValidationField.Category).Should().Contain("Port Type must have at least one category.");
        }
    }
}
