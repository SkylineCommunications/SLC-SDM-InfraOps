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
