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
        [DataTestMethod]
        [DataRow(RepositoryAction.Create)]
        [DataRow(RepositoryAction.Update)]
        public void PortTypeValidator_Validate_WithValidPortType_ShouldReturnValid(RepositoryAction action)
        {
            var result = Helper.AssetManagement.PortTypeValidator.Validate(referencePortType, action);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [DataTestMethod]
        [DataRow(null, RepositoryAction.Create)]
        [DataRow("", RepositoryAction.Create)]
        [DataRow("   ", RepositoryAction.Create)]
        [DataRow("\t\r\n", RepositoryAction.Create)]
        [DataRow(null, RepositoryAction.Update)]
        [DataRow("", RepositoryAction.Update)]
        [DataRow("   ", RepositoryAction.Update)]
        [DataRow("\t\r\n", RepositoryAction.Update)]
        public void PortTypeValidator_Validate_WithMissingName_ShouldReturnNameFailure(string? name, RepositoryAction action)
        {
            referencePortType.Name = name!;

            var result = Helper.AssetManagement.PortTypeValidator.Validate(referencePortType, action);

            result.IsValid.Should().BeFalse();
            result.GetFailReason(PortTypeValidationField.Name).Should().Contain("Port Type Name cannot be empty or whitespace.");
            result.FailureReasons.Should().HaveCount(1);
        }

        [DataTestMethod]
        [DataRow(RepositoryAction.Create)]
        [DataRow(RepositoryAction.Update)]
        public void PortTypeValidator_ValidateBulk_WithMissingName_ShouldKeepFailuresAligned(RepositoryAction action)
        {
            var invalidPortType = new Skyline.DataMiner.SDM.AssetManagement.Models.PortType { Name = " " };
            invalidPortType.CategoryLinks.Categories = referencePortType.CategoryLinks.Categories;
            invalidPortType.CableFKs.CableTypeFks = referencePortType.CableFKs.CableTypeFks;

            var results = Helper.AssetManagement.PortTypeValidator.ValidateBulk([referencePortType, invalidPortType], action);

            results.Should().HaveCount(2);
            results[0].IsValid.Should().BeTrue();
            results[1].IsValid.Should().BeFalse();
            results[1].GetFailReason(PortTypeValidationField.Name).Should().Contain("Port Type Name cannot be empty or whitespace.");
        }

        [TestMethod]
        public void PortTypeValidator_ValidateForDelete_WithMissingName_ShouldNotApplyNameRule()
        {
            referencePortType.Name = null!;

            var result = Helper.AssetManagement.PortTypeValidator.Validate(referencePortType, RepositoryAction.Delete);

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
