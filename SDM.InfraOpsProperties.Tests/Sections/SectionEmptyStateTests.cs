namespace SDM.InfraOpsProperties.Tests
{
	using System;

	using FluentAssertions;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	[TestClass]
	public class SectionEmptyStateTests
	{
		[TestMethod]
		public void PropertyLayout_DefaultState_IsEmpty()
		{
			new PropertyLayout().IsEmpty.Should().BeTrue();
		}

		[TestMethod]
		public void PropertyLayout_AnyFieldSet_IsNotEmpty()
		{
			new PropertyLayout().Also(layout => layout.SectionName = "General").IsEmpty.Should().BeFalse();
			new PropertyLayout().Also(layout => layout.Order = 1).IsEmpty.Should().BeFalse();
		}

		[TestMethod]
		public void PropertyOption_DefaultState_IsEmpty()
		{
			new PropertyOption().IsEmpty.Should().BeTrue();
		}

		[TestMethod]
		public void PropertyOption_OptionSet_IsNotEmpty()
		{
			new PropertyOption().Also(option => option.Option = "Enabled").IsEmpty.Should().BeFalse();
		}

		[TestMethod]
		public void PropertyValue_DefaultState_IsEmpty()
		{
			new PropertyValue().IsEmpty.Should().BeTrue();
		}

		[TestMethod]
		public void PropertyValue_AnyFieldSet_IsNotEmpty()
		{
			new PropertyValue().Also(value => value.PropertyName = "Hostname").IsEmpty.Should().BeFalse();
			new PropertyValue().Also(value => value.Value = "server-01").IsEmpty.Should().BeFalse();
			new PropertyValue().Also(value => value.PropertyId = new SdmObjectReference<Property>(Guid.NewGuid().ToString())).IsEmpty.Should().BeFalse();
		}
	}

	internal static class ObjectExtensions
	{
		public static T Also<T>(this T obj, Action<T> action)
		{
			action(obj);
			return obj;
		}
	}
}
