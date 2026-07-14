namespace SDM.InfraOpsProperties.Tests.Extensions
{
	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.InfraOpsProperties.Tests.Setup;

	using Skyline.DataMiner.SDM.InfraOpsProperties.Extensions;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	/// <summary>
	/// Tests for <see cref="PropertyValueExtensions"/>.
	/// </summary>
	[TestClass]
	public class PropertyValueExtensionsTests : BaseRepositoryTest
	{
		[TestInitialize]
		public void ExtensionsTestInitialize()
		{
			Helper.PopulateProperties();
		}

		#region IsCustom

		[TestMethod]
		public void IsCustom_WithoutPropertyId_ShouldReturnTrue()
		{
			var value = new PropertyValue { PropertyName = "Ad-hoc", Value = "SomeValue" };

			value.IsCustom().Should().BeTrue();
		}

		[TestMethod]
		public void IsCustom_WithPropertyId_ShouldReturnFalse()
		{
			var value = new PropertyValue
			{
				PropertyName = "Asset Owner",
				Value = "John",
				PropertyId = new Skyline.DataMiner.SDM.SdmObjectReference<Property>(DemoData.Properties[0].Identifier),
			};

			value.IsCustom().Should().BeFalse();
		}

		[TestMethod]
		public void IsCustom_WithNullValue_ShouldThrowArgumentNullException()
		{
			Action act = () => PropertyValueExtensions.IsCustom(null!);

			act.Should().Throw<ArgumentNullException>();
		}

		#endregion

		#region GetProperty

		[TestMethod]
		public void GetProperty_WithLinkedProperty_ShouldResolveIt()
		{
			var expected = DemoData.Properties[0];
			var value = new PropertyValue
			{
				PropertyName = expected.Name,
				Value = "John",
				PropertyId = new Skyline.DataMiner.SDM.SdmObjectReference<Property>(expected.Identifier),
			};

			var result = value.GetProperty(Helper.Properties);

			using (new FluentAssertions.Execution.AssertionScope())
			{
				result.Should().NotBeNull();
				result!.Identifier.Should().Be(expected.Identifier);
			}
		}

		[TestMethod]
		public void GetProperty_WithCustomValue_ShouldReturnNull()
		{
			var value = new PropertyValue { PropertyName = "Ad-hoc", Value = "SomeValue" };

			var result = value.GetProperty(Helper.Properties);

			result.Should().BeNull();
		}

		[TestMethod]
		public void GetProperty_WithNullValue_ShouldThrowArgumentNullException()
		{
			Action act = () => PropertyValueExtensions.GetProperty(null!, Helper.Properties);

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void GetProperty_WithNullRepository_ShouldThrowArgumentNullException()
		{
			var value = new PropertyValue { PropertyName = "Ad-hoc", Value = "SomeValue" };

			Action act = () => value.GetProperty(null!);

			act.Should().Throw<ArgumentNullException>();
		}

		#endregion
	}
}
