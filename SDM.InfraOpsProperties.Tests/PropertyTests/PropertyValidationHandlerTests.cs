namespace SDM.InfraOpsProperties.Tests.Properties
{
	using System.Collections.Generic;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SharedMappers.DomIds;

	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Validation;

	/// <summary>
	/// Unit tests for Property validation business rules.
	/// Tests the static validation methods in PropertyValidationHandler.
	/// </summary>
	[TestClass]
	public class PropertyValidationHandlerTests
	{
		#region Name Validation

		[TestMethod]
		public void IsNameValid_WithNullProperty_ShouldBeInvalid()
		{
			var isValid = PropertyValidationHandler.IsNameValid(null!, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeFalse();
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValidationHandler.PropertyValidationField.Property, out var reason).Should().BeTrue();
				reason.Should().Contain("cannot be null");
			}
		}

		[TestMethod]
		[DataRow("", DisplayName = "Empty Name")]
		[DataRow("   ", DisplayName = "Whitespace Name")]
		[DataRow(null, DisplayName = "Null Name")]
		public void IsNameValid_WithInvalidName_ShouldBeInvalid(string name)
		{
			var property = new Property { Name = name };

			var isValid = PropertyValidationHandler.IsNameValid(property, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValidationHandler.PropertyValidationField.Name, out var reason).Should().BeTrue();
				reason.Should().Contain("cannot be empty or whitespace");
			}
		}

		[TestMethod]
		public void IsNameValid_WithValidName_ShouldBeValid()
		{
			var property = new Property { Name = "Serial Number" };

			var isValid = PropertyValidationHandler.IsNameValid(property, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeTrue();
				result.IsValid.Should().BeTrue();
			}
		}

		#endregion

		#region Scope Validation

		[TestMethod]
		[DataRow("", DisplayName = "Empty Scope")]
		[DataRow("   ", DisplayName = "Whitespace Scope")]
		[DataRow(null, DisplayName = "Null Scope")]
		public void IsScopeValid_WithInvalidScope_ShouldBeInvalid(string scope)
		{
			var property = new Property { Scope = scope };

			var isValid = PropertyValidationHandler.IsScopeValid(property, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValidationHandler.PropertyValidationField.Scope, out var reason).Should().BeTrue();
				reason.Should().Contain("cannot be empty or whitespace");
			}
		}

		[TestMethod]
		public void IsScopeValid_WithValidScope_ShouldBeValid()
		{
			var property = new Property { Scope = "Asset" };

			var isValid = PropertyValidationHandler.IsScopeValid(property, out var result);

			isValid.Should().BeTrue();
		}

		[TestMethod]
		public void IsScopeValid_WithNullProperty_ShouldBeInvalid()
		{
			var isValid = PropertyValidationHandler.IsScopeValid(null!, out var result);

			isValid.Should().BeFalse();
		}

		#endregion

		#region String Size Limit Validation

		[TestMethod]
		[DataRow(1L, DisplayName = "Minimum Positive Value")]
		[DataRow(1000L, DisplayName = "Large Value")]
		public void IsStringSizeLimitValid_WithPositiveValue_ShouldBeValid(long limit)
		{
			var property = new Property { StringSizeLimit = limit };

			var isValid = PropertyValidationHandler.IsStringSizeLimitValid(property, out var result);

			isValid.Should().BeTrue();
		}

		[TestMethod]
		public void IsStringSizeLimitValid_WithNull_ShouldBeValid()
		{
			var property = new Property { StringSizeLimit = null };

			var isValid = PropertyValidationHandler.IsStringSizeLimitValid(property, out var result);

			isValid.Should().BeTrue();
		}

		[TestMethod]
		[DataRow(0L, DisplayName = "Zero")]
		[DataRow(-1L, DisplayName = "Negative")]
		public void IsStringSizeLimitValid_WithNonPositiveValue_ShouldBeInvalid(long limit)
		{
			var property = new Property { StringSizeLimit = limit };

			var isValid = PropertyValidationHandler.IsStringSizeLimitValid(property, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValidationHandler.PropertyValidationField.StringSizeLimit, out var reason).Should().BeTrue();
				reason.Should().Contain("greater than 0");
			}
		}

		[TestMethod]
		public void IsStringSizeLimitValid_WithNullProperty_ShouldBeInvalid()
		{
			var isValid = PropertyValidationHandler.IsStringSizeLimitValid(null!, out var result);

			isValid.Should().BeFalse();
		}

		#endregion

		#region Options Validation

		[TestMethod]
		public void IsOptionsValid_DiscreteWithOptions_ShouldBeValid()
		{
			var property = new Property
			{
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.Discrete,
				Options = new List<string> { "A", "B" },
			};

			var isValid = PropertyValidationHandler.IsOptionsValid(property, out var result);

			isValid.Should().BeTrue();
		}

		[TestMethod]
		public void IsOptionsValid_DiscreteWithoutOptions_ShouldBeInvalid()
		{
			var property = new Property
			{
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.Discrete,
				Options = new List<string>(),
			};

			var isValid = PropertyValidationHandler.IsOptionsValid(property, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValidationHandler.PropertyValidationField.Options, out var reason).Should().BeTrue();
				reason.Should().Contain("cannot be empty");
			}
		}

		[TestMethod]
		public void IsOptionsValid_NonDiscreteWithOptions_ShouldBeInvalid()
		{
			var property = new Property
			{
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.String,
				Options = new List<string> { "A" },
			};

			var isValid = PropertyValidationHandler.IsOptionsValid(property, out var result);

			using (new AssertionScope())
			{
				isValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValidationHandler.PropertyValidationField.Options, out var reason).Should().BeTrue();
				reason.Should().Contain("must be empty");
			}
		}

		[TestMethod]
		public void IsOptionsValid_NonDiscreteWithoutOptions_ShouldBeValid()
		{
			var property = new Property
			{
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.Boolean,
				Options = new List<string>(),
			};

			var isValid = PropertyValidationHandler.IsOptionsValid(property, out var result);

			isValid.Should().BeTrue();
		}

		[TestMethod]
		public void IsOptionsValid_WithNullProperty_ShouldBeInvalid()
		{
			var isValid = PropertyValidationHandler.IsOptionsValid(null!, out var result);

			isValid.Should().BeFalse();
		}

		[TestMethod]
		public void IsOptionsValid_DiscreteWithDuplicateOptions_ShouldBeValid_KnownGap()
		{
			// Corner case: IsOptionsValid only checks that Options is non-empty for Discrete
			// properties - it does NOT check uniqueness. Duplicate option strings (e.g. "Low"
			// twice) currently pass validation. This test pins the current behavior; if
			// uniqueness enforcement is later added, this test should be updated to expect BeFalse().
			var property = new Property
			{
				PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.Discrete,
				Options = new List<string> { "Low", "Low", "High" },
			};

			var isValid = PropertyValidationHandler.IsOptionsValid(property, out var result);

			isValid.Should().BeTrue();
		}

		#endregion
	}
}
