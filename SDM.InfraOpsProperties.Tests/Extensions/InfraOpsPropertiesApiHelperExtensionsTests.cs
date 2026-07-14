namespace SDM.InfraOpsProperties.Tests.Extensions
{
	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.InfraOpsProperties.Tests.Setup;

	using Skyline.DataMiner.SDM.InfraOpsProperties.Extensions;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	/// <summary>
	/// Tests for <see cref="InfraOpsPropertiesApiHelperExtensions"/>.
	/// </summary>
	[TestClass]
	public class InfraOpsPropertiesApiHelperExtensionsTests : BaseRepositoryTest
	{
		[TestInitialize]
		public void ExtensionsTestInitialize()
		{
			Helper.PopulateProperties();
			Helper.PopulatePropertyValues();
		}

		[TestMethod]
		public void DeletePropertyWithCascade_RemovesReferencingValuesAndDeletesProperty()
		{
			// "Asset Owner" (index 0) is referenced by PropertyValuesList[0] and [1], each also referencing "Criticality" (index 2).
			var assetOwnerProperty = Helper.Properties.GetByScopeAndName("Asset", "Asset Owner")!;

			Helper.DeletePropertyWithCascade(assetOwnerProperty);

			var remainingProperty = Helper.Properties.GetByScopeAndName("Asset", "Asset Owner");
			var affectedValues = Helper.PropertyValues.GetByLinkedObjectID(DemoData.PropertyValuesList[0].LinkedObjectID, "Asset").Single();
			var otherAffectedValues = Helper.PropertyValues.GetByLinkedObjectID(DemoData.PropertyValuesList[1].LinkedObjectID, "Asset").Single();

			using (new FluentAssertions.Execution.AssertionScope())
			{
				remainingProperty.Should().BeNull("the property itself should have been deleted");
				affectedValues.Values.Should().NotContain(v => v.PropertyName == "Asset Owner");
				affectedValues.Values.Should().Contain(v => v.PropertyName == "Criticality", "unrelated values should be preserved");
				otherAffectedValues.Values.Should().NotContain(v => v.PropertyName == "Asset Owner");
			}
		}

		[TestMethod]
		public void DeletePropertyWithCascade_WithPropertyNotReferencedAnywhere_ShouldOnlyDeleteProperty()
		{
			// "Maintenance Notes" (index 1) is not referenced by any demo PropertyValues.
			var unusedProperty = Helper.Properties.GetByScopeAndName("Asset", "Maintenance Notes")!;

			Helper.DeletePropertyWithCascade(unusedProperty);

			var remainingProperty = Helper.Properties.GetByScopeAndName("Asset", "Maintenance Notes");

			remainingProperty.Should().BeNull();
		}

		[TestMethod]
		public void DeletePropertyWithCascade_WithNullHelper_ShouldThrowArgumentNullException()
		{
			Action act = () => InfraOpsPropertiesApiHelperExtensions.DeletePropertyWithCascade(null!, DemoData.Properties[0]);

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void DeletePropertyWithCascade_WithNullProperty_ShouldThrowArgumentNullException()
		{
			Action act = () => Helper.DeletePropertyWithCascade(null!);

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void DeletePropertyWithCascade_WithNewProperty_ShouldThrowArgumentException()
		{
			var newProperty = new Property { Name = "New", Scope = "Asset" };

			Action act = () => Helper.DeletePropertyWithCascade(newProperty);

			act.Should().Throw<ArgumentException>();
		}
	}
}
