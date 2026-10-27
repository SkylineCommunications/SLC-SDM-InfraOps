namespace SDM.InfraOpsProperties.Tests.Extensions
{
	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.InfraOpsProperties.Tests.Setup;

	using Skyline.DataMiner.SDM.InfraOps.Core.Models;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Extensions;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	[PropertyScope("Asset")]
	public sealed class ScopedAssetTestObject : SdmObjectBase<ScopedAssetTestObject>
	{
	}

	[PropertyScope("Facility")]
	public sealed class ScopedFacilityTestObject : SdmObjectBase<ScopedFacilityTestObject>
	{
	}

	public sealed class UnscopedTestObject : SdmObjectBase<UnscopedTestObject>
	{
	}

	/// <summary>
	/// Tests for <see cref="PropertyScopeAttribute"/> and the scope-by-generic overloads of
	/// <see cref="PropertyRepositoryExtensions"/> and <see cref="PropertyValuesRepositoryExtensions"/>.
	/// </summary>
	[TestClass]
	public class PropertyScopeGenericExtensionsTests : BaseRepositoryTest
	{
		[TestInitialize]
		public void GenericExtensionsTestInitialize()
		{
			Helper.PopulateProperties();
			Helper.PopulatePropertyValues();
		}

		#region PropertyScopeAttribute

		[TestMethod]
		[DataRow(null)]
		[DataRow("")]
		[DataRow(" ")]
		public void Attribute_WithInvalidScope_ShouldThrowArgumentException(string? scope)
		{
			Action act = () => new PropertyScopeAttribute(scope!);

			act.Should().Throw<ArgumentException>();
		}

		[TestMethod]
		public void GetScope_WithDecoratedType_ShouldReturnDeclaredScope()
		{
			using (new FluentAssertions.Execution.AssertionScope())
			{
				PropertyScopeAttribute.GetScope<ScopedAssetTestObject>().Should().Be("Asset");
				PropertyScopeAttribute.GetScope<ScopedFacilityTestObject>().Should().Be("Facility");
			}
		}

		[TestMethod]
		public void GetScope_WithoutAttribute_ShouldThrowInvalidOperationException()
		{
			Action act = () => PropertyScopeAttribute.GetScope<UnscopedTestObject>();

			act.Should().Throw<InvalidOperationException>().WithMessage("*UnscopedTestObject*");
		}

		[TestMethod]
		public void GetScope_WithNullType_ShouldThrowArgumentNullException()
		{
			Action act = () => PropertyScopeAttribute.GetScope(null!);

			act.Should().Throw<ArgumentNullException>();
		}

		#endregion

		#region GetProperties / GetProperty

		[TestMethod]
		public void GetProperties_WithAssetScopedType_ShouldReturnOnlyAssetProperties()
		{
			var result = Helper.Properties.GetByScope<ScopedAssetTestObject>().ToList();

			using (new FluentAssertions.Execution.AssertionScope())
			{
				result.Should().HaveCount(3);
				result.Should().OnlyContain(p => p.Scope == "Asset");
			}
		}

		[TestMethod]
		public void GetProperties_WithFacilityScopedType_ShouldReturnOnlyFacilityProperties()
		{
			var result = Helper.Properties.GetByScope<ScopedFacilityTestObject>().ToList();

			result.Should().NotBeEmpty().And.OnlyContain(p => p.Scope == "Facility");
		}

		[TestMethod]
		public void GetProperties_WithoutAttribute_ShouldThrowInvalidOperationException()
		{
			Action act = () => Helper.Properties.GetByScope<UnscopedTestObject>();

			act.Should().Throw<InvalidOperationException>();
		}

		[TestMethod]
		public void GetProperties_WithNullRepository_ShouldThrowArgumentNullException()
		{
			Action act = () => PropertyRepositoryExtensions.GetByScope<ScopedAssetTestObject>(null!);

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void GetProperty_WithExistingName_ShouldReturnProperty()
		{
			var result = Helper.Properties.GetByScopeAndName<ScopedAssetTestObject>("Asset Owner");

			using (new FluentAssertions.Execution.AssertionScope())
			{
				result.Should().NotBeNull();
				result!.Scope.Should().Be("Asset");
				result.Name.Should().Be("Asset Owner");
			}
		}

		[TestMethod]
		public void GetProperty_WithNameFromOtherScope_ShouldReturnNull()
		{
			var facilityProperty = DemoData.Properties.First(p => p.Scope == "Facility");

			var result = Helper.Properties.GetByScopeAndName<ScopedAssetTestObject>(facilityProperty.Name);

			result.Should().BeNull();
		}

		[TestMethod]
		public void GetProperty_WithoutAttribute_ShouldThrowInvalidOperationException()
		{
			Action act = () => Helper.Properties.GetByScopeAndName<UnscopedTestObject>("Asset Owner");

			act.Should().Throw<InvalidOperationException>();
		}

		#endregion

		#region GetPropertyValues / GetSinglePropertyValues

		[TestMethod]
		public void GetPropertyValues_WithScopedObject_ShouldReturnMatch()
		{
			var expected = DemoData.PropertyValuesList[0];
			var owner = new ScopedAssetTestObject { Identifier = expected.LinkedObjectID.ToString() };

			var result = Helper.PropertyValues.GetByLinkedObjectID(owner).ToList();

			using (new FluentAssertions.Execution.AssertionScope())
			{
				result.Should().HaveCount(1);
				result[0].Identifier.Should().Be(expected.Identifier);
			}
		}

		[TestMethod]
		public void GetPropertyValues_WithOtherScopeType_ShouldReturnEmpty()
		{
			var expected = DemoData.PropertyValuesList[0];
			var owner = new ScopedFacilityTestObject { Identifier = expected.LinkedObjectID.ToString() };

			var result = Helper.PropertyValues.GetByLinkedObjectID(owner).ToList();

			result.Should().BeEmpty("the linked object only has PropertyValues in the Asset scope");
		}

		[TestMethod]
		public void GetPropertyValues_WithSubIdAndWildcard_ShouldFollowSubIdSemantics()
		{
			var linkedObjectId = Guid.NewGuid();
			var owner = new ScopedAssetTestObject { Identifier = linkedObjectId.ToString() };
			Helper.PropertyValues.Create(new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset" });
			Helper.PropertyValues.Create(new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset", SubID = "Port1" });

			using (new FluentAssertions.Execution.AssertionScope())
			{
				Helper.PropertyValues.GetByLinkedObjectID(owner).Should().ContainSingle(x => x.SubID == null);
				Helper.PropertyValues.GetByLinkedObjectID(owner, "Port1").Should().ContainSingle(x => x.SubID == "Port1");
				Helper.PropertyValues.GetByLinkedObjectID(owner, "*").Should().HaveCount(2);
			}
		}

		[TestMethod]
		public void GetPropertyValues_WithoutAttribute_ShouldThrowInvalidOperationException()
		{
			var owner = new UnscopedTestObject { Identifier = Guid.NewGuid().ToString() };

			Action act = () => Helper.PropertyValues.GetByLinkedObjectID(owner);

			act.Should().Throw<InvalidOperationException>();
		}

		[TestMethod]
		public void GetPropertyValues_WithNullObject_ShouldThrowArgumentNullException()
		{
			Action act = () => Helper.PropertyValues.GetByLinkedObjectID<ScopedAssetTestObject>(null!);

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void GetPropertyValues_WithNullRepository_ShouldThrowArgumentNullException()
		{
			var owner = new ScopedAssetTestObject { Identifier = Guid.NewGuid().ToString() };

			Action act = () => PropertyValuesRepositoryExtensions.GetByLinkedObjectID(null!, owner);

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void GetSinglePropertyValues_WithScopedObject_ShouldReturnMatchOrNull()
		{
			var expected = DemoData.PropertyValuesList[0];
			var owner = new ScopedAssetTestObject { Identifier = expected.LinkedObjectID.ToString() };
			var unknown = new ScopedAssetTestObject { Identifier = Guid.NewGuid().ToString() };

			using (new FluentAssertions.Execution.AssertionScope())
			{
				Helper.PropertyValues.GetSingleOrDefaultByLinkedObjectID(owner)!.Identifier.Should().Be(expected.Identifier);
				Helper.PropertyValues.GetSingleOrDefaultByLinkedObjectID(unknown).Should().BeNull();
			}
		}

		#endregion

		#region CopyPropertyValues

		[TestMethod]
		public void CopyPropertyValues_WithScopedObjects_ShouldCreateDuplicateForTarget()
		{
			var source = DemoData.PropertyValuesList[0];
			var from = new ScopedAssetTestObject { Identifier = source.LinkedObjectID.ToString() };
			var to = new ScopedAssetTestObject { Identifier = Guid.NewGuid().ToString() };

			var result = Helper.PropertyValues.CopyPropertyValues(from, to);

			using (new FluentAssertions.Execution.AssertionScope())
			{
				result.Should().NotBeNull();
				result!.Scope.Should().Be("Asset");
				result.LinkedObjectID.Should().Be(Guid.Parse(to.Identifier));
				result.Values.Should().HaveCount(source.Values.Count);
			}
		}

		[TestMethod]
		public void CopyPropertyValues_WithoutAttribute_ShouldThrowInvalidOperationException()
		{
			var from = new UnscopedTestObject { Identifier = Guid.NewGuid().ToString() };
			var to = new UnscopedTestObject { Identifier = Guid.NewGuid().ToString() };

			Action act = () => Helper.PropertyValues.CopyPropertyValues(from, to);

			act.Should().Throw<InvalidOperationException>();
		}

		[TestMethod]
		public void CopyPropertyValues_WithNullObjects_ShouldThrowArgumentNullException()
		{
			var owner = new ScopedAssetTestObject { Identifier = Guid.NewGuid().ToString() };

			using (new FluentAssertions.Execution.AssertionScope())
			{
				((Action)(() => Helper.PropertyValues.CopyPropertyValues(null!, owner))).Should().Throw<ArgumentNullException>();
				((Action)(() => Helper.PropertyValues.CopyPropertyValues(owner, null!))).Should().Throw<ArgumentNullException>();
			}
		}

		#endregion
	}
}
