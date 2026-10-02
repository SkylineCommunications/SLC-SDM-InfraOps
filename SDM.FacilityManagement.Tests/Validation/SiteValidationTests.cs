namespace SDM.FacilityManagement.Tests.Validation
{
    using System;
    using System.Linq;

    using FluentAssertions;

    using SDM.FacilityManagement.Tests.Setup;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.FacilityManagement.Services;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.FacilityManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    [TestClass]
    public class SiteValidationTests : BaseRepositoryTest
    {
        [TestMethod]
        public void Site_Create_WithEmptyId_ShouldThrow()
        {
            var entity = new Site { Identifier = Guid.NewGuid().ToString(), Name = "Site", SiteId = string.Empty };

            var action = () => Helper.Sites.Create(entity);

            action.Should().Throw<Exception>().WithMessage("*cannot be empty*");
        }

        [TestMethod]
        public void Site_Create_WithEmptyName_ShouldThrow()
        {
            var entity = new Site { Identifier = Guid.NewGuid().ToString(), Name = string.Empty, SiteId = "SITE-1" };

            var action = () => Helper.Sites.Create(entity);

            action.Should().Throw<Exception>().WithMessage("*Site Name cannot be empty*");
        }

        [TestMethod]
        public void Site_CreateOrUpdate_WithDuplicateIdInBatch_ShouldThrow()
        {
            var first = new Site { Identifier = Guid.NewGuid().ToString(), Name = "Site 1", SiteId = "DUP-1" };
            var second = new Site { Identifier = Guid.NewGuid().ToString(), Name = "Site 2", SiteId = "DUP-1" };

            var action = () => Helper.Sites.CreateOrUpdate(new[] { first, second });

            action.Should().Throw<Exception>().WithMessage("*duplicated within the batch*");
        }

        [TestMethod]
        public void Site_Create_WithDuplicateIdInDatabase_ShouldThrow()
        {
            var existing = new Site { Identifier = Guid.NewGuid().ToString(), Name = "Existing Site", SiteId = "EXIST-1" };
            Helper.Sites.Create(existing);

            var duplicate = new Site { Identifier = Guid.NewGuid().ToString(), Name = "Duplicate Site", SiteId = "EXIST-1" };
            var action = () => Helper.Sites.Create(duplicate);

            action.Should().Throw<Exception>().WithMessage("*already in use*");
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void SiteValidationHandler_WithEmptyOrWhitespaceSiteId_ShouldReturnExactMessage(string id)
        {
            var entity = new Site { SiteId = id };

            SiteValidationHandler.IsSiteIdValid(entity, out var result).Should().BeFalse();

            result.GetFailReason(SiteValidationHandler.SiteValidationField.SiteId).Should().Be("Site Id cannot be empty or whitespace.");
        }

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void SiteValidator_IsSiteIdValid_WithEmptyOrWhitespaceId_ShouldReturnExactMessage(string id)
        {
            var validator = CreateValidator();

            var result = validator.IsSiteIdValid(id);

            result.IsValid.Should().BeFalse();
            result.GetFailReason(SiteValidationHandler.SiteValidationField.SiteId).Should().Be("Site Id cannot be empty or whitespace.");
        }

        [TestMethod]
        public void SiteValidator_IsSiteIdValid_WithUniqueId_ShouldBeValid()
        {
            var validator = CreateValidator();

            var result = validator.IsSiteIdValid("UNIQUE");

            result.IsValid.Should().BeTrue();
        }

        [TestMethod]
        public void SiteValidator_IsSiteIdValid_WithIdAlreadyInStore_ShouldReturnExactMessage()
        {
            Helper.Sites.Create(NewSite("EXISTING"));
            var validator = CreateValidator();

            var result = validator.IsSiteIdValid("EXISTING");

            result.IsValid.Should().BeFalse();
            result.GetFailReason(SiteValidationHandler.SiteValidationField.SiteId).Should().Be("Site Id 'EXISTING' is already in use.");
        }

        [TestMethod]
        public void SiteValidator_ValidateBulk_WithIdInOtherChangedEntries_ShouldReturnExactMessage()
        {
            var validator = CreateValidator();

            var results = validator.ValidateBulk(new List<Site> { NewSite("BASE"), NewSite("DUP"), NewSite("DUP") }, RepositoryAction.Create);

            results[0].IsValid.Should().BeTrue(results[0].GetCombinedFailureReasons(";"));
            results[1].IsValid.Should().BeFalse();
            results[1].GetFailReason(SiteValidationHandler.SiteValidationField.SiteId).Should().Be("Site Id 'DUP' is duplicated within the batch.");
            results[2].GetFailReason(SiteValidationHandler.SiteValidationField.SiteId).Should().Be("Site Id 'DUP' is duplicated within the batch.");
        }

        [TestMethod]
        public void SiteValidator_Validate_WithValidSite_ShouldBeValid()
        {
            var entity = NewSite("VALID");
            var validator = CreateValidator();

            var result = validator.Validate(entity, RepositoryAction.Create);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void SiteValidator_Validate_SavedSiteWithClearedId_ShouldBeInvalid()
        {
            var created = NewSite("VALID");
            Helper.Sites.Create(created);
            var existing = Helper.Sites.Read(SiteExposers.Identifier.Equal(created.Identifier)).Single();
            existing.SiteId = string.Empty;
            var validator = CreateValidator();

            var result = validator.Validate(existing, RepositoryAction.Update);

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(SiteValidationHandler.SiteValidationField.SiteId, out _).Should().BeTrue();
        }

        private SiteValidator CreateValidator()
        {
            return new SiteValidator(new FacilityEntityLoader(Helper));
        }

        private static Site NewSite(string id)
        {
            return new Site { Identifier = Guid.NewGuid().ToString(), Name = "Site " + id, SiteId = id };
        }
    }
}
