namespace SDM.InfraOpsProperties.Tests.Extensions
{
	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.SDM.InfraOpsProperties.Extensions;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	/// <summary>
	/// Tests for <see cref="PropertyValuesExtensions"/>.
	/// </summary>
	[TestClass]
	public class PropertyValuesExtensionsTests
	{
		[TestMethod]
		public void GetPropertyValue_ExistingProperty_ShouldReturnMatchingValue()
		{
			var property = new Property { Identifier = Guid.NewGuid().ToString(), Name = "Owner" };
			var reference = new Skyline.DataMiner.SDM.SdmObjectReference<Property>(property.Identifier);
			var propertyValue = new PropertyValue { PropertyName = "Owner", Value = "Alice", PropertyId = reference };
			var source = new PropertyValues { Values = new List<PropertyValue> { propertyValue } };

			var found = source.GetPropertyValue(property);

			found.Should().Be(propertyValue);
		}

		[TestMethod]
		public void GetPropertyValue_NoMatch_ShouldReturnNull()
		{
			var property = new Property { Name = "Owner" };
			var source = new PropertyValues { Values = new List<PropertyValue>() };

			var found = source.GetPropertyValue(property);

			found.Should().BeNull();
		}

		[TestMethod]
		public void GetPropertyValue_NullProperty_ShouldThrow()
		{
			var source = new PropertyValues();

			Action act = () => source.GetPropertyValue(null);

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void AddPropertyValue_NewValue_ShouldBeAdded()
		{
			var source = new PropertyValues();
			var propertyValue = new PropertyValue { PropertyName = "Owner", Value = "Alice", PropertyId = new Skyline.DataMiner.SDM.SdmObjectReference<Property>(Guid.NewGuid().ToString()) };

			source.AddPropertyValue(propertyValue);

			source.Values.Should().ContainSingle().Which.Should().Be(propertyValue);
		}

		[TestMethod]
		public void AddPropertyValue_Null_ShouldThrow()
		{
			var source = new PropertyValues();

			Action act = () => source.AddPropertyValue(null);

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void AddPropertyValue_DuplicatePropertyId_ShouldThrow()
		{
			var source = new PropertyValues();
			var reference = new Skyline.DataMiner.SDM.SdmObjectReference<Property>(Guid.NewGuid().ToString());
			source.AddPropertyValue(new PropertyValue { PropertyName = "Owner", Value = "Alice", PropertyId = reference });

			Action act = () => source.AddPropertyValue(new PropertyValue { PropertyName = "Owner", Value = "Bob", PropertyId = reference });

			act.Should().Throw<InvalidOperationException>();
		}

		[TestMethod]
		public void RemovePropertyValue_ExistingValue_ShouldBeRemoved()
		{
			var source = new PropertyValues();
			var propertyValue = new PropertyValue { PropertyName = "Owner", Value = "Alice", PropertyId = new Skyline.DataMiner.SDM.SdmObjectReference<Property>(Guid.NewGuid().ToString()) };
			source.AddPropertyValue(propertyValue);

			source.RemovePropertyValue(propertyValue);

			source.Values.Should().BeEmpty();
		}

		[TestMethod]
		public void RemovePropertyValue_Null_ShouldThrow()
		{
			var source = new PropertyValues();

			Action act = () => source.RemovePropertyValue(null);

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void RemovePropertyValue_NotFound_ShouldThrow()
		{
			var source = new PropertyValues();
			var propertyValue = new PropertyValue { PropertyName = "Owner", Value = "Alice", PropertyId = new Skyline.DataMiner.SDM.SdmObjectReference<Property>(Guid.NewGuid().ToString()) };

			Action act = () => source.RemovePropertyValue(propertyValue);

			act.Should().Throw<ArgumentException>();
		}

		[TestMethod]
		public void Duplicate_WithValidSource_ShouldCopyScopeAndValuesToNewLinkedObject()
		{
			var propertyIdentifier = Guid.NewGuid().ToString();
			var source = new PropertyValues
			{
				LinkedObjectID = Guid.NewGuid(),
				Scope = "Asset",
				SubID = "Source-Sub",
				Values = new List<PropertyValue>
				{
					new PropertyValue { PropertyName = "Owner", Value = "John", PropertyId = new Skyline.DataMiner.SDM.SdmObjectReference<Property>(propertyIdentifier) },
				},
			};

			var targetId = Guid.NewGuid();
			var duplicate = source.Duplicate(targetId, "Target-Sub");

			using (new FluentAssertions.Execution.AssertionScope())
			{
				duplicate.LinkedObjectID.Should().Be(targetId);
				duplicate.Scope.Should().Be(source.Scope);
				duplicate.SubID.Should().Be("Target-Sub");
				duplicate.Values.Should().HaveCount(1);
				duplicate.Values[0].PropertyName.Should().Be("Owner");
				duplicate.Values[0].Value.Should().Be("John");
				duplicate.Values[0].PropertyId!.Identifier.Should().Be(propertyIdentifier);
				duplicate.IsNew.Should().BeTrue("Duplicate should return a new, unsaved instance");
			}
		}

		[TestMethod]
		public void Duplicate_WithNullValuesInSource_ShouldSkipThem()
		{
			var source = new PropertyValues
			{
				LinkedObjectID = Guid.NewGuid(),
				Scope = "Asset",
				Values = new List<PropertyValue> { null!, new PropertyValue { PropertyName = "Owner", Value = "John" } },
			};

			var duplicate = source.Duplicate(Guid.NewGuid());

			duplicate.Values.Should().HaveCount(1);
		}

		[TestMethod]
		public void Duplicate_WithNullSource_ShouldThrowArgumentNullException()
		{
			Action act = () => PropertyValuesExtensions.Duplicate(null!, Guid.NewGuid());

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void Duplicate_WithEmptyLinkedObjectId_ShouldThrowArgumentException()
		{
			var source = new PropertyValues { LinkedObjectID = Guid.NewGuid(), Scope = "Asset" };

			Action act = () => source.Duplicate(Guid.Empty);

			act.Should().Throw<ArgumentException>();
		}
	}
}
