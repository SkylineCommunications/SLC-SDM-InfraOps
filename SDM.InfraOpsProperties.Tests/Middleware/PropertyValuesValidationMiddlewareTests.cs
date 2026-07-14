namespace SDM.InfraOpsProperties.Tests.Middleware
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.InfraOpsProperties.Tests.Setup;

	using Skyline.DataMiner.SDM.InfraOpsProperties.Middleware;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Validation;
	using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;
	using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

	/// <summary>
	/// Tests for <see cref="PropertyValuesValidationMiddleware"/>.
	/// </summary>
	[TestClass]
	public class PropertyValuesValidationMiddlewareTests : BaseRepositoryTest
	{
		private PropertyValuesValidationMiddleware _middleware = null!;

		[TestInitialize]
		public void Setup()
		{
			_middleware = new PropertyValuesValidationMiddleware(new PropertyValuesValidator(Helper));
		}

		private static PropertyValues ValidPropertyValues() => new PropertyValues
		{
			LinkedObjectID = Guid.NewGuid(),
			Scope = "Asset",
		};

		private static PropertyValues InvalidPropertyValues() => new PropertyValues
		{
			LinkedObjectID = Guid.Empty,
			Scope = string.Empty,
		};

		#region Single Create/Update

		[TestMethod]
		public void OnCreate_Single_WithValidPropertyValues_ShouldCallNext()
		{
			var propertyValues = ValidPropertyValues();
			var nextCalled = false;

			var result = _middleware.OnCreate(propertyValues, p => { nextCalled = true; return p; });

			using (new FluentAssertions.Execution.AssertionScope())
			{
				nextCalled.Should().BeTrue();
				result.Should().Be(propertyValues);
			}
		}

		[TestMethod]
		public void OnCreate_Single_WithInvalidPropertyValues_ShouldThrowAndNotCallNext()
		{
			var propertyValues = InvalidPropertyValues();
			var nextCalled = false;

			Action act = () => _middleware.OnCreate(propertyValues, p => { nextCalled = true; return p; });

			using (new FluentAssertions.Execution.AssertionScope())
			{
				act.Should().Throw<ValidationException>();
				nextCalled.Should().BeFalse();
			}
		}

		[TestMethod]
		public void OnUpdate_Single_WithInvalidPropertyValues_ShouldThrow()
		{
			var propertyValues = InvalidPropertyValues();

			Action act = () => _middleware.OnUpdate(propertyValues, p => p);

			act.Should().Throw<ValidationException>();
		}

		#endregion

		#region Bulk Create/Update

		[TestMethod]
		public void OnCreate_Bulk_WithAllValidPropertyValues_ShouldCallNext()
		{
			var propertyValuesList = new List<PropertyValues> { ValidPropertyValues(), ValidPropertyValues() };
			var nextCalled = false;

			_middleware.OnCreate(propertyValuesList, p => { nextCalled = true; return p.ToList(); });

			nextCalled.Should().BeTrue();
		}

		[TestMethod]
		public void OnCreate_Bulk_WithOneInvalidPropertyValues_ShouldThrowBulkValidationException()
		{
			var propertyValuesList = new List<PropertyValues> { ValidPropertyValues(), InvalidPropertyValues() };

			Action act = () => _middleware.OnCreate(propertyValuesList, p => p.ToList());

			var exception = act.Should().Throw<BulkValidationException<PropertyValues>>().Which;
			exception.FailedCount.Should().Be(1);
		}

		[TestMethod]
		public void OnCreateOrUpdate_Bulk_WithOneInvalidPropertyValues_ShouldThrowBulkValidationException()
		{
			var propertyValuesList = new List<PropertyValues> { ValidPropertyValues(), InvalidPropertyValues(), InvalidPropertyValues() };

			Action act = () => _middleware.OnCreateOrUpdate(propertyValuesList, p => p.ToList());

			var exception = act.Should().Throw<BulkValidationException<PropertyValues>>().Which;
			exception.FailedCount.Should().Be(2);
		}

		[TestMethod]
		public void OnUpdate_Bulk_WithAllValidPropertyValues_ShouldCallNext()
		{
			var propertyValuesList = new List<PropertyValues> { ValidPropertyValues(), ValidPropertyValues() };
			var nextCalled = false;

			_middleware.OnUpdate(propertyValuesList, p => { nextCalled = true; return p.ToList(); });

			nextCalled.Should().BeTrue();
		}

		[TestMethod]
		public void OnCreate_Bulk_WithDuplicateComboInBatch_ShouldThrowBulkValidationException()
		{
			// Regression test: two brand-new PropertyValues sharing a (LinkedObjectID, Scope, SubID) combo in the
			// same bulk create call must be rejected even though neither exists in the DOM yet (in-memory batch
			// conflict detection).
			var linkedObjectId = Guid.NewGuid();
			var propertyValuesList = new List<PropertyValues>
			{
				new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset" },
				new PropertyValues { LinkedObjectID = linkedObjectId, Scope = "Asset" },
			};
			var nextCalled = false;

			Action act = () => _middleware.OnCreate(propertyValuesList, p => { nextCalled = true; return p.ToList(); });

			using (new FluentAssertions.Execution.AssertionScope())
			{
				var exception = act.Should().Throw<BulkValidationException<PropertyValues>>().Which;
				exception.FailedCount.Should().Be(2);
				nextCalled.Should().BeFalse();
			}
		}

		#endregion

		#region Pass-through operations

		[TestMethod]
		public void OnDelete_Single_WithNullPropertyValues_ShouldThrowArgumentNullException()
		{
			Action act = () => _middleware.OnDelete((PropertyValues)null!, p => { });

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void OnDelete_Bulk_WithNullCollection_ShouldThrowArgumentNullException()
		{
			Action act = () => _middleware.OnDelete((IEnumerable<PropertyValues>)null!, p => { });

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void OnRead_WithNullFilter_ShouldThrowArgumentNullException()
		{
			Action act = () => _middleware.OnRead((Skyline.DataMiner.Net.Messages.SLDataGateway.FilterElement<PropertyValues>)null!, f => Enumerable.Empty<PropertyValues>());

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void OnCount_WithNullFilter_ShouldThrowArgumentNullException()
		{
			Action act = () => _middleware.OnCount((Skyline.DataMiner.Net.Messages.SLDataGateway.FilterElement<PropertyValues>)null!, f => 0L);

			act.Should().Throw<ArgumentNullException>();
		}

		#endregion
	}
}
