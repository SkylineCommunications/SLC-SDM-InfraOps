namespace SDM.AssetManagement.Tests.CableTypes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    using static Skyline.DataMiner.SDM.AssetManagement.Validation.CableTypeValidationHandler;

    [TestClass]
    public class CableTypeValidatorTests : BaseRepositoryTest
    {
        private CableTypeValidator Validator => Helper.AssetManagement.CableTypeValidator;

        [TestMethod]
        public void IsCableTypeNameValid_WithEmptyName_ShouldReturnInvalid()
        {
            var result = Validator.IsCableTypeNameValid(string.Empty);

            result.IsValid.Should().BeFalse();
            result.GetFailReason(CableTypeValidationField.Name).Should().Be("Cable Type Name cannot be empty or whitespace.");
        }

        [TestMethod]
        public void IsCableTypeNameValid_WithUniqueName_ShouldReturnValid()
        {
            Helper.AssetManagement.CableTypes.Create(CreateCableType("Existing"));

            var result = Validator.IsCableTypeNameValid("UniqueCable");

            result.IsValid.Should().BeTrue();
            result.FailureReasons.Should().BeEmpty();
        }

        [TestMethod]
        public void IsCableTypeNameValid_WithNameAlreadyInStore_ShouldReturnInvalid()
        {
            Helper.AssetManagement.CableTypes.Create(CreateCableType("Existing"));

            var result = Validator.IsCableTypeNameValid("Existing");

            result.IsValid.Should().BeFalse();
            result.GetFailReason(CableTypeValidationField.Name).Should().Be("Cable Type Name 'Existing' is already in use.");
        }

        [TestMethod]
        public void ValidateBulk_WithNameDuplicatedInOtherChangedEntry_ShouldReturnInvalid()
        {
            var entries = new List<CableType>
            {
                CreateCableType("Base"),
                CreateCableType("Duplicate"),
                CreateCableType("Duplicate"),
            };

            var results = Validator.ValidateBulk(entries, RepositoryAction.Create);

            results[0].IsValid.Should().BeTrue();
            results[1].IsValid.Should().BeFalse();
            results[1].GetFailReason(CableTypeValidationField.Name).Should().Be("Cable Type Name 'Duplicate' is duplicated within the batch.");
            results[2].GetFailReason(CableTypeValidationField.Name).Should().Be("Cable Type Name 'Duplicate' is duplicated within the batch.");
        }

        [TestMethod]
        public void Validate_WithValidCableType_ShouldReturnValid()
        {
            var cableType = CreateCableType("ValidCable");

            var result = Validator.Validate(cableType, RepositoryAction.Create);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void Validate_WithNameClearedOnSavedCableType_ShouldReturnInvalid()
        {
            Helper.AssetManagement.CableTypes.Create(CreateCableType("Valid"));
            var saved = Helper.AssetManagement.CableTypes.Read(new TRUEFilterElement<CableType>()).Single();
            saved.Name = string.Empty;

            var result = Validator.Validate(saved, RepositoryAction.Update);

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(CableTypeValidationField.Name, out _).Should().BeTrue();
        }

        [TestMethod]
        public void Validate_WithEmptyCategories_ShouldReturnInvalid()
        {
            var cableType = CreateCableType("ValidNoCats");
            cableType.CategoryLinks.Categories = new List<SlcAsset_Management.Enums.CategoriesEnum>();

            var result = Validator.Validate(cableType, RepositoryAction.Create);

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(CableTypeValidationField.Category, out _).Should().BeTrue();
        }

        private static CableType CreateCableType(string name)
        {
            return new CableType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = name,
                Description = "Desc",
                CategoryLinks =
                {
                    Categories = new List<SlcAsset_Management.Enums.CategoriesEnum> { SlcAsset_Management.Enums.CategoriesEnum.Data },
                },
            };
        }
    }
}
