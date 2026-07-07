namespace SDM.InfraOpsProperties.Tests.Properties
{
	using System;
	using System.Collections.Generic;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SharedMappers.DomIds;

	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Validation;

	/// <summary>
	/// Tests for PropertyValidator which validates Property business rules.
	/// </summary>
	[TestClass]
	public class PropertyValidatorTests
	{
		private PropertyValidator _validator = null!;

		[TestInitialize]
		public void Setup()
		{
			_validator = new PropertyValidator();
		}

		#region Validate - Happy Path

		[TestMethod]
		public void Validate_WithAllValidFields_ShouldReturnValid()
		{
			var property = new Property
			{
				Name = "Serial Number",
				Scope = "Asset",
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.String,
				StringSizeLimit = 64,
			};

			var result = _validator.Validate(property);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeTrue();
				result.FailureReasons.Should().BeEmpty();
			}
		}

		[TestMethod]
		public void Validate_WithNullProperty_ShouldThrowArgumentNullException()
		{
			_validator.Invoking(v => v.Validate(null!))
				.Should().Throw<ArgumentNullException>();
		}

		#endregion

		#region Validate - Multiple Errors

		[TestMethod]
		public void Validate_WithMultipleInvalidFields_ShouldReturnAllErrors()
		{
			var property = new Property
			{
				Name = "Test",
				Scope = "Asset",
				StringSizeLimit = -5,
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.Discrete,
				Options = new List<string>(),
			};

			var result = _validator.Validate(property);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.FailureReasons.Should().HaveCountGreaterOrEqualTo(2,
					"should report errors for both string size limit and discrete options");
			}
		}

		[TestMethod]
		public void Validate_WithEmptyNameAndScope_ShouldStopAtCriticalValidations()
		{
			var property = new Property
			{
				Name = string.Empty,
				Scope = string.Empty,
				StringSizeLimit = -5,
			};

			var result = _validator.Validate(property);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValidationHandler.PropertyValidationField.Name, out _).Should().BeTrue();
				result.TryGetFailReason(PropertyValidationHandler.PropertyValidationField.Scope, out _).Should().BeTrue();
			}
		}

		#endregion

		#region Discrete Options Validation

		[TestMethod]
		public void Validate_WithDiscreteTypeAndOptions_ShouldReturnValid()
		{
			var property = new Property
			{
				Name = "Criticality",
				Scope = "Asset",
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.Discrete,
				Options = new List<string> { "Low", "High" },
			};

			var result = _validator.Validate(property);

			result.IsValid.Should().BeTrue();
		}

		[TestMethod]
		public void Validate_WithDiscreteTypeAndNoOptions_ShouldReturnInvalid()
		{
			var property = new Property
			{
				Name = "Criticality",
				Scope = "Asset",
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.Discrete,
			};

			var result = _validator.Validate(property);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValidationHandler.PropertyValidationField.Options, out var reason).Should().BeTrue();
				reason.Should().Contain("cannot be empty");
			}
		}

		#endregion

		#region Change Tracking

		[TestMethod]
		public void Validate_OnlyValidatesChangedFields()
		{
			// Create a valid property and simulate it being "loaded" from the database.
			var property = new Property
			{
				Name = "Test Change Tracking",
				Scope = "Asset",
				StringSizeLimit = 10,
			};
			property.ResetChangeTracking(); // Establish this as the "loaded state"
			property.IsNewInternal = false; // Simulate the entity being loaded (not new), so only changed fields are validated.

			// Introduce an invalid value directly on the tracking field without going through the normal setter flow
			// by setting scope to empty via the property and then resetting, simulating a scenario where a
			// non-relevant field was changed only.
			property.Scope = string.Empty; // Invalid, but we won't count this change.
			property.ResetChangeTracking(); // Reset again so the "invalid" scope becomes the base state.

			// Now only change StringSizeLimit to something valid.
			property.StringSizeLimit = 20;

			var result = _validator.Validate(property);

			result.IsValid.Should().BeTrue("Scope error should not be reported since it wasn't changed after the reset");
		}

		#endregion
	}
}
