namespace SDM.InfraOpsProperties.Tests.Properties
{
	using System;
	using System.Collections.Generic;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.InfraOpsProperties.Tests.Setup;

	using SharedMappers.DomIds;

	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Validation;

	/// <summary>
	/// Tests for PropertyValidator which validates Property business rules.
	/// </summary>
	[TestClass]
	public class PropertyValidatorTests : BaseRepositoryTest
	{
		private PropertyValidator _validator = null!;

		[TestInitialize]
		public void Setup()
		{
			_validator = new PropertyValidator(Helper);
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
				Discreets = new List<PropertyOption>(),
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
				Discreets = new List<PropertyOption> { new PropertyOption { Option = "Low" }, new PropertyOption { Option = "High" } },
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
				result.TryGetFailReason(PropertyValidationHandler.PropertyValidationField.Discreets, out var reason).Should().BeTrue();
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

		#region Name Uniqueness

		[TestMethod]
		public void Validate_WithDuplicateNameInSameScope_ShouldReturnInvalid()
		{
			Helper.Properties.Create(new Property { Name = "Serial Number", Scope = "Asset" });

			var newProperty = new Property { Name = "Serial Number", Scope = "Asset" };

			var result = _validator.Validate(newProperty);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValidationHandler.PropertyValidationField.Name, out var reason).Should().BeTrue();
				reason.Should().Contain("already in use");
			}
		}

		[TestMethod]
		public void Validate_WithSameNameInDifferentScope_ShouldReturnValid()
		{
			// (Scope, Name) is the natural key - the same Name may be reused across different Scopes.
			Helper.Properties.Create(new Property { Name = "Description", Scope = "Asset" });

			var newProperty = new Property { Name = "Description", Scope = "Facility" };

			var result = _validator.Validate(newProperty);

			result.IsValid.Should().BeTrue();
		}

		[TestMethod]
		public void Validate_ExistingPropertyUnchanged_ShouldNotConflictWithItself()
		{
			var created = Helper.Properties.Create(new Property { Name = "Serial Number", Scope = "Asset" });

			var result = _validator.Validate(created);

			result.IsValid.Should().BeTrue("the uniqueness check must exclude the Property's own identifier");
		}

		#endregion

		#region ValidateBulk

		[TestMethod]
		public void ValidateBulk_WithNullList_ShouldReturnEmptyResults()
		{
			var results = _validator.ValidateBulk(null!);

			results.Should().BeEmpty();
		}

		[TestMethod]
		public void ValidateBulk_WithEmptyList_ShouldReturnEmptyResults()
		{
			var results = _validator.ValidateBulk(new List<Property>());

			results.Should().BeEmpty();
		}

		[TestMethod]
		public void ValidateBulk_WithAllValidProperties_ShouldReturnAllValid()
		{
			var properties = new List<Property>
			{
				new Property { Name = "Property One", Scope = "Asset" },
				new Property { Name = "Property Two", Scope = "Asset" },
			};

			var results = _validator.ValidateBulk(properties);

			using (new AssertionScope())
			{
				results.Should().HaveCount(2);
				results.Should().OnlyContain(r => r.IsValid);
			}
		}

		[TestMethod]
		public void ValidateBulk_WithDuplicateScopeAndNameWithinBatch_ShouldFlagBothAsInvalid()
		{
			// Two Properties being created together share (Scope, Name). Neither is persisted yet, so a
			// single-Property DB uniqueness query alone would miss this - the in-memory batch check must catch it.
			var properties = new List<Property>
			{
				new Property { Name = "Serial Number", Scope = "Asset" },
				new Property { Name = "Serial Number", Scope = "Asset" },
			};

			var results = _validator.ValidateBulk(properties);

			using (new AssertionScope())
			{
				results.Should().HaveCount(2);
				results[0].IsValid.Should().BeFalse();
				results[1].IsValid.Should().BeFalse();
				results[0].TryGetFailReason(PropertyValidationHandler.PropertyValidationField.Name, out var reason0).Should().BeTrue();
				reason0.Should().Contain("duplicated within the validation batch");
				results[1].TryGetFailReason(PropertyValidationHandler.PropertyValidationField.Name, out var reason1).Should().BeTrue();
				reason1.Should().Contain("duplicated within the validation batch");
			}
		}

		[TestMethod]
		public void ValidateBulk_WithSameNameDifferentScopeWithinBatch_ShouldNotFlagBatchConflict()
		{
			var properties = new List<Property>
			{
				new Property { Name = "Description", Scope = "Asset" },
				new Property { Name = "Description", Scope = "Facility" },
			};

			var results = _validator.ValidateBulk(properties);

			results.Should().OnlyContain(r => r.IsValid);
		}

		[TestMethod]
		public void ValidateBulk_WithDuplicateNamesDifferentCasing_ShouldFlagBothAsInvalid()
		{
			var properties = new List<Property>
			{
				new Property { Name = "Serial Number", Scope = "Asset" },
				new Property { Name = "SERIAL NUMBER", Scope = "ASSET" },
			};

			var results = _validator.ValidateBulk(properties);

			results.Should().OnlyContain(r => !r.IsValid);
		}

		[TestMethod]
		public void ValidateBulk_WithBatchNameDuplicatingExistingDomProperty_ShouldFlagOnlyThatEntry()
		{
			// One of the batch entries collides with an already-persisted Property (DB check), while the batch
			// itself has no in-memory duplicates - only the DB-colliding entry should be invalid.
			Helper.Properties.Create(new Property { Name = "Serial Number", Scope = "Asset" });

			var properties = new List<Property>
			{
				new Property { Name = "Serial Number", Scope = "Asset" },
				new Property { Name = "Brand New Property", Scope = "Asset" },
			};

			var results = _validator.ValidateBulk(properties);

			using (new AssertionScope())
			{
				results.Should().HaveCount(2);
				results[0].IsValid.Should().BeFalse();
				results[0].TryGetFailReason(PropertyValidationHandler.PropertyValidationField.Name, out var reason).Should().BeTrue();
				reason.Should().Contain("already in use");
				results[1].IsValid.Should().BeTrue();
			}
		}

		#endregion

		#region ValidateBatchConflicts

		[TestMethod]
		public void ValidateBatchConflicts_WithDuplicateScopeAndName_ShouldFlagBothEntries()
		{
			var properties = new List<Property>
			{
				new Property { Name = "Duplicate Name", Scope = "Asset" },
				new Property { Name = "Duplicate Name", Scope = "Asset" },
				new Property { Name = "Unique Name", Scope = "Asset" },
			};

			var results = _validator.ValidateBatchConflicts(properties);

			using (new AssertionScope())
			{
				results.Should().HaveCount(3);
				results[0].IsValid.Should().BeFalse();
				results[1].IsValid.Should().BeFalse();
				results[2].IsValid.Should().BeTrue();
			}
		}

		[TestMethod]
		public void ValidateBatchConflicts_WithBlankNameOrScope_ShouldIgnoreThem()
		{
			// Blank names/scopes are covered by the info/presence check, not the batch-duplicate check.
			var properties = new List<Property>
			{
				new Property { Name = string.Empty, Scope = "Asset" },
				new Property { Name = "Some Name", Scope = string.Empty },
			};

			var results = _validator.ValidateBatchConflicts(properties);

			results.Should().OnlyContain(r => r.IsValid);
		}

		#endregion
	}
}
