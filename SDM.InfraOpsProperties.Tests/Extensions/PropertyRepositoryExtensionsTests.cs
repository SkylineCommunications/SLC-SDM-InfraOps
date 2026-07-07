namespace SDM.InfraOpsProperties.Tests.Extensions
{
	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.InfraOpsProperties.Tests.Setup;

	using SharedMappers.DomIds;

	using Skyline.DataMiner.SDM.InfraOpsProperties.Extensions;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	/// <summary>
	/// Tests for <see cref="PropertyRepositoryExtensions"/>.
	/// </summary>
	[TestClass]
	public class PropertyRepositoryExtensionsTests : BaseRepositoryTest
	{
		[TestInitialize]
		public void ExtensionsTestInitialize()
		{
			Helper.PopulateProperties();
		}

		#region GetByScope

		[TestMethod]
		public void GetByScope_WithAssetScope_ShouldReturnOnlyAssetProperties()
		{
			var result = Helper.Properties.GetByScope("Asset").ToList();

			using (new FluentAssertions.Execution.AssertionScope())
			{
				result.Should().HaveCount(3);
				result.Should().OnlyContain(p => p.Scope == "Asset");
			}
		}

		[TestMethod]
		public void GetByScope_WithFacilityScope_ShouldReturnOnlyFacilityProperties()
		{
			var result = Helper.Properties.GetByScope("Facility").ToList();

			using (new FluentAssertions.Execution.AssertionScope())
			{
				result.Should().HaveCount(2);
				result.Should().OnlyContain(p => p.Scope == "Facility");
			}
		}

		[TestMethod]
		public void GetByScope_WithUnknownScope_ShouldReturnEmpty()
		{
			var result = Helper.Properties.GetByScope("Unknown").ToList();

			result.Should().BeEmpty();
		}

		[TestMethod]
		public void GetByScope_WithNullRepository_ShouldThrowArgumentNullException()
		{
			Action act = () => PropertyRepositoryExtensions.GetByScope(null!, "Asset");

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		[DataRow(null)]
		[DataRow("")]
		[DataRow(" ")]
		public void GetByScope_WithInvalidScope_ShouldThrowArgumentException(string? scope)
		{
			Action act = () => Helper.Properties.GetByScope(scope!);

			act.Should().Throw<ArgumentException>();
		}

		#endregion

		#region GetByScopeAndName

		[TestMethod]
		public void GetByScopeAndName_WithExistingScopeAndName_ShouldReturnProperty()
		{
			var result = Helper.Properties.GetByScopeAndName("Asset", "Asset Owner");

			using (new FluentAssertions.Execution.AssertionScope())
			{
				result.Should().NotBeNull();
				result!.Name.Should().Be("Asset Owner");
				result.Scope.Should().Be("Asset");
			}
		}

		[TestMethod]
		public void GetByScopeAndName_WithUnknownName_ShouldReturnNull()
		{
			var result = Helper.Properties.GetByScopeAndName("Asset", "Does Not Exist");

			result.Should().BeNull();
		}

		[TestMethod]
		public void GetByScopeAndName_WithNullRepository_ShouldThrowArgumentNullException()
		{
			Action act = () => PropertyRepositoryExtensions.GetByScopeAndName(null!, "Asset", "Asset Owner");

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		[DataRow(null, "Asset Owner")]
		[DataRow("", "Asset Owner")]
		[DataRow("Asset", null)]
		[DataRow("Asset", "")]
		public void GetByScopeAndName_WithInvalidArguments_ShouldThrowArgumentException(string? scope, string? name)
		{
			Action act = () => Helper.Properties.GetByScopeAndName(scope!, name!);

			act.Should().Throw<ArgumentException>();
		}

		#endregion
	}
}
