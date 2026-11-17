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

        [TestMethod]
        public void PortTypeValidator_Validate_WithNameOfExistingPortType_ShouldReturnNameInUse()
        {
            Helper.AssetManagement.PortTypes.Create(referencePortType);
            var duplicate = CreatePortTypeNamed(referencePortType.Name);

            var result = Helper.AssetManagement.PortTypeValidator.Validate(duplicate, RepositoryAction.Create);

            result.IsValid.Should().BeFalse();
            result.GetFailReason(PortTypeValidationField.Name).Should().Be($"Port Type Name '{referencePortType.Name}' is already in use.");
        }

        [TestMethod]
        public void PortTypeValidator_Validate_WhenRenamingToExistingName_ShouldReturnNameInUse()
        {
            Helper.AssetManagement.PortTypes.Create(referencePortType);
            var other = Helper.AssetManagement.PortTypes.Create(CreatePortTypeNamed("Other PortType"));
            other.Name = referencePortType.Name;

            var result = Helper.AssetManagement.PortTypeValidator.Validate(other, RepositoryAction.Update);

            result.IsValid.Should().BeFalse();
            result.GetFailReason(PortTypeValidationField.Name).Should().Be($"Port Type Name '{referencePortType.Name}' is already in use.");
        }

        [TestMethod]
        public void PortTypeValidator_Validate_WhenUpdatingWithOwnName_ShouldReturnValid()
        {
            Helper.AssetManagement.PortTypes.Create(referencePortType);
            referencePortType.Description = "Changed";

            var result = Helper.AssetManagement.PortTypeValidator.Validate(referencePortType, RepositoryAction.Update);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void PortTypeValidator_ValidateBulk_WithDuplicateNamesInBatch_ShouldFailBoth()
        {
            var second = CreatePortTypeNamed(referencePortType.Name.ToUpperInvariant());

            var results = Helper.AssetManagement.PortTypeValidator.ValidateBulk([referencePortType, second], RepositoryAction.Create);

            results.Should().HaveCount(2);
            results[0].GetFailReason(PortTypeValidationField.Name).Should().Be($"Port Type Name '{referencePortType.Name}' is duplicated within the batch.");
            results[1].GetFailReason(PortTypeValidationField.Name).Should().Be($"Port Type Name '{second.Name}' is duplicated within the batch.");
        }

        [TestMethod]
        public void PortTypeDomStorageProvider_Create_WithNameOfExistingPortType_ShouldFailWithoutPersisting()
        {
            Helper.AssetManagement.PortTypes.Create(referencePortType);

            System.Action act = () => Helper.AssetManagement.PortTypes.Create(CreatePortTypeNamed(referencePortType.Name));

            act.Should().Throw<System.Exception>().WithMessage("*is already in use*");
            Helper.AssetManagement.PortTypes.Count(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.AssetManagement.Models.PortType>()).Should().Be(1);
        }

        private Skyline.DataMiner.SDM.AssetManagement.Models.PortType CreatePortTypeNamed(string name)
        {
            var portType = new Skyline.DataMiner.SDM.AssetManagement.Models.PortType
            {
                Identifier = System.Guid.NewGuid().ToString(),
                Name = name,
            };
            portType.CategoryLinks.Categories = referencePortType.CategoryLinks.Categories;
            portType.CableFKs.CableTypeFks = referencePortType.CableFKs.CableTypeFks;
            return portType;
        }
    }
}
