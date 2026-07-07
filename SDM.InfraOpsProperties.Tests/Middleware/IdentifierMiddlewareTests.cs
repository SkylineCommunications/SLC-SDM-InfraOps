namespace SDM.InfraOpsProperties.Tests.Middleware
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.SDM.InfraOpsProperties.Middleware;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	/// <summary>
	/// Tests for <see cref="IdentifierMiddleware{T}"/> using <see cref="Property"/> as the target type.
	/// </summary>
	[TestClass]
	public class IdentifierMiddlewareTests
	{
		private IdentifierMiddleware<Property> _middleware = null!;

		[TestInitialize]
		public void Setup()
		{
			_middleware = new IdentifierMiddleware<Property>();
		}

		[TestMethod]
		public void OnCreate_Single_WithoutIdentifier_ShouldAssignGuid()
		{
			var property = new Property { Name = "Test" };

			var result = _middleware.OnCreate(property, p => p);

			result.Identifier.Should().NotBeNullOrWhiteSpace();
			Guid.TryParse(result.Identifier, out _).Should().BeTrue();
		}

		[TestMethod]
		public void OnCreate_Single_WithExistingIdentifier_ShouldNotOverwrite()
		{
			var existingId = Guid.NewGuid().ToString();
			var property = new Property { Identifier = existingId, Name = "Test" };

			var result = _middleware.OnCreate(property, p => p);

			result.Identifier.Should().Be(existingId);
		}

		[TestMethod]
		public void OnCreate_Bulk_WithoutIdentifiers_ShouldAssignGuidToAll()
		{
			var properties = new List<Property>
			{
				new Property { Name = "A" },
				new Property { Name = "B" },
			};

			var result = _middleware.OnCreate(properties, p => p.ToList());

			using (new AssertionScope())
			{
				result.Should().OnlyContain(p => !string.IsNullOrWhiteSpace(p.Identifier));
				result.Select(p => p.Identifier).Distinct().Should().HaveCount(2);
			}
		}

		[TestMethod]
		public void OnCreateOrUpdate_Bulk_WithMixedIdentifiers_ShouldOnlyAssignMissingOnes()
		{
			var existingId = Guid.NewGuid().ToString();
			var properties = new List<Property>
			{
				new Property { Identifier = existingId, Name = "A" },
				new Property { Name = "B" },
			};

			var result = _middleware.OnCreateOrUpdate(properties, p => p.ToList());

			using (new AssertionScope())
			{
				result.First().Identifier.Should().Be(existingId);
				result.Last().Identifier.Should().NotBeNullOrWhiteSpace();
			}
		}

		[TestMethod]
		public void OnUpdate_Single_WithoutIdentifier_ShouldAssignGuid()
		{
			var property = new Property { Name = "Test" };

			var result = _middleware.OnUpdate(property, p => p);

			result.Identifier.Should().NotBeNullOrWhiteSpace();
		}

		[TestMethod]
		public void OnDelete_Single_ShouldNotAssignIdentifier()
		{
			var property = new Property { Name = "Test" };

			_middleware.OnDelete(property, p => { });

			property.Identifier.Should().BeNullOrWhiteSpace();
		}
	}
}
