namespace SDM.InfraOpsProperties.Tests.Extensions
{
	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.InfraOpsProperties.Tests.Setup;

	using Skyline.DataMiner.SDM.InfraOpsProperties.Extensions;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	/// <summary>
	/// Tests for <see cref="PropertyValuesRepositoryExtensions"/>.
	/// </summary>
	[TestClass]
	public class PropertyValuesRepositoryExtensionsTests : BaseRepositoryTest
	{
		[TestInitialize]
		public void ExtensionsTestInitialize()
		{
			Helper.PopulateProperties();
			Helper.PopulatePropertyValues();
		}

		#region GetByLinkedObjectID

		[TestMethod]
		public void GetByLinkedObjectID_WithExistingLinkedObjectID_ShouldReturnMatch()
		{
			var expected = DemoData.PropertyValuesList[0];

			var result = Helper.PropertyValues.GetByLinkedObjectID(expected.LinkedObjectID, expected.Scope).ToList();

			using (new FluentAssertions.Execution.AssertionScope())
			{
				result.Should().HaveCount(1);
				result[0].Identifier.Should().Be(expected.Identifier);
			}
		}

		[TestMethod]
		public void GetByLinkedObjectID_WithSubID_ShouldFilterBySubID()
		{
			var expected = DemoData.PropertyValuesList[2];

			var result = Helper.PropertyValues.GetByLinkedObjectID(expected.LinkedObjectID, expected.Scope, expected.SubID).ToList();

			using (new FluentAssertions.Execution.AssertionScope())
			{
				result.Should().HaveCount(1);
				result[0].Identifier.Should().Be(expected.Identifier);
			}
		}

		[TestMethod]
		public void GetByLinkedObjectID_WithUnknownLinkedObjectID_ShouldReturnEmpty()
		{
			var result = Helper.PropertyValues.GetByLinkedObjectID(Guid.NewGuid(), "Asset").ToList();

			result.Should().BeEmpty();
		}

