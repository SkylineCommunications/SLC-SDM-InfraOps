namespace SDM.InfraOpsProperties.Tests.PropertyValuesTests
{
	using System;
	using System.Collections.Generic;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	/// <summary>
	/// Corner-case tests documenting the change-tracking blind spot in
	/// <see cref="PropertyValues.Values"/>: <c>ChangeTrackingArrayField&lt;T&gt;</c> snapshots the list
	/// shallowly (same item references in both the original and current snapshot), so mutating an
	/// existing <see cref="PropertyValue"/> instance's properties in place is invisible to
	/// <c>ValuesField.Changed</c> / <see cref="PropertyValues.Changed"/>. Only replacing an entry with a
	/// different object (or adding/removing entries) is detected.
	/// If this ever changes (e.g. array field snapshotting becomes a deep clone), these tests should be
	/// updated to expect <c>Changed == true</c> for in-place mutation.
	/// </summary>
	[TestClass]
	public class PropertyValuesChangeTrackingTests
	{
		[TestMethod]
		public void Changed_WhenExistingValueEntryMutatedInPlace_ShouldNotBeDetected_KnownGap()
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
				propertyValues.Changed.Should().BeFalse("in-place mutation of a list entry is not tracked by ChangeTrackingArrayField");
				propertyValues.Values[0].Value.Should().Be("Bob", "the underlying data is still mutated even though tracking misses it");
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
	}
}
