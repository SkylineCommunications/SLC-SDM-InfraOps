namespace SDM.InfraOpsProperties.Tests.Setup
{
	using System;
	using System.Collections.Generic;
	using System.Linq;

	using Microsoft.VisualStudio.TestTools.UnitTesting;

	using SharedMappers.DomIds;

	/// <summary>
	/// Tests to validate DemoData templates and population process.
	/// If these tests fail, fix the demo data templates in DemoData.cs.
	/// </summary>
	[TestClass]
	public class DemoDataValidationTests : BaseRepositoryTest
	{
		/// <summary>
		/// Main validation test - ensures all demo data can be populated through validation middleware.
		/// If this fails, validation middleware caught an issue during Create().
		/// </summary>
		[TestMethod]
		public void DemoData_ShouldPopulateWithoutValidationErrors()
		{
			// Act - This will throw if validation middleware fails
			Helper.PopulateProperties();
			Helper.PopulatePropertyValues();

			// Assert
			Assert.IsTrue(Helper.Properties.Read(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.InfraOpsProperties.Models.Property>()).Any(), "Properties should be populated");
			Assert.IsTrue(Helper.PropertyValues.Read(new Skyline.DataMiner.Net.Messages.SLDataGateway.TRUEFilterElement<Skyline.DataMiner.SDM.InfraOpsProperties.Models.PropertyValues>()).Any(), "PropertyValues should be populated");
		}

		/// <summary>
		/// Validates Property templates have required fields before population.
		/// </summary>
		[TestMethod]
		public void DemoData_Properties_ShouldHaveRequiredFields()
		{
			var properties = DemoData.Properties.ToList();

			if (!properties.Any())
			{
				Assert.Inconclusive("No properties in DemoData to validate");
				return;
			}

			for (int i = 0; i < properties.Count; i++)
			{
				var property = properties[i];
				Assert.IsFalse(string.IsNullOrWhiteSpace(property.Name), $"Property at index {i}: Name should not be empty");
				Assert.IsFalse(string.IsNullOrWhiteSpace(property.Scope), $"Property at index {i}: Scope should not be empty");
			}
		}

		/// <summary>
		/// Checks for duplicate (Scope, Name) pairs in Property templates, which would fail
		/// GetByScopeAndName lookups and any future uniqueness validation.
		/// </summary>
		[TestMethod]
		public void DemoData_Properties_ShouldHaveUniqueScopeAndName()
		{
			var properties = DemoData.Properties.ToList();

			if (!properties.Any())
			{
				Assert.Inconclusive("No properties in DemoData to check");
				return;
			}

			var duplicates = properties
				.GroupBy(p => (p.Scope, p.Name), StringComparerTuple.Instance)
				.Where(g => g.Count() > 1)
				.ToList();

			if (duplicates.Any())
			{
				var duplicateList = string.Join(", ", duplicates.Select(g => $"'{g.Key.Scope}/{g.Key.Name}' ({g.Count()}x)"));
				Assert.Fail($"Found {duplicates.Count} duplicate Scope/Name pair(s) in demo data templates: {duplicateList}");
			}
		}

		/// <summary>
		/// Ensures Discrete Properties define Options, and non-Discrete Properties don't -
		/// otherwise DemoData_ShouldPopulateWithoutValidationErrors would fail with a validation error.
		/// </summary>
		[TestMethod]
		public void DemoData_DiscreteProperties_MustHaveOptionsAndOthersMustNot()
		{
			var properties = DemoData.Properties.ToList();

			var invalid = properties
				.Where(p => (p.PropertyType == InfraopsProperties.Enums.PropertyTypeEnum.Discrete) != (p.Options?.Any() == true))
				.ToList();

			if (invalid.Any())
			{
				var invalidList = string.Join(", ", invalid.Select(p => $"'{p.Scope}/{p.Name}' (Type: {p.PropertyType}, OptionsCount: {p.Options?.Count ?? 0})"));
				Assert.Fail($"Found {invalid.Count} Property(ies) with inconsistent PropertyType/Options: {invalidList}");
			}
		}

		/// <summary>
		/// Ensures every PropertyValue entry in the PropertyValuesList demo data references a
		/// PropertyName that actually exists in the Properties demo data for the same Scope -
		/// otherwise the fixture would silently reference a Property that doesn't exist.
		/// </summary>
		[TestMethod]
		public void DemoData_PropertyValues_MustReferenceExistingPropertyNamesForTheirScope()
		{
			var properties = DemoData.Properties.ToList();
			var propertyValuesList = DemoData.PropertyValuesList.ToList();

			if (!propertyValuesList.Any())
			{
				Assert.Inconclusive("No PropertyValues in DemoData to validate");
				return;
			}

			var errors = new System.Collections.Generic.List<string>();

			foreach (var propertyValues in propertyValuesList)
			{
				foreach (var value in propertyValues.Values)
				{
					var matchingProperty = properties.FirstOrDefault(p =>
						string.Equals(p.Name, value.PropertyName, StringComparison.OrdinalIgnoreCase) &&
						string.Equals(p.Scope, propertyValues.Scope, StringComparison.OrdinalIgnoreCase));

					if (matchingProperty == null)
					{
						errors.Add($"PropertyValues '{propertyValues.Identifier}' (Scope: {propertyValues.Scope}) references unknown Property '{value.PropertyName}'");
					}
				}
			}

			if (errors.Any())
			{
				Assert.Fail($"Found {errors.Count} PropertyValues entry(ies) referencing unknown Properties:\n{string.Join("\n", errors)}");
			}
		}

		/// <summary>
		/// Helper comparer used to group demo Properties by (Scope, Name) using ordinal case-insensitive comparison.
		/// </summary>
		private sealed class StringComparerTuple : IEqualityComparer<(string Scope, string Name)>
		{
			public static readonly StringComparerTuple Instance = new StringComparerTuple();

			public bool Equals((string Scope, string Name) x, (string Scope, string Name) y)
			{
				return string.Equals(x.Scope, y.Scope, StringComparison.OrdinalIgnoreCase) &&
					string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
			}

			public int GetHashCode((string Scope, string Name) obj)
			{
				unchecked
				{
					int hash = 17;
					hash = (hash * 23) + (obj.Scope?.ToUpperInvariant().GetHashCode() ?? 0);
					hash = (hash * 23) + (obj.Name?.ToUpperInvariant().GetHashCode() ?? 0);
					return hash;
				}
			}
		}
	}
}
