namespace SDM.PlanAndBuild.Tests.Middleware
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.SDM.PlanAndBuild.Middleware;
	using Skyline.DataMiner.SDM.PlanAndBuild.Models;

	/// <summary>
	/// Tests for <see cref="IdentifierMiddleware{T}"/> using <see cref="JobType"/> as the target type.
	/// </summary>
	[TestClass]
	public class IdentifierMiddlewareTests
	{
		private IdentifierMiddleware<JobType> _middleware = null!;

		[TestInitialize]
		public void Setup()
		{
			_middleware = new IdentifierMiddleware<JobType>();
		}

		[TestMethod]
		public void OnCreate_Single_WithoutIdentifier_ShouldAssignGuid()
		{
			var jobType = new JobType { Name = "Test" };

			var result = _middleware.OnCreate(jobType, jt => jt);

			using (new AssertionScope())
			{
				result.Identifier.Should().NotBeNullOrWhiteSpace();
				Guid.TryParse(result.Identifier, out _).Should().BeTrue();
			}
		}

		[TestMethod]
		public void OnCreate_Single_WithExistingIdentifier_ShouldNotOverwrite()
		{
			var existingId = Guid.NewGuid().ToString();
			var jobType = new JobType { Identifier = existingId, Name = "Test" };

			var result = _middleware.OnCreate(jobType, jt => jt);

			result.Identifier.Should().Be(existingId);
		}

		[TestMethod]
		public void OnCreate_Bulk_WithoutIdentifiers_ShouldAssignGuidToAll()
		{
			var jobTypes = new List<JobType>
			{
				new JobType { Name = "A" },
				new JobType { Name = "B" },
			};

			var result = _middleware.OnCreate(jobTypes, jt => jt.ToList());

			using (new AssertionScope())
			{
				result.Should().OnlyContain(jt => !string.IsNullOrWhiteSpace(jt.Identifier));
				result.Select(jt => jt.Identifier).Distinct().Should().HaveCount(2);
			}
		}

		[TestMethod]
		public void OnCreateOrUpdate_Bulk_WithMixedIdentifiers_ShouldOnlyAssignMissingOnes()
		{
			var existingId = Guid.NewGuid().ToString();
			var jobTypes = new List<JobType>
			{
				new JobType { Identifier = existingId, Name = "A" },
				new JobType { Name = "B" },
			};

			var result = _middleware.OnCreateOrUpdate(jobTypes, jt => jt.ToList());

			using (new AssertionScope())
			{
				result.First().Identifier.Should().Be(existingId);
				result.Last().Identifier.Should().NotBeNullOrWhiteSpace();
			}
		}

		[TestMethod]
		public void OnUpdate_Single_WithoutIdentifier_ShouldAssignGuid()
		{
			var jobType = new JobType { Name = "Test" };

			var result = _middleware.OnUpdate(jobType, jt => jt);

			result.Identifier.Should().NotBeNullOrWhiteSpace();
		}

		[TestMethod]
		public void OnDelete_Single_ShouldNotAssignIdentifier()
		{
			var jobType = new JobType { Name = "Test" };

			_middleware.OnDelete(jobType, jt => { });

			jobType.Identifier.Should().BeNullOrWhiteSpace();
		}
	}
}
