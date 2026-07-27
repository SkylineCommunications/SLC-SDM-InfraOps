namespace SDM.InfraOpsProperties.Tests.Middleware
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.InfraOpsProperties.Tests.Setup;

	using SharedMappers.DomIds;

	using Skyline.DataMiner.SDM.InfraOpsProperties.Extensions;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Middleware;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Validation;
	using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;
	using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

	/// <summary>
	/// Tests for <see cref="PropertyValidationMiddleware"/>.
	/// </summary>
	[TestClass]
	public class PropertyValidationMiddlewareTests : BaseRepositoryTest
	{
		private PropertyValidationMiddleware _middleware = null!;

		[TestInitialize]
		public void Setup()
		{
			_middleware = new PropertyValidationMiddleware(new PropertyValidator(Helper));
		}

		private static Property ValidProperty(string name = "Valid Property") => new Property
		{
			Name = name,
			Scope = "Asset",
			PropertyType = InfraopsProperties.Enums.PropertyTypeEnum.String,
			StringSizeLimit = 10,
		};

		private static Property InvalidProperty() => new Property
		{
			Name = string.Empty,
			Scope = string.Empty,
		};

		#region Single Create/Update

		[TestMethod]
		public void OnCreate_Single_WithValidProperty_ShouldCallNext()
		{
			var property = ValidProperty();
			var nextCalled = false;

			var result = _middleware.OnCreate(property, p => { nextCalled = true; return p; });

			using (new FluentAssertions.Execution.AssertionScope())
			{
				nextCalled.Should().BeTrue();
				result.Should().Be(property);
			}
		}

		[TestMethod]
		public void OnCreate_Single_WithInvalidProperty_ShouldThrowAndNotCallNext()
		{
			var property = InvalidProperty();
			var nextCalled = false;

			Action act = () => _middleware.OnCreate(property, p => { nextCalled = true; return p; });

			using (new FluentAssertions.Execution.AssertionScope())
			{
				act.Should().Throw<ValidationException>();
				nextCalled.Should().BeFalse();
			}
		}

		[TestMethod]
		public void OnUpdate_Single_WithInvalidProperty_ShouldThrow()
		{
			var property = InvalidProperty();

			Action act = () => _middleware.OnUpdate(property, p => p);

			act.Should().Throw<ValidationException>();
		}

		#endregion

		#region Bulk Create/Update

		[TestMethod]
		public void OnCreate_Bulk_WithAllValidProperties_ShouldCallNext()
		{
			var properties = new List<Property> { ValidProperty("Property One"), ValidProperty("Property Two") };
			var nextCalled = false;

			_middleware.OnCreate(properties, p => { nextCalled = true; return p.ToList(); });

			nextCalled.Should().BeTrue();
		}

		[TestMethod]
		public void OnCreate_Bulk_WithOneInvalidProperty_ShouldThrowBulkValidationException()
		{
			var properties = new List<Property> { ValidProperty(), InvalidProperty() };

			Action act = () => _middleware.OnCreate(properties, p => p.ToList());

			var exception = act.Should().Throw<BulkValidationException<Property>>().Which;
			exception.FailedCount.Should().Be(1);
		}

		[TestMethod]
		public void OnCreateOrUpdate_Bulk_WithOneInvalidProperty_ShouldThrowBulkValidationException()
		{
			var properties = new List<Property> { ValidProperty(), InvalidProperty(), InvalidProperty() };

			Action act = () => _middleware.OnCreateOrUpdate(properties, p => p.ToList());

			var exception = act.Should().Throw<BulkValidationException<Property>>().Which;
			exception.FailedCount.Should().Be(2);
		}

		[TestMethod]
		public void OnUpdate_Bulk_WithAllValidProperties_ShouldCallNext()
		{
			var properties = new List<Property> { ValidProperty("Property One"), ValidProperty("Property Two") };
			var nextCalled = false;

			_middleware.OnUpdate(properties, p => { nextCalled = true; return p.ToList(); });

			nextCalled.Should().BeTrue();
		}

		[TestMethod]
		public void OnCreate_Bulk_WithDuplicateNamesInBatch_ShouldThrowBulkValidationException()
		{
			// Regression test: two brand-new Properties sharing a (Scope, Name) in the same bulk create call
			// must be rejected even though neither exists in the DOM yet (in-memory batch conflict detection).
			var properties = new List<Property> { ValidProperty("Duplicate Property"), ValidProperty("Duplicate Property") };
			var nextCalled = false;

			Action act = () => _middleware.OnCreate(properties, p => { nextCalled = true; return p.ToList(); });

			using (new FluentAssertions.Execution.AssertionScope())
			{
				var exception = act.Should().Throw<BulkValidationException<Property>>().Which;
				exception.FailedCount.Should().Be(2);
				nextCalled.Should().BeFalse();
			}
		}

		#endregion

		#region Pass-through operations

		[TestMethod]
		public void OnDelete_Single_WithNullProperty_ShouldThrowArgumentNullException()
		{
			Action act = () => _middleware.OnDelete((Property)null!, p => { });

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void OnDelete_Bulk_WithNullCollection_ShouldThrowArgumentNullException()
		{
			Action act = () => _middleware.OnDelete((IEnumerable<Property>)null!, p => { });

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void OnRead_WithNullFilter_ShouldThrowArgumentNullException()
		{
			Action act = () => _middleware.OnRead((Skyline.DataMiner.Net.Messages.SLDataGateway.FilterElement<Property>)null!, f => Enumerable.Empty<Property>());

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void OnCount_WithNullFilter_ShouldThrowArgumentNullException()
		{
			Action act = () => _middleware.OnCount((Skyline.DataMiner.Net.Messages.SLDataGateway.FilterElement<Property>)null!, f => 0L);

			act.Should().Throw<ArgumentNullException>();
		}

		#endregion

		#region Cascade delete

		[TestMethod]
		public void Delete_Single_ByDefault_CascadesAndRemovesReferencingValues()
		{
			Helper.PopulateProperties();
			Helper.PopulatePropertyValues();

			// "Asset Owner" (index 0) is referenced by PropertyValuesList[0] and [1], each also referencing "Criticality" (index 2).
			var assetOwnerProperty = Helper.Properties.GetByScopeAndName("Asset", "Asset Owner")!;

			Helper.Properties.Delete(assetOwnerProperty);

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
		public void Delete_Single_WithPropertyNotReferencedAnywhere_ShouldOnlyDeleteProperty()
		{
			Helper.PopulateProperties();
			Helper.PopulatePropertyValues();

			// "Maintenance Notes" (index 1) is not referenced by any demo PropertyValues.
			var unusedProperty = Helper.Properties.GetByScopeAndName("Asset", "Maintenance Notes")!;

			Helper.Properties.Delete(unusedProperty);

			var remainingProperty = Helper.Properties.GetByScopeAndName("Asset", "Maintenance Notes");

			remainingProperty.Should().BeNull();
		}

		[TestMethod]
		public void Delete_Bulk_ByDefault_CascadesForEveryDeletedProperty()
		{
			Helper.PopulateProperties();
			Helper.PopulatePropertyValues();

			var assetOwnerProperty = Helper.Properties.GetByScopeAndName("Asset", "Asset Owner")!;
			var criticalityProperty = Helper.Properties.GetByScopeAndName("Asset", "Criticality")!;

			Helper.Properties.Delete(new Property[] { assetOwnerProperty, criticalityProperty });

			var affectedValues = Helper.PropertyValues.GetByLinkedObjectID(DemoData.PropertyValuesList[0].LinkedObjectID, "Asset").Single();

			using (new FluentAssertions.Execution.AssertionScope())
			{
				affectedValues.Values.Should().NotContain(v => v.PropertyName == "Asset Owner");
				affectedValues.Values.Should().NotContain(v => v.PropertyName == "Criticality");
			}
		}

		[TestMethod]
		public void Delete_Single_WhenCascadeOptedOut_LeavesReferencingValuesInPlace()
		{
			var optedOutHelper = RepositoryInitialize.InitializeEmptyRepositories(cascadeDeleteOnProperty: false);
			optedOutHelper.PopulateProperties();
			optedOutHelper.PopulatePropertyValues();

			var assetOwnerProperty = optedOutHelper.Properties.GetByScopeAndName("Asset", "Asset Owner")!;

			optedOutHelper.Properties.Delete(assetOwnerProperty);

			var remainingProperty = optedOutHelper.Properties.GetByScopeAndName("Asset", "Asset Owner");
			var affectedValues = optedOutHelper.PropertyValues.GetByLinkedObjectID(DemoData.PropertyValuesList[0].LinkedObjectID, "Asset").Single();

			using (new FluentAssertions.Execution.AssertionScope())
			{
				remainingProperty.Should().BeNull("the property itself should still be deleted");
				affectedValues.Values.Should().Contain(v => v.PropertyName == "Asset Owner", "cascade was opted out, so the stale reference should remain");
			}
		}

		#endregion
	}
}
