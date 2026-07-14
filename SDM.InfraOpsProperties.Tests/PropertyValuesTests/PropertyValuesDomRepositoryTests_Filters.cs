namespace SDM.InfraOpsProperties.Tests.PropertyValuesTests
{
	using System.Linq;

	using FluentAssertions;
	using FluentAssertions.Execution;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SDM.InfraOpsProperties.Tests.Setup;

	using Skyline.DataMiner.Net.Messages.SLDataGateway;
	using Skyline.DataMiner.SDM;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Models;

	public partial class PropertyValuesDomRepositoryTests
	{
		[TestMethod]
		public void PropertyValuesDomRepository_ReadFilter_LinkedObjectID_Equal()
		{
			Helper.PopulatePropertyValues();

			var expected = DemoData.PropertyValuesList[0];
			var filter = PropertyValuesExposers.LinkedObjectID.Equal(expected.LinkedObjectID);

			var retrieved = Helper.PropertyValues.Read(filter);

			using (new AssertionScope())
			{
				retrieved.Should().NotBeNull();
				retrieved.Count().Should().Be(1);
				retrieved.First().Identifier.Should().Be(expected.Identifier);
			}
		}

		[TestMethod]
		public void PropertyValuesDomRepository_ReadFilter_Scope_Equal()
		{
			Helper.PopulatePropertyValues();

			var scope = "Facility";
			var filter = PropertyValuesExposers.Scope.Equal(scope);
			var expected = DemoData.PropertyValuesList.Where(p => p.Scope == scope).ToArray();

			var retrieved = Helper.PropertyValues.Read(filter);

			using (new AssertionScope())
			{
				retrieved.Should().NotBeNull();
				retrieved.Count().Should().Be(expected.Length);
			}
		}

		[TestMethod]
		public void PropertyValuesDomRepository_ReadFilter_SubID_Equal()
		{
			Helper.PopulatePropertyValues();

			var subId = "Rack-1";
			var filter = PropertyValuesExposers.SubID.Equal(subId);
			var expected = DemoData.PropertyValuesList.Where(p => p.SubID == subId).ToArray();

			var retrieved = Helper.PropertyValues.Read(filter);

			using (new AssertionScope())
			{
				retrieved.Should().NotBeNull();
				retrieved.Count().Should().Be(expected.Length);
			}
		}

		[TestMethod]
		public void PropertyValuesDomRepository_ReadFilter_ValuesPropertyName_Equal()
		{
			Helper.PopulatePropertyValues();

			var filter = PropertyValuesExposers.Values.PropertyName.Equal("Region");
			var expected = DemoData.PropertyValuesList.Where(p => p.Values.Any(v => v.PropertyName == "Region")).ToArray();

			var retrieved = Helper.PropertyValues.Read(filter);

			using (new AssertionScope())
			{
				retrieved.Should().NotBeNull();
				retrieved.Count().Should().Be(expected.Length);
				retrieved.Should().OnlyContain(pv => pv.Values.Any(v => v.PropertyName == "Region"));
			}
		}

		[TestMethod]
		public void PropertyValuesDomRepository_ReadFilter_ValuesValue_Equal()
		{
			Helper.PopulatePropertyValues();

			var filter = PropertyValuesExposers.Values.Value.Equal("EMEA");
			var expected = DemoData.PropertyValuesList.Where(p => p.Values.Any(v => v.Value == "EMEA")).ToArray();

			var retrieved = Helper.PropertyValues.Read(filter);

			using (new AssertionScope())
			{
				retrieved.Should().NotBeNull();
				retrieved.Count().Should().Be(expected.Length);
			}
		}

		[TestMethod]
		public void PropertyValuesDomRepository_ReadFilter_ValuesPropertyId_Contains()
		{
			Helper.PopulatePropertyValues();

			var propertyReference = new SdmObjectReference<Property>(DemoData.Properties[2].Identifier);
			var filter = PropertyValuesExposers.Values.PropertyId.Contains(propertyReference);
			var expected = DemoData.PropertyValuesList.Where(p => p.Values.Any(v => v.PropertyId != null && v.PropertyId.Identifier == DemoData.Properties[2].Identifier)).ToArray();

			var retrieved = Helper.PropertyValues.Read(filter);

			using (new AssertionScope())
			{
				retrieved.Should().NotBeNull();
				retrieved.Count().Should().Be(expected.Length);
			}
		}

		[TestMethod]
		public void PropertyValuesDomRepository_ReadFilter_ScopeAndLinkedObjectID_Equal()
		{
			Helper.PopulatePropertyValues();

			var expected = DemoData.PropertyValuesList[2];
			var filter = PropertyValuesExposers.Scope.Equal(expected.Scope)
				.AND(PropertyValuesExposers.LinkedObjectID.Equal(expected.LinkedObjectID));

			var retrieved = Helper.PropertyValues.Read(filter);

			using (new AssertionScope())
			{
				retrieved.Should().NotBeNull();
				retrieved.Count().Should().Be(1);
				retrieved.First().Identifier.Should().Be(expected.Identifier);
			}
		}
	}
}
