namespace SDM.InfraOpsProperties.Tests.PropertyValuesTests
{
	using System;
	using System.Collections.Generic;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.InfraOpsProperties.Tests.Setup;

	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Validation;
	using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

	/// <summary>
	/// Tests for PropertyValuesValidator which validates PropertyValues business rules.
	/// </summary>
	[TestClass]
	public class PropertyValuesValidatorTests : BaseRepositoryTest
	{
		private PropertyValuesValidator _validator = null!;

		[TestInitialize]
		public void Setup()
		{
			_validator = new PropertyValuesValidator(Helper);
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

			var result = _validator.Validate(propertyValues, RepositoryAction.Create);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeTrue();
				result.FailureReasons.Should().BeEmpty();
			}
		}

		[TestMethod]
		public void Validate_WithNullPropertyValues_ShouldThrowArgumentNullException()
		{
			_validator.Invoking(v => v.Validate(null!, RepositoryAction.Create))
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

			var result = _validator.Validate(propertyValues, RepositoryAction.Create);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.LinkedObjectID, out var reason).Should().BeTrue();
				reason.Should().Be("PropertyValues Linked Object ID cannot be empty.");
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

			var result = _validator.Validate(propertyValues, RepositoryAction.Create);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.Scope, out var reason).Should().BeTrue();
				reason.Should().Be("PropertyValues Scope cannot be empty or whitespace.");
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

			var result = _validator.Validate(propertyValues, RepositoryAction.Create);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.Values, out var reason).Should().BeTrue();
				reason.Should().Be("Duplicate Property Name(s) found in Values: Owner.");
			}
		}

		[TestMethod]
		public void Validate_WithMissingPropertyNameInValues_ShouldReturnExactFailureMessage()
		{
			var propertyValues = new PropertyValues
			{
				LinkedObjectID = Guid.NewGuid(),
				Scope = "Asset",
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = string.Empty, Value = "Alice" },
				},
			};

			var result = _validator.Validate(propertyValues, RepositoryAction.Create);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.Values, out var reason).Should().BeTrue();
				reason.Should().Be("Every entry in Values must have a non-empty Property Name.");
			}
		}

		#endregion

		#region Property References

		[TestMethod]
		public void Validate_WithExistingPropertyReference_ShouldReturnValid()
		{
			var property = Helper.Properties.Create(new Property { Name = "Owner", Scope = "Asset" });
			var propertyValues = new PropertyValues
			{
				LinkedObjectID = Guid.NewGuid(),
				Scope = "Asset",
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Owner", Value = "Alice", PropertyId = new SdmObjectReference<Property>(property.Identifier) },
				},
			};

			var result = _validator.Validate(propertyValues, RepositoryAction.Create);

			result.IsValid.Should().BeTrue();
		}

		[TestMethod]
		public void Validate_WithUnknownPropertyReference_ShouldReturnInvalid()
		{
			var propertyId = Guid.NewGuid().ToString();
			var propertyValues = new PropertyValues
			{
				LinkedObjectID = Guid.NewGuid(),
				Scope = "Asset",
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Owner", Value = "Alice", PropertyId = new SdmObjectReference<Property>(propertyId) },
				},
			};

			var result = _validator.Validate(propertyValues, RepositoryAction.Create);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.Values, out var reason).Should().BeTrue();
				reason.Should().Be($"Referenced Property '{propertyId}' does not exist.");
			}
		}

		[TestMethod]
		public void Validate_WithNullPropertyReference_ShouldReturnValid()
		{
			var propertyValues = new PropertyValues
			{
				LinkedObjectID = Guid.NewGuid(),
				Scope = "Asset",
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Owner", Value = "Alice", PropertyId = null },
				},
			};

			var result = _validator.Validate(propertyValues, RepositoryAction.Create);

			result.IsValid.Should().BeTrue();
		}

		[TestMethod]
		public void ValidateBulk_WithUnknownPropertyReference_ShouldFlagOnlyThatEntry()
		{
			var property = Helper.Properties.Create(new Property { Name = "Owner", Scope = "Asset" });
			var unknownPropertyId = Guid.NewGuid().ToString();
			var propertyValuesList = new List<PropertyValues>
			{
				new PropertyValues
				{
					LinkedObjectID = Guid.NewGuid(),
					Scope = "Asset",
					Values = new List<PropertyValue> { new PropertyValue { PropertyName = "Owner", PropertyId = new SdmObjectReference<Property>(unknownPropertyId) } },
				},
				new PropertyValues
				{
					LinkedObjectID = Guid.NewGuid(),
					Scope = "Asset",
					Values = new List<PropertyValue> { new PropertyValue { PropertyName = "Owner", PropertyId = new SdmObjectReference<Property>(property.Identifier) } },
				},
			};

			var results = _validator.ValidateBulk(propertyValuesList, RepositoryAction.Create);

			using (new AssertionScope())
			{
				results.Should().HaveCount(2);
				results[0].IsValid.Should().BeFalse();
				results[0].TryGetFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.Values, out var reason).Should().BeTrue();
				reason.Should().Be($"Referenced Property '{unknownPropertyId}' does not exist.");
				results[1].IsValid.Should().BeTrue();
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

			var result = _validator.Validate(propertyValues, RepositoryAction.Create);

			result.IsValid.Should().BeTrue("Scope error should not be reported since it wasn't changed after the reset");
		}

		#endregion

		#region (LinkedObjectID, Scope, SubID) Uniqueness

		[TestMethod]
		public void Validate_WithDuplicateComboNoSubID_ShouldReturnInvalid()
		{
			var linkedObjectId = Guid.NewGuid();
			Helper.PropertyValues.Create(new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset" });

			var newPropertyValues = new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset" };

			var result = _validator.Validate(newPropertyValues, RepositoryAction.Create);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse();
				result.TryGetFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.PropertyValues, out var reason).Should().BeTrue();
				reason.Should().Be($"PropertyValues for Linked Object '{linkedObjectId}', Scope 'Asset' (no SubID) already exist.");
			}
		}

		[TestMethod]
		public void Validate_WithDuplicateComboSameSubID_ShouldReturnInvalid()
		{
			var linkedObjectId = Guid.NewGuid();
			Helper.PropertyValues.Create(new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset", SubID = "Port1" });

			var newPropertyValues = new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset", SubID = "Port1" };

			var result = _validator.Validate(newPropertyValues, RepositoryAction.Create);

			using (new AssertionScope())
			{
				result.IsValid.Should().BeFalse("a PropertyValues row already exists for this (LinkedObjectID, Scope, SubID) combo");
				result.TryGetFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.PropertyValues, out var reason).Should().BeTrue();
				reason.Should().Be($"PropertyValues for Linked Object '{linkedObjectId}', Scope 'Asset', SubID 'Port1' already exist.");
			}
		}

		[TestMethod]
		public void Validate_WithNoSubIDAndExistingSpecificSubID_ShouldReturnValid()
		{
			// A "no SubID" PropertyValues and a "SubID = Port1" PropertyValues for the same LinkedObjectID/Scope
			// are distinct buckets, matching the legacy behavior (KeyExists(SubID) == false is a separate filter
			// branch from an exact SubID match).
			var linkedObjectId = Guid.NewGuid();
			Helper.PropertyValues.Create(new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset", SubID = "Port1" });

			var newPropertyValues = new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset" };

			var result = _validator.Validate(newPropertyValues, RepositoryAction.Create);

			result.IsValid.Should().BeTrue("a PropertyValues without a SubID does not conflict with one that has a specific SubID");
		}

		[TestMethod]
		public void Validate_WithDifferentSubID_ShouldReturnValid()
		{
			var linkedObjectId = Guid.NewGuid();
			Helper.PropertyValues.Create(new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset", SubID = "Port1" });

			var newPropertyValues = new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset", SubID = "Port2" };

			var result = _validator.Validate(newPropertyValues, RepositoryAction.Create);

			result.IsValid.Should().BeTrue();
		}

		[TestMethod]
		public void Validate_WithSameComboDifferentScope_ShouldReturnValid()
		{
			var linkedObjectId = Guid.NewGuid();
			Helper.PropertyValues.Create(new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset" });

			var newPropertyValues = new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Facility" };

			var result = _validator.Validate(newPropertyValues, RepositoryAction.Create);

			result.IsValid.Should().BeTrue("(LinkedObjectID, Scope, SubID) is the natural key - Scope differs so no conflict");
		}

		[TestMethod]
		public void Validate_WithSameComboDifferentLinkedObjectID_ShouldReturnValid()
		{
			Helper.PropertyValues.Create(new PropertyValues { LinkedObjectID = Guid.NewGuid(), Scope = "Asset" });

			var newPropertyValues = new PropertyValues { LinkedObjectID = Guid.NewGuid(), Scope = "Asset" };

			var result = _validator.Validate(newPropertyValues, RepositoryAction.Create);

			result.IsValid.Should().BeTrue();
		}

		[TestMethod]
		public void Validate_ExistingPropertyValuesUnchanged_ShouldNotConflictWithItself()
		{
			var created = Helper.PropertyValues.Create(new PropertyValues { LinkedObjectID = Guid.NewGuid(), Scope = "Asset", SubID = "Port1" });

			var result = _validator.Validate(created, RepositoryAction.Create);

			result.IsValid.Should().BeTrue("the uniqueness check must exclude the PropertyValues' own identifier");
		}

		#endregion

		#region ValidateBulk

		[TestMethod]
		public void ValidateBulk_WithNullList_ShouldReturnEmptyResults()
		{
			var results = _validator.ValidateBulk(null!, RepositoryAction.Create);

			results.Should().BeEmpty();
		}

		[TestMethod]
		public void ValidateBulk_WithEmptyList_ShouldReturnEmptyResults()
		{
			var results = _validator.ValidateBulk(new List<PropertyValues>(), RepositoryAction.Create);

			results.Should().BeEmpty();
		}

		[TestMethod]
		public void ValidateBulk_WithAllValidEntries_ShouldReturnAllValid()
		{
			var propertyValuesList = new List<PropertyValues>
			{
				new PropertyValues { LinkedObjectID = Guid.NewGuid(), Scope = "Asset" },
				new PropertyValues { LinkedObjectID = Guid.NewGuid(), Scope = "Asset" },
			};

			var results = _validator.ValidateBulk(propertyValuesList, RepositoryAction.Create);

			using (new AssertionScope())
			{
				results.Should().HaveCount(2);
				results.Should().OnlyContain(r => r.IsValid);
			}
		}

		[TestMethod]
		public void ValidateBulk_WithDuplicateComboWithinBatch_ShouldFlagBothAsInvalid()
		{
			// Two entries being created together share (LinkedObjectID, Scope, SubID). Neither is persisted yet,
			// so a single-entry DB uniqueness query alone would miss this - the in-memory batch check must catch it.
			var linkedObjectId = Guid.NewGuid();
			var propertyValuesList = new List<PropertyValues>
			{
				new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset", SubID = "Port1" },
				new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset", SubID = "Port1" },
			};

			var results = _validator.ValidateBulk(propertyValuesList, RepositoryAction.Create);

			using (new AssertionScope())
			{
				results.Should().HaveCount(2);
				results[0].IsValid.Should().BeFalse();
				results[1].IsValid.Should().BeFalse();
				results[0].TryGetFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.PropertyValues, out var reason0).Should().BeTrue();
				reason0.Should().Be($"PropertyValues for Linked Object '{linkedObjectId}', Scope 'Asset', SubID 'Port1' is duplicated within the validation batch.");
				results[1].TryGetFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.PropertyValues, out var reason1).Should().BeTrue();
				reason1.Should().Be($"PropertyValues for Linked Object '{linkedObjectId}', Scope 'Asset', SubID 'Port1' is duplicated within the validation batch.");
			}
		}

		[TestMethod]
		public void ValidateBulk_WithDuplicateLinkedObjectAndScopeButDifferentSubID_ShouldNotFlagBatchConflict()
		{
			var linkedObjectId = Guid.NewGuid();
			var propertyValuesList = new List<PropertyValues>
			{
				new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset", SubID = "Port1" },
				new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset", SubID = "Port2" },
			};

			var results = _validator.ValidateBulk(propertyValuesList, RepositoryAction.Create);

			results.Should().OnlyContain(r => r.IsValid);
		}

		[TestMethod]
		public void ValidateBulk_WithNoSubIDAndSpecificSubIDWithinBatch_ShouldNotFlagBatchConflict()
		{
			var linkedObjectId = Guid.NewGuid();
			var propertyValuesList = new List<PropertyValues>
			{
				new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset" },
				new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset", SubID = "Port1" },
			};

			var results = _validator.ValidateBulk(propertyValuesList, RepositoryAction.Create);

			results.Should().OnlyContain(r => r.IsValid);
		}

		[TestMethod]
		public void ValidateBulk_WithBatchComboDuplicatingExistingDomEntry_ShouldFlagOnlyThatEntry()
		{
			// One of the batch entries collides with an already-persisted PropertyValues (DB check), while the
			// batch itself has no in-memory duplicates - only the DB-colliding entry should be invalid.
			var linkedObjectId = Guid.NewGuid();
			Helper.PropertyValues.Create(new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset" });

			var propertyValuesList = new List<PropertyValues>
			{
				new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset" },
				new PropertyValues { LinkedObjectID = Guid.NewGuid(), Scope = "Asset" },
			};

			var results = _validator.ValidateBulk(propertyValuesList, RepositoryAction.Create);

			using (new AssertionScope())
			{
				results.Should().HaveCount(2);
				results[0].IsValid.Should().BeFalse();
				results[0].TryGetFailReason(PropertyValuesValidationHandler.PropertyValuesValidationField.PropertyValues, out var reason).Should().BeTrue();
				reason.Should().Be($"PropertyValues for Linked Object '{linkedObjectId}', Scope 'Asset' (no SubID) already exist.");
				results[1].IsValid.Should().BeTrue();
			}
		}

		#endregion

		#region ValidateBatchConflicts

		[TestMethod]
		public void ValidateBatchConflicts_WithDuplicateCombo_ShouldFlagBothEntries()
		{
			var linkedObjectId = Guid.NewGuid();
			var propertyValuesList = new List<PropertyValues>
			{
				new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset" },
				new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset" },
				new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset", SubID = "Port1" },
			};

			var results = _validator.ValidateBatchConflicts(propertyValuesList);

			using (new AssertionScope())
			{
				results.Should().HaveCount(3);
				results[0].IsValid.Should().BeFalse();
				results[1].IsValid.Should().BeFalse();
				results[2].IsValid.Should().BeTrue();
			}
		}

		[TestMethod]
		public void ValidateBatchConflicts_WithEmptyLinkedObjectIDOrScope_ShouldIgnoreThem()
		{
			// Missing key parts are covered by the info/presence check, not the batch-duplicate check.
			var propertyValuesList = new List<PropertyValues>
			{
				new PropertyValues { LinkedObjectID = Guid.Empty, Scope = "Asset" },
				new PropertyValues { LinkedObjectID = Guid.NewGuid(), Scope = string.Empty },
			};

			var results = _validator.ValidateBatchConflicts(propertyValuesList);

			results.Should().OnlyContain(r => r.IsValid);
		}

		#endregion
	}
}
