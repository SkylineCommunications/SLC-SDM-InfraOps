namespace SDM.PlanAndBuild.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;

    /// <summary>
    /// Reflection-driven coverage of GetHashCode() for every model class in the
    /// Skyline.DataMiner.SDM.PlanAndBuild.Models namespace. Guards against regressions such
    /// as calling GetHashCode() on a default (null-Identifier) SdmObjectReference&lt;T&gt; property.
    /// </summary>
    [TestClass]
    public class ModelHashCodeTests
    {
        public static IEnumerable<object[]> ModelTypes()
        {
            return GetModelTypes().Select(t => new object[] { t }).ToList();
        }

        public static IEnumerable<object[]> SdmObjectReferenceProperties()
        {
            return GetModelTypes()
                .SelectMany(t => GetSdmObjectReferenceProperties(t).Select(p => new object[] { t, p.Name }))
                .ToList();
        }

        [DataTestMethod]
        [DynamicData(nameof(ModelTypes))]
        public void GetHashCode_ShouldNotThrow_ForDefaultInstance(Type modelType)
        {
            object instance = Activator.CreateInstance(modelType)!;

            Action act = () => instance.GetHashCode();

            act.Should().NotThrow($"{modelType.Name}.GetHashCode() must handle default/null property values safely");
        }

        [DataTestMethod]
        [DynamicData(nameof(ModelTypes))]
        public void GetHashCode_ShouldBeIdempotent_ForSameInstance(Type modelType)
        {
            object instance = Activator.CreateInstance(modelType)!;

            int first = instance.GetHashCode();
            int second = instance.GetHashCode();

            second.Should().Be(first, $"{modelType.Name}.GetHashCode() must return a stable value across calls");
        }

        [DataTestMethod]
        [DynamicData(nameof(ModelTypes))]
        public void GetHashCode_ShouldBeConsistentWithEquals_ForTwoDefaultInstances(Type modelType)
        {
            object first = Activator.CreateInstance(modelType)!;
            object second = Activator.CreateInstance(modelType)!;

            bool areEqual;
            try
            {
                areEqual = first.Equals(second);
            }
            catch (Exception ex)
            {
                Assert.Inconclusive($"{modelType.Name}: Equals(object) threw {ex.GetType().Name} on two default instances; skipping hash/equals consistency check.");
                return;
            }

            if (!areEqual)
            {
                Assert.Inconclusive($"{modelType.Name}: two default instances are not equal; hash/equals consistency check not applicable.");
                return;
            }

            first.GetHashCode().Should().Be(second.GetHashCode(), $"{modelType.Name}: equal instances must produce equal hash codes");
        }

        [DataTestMethod]
        [DynamicData(nameof(SdmObjectReferenceProperties))]
        public void GetHashCode_ShouldTreatNullIdentifierSafely_ForSdmObjectReferenceProperty(Type modelType, string propertyName)
        {
            PropertyInfo property = modelType.GetProperty(propertyName)!;
            Type referenceType = property.PropertyType;

            object withNullIdentifier = Activator.CreateInstance(modelType)!;
            property.SetValue(withNullIdentifier, Activator.CreateInstance(referenceType));

            Action act = () => withNullIdentifier.GetHashCode();
            act.Should().NotThrow($"{modelType.Name}.{propertyName} must tolerate a null Identifier in GetHashCode()");

            string identifier = Guid.NewGuid().ToString();
            object first = Activator.CreateInstance(modelType)!;
            object second = Activator.CreateInstance(modelType)!;
            property.SetValue(first, Activator.CreateInstance(referenceType, identifier));
            property.SetValue(second, Activator.CreateInstance(referenceType, identifier));

            first.GetHashCode().Should().Be(second.GetHashCode(), $"{modelType.Name}.{propertyName}: equal identifiers must produce equal hash codes");
        }

        private static IEnumerable<Type> GetModelTypes()
        {
            return typeof(PlanAndBuildJob).Assembly
                .GetTypes()
                .Where(t => t.IsClass
                    && t.IsPublic
                    && !t.IsAbstract
                    && !t.IsGenericTypeDefinition
                    && t.Namespace == "Skyline.DataMiner.SDM.PlanAndBuild.Models"
                    && t.GetConstructor(Type.EmptyTypes) != null
                    && t.GetMethod(nameof(GetHashCode), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly) != null)
                .OrderBy(t => t.Name);
        }

        private static IEnumerable<PropertyInfo> GetSdmObjectReferenceProperties(Type modelType)
        {
            return modelType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead
                    && p.CanWrite
                    && p.PropertyType.IsGenericType
                    && p.PropertyType.GetGenericTypeDefinition() == typeof(SdmObjectReference<>));
        }
    }
}
