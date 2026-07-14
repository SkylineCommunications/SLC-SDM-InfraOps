namespace SDM.InfraOpsProperties.Tests.Properties
{
	using System;
	using System.Linq;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.InfraOpsProperties.Tests.Setup;

	using SharedMappers.DomIds;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	public partial class PropertyDomRepositoryTests
	{
		[TestMethod]
		public void PropertyDomRepository_ReadFilter_Name_Equals()
		{
			Helper.PopulateProperties();

			string propertyName = "Criticality";
			var nameFilter = PropertyExposers.Name.Equal(propertyName);
			var expected = DemoData.Properties.Single(property => property.Name.Equals(propertyName));

			var propertiesRetrieved = Helper.Properties.Read(nameFilter);

			using (new AssertionScope())
			{
				propertiesRetrieved.Should().NotBeNull();
				propertiesRetrieved.Count().Should().Be(1);
				var property = propertiesRetrieved.First();

				property.Name.Should().Be(expected.Name);
				property.PropertyType.Should().Be(expected.PropertyType);
				property.Scope.Should().Be(expected.Scope);
				property.Default.Should().Be(expected.Default);
			}
		}

		[TestMethod]
		public void PropertyDomRepository_ReadFilter_Scope_Equal()
		{
			Helper.PopulateProperties();

			var scope = "Asset";
			var scopeFilter = PropertyExposers.Scope.Equal(scope);

			var propertiesRetrieved = Helper.Properties.Read(scopeFilter);
			var expected = DemoData.Properties.Where(p => p.Scope == scope).ToArray();

			using (new AssertionScope())
			{
				propertiesRetrieved.Should().NotBeNull();
				propertiesRetrieved.Count().Should().Be(expected.Length);
				propertiesRetrieved.Select(p => p.Name).Should().BeEquivalentTo(expected.Select(p => p.Name));
			}
		}

		[TestMethod]
		public void PropertyDomRepository_ReadFilter_PropertyType_Equal()
		{
			Helper.PopulateProperties();

			var typeFilter = PropertyExposers.PropertyType.Equal(InfraopsProperties.Enums.PropertyTypeEnum.Discrete);
			var expected = DemoData.Properties.Where(p => p.PropertyType == InfraopsProperties.Enums.PropertyTypeEnum.Discrete).ToArray();

			var propertiesRetrieved = Helper.Properties.Read(typeFilter);

			using (new AssertionScope())
			{
				propertiesRetrieved.Should().NotBeNull();
				propertiesRetrieved.Count().Should().Be(expected.Length);
				propertiesRetrieved.Should().OnlyContain(p => p.PropertyType == InfraopsProperties.Enums.PropertyTypeEnum.Discrete);
			}
		}

		[TestMethod]
		public void PropertyDomRepository_ReadFilter_PropertyType_NotEqual()
		{
			Helper.PopulateProperties();

			var typeFilter = PropertyExposers.PropertyType.UncheckedNotEqual(InfraopsProperties.Enums.PropertyTypeEnum.String);
			var expected = DemoData.Properties.Where(p => p.PropertyType != InfraopsProperties.Enums.PropertyTypeEnum.String).ToArray();

			var propertiesRetrieved = Helper.Properties.Read(typeFilter);

			using (new AssertionScope())
			{
				propertiesRetrieved.Should().NotBeNull();
				propertiesRetrieved.Count().Should().Be(expected.Length);
			}
		}

		[TestMethod]
		public void PropertyDomRepository_ReadFilter_StringSizeLimit_GreaterThanOrEqual()
		{
			Helper.PopulateProperties();

			long threshold = 100;
			var filter = PropertyExposers.StringSizeLimit.GreaterThanOrEqual(threshold);
			var expected = DemoData.Properties.Where(p => p.StringSizeLimit != null && p.StringSizeLimit >= threshold).ToArray();

			var propertiesRetrieved = Helper.Properties.Read(filter);

			using (new AssertionScope())
			{
				propertiesRetrieved.Should().NotBeNull();
				propertiesRetrieved.Count().Should().Be(expected.Length);
			}
		}

		[TestMethod]
		public void PropertyDomRepository_ReadFilter_IsMultiLineString_Equal()
		{
			Helper.PopulateProperties();

			var filter = PropertyExposers.IsMultiLineString.Equal(true);
			var expected = DemoData.Properties.Where(p => p.IsMultiLineString).ToArray();

			var propertiesRetrieved = Helper.Properties.Read(filter);

			using (new AssertionScope())
			{
				propertiesRetrieved.Should().NotBeNull();
				propertiesRetrieved.Count().Should().Be(expected.Length);
				propertiesRetrieved.Should().OnlyContain(p => p.IsMultiLineString);
			}
		}

		[TestMethod]
		public void PropertyDomRepository_ReadFilter_SectionName_Equal()
		{
			Helper.PopulateProperties();

			var sectionName = "General";
			var filter = PropertyExposers.Layout.SectionName.Equal(sectionName);
			var expected = DemoData.Properties.Where(p => p.Layout?.SectionName == sectionName).ToArray();

			var propertiesRetrieved = Helper.Properties.Read(filter);

			using (new AssertionScope())
			{
				propertiesRetrieved.Should().NotBeNull();
				propertiesRetrieved.Count().Should().Be(expected.Length);
			}
		}

		[TestMethod]
		public void PropertyDomRepository_ReadFilter_Order_LessThanOrEqual()
		{
			Helper.PopulateProperties();

			long threshold = 1;
			var filter = PropertyExposers.Layout.Order.LessThanOrEqual(threshold);
			var expected = DemoData.Properties.Where(p => p.Layout?.Order != null && p.Layout.Order <= threshold).ToArray();

			var propertiesRetrieved = Helper.Properties.Read(filter);

			using (new AssertionScope())
			{
				propertiesRetrieved.Should().NotBeNull();
				propertiesRetrieved.Count().Should().Be(expected.Length);
			}
		}

		[TestMethod]
		public void PropertyDomRepository_ReadFilter_ScopeAndPropertyType_Equal()
		{
			Helper.PopulateProperties();

			var scope = "Facility";
			var type = InfraopsProperties.Enums.PropertyTypeEnum.Discrete;

			var combinedFilter = PropertyExposers.Scope.Equal(scope)
				.AND(PropertyExposers.PropertyType.Equal(type));

			var propertiesRetrieved = Helper.Properties.Read(combinedFilter);
			var expected = DemoData.Properties.Where(p => p.Scope == scope && p.PropertyType == type).ToArray();

			using (new AssertionScope())
			{
				propertiesRetrieved.Should().NotBeNull();
				propertiesRetrieved.Count().Should().Be(expected.Length);
			}
		}

		[TestMethod]
		public void PropertyDomRepository_ReadFilter_Options_Contains()
		{
			Helper.PopulateProperties();

			var filter = PropertyExposers.Discreets.Option.Contains("APAC");
			var expected = DemoData.Properties.Where(p => p.Discreets != null && p.Discreets.Any(o => o.Option == "APAC")).ToArray();

			var propertiesRetrieved = Helper.Properties.Read(filter);

			using (new AssertionScope())
			{
				propertiesRetrieved.Should().NotBeNull();
				propertiesRetrieved.Count().Should().Be(expected.Length);
				propertiesRetrieved.Should().OnlyContain(p => p.Discreets.Any(o => o.Option == "APAC"));
			}
		}

		[TestMethod]
		public void PropertyDomRepository_ReadFilter_Name_Contains()
		{
			Helper.PopulateProperties();

			var filter = PropertyExposers.Name.Contains("Owner", StringComparison.OrdinalIgnoreCase);
			var expected = DemoData.Properties.Where(p => p.Name.Contains("Owner", StringComparison.OrdinalIgnoreCase)).ToArray();

			var propertiesRetrieved = Helper.Properties.Read(filter);

			using (new AssertionScope())
			{
				propertiesRetrieved.Should().NotBeNull();
				propertiesRetrieved.Count().Should().Be(expected.Length);
			}
		}
	}
}
