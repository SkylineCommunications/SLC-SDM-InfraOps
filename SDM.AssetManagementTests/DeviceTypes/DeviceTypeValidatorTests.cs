namespace SDM.AssetManagement.Tests.DeviceTypes
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

    using static Skyline.DataMiner.SDM.AssetManagement.Validation.DeviceTypeValidationHandler;

    [TestClass]
    public class DeviceTypeValidatorTests : BaseRepositoryTest
    {
        private DeviceTypeValidator Validator => Helper.AssetManagement.DeviceTypeValidator;

        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        public void Validate_WithEmptyOrWhitespaceName_ShouldReturnInvalid(string name)
        {
            var deviceType = CreateDeviceType(name);

            var result = Validator.Validate(deviceType, RepositoryAction.Create);

            result.IsValid.Should().BeFalse();
            result.GetFailReason(DeviceTypeValidationField.Name).Should().Be("Device Type Name cannot be empty or whitespace.");
        }

        [TestMethod]
        public void Validate_WithUniqueName_ShouldReturnValid()
        {
            Helper.AssetManagement.DeviceTypes.Create(CreateDeviceType("Existing"));

            var result = Validator.Validate(CreateDeviceType("UniqueDevice"), RepositoryAction.Create);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void Validate_WithNameAlreadyInStore_ShouldReturnInvalid()
        {
            Helper.AssetManagement.DeviceTypes.Create(CreateDeviceType("Existing"));

            var result = Validator.Validate(CreateDeviceType("Existing"), RepositoryAction.Create);

            result.IsValid.Should().BeFalse();
            result.GetFailReason(DeviceTypeValidationField.Name).Should().Be("Device Type Name 'Existing' is already in use.");
        }

        [TestMethod]
        public void ValidateBulk_WithNameDuplicatedInOtherChangedEntry_ShouldReturnInvalid()
        {
            var entries = new List<DeviceType>
            {
                CreateDeviceType("Base"),
                CreateDeviceType("Duplicate"),
                CreateDeviceType("Duplicate"),
            };

            var results = Validator.ValidateBulk(entries, RepositoryAction.Create);

            results[0].IsValid.Should().BeTrue();
            results[1].IsValid.Should().BeFalse();
            results[1].GetFailReason(DeviceTypeValidationField.Name).Should().Be("Device Type Name 'Duplicate' is duplicated within the batch.");
            results[2].GetFailReason(DeviceTypeValidationField.Name).Should().Be("Device Type Name 'Duplicate' is duplicated within the batch.");
        }

        [TestMethod]
        public void Validate_WithValidDeviceType_ShouldReturnValid()
        {
            var result = Validator.Validate(CreateDeviceType("ValidDevice"), RepositoryAction.Create);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        [TestMethod]
        public void Validate_WithNameClearedOnSavedDeviceType_ShouldReturnInvalid()
        {
            Helper.AssetManagement.DeviceTypes.Create(CreateDeviceType("Valid"));
            var saved = Helper.AssetManagement.DeviceTypes.Read(new TRUEFilterElement<DeviceType>()).Single();
            saved.Name = string.Empty;

            var result = Validator.Validate(saved, RepositoryAction.Update);

            result.IsValid.Should().BeFalse();
            result.TryGetFailReason(DeviceTypeValidationField.Name, out _).Should().BeTrue();
        }

        [TestMethod]
        public void ValidateForDelete_WithNoAssetsAssigned_ShouldReturnValid()
        {
            var saved = Helper.AssetManagement.DeviceTypes.Create(CreateDeviceType("UnusedDevice"));

            var result = Validator.Validate(saved, RepositoryAction.Delete);

            result.IsValid.Should().BeTrue(result.GetCombinedFailureReasons(";"));
        }

        private static DeviceType CreateDeviceType(string name)
        {
            return new DeviceType
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = name,
                Description = "Desc",
                HierarchyInfo =
                {
                    HierarchyRole = SlcAsset_Management.Enums.HierarchyRoleEnum.Chassis,
                },
                TagsInfo =
                {
                    Tags = new List<SlcAsset_Management.Enums.TagOption>(),
                },
            };
        }
    }
}
