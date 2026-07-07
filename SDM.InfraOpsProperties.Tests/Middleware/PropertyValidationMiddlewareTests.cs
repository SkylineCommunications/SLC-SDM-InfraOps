namespace SDM.InfraOpsProperties.Tests.Middleware
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SharedMappers.DomIds;

	using Skyline.DataMiner.SDM.InfraOpsProperties.Middleware;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Validation;
	using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;
	using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

	/// <summary>
	/// Tests for <see cref="PropertyValidationMiddleware"/>.
	/// </summary>
	[TestClass]
	public class PropertyValidationMiddlewareTests
	{
		private PropertyValidationMiddleware _middleware = null!;

		[TestInitialize]
		public void Setup()
		{
			_middleware = new PropertyValidationMiddleware(new PropertyValidator());
		}

		private static Property ValidProperty() => new Property
		{
			Name = "Valid Property",
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
			var properties = new List<Property> { ValidProperty(), ValidProperty() };
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
			var properties = new List<Property> { ValidProperty(), ValidProperty() };
			var nextCalled = false;

			_middleware.OnUpdate(properties, p => { nextCalled = true; return p.ToList(); });

			nextCalled.Should().BeTrue();
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
	}
}
