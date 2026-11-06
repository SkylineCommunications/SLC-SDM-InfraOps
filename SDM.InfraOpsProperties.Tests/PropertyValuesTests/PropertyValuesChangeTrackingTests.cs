namespace SDM.InfraOpsProperties.Tests.PropertyValuesTests
{
	using System;
	using System.Collections.Generic;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	/// <summary>
	/// Tests verifying change tracking in <see cref="PropertyValues.Values"/>, including in-place
	/// item mutation, item replacement, item addition, and field reset.
	/// </summary>
	[TestClass]
	public class PropertyValuesChangeTrackingTests
	{
		[TestMethod]
		public void Changed_WhenExistingValueEntryMutatedInPlace_ShouldBeDetectedAndRevertedOnReset()
		{
			var propertyValues = new PropertyValues
			{
				Identifier = Guid.NewGuid().ToString(),
				LinkedObjectID = Guid.NewGuid(),
				Scope = "Asset",
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Owner", Value = "Alice" },
				},
			};

			propertyValues.ResetChangeTracking();
			propertyValues.IsNewInternal = false;

			// Mutate the existing PropertyValue instance in place, rather than replacing it.
			propertyValues.Values[0].Value = "Bob";

			using (new AssertionScope())
			{
				propertyValues.Changed.Should().BeTrue("in-place mutation of a ChangeTrackingBase list entry is tracked by ChangeTrackingArrayField");
				var change = propertyValues.GetChanges().Should().ContainSingle(c => c.FieldName == nameof(PropertyValues.Values)).Subject;
				((List<PropertyValue>)change.OldValue)[0].Value.Should().Be("Alice");
				((List<PropertyValue>)change.NewValue)[0].Value.Should().Be("Bob");
			}

			Skyline.DataMiner.Utils.InfraOps.Common.Fields.SdmChangeTracking.RevertFieldChanges(propertyValues);

			using (new AssertionScope())
			{
				propertyValues.Changed.Should().BeFalse();
				propertyValues.Values[0].Value.Should().Be("Alice");
			}
		}

		[TestMethod]
		public void Changed_WhenValueEntryReplacedWithNewInstance_ShouldBeDetected()
		{
			var propertyValues = new PropertyValues
			{
				Identifier = Guid.NewGuid().ToString(),
				LinkedObjectID = Guid.NewGuid(),
				Scope = "Asset",
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Owner", Value = "Alice" },
				},
			};

			propertyValues.ResetChangeTracking();
			propertyValues.IsNewInternal = false;

			// Replace the entry with a new PropertyValue instance carrying different data.
			propertyValues.Values[0] = new PropertyValue { PropertyName = "Owner", Value = "Bob" };

			propertyValues.Changed.Should().BeTrue("replacing a list entry with a different object is detected via value-equality comparison");
		}

		[TestMethod]
		public void Changed_WhenValueEntryAdded_ShouldBeDetected()
		{
			var propertyValues = new PropertyValues
			{
				Identifier = Guid.NewGuid().ToString(),
				LinkedObjectID = Guid.NewGuid(),
				Scope = "Asset",
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Owner", Value = "Alice" },
				},
			};

			propertyValues.ResetChangeTracking();
			propertyValues.IsNewInternal = false;

			propertyValues.Values.Add(new PropertyValue { PropertyName = "Region", Value = "EMEA" });

			propertyValues.Changed.Should().BeTrue("adding an entry changes the list count, which is detected");
		}

		[TestMethod]
		public void LinkedObjectIDField_WhenUnsetExistingEntityExplicitlySetToEmptyGuid_ShouldMarkChangedAndRevertOnReset()
		{
			var propertyValues = new PropertyValues
			{
				Identifier = Guid.NewGuid().ToString(),
				Scope = "Asset",
			};
			propertyValues.ResetChangeTracking();
			propertyValues.IsNewInternal = false;

			propertyValues.LinkedObjectIDField.Changed.Should().BeFalse();

			propertyValues.LinkedObjectID = Guid.Empty;

			propertyValues.LinkedObjectIDField.Changed.Should().BeTrue();
			propertyValues.Changed.Should().BeTrue();

			Skyline.DataMiner.Utils.InfraOps.Common.Fields.SdmChangeTracking.RevertFieldChanges(propertyValues);

			propertyValues.LinkedObjectIDField.Changed.Should().BeFalse();
			propertyValues.Changed.Should().BeFalse();
		}
	}
}
