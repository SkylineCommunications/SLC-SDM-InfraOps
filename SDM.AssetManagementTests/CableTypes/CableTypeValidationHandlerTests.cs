namespace SDM.AssetManagement.Tests.CableTypes
{
    using System.Collections.Generic;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.AssetManagement.Validation;

    [TestClass]
    public class CableTypeValidationHandlerTests
    {
        [TestMethod]
        public void Name_WithWhitespace_ShouldFail()
        {
            var isValid = CableTypeValidationHandler.IsCableTypeNameValid(" ", out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Cable Type Name cannot be empty or whitespace."));
        }

        [TestMethod]
        public void Categories_WithNullCableType_ShouldFail()
        {
            var isValid = CableTypeValidationHandler.IsCableTypeCategoriesValid(null, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Cable Type must be provided."));
        }

        [TestMethod]
        public void Categories_WithNoCategories_ShouldFail()
        {
            var cableType = new CableType
            {
                Name = "No Category",
                CategoryLinks = new CategoryRelation
                {
                    Categories = new List<SharedMappers.DomIds.SlcAsset_Management.Enums.CategoriesEnum>(),
                },
            };

            var isValid = CableTypeValidationHandler.IsCableTypeCategoriesValid(cableType, out var result);

            isValid.Should().BeFalse();
            result.FailureReasons.Should().Contain(reason => reason.ToString().Contains("Cable Type must have at least one category."));
        }
    }
}
