namespace SDM.InfraOpsProperties.Tests.PropertyValuesTests
{
	using System;
	using System.Collections.Generic;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Validation;

	/// <summary>
	/// Tests for PropertyValuesValidator which validates PropertyValues business rules.
	/// </summary>
	[TestClass]
	public class PropertyValuesValidatorTests
	{
		private PropertyValuesValidator _validator = null!;

		[TestInitialize]
		public void Setup()
		{
			_validator = new PropertyValuesValidator();
		}

		#region Validate - Happy Path

		[TestMethod]
		public void Validate_WithAllValidFields_ShouldReturnValid()
		{
			var propertyValues = new PropertyValues
			{
				LinkedObjectID = Guid.NewGuid(),
				Scope = "Asset",
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Owner", Value = "Alice" },
				},
			};

			var result = _validator.Validate(propertyValues);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeTrue();
				result.FailureReasons.Should().BeEmpty();
			}
		}

		[TestMethod]
		public void Validate_WithNullPropertyValues_ShouldThrowArgumentNullException()
		{
			_validator.Invoking(v => v.Validate(null!))
				.Should().Throw<ArgumentNullException>();
		}

		#endregion

		#region Validate - Critical Failures

		[TestMethod]
		public void Validate_WithEmptyLinkedObjectID_ShouldReturnInvalid()
		{
			var propertyValues = new PropertyValues
			{
				LinkedObjectID = Guid.Empty,
				Scope = "Asset",
			};

			var result = _validator.Validate(propertyValues);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.LinkedObjectID, out _).Should().BeTrue();
			}
		}

		[TestMethod]
		public void Validate_WithEmptyScope_ShouldReturnInvalid()
		{
			var propertyValues = new PropertyValues
			{
				LinkedObjectID = Guid.NewGuid(),
				Scope = string.Empty,
			};

			var result = _validator.Validate(propertyValues);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.Scope, out _).Should().BeTrue();
			}
		}

		#endregion

		#region Validate - Values Errors

		[TestMethod]
		public void Validate_WithDuplicatePropertyNamesInValues_ShouldReturnInvalid()
		{
			var propertyValues = new PropertyValues
			{
				LinkedObjectID = Guid.NewGuid(),
				Scope = "Asset",
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Owner", Value = "Alice" },
					new PropertyValue { PropertyName = "Owner", Value = "Bob" },
				},
			};

			var result = _validator.Validate(propertyValues);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.Values, out var reason).Should().BeTrue();
				reason.Should().Contain("Duplicate Property Name");
			}
		}

		#endregion

		#region Change Tracking

		[TestMethod]
		public void Validate_OnlyValidatesChangedFields()
		{
			var propertyValues = new PropertyValues
			{
				LinkedObjectID = Guid.NewGuid(),
				Scope = "Asset",
			};
			propertyValues.ResetChangeTracking();
			propertyValues.IsNewInternal = false; // Simulate the entity being loaded (not new), so only changed fields are validated.

			// Make Scope invalid, then reset so it's the established baseline (not "changed" anymore).
			propertyValues.Scope = string.Empty;
			propertyValues.ResetChangeTracking();

			// Only change LinkedObjectID now.
			propertyValues.LinkedObjectID = Guid.NewGuid();

			var result = _validator.Validate(propertyValues);

			result.IsValid.Should().BeTrue("Scope error should not be reported since it wasn't changed after the reset");
		}

		#endregion
	}
}
