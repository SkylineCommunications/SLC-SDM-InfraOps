namespace SDM.InfraOpsProperties.Tests.Extensions
{
	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.SDM.InfraOpsProperties.Extensions;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	/// <summary>
	/// Tests for <see cref="PropertyExtensions"/>
	/// (AddDiscrete/RemoveDiscrete/ClearDiscretes), mirroring InfraOpsShared's PropertyWrapper API.
	/// </summary>
	[TestClass]
	public class PropertyExtensionsTests
	{
		[TestMethod]
		public void AddDiscrete_NewOption_ShouldBeAdded()
		{
			var property = new Property();
			var discrete = new PropertyOption { Option = "Red" };

			property.AddDiscrete(discrete);

			property.Discreets.Should().ContainSingle().Which.Should().Be(discrete);
		}

		[TestMethod]
		public void AddDiscrete_Null_ShouldThrow()
		{
			var property = new Property();

			Action act = () => property.AddDiscrete(null);

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void AddDiscrete_DuplicateOption_ShouldThrow()
		{
			var property = new Property();
			property.AddDiscrete(new PropertyOption { Option = "Red" });

			Action act = () => property.AddDiscrete(new PropertyOption { Option = "Red" });

			act.Should().Throw<InvalidOperationException>();
		}

		[TestMethod]
		public void RemoveDiscrete_ExistingOption_ShouldBeRemoved()
		{
			var property = new Property();
			var discrete = new PropertyOption { Option = "Red" };
			property.AddDiscrete(discrete);

			property.RemoveDiscrete(discrete);

			property.Discreets.Should().BeEmpty();
		}

		[TestMethod]
		public void RemoveDiscrete_Null_ShouldThrow()
		{
			var property = new Property();

			Action act = () => property.RemoveDiscrete(null);

			act.Should().Throw<ArgumentNullException>();
		}

		[TestMethod]
		public void RemoveDiscrete_NotFound_ShouldThrow()
		{
			var property = new Property();
			var discrete = new PropertyOption { Option = "Red" };

			Action act = () => property.RemoveDiscrete(discrete);

			act.Should().Throw<ArgumentException>();
		}

		[TestMethod]
		public void ClearDiscretes_ShouldEmptyList()
		{
			var property = new Property();
			property.AddDiscrete(new PropertyOption { Option = "Red" });

			property.ClearDiscretes();

			property.Discreets.Should().BeEmpty();
		}
	}
}
