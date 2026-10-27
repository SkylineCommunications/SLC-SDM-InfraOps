namespace SDM.InfraOps.PackagingTests
{
    using System;
    using System.Linq;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.SDM.InfraOps.Core.Models;

    /// <summary>
    /// Guards the [PropertyScope] convention: every concrete SDM model in the Common assemblies must declare
    /// its Properties scope, equal to its class name, otherwise the generic Properties/PropertyValues lookups
    /// would throw at runtime.
    /// </summary>
    [TestClass]
    public class PropertyScopeConventionTests
    {
        private static readonly Assembly[] ModelAssemblies =
        [
            typeof(Skyline.DataMiner.SDM.InfraOpsProperties.Models.Property).Assembly,
            typeof(Skyline.DataMiner.SDM.FacilityManagement.Models.Facility).Assembly,
            typeof(Skyline.DataMiner.SDM.AssetManagement.Models.Asset).Assembly,
            typeof(Skyline.DataMiner.SDM.PlanAndBuild.Models.PlanAndBuildJob).Assembly,
        ];

        // Property and PropertyValues are the Properties module's own storage models; they can't carry properties themselves.
        private static readonly Type[] ExcludedModels =
        [
            typeof(Skyline.DataMiner.SDM.InfraOpsProperties.Models.Property),
            typeof(Skyline.DataMiner.SDM.InfraOpsProperties.Models.PropertyValues),
        ];

        [TestMethod]
        public void EverySdmModelShouldDeclarePropertyScopeEqualToItsClassName()
        {
            var models = ModelAssemblies.SelectMany(GetSdmModels).ToList();

            Assert.IsTrue(models.Count > 0, "No SDM models were found; the scan is broken.");

            var violations = models
                .Select(type => new { Type = type, Attribute = type.GetCustomAttribute<PropertyScopeAttribute>(inherit: false) })
                .Where(x => x.Attribute == null || x.Attribute.Scope != x.Type.Name)
                .Select(x => x.Attribute == null
                    ? $"  {x.Type.FullName}: missing [PropertyScope(\"{x.Type.Name}\")]"
                    : $"  {x.Type.FullName}: scope is '{x.Attribute.Scope}', expected '{x.Type.Name}'")
                .ToList();

            Assert.AreEqual(0, violations.Count, "SDM models violating the [PropertyScope] convention:\n" + string.Join("\n", violations));
        }

        [TestMethod]
        public void PropertyScopesShouldBeUnique()
        {
            var duplicates = ModelAssemblies
                .SelectMany(GetSdmModels)
                .Select(type => new { Type = type, type.GetCustomAttribute<PropertyScopeAttribute>(inherit: false)?.Scope })
                .Where(x => x.Scope != null)
                .GroupBy(x => x.Scope, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => $"  '{g.Key}': {string.Join(", ", g.Select(x => x.Type.FullName))}")
                .ToList();

            Assert.AreEqual(0, duplicates.Count, "Duplicate Properties scopes:\n" + string.Join("\n", duplicates));
        }

        private static System.Collections.Generic.IEnumerable<Type> GetSdmModels(Assembly assembly)
        {
            return assembly.GetTypes().Where(type => type.IsClass && !type.IsAbstract && !ExcludedModels.Contains(type) && InheritsFromSdmObjectBase(type));
        }

        private static bool InheritsFromSdmObjectBase(Type type)
        {
            for (var current = type.BaseType; current != null; current = current.BaseType)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(SdmObjectBase<>))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
