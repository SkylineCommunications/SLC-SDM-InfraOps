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
	/// Unit tests for PropertyValues validation business rules.
	/// Tests the static validation methods in PropertyValuesValidationHandler.
	/// </summary>
	[TestClass]
	public class PropertyValuesValidationHandlerTests
	{
		#region LinkedObjectID Validation

		[TestMethod]
		public void IsLinkedObjectIDValid_WithNullPropertyValues_ShouldBeInvalid()
		{
			var isValid = PropertyValuesValidationHandler.IsLinkedObjectIDValid(null!, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.PropertyValues, out var reason).Should().BeTrue();
				reason.Should().Contain("cannot be null");
			}
		}

		[TestMethod]
		public void IsLinkedObjectIDValid_WithEmptyGuid_ShouldBeInvalid()
		{
			var propertyValues = new PropertyValues { LinkedObjectID = Guid.Empty };

			var isValid = PropertyValuesValidationHandler.IsLinkedObjectIDValid(propertyValues, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.LinkedObjectID, out var reason).Should().BeTrue();
				reason.Should().Contain("cannot be empty");
			}
		}

		[TestMethod]
		public void IsLinkedObjectIDValid_WithValidGuid_ShouldBeValid()
		{
			var propertyValues = new PropertyValues { LinkedObjectID = Guid.NewGuid() };

			var isValid = PropertyValuesValidationHandler.IsLinkedObjectIDValid(propertyValues, out var result);

			isValid.Should().BeTrue();
		}

		#endregion

		#region Scope Validation

		[TestMethod]
		[DataRow("", DisplayName = "Empty Scope")]
		[DataRow("   ", DisplayName = "Whitespace Scope")]
		[DataRow(null, DisplayName = "Null Scope")]
		public void IsScopeValid_WithInvalidScope_ShouldBeInvalid(string scope)
		{
			var propertyValues = new PropertyValues { Scope = scope };

			var isValid = PropertyValuesValidationHandler.IsScopeValid(propertyValues, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.Scope, out var reason).Should().BeTrue();
				reason.Should().Contain("cannot be empty or whitespace");
			}
		}

		[TestMethod]
		public void IsScopeValid_WithValidScope_ShouldBeValid()
		{
			var propertyValues = new PropertyValues { Scope = "Asset" };

			var isValid = PropertyValuesValidationHandler.IsScopeValid(propertyValues, out var result);

			isValid.Should().BeTrue();
		}

		[TestMethod]
		public void IsScopeValid_WithNullPropertyValues_ShouldBeInvalid()
		{
			var isValid = PropertyValuesValidationHandler.IsScopeValid(null!, out var result);

			isValid.Should().BeFalse();
		}

		#endregion

		#region Values Validation

		[TestMethod]
		public void IsValuesValid_WithNullPropertyValues_ShouldBeInvalid()
		{
			var isValid = PropertyValuesValidationHandler.IsValuesValid(null!, out var result);

			isValid.Should().BeFalse();
		}

		[TestMethod]
		public void IsValuesValid_WithNullValues_ShouldBeValid()
		{
			var propertyValues = new PropertyValues { Values = null! };

			var isValid = PropertyValuesValidationHandler.IsValuesValid(propertyValues, out var result);

			isValid.Should().BeTrue();
		}

		[TestMethod]
		public void IsValuesValid_WithEmptyValues_ShouldBeValid()
		{
			var propertyValues = new PropertyValues { Values = new List<PropertyValue>() };

			var isValid = PropertyValuesValidationHandler.IsValuesValid(propertyValues, out var result);

			isValid.Should().BeTrue();
		}

		[TestMethod]
		public void IsValuesValid_WithValidEntries_ShouldBeValid()
		{
			var propertyValues = new PropertyValues
			{
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Owner", Value = "Alice" },
					new PropertyValue { PropertyName = "Region", Value = "EMEA" },
				},
			};

			var isValid = PropertyValuesValidationHandler.IsValuesValid(propertyValues, out var result);

			isValid.Should().BeTrue();
		}

		[TestMethod]
		[DataRow("", DisplayName = "Empty PropertyName")]
		[DataRow("   ", DisplayName = "Whitespace PropertyName")]
		[DataRow(null, DisplayName = "Null PropertyName")]
		public void IsValuesValid_WithMissingPropertyName_ShouldBeInvalid(string propertyName)
		{
			var propertyValues = new PropertyValues
			{
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = propertyName, Value = "Alice" },
				},
			};

			var isValid = PropertyValuesValidationHandler.IsValuesValid(propertyValues, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.Values, out var reason).Should().BeTrue();
				reason.Should().Contain("non-empty Property Name");
			}
		}

		[TestMethod]
		public void IsValuesValid_WithDuplicatePropertyNames_ShouldBeInvalid()
		{
			var propertyValues = new PropertyValues
			{
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Owner", Value = "Alice" },
					new PropertyValue { PropertyName = "owner", Value = "Bob" }, // duplicate, case-insensitive
				},
			};

			var isValid = PropertyValuesValidationHandler.IsValuesValid(propertyValues, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.Values, out var reason).Should().BeTrue();
				reason.Should().Contain("Duplicate Property Name");
			}
		}

		[TestMethod]
		public void IsValuesValid_WithNullEntryInValuesList_ShouldBeInvalid()
		{
			// Corner case: a literal `null` element in the Values list (not just a null PropertyName)
			// must be caught by the same "non-empty Property Name" guard via the `v?.PropertyName` null-check.
			var propertyValues = new PropertyValues
			{
				Values = new List<PropertyValue>
				{
					null!,
					new PropertyValue { PropertyName = "Owner", Value = "Alice" },
				},
			};

			var isValid = PropertyValuesValidationHandler.IsValuesValid(propertyValues, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.Values, out var reason).Should().BeTrue();
				reason.Should().Contain("non-empty Property Name");
			}
		}

		#endregion
	}
}