		[TestMethod]
		public void GetByLinkedObjectID_WithNullRepository_ShouldThrowArgumentNullException()
		{
			Action act = () => PropertyValuesRepositoryExtensions.GetByLinkedObjectID(null!, Guid.NewGuid(), "Asset");

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void GetByLinkedObjectID_WithDefaultSubID_ShouldOnlyMatchEntriesWithNoSubID()
		{
			// Regression test: passing no subId must filter for the "no SubID" bucket only, matching the
			// legacy PropertyValuesDefinitionHandler behavior (subId == null => KeyExists(SubID) == false).
			// It must NOT behave as a wildcard matching any SubID.
			var linkedObjectId = Guid.NewGuid();
			Helper.PropertyValues.Create(new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset" });
			Helper.PropertyValues.Create(new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset", SubID = "Port1" });

			var result = Helper.PropertyValues.GetByLinkedObjectID(linkedObjectId, "Asset").ToList();

			using (new FluentAssertions.Execution.AssertionScope())
			{
				result.Should().HaveCount(1, "only the entry without a SubID should match");
				result[0].SubID.Should().BeNull();
			}
		}

		[TestMethod]
		public void GetByLinkedObjectID_WithWildcardSubID_ShouldMatchAllEntriesRegardlessOfSubID()
		{
			// "*" is the explicit wildcard, distinct from the default (null) "no SubID" behavior.
			var linkedObjectId = Guid.NewGuid();
			Helper.PropertyValues.Create(new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset" });
			Helper.PropertyValues.Create(new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset", SubID = "Port1" });
			Helper.PropertyValues.Create(new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset", SubID = "Port2" });

			var result = Helper.PropertyValues.GetByLinkedObjectID(linkedObjectId, "Asset", "*").ToList();

			result.Should().HaveCount(3, "the wildcard SubID must match every entry regardless of its SubID");
		}

		[TestMethod]
		public void GetByLinkedObjectID_WithEmptyLinkedObjectID_ShouldThrowArgumentException()
		{
			Action act = () => Helper.PropertyValues.GetByLinkedObjectID(Guid.Empty, "Asset");

			act.Should().Throw<ArgumentException>();
		}

		[TestMethod]
		[DataRow(null)]
		[DataRow("")]
		[DataRow(" ")]
		public void GetByLinkedObjectID_WithInvalidScope_ShouldThrowArgumentException(string? scope)
		{
			Action act = () => Helper.PropertyValues.GetByLinkedObjectID(Guid.NewGuid(), scope!);

			act.Should().Throw<ArgumentException>();
		}

		#endregion

		#region GetSingleOrDefaultByLinkedObjectID

		[TestMethod]
		public void GetSingleOrDefaultByLinkedObjectID_WithExistingEntry_ShouldReturnIt()
		{
			var expected = DemoData.PropertyValuesList[1];

			var result = Helper.PropertyValues.GetSingleOrDefaultByLinkedObjectID(expected.LinkedObjectID, expected.Scope);

			using (new FluentAssertions.Execution.AssertionScope())
			{
				result.Should().NotBeNull();
				result!.Identifier.Should().Be(expected.Identifier);
			}
		}

		[TestMethod]
		public void GetSingleOrDefaultByLinkedObjectID_WithUnknownEntry_ShouldReturnNull()
		{
			var result = Helper.PropertyValues.GetSingleOrDefaultByLinkedObjectID(Guid.NewGuid(), "Asset");

			result.Should().BeNull();
		}

		[TestMethod]
		public void GetSingleOrDefaultByLinkedObjectID_WithDefaultSubIDAndOtherSubIDsPresent_ShouldReturnOnlyNoSubIDEntry()
		{
			// Regression test: previously the default (null) subId behaved as a wildcard and this would throw
			// InvalidOperationException from SingleOrDefault() once more than one SubID existed for the same
			// LinkedObjectID/Scope. It must now resolve to exactly the "no SubID" entry.
			var linkedObjectId = Guid.NewGuid();
			var noSubIdEntry = Helper.PropertyValues.Create(new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset" });
			Helper.PropertyValues.Create(new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset", SubID = "Port1" });

			var result = Helper.PropertyValues.GetSingleOrDefaultByLinkedObjectID(linkedObjectId, "Asset");

			using (new FluentAssertions.Execution.AssertionScope())
			{
				result.Should().NotBeNull();
				result!.Identifier.Should().Be(noSubIdEntry.Identifier);
			}
		}

		#endregion

		#region GetByPropertyID

		[TestMethod]
		public void GetByPropertyID_WithPropertyUsedInValues_ShouldReturnReferencingPropertyValues()
		{
			// "Asset Owner" is referenced by PropertyValuesList[0] and [1]; fetch via repository so IsNew is false.
			var assetOwnerProperty = Helper.Properties.GetByScopeAndName("Asset", "Asset Owner")!;

			var result = Helper.PropertyValues.GetByPropertyID(assetOwnerProperty).ToList();

			result.Should().HaveCount(2);
		}

		[TestMethod]
		public void GetByPropertyID_WithPropertyNotUsedInAnyValues_ShouldReturnEmpty()
		{
			// "Maintenance Notes" is not referenced by any demo PropertyValues.
			var unusedProperty = Helper.Properties.GetByScopeAndName("Asset", "Maintenance Notes")!;

			var result = Helper.PropertyValues.GetByPropertyID(unusedProperty).ToList();

			result.Should().BeEmpty();
		}

		[TestMethod]
		public void GetByPropertyID_WithNullRepository_ShouldThrowArgumentNullException()
		{
			Action act = () => PropertyValuesRepositoryExtensions.GetByPropertyID(null!, DemoData.Properties[0]);

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void GetByPropertyID_WithNullProperty_ShouldThrowArgumentNullException()
		{
			Action act = () => Helper.PropertyValues.GetByPropertyID(null!);

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void GetByPropertyID_WithNewProperty_ShouldThrowArgumentException()
		{
			var newProperty = new Property { Name = "New", Scope = "Asset" };

			Action act = () => Helper.PropertyValues.GetByPropertyID(newProperty);

			act.Should().Throw<ArgumentException>();
		}

		#endregion

		#region CopyPropertyValues

		[TestMethod]
		public void CopyPropertyValues_WithExistingSource_ShouldCreateDuplicateForTarget()
		{
			var source = DemoData.PropertyValuesList[0];
			var targetObjectId = Guid.NewGuid();

			var result = Helper.PropertyValues.CopyPropertyValues(source.Scope, source.LinkedObjectID, targetObjectId);

			using (new FluentAssertions.Execution.AssertionScope())
			{
				result.Should().NotBeNull();
				result!.LinkedObjectID.Should().Be(targetObjectId);
				result.Scope.Should().Be(source.Scope);
				result.Values.Should().HaveCount(source.Values.Count);
			}
		}

		[TestMethod]
		public void CopyPropertyValues_WithExistingTarget_ShouldOverwriteIt()
		{
			var source = DemoData.PropertyValuesList[0];
			var existingTarget = DemoData.PropertyValuesList[1];

			var result = Helper.PropertyValues.CopyPropertyValues(source.Scope, source.LinkedObjectID, existingTarget.LinkedObjectID);

			var afterCopy = Helper.PropertyValues.GetByLinkedObjectID(existingTarget.LinkedObjectID, existingTarget.Scope).ToList();

			using (new FluentAssertions.Execution.AssertionScope())
			{
				result.Should().NotBeNull();
				afterCopy.Should().HaveCount(1);
				afterCopy[0].Identifier.Should().Be(result!.Identifier);
			}
		}

		[TestMethod]
		public void CopyPropertyValues_WithNoSource_ShouldReturnNullAndDeleteExistingTarget()
		{
			var existingTarget = DemoData.PropertyValuesList[1];

			var result = Helper.PropertyValues.CopyPropertyValues("Asset", Guid.NewGuid(), existingTarget.LinkedObjectID);

			var afterCopy = Helper.PropertyValues.GetByLinkedObjectID(existingTarget.LinkedObjectID, existingTarget.Scope).ToList();

			using (new FluentAssertions.Execution.AssertionScope())
			{
				result.Should().BeNull();
				afterCopy.Should().BeEmpty();
			}
		}

		[TestMethod]
		public void CopyPropertyValues_WithNullRepository_ShouldThrowArgumentNullException()
		{
			Action act = () => PropertyValuesRepositoryExtensions.CopyPropertyValues(null!, "Asset", Guid.NewGuid(), Guid.NewGuid());

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void CopyPropertyValues_WithEmptySourceObjectId_ShouldThrowArgumentException()
		{
			Action act = () => Helper.PropertyValues.CopyPropertyValues("Asset", Guid.Empty, Guid.NewGuid());

			act.Should().Throw<ArgumentException>();
		}

		[TestMethod]
		public void CopyPropertyValues_WithEmptyTargetObjectId_ShouldThrowArgumentException()
		{
			Action act = () => Helper.PropertyValues.CopyPropertyValues("Asset", Guid.NewGuid(), Guid.Empty);

			act.Should().Throw<ArgumentException>();
		}

		[TestMethod]
		[DataRow(null)]
		[DataRow("")]
		[DataRow(" ")]
		public void CopyPropertyValues_WithInvalidScope_ShouldThrowArgumentException(string? scope)
		{
			Action act = () => Helper.PropertyValues.CopyPropertyValues(scope!, Guid.NewGuid(), Guid.NewGuid());

			act.Should().Throw<ArgumentException>();
		}

		[TestMethod]
		public void CopyPropertyValues_WithSourceAndTargetSame_ShouldStillSucceedButMintNewIdentifier()
		{
			// Corner case: copying an object's PropertyValues onto itself (objectIdA == objectIdB).
			// The existing record is deleted first, then re-created from the stale in-memory
			// reference, so the operation succeeds and data is preserved, but the Identifier changes.
			var source = DemoData.PropertyValuesList[0];
			var originalIdentifier = source.Identifier;

			var result = Helper.PropertyValues.CopyPropertyValues(source.Scope, source.LinkedObjectID, source.LinkedObjectID);

			var afterCopy = Helper.PropertyValues.GetByLinkedObjectID(source.LinkedObjectID, source.Scope).ToList();

			using (new FluentAssertions.Execution.AssertionScope())
			{
				result.Should().NotBeNull();
				result!.LinkedObjectID.Should().Be(source.LinkedObjectID);
				result.Scope.Should().Be(source.Scope);
				result.Values.Should().HaveCount(source.Values.Count);
				result.Identifier.Should().NotBe(originalIdentifier, "self-copy re-creates the record with a brand new Identifier");
				afterCopy.Should().HaveCount(1);
				afterCopy[0].Identifier.Should().Be(result.Identifier);
			}
		}

		#endregion
	}
}
