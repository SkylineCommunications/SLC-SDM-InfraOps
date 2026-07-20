namespace SDM.InfraOps.PackagingTests
{
    using System.Linq;
    using System.Reflection;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    /// <summary>
    /// Validates that no public type is compiled into more than one SDM Common assembly.
    /// A failure here means a CS0433 compiler error will surface in any project that
    /// references multiple SDM Common packages simultaneously.
    ///
    /// This test acts as the acceptance criterion for SDM.InfraOps.Core (GitHub issue #9):
    /// once shared types live in Core, each public type appears in exactly one assembly.
    /// </summary>
    [TestClass]
    public class NuGetPackagingTests
    {
        // One anchor type per assembly — unique types that exist in exactly one package.
        // These are used solely to locate the assembly; they are never themselves ambiguous.
        private static readonly Assembly[] CommonAssemblies =
        [
            typeof(Skyline.DataMiner.SDM.PropertyDomRepository_Extensions).Assembly,   // SDM.InfraOpsProperties.Common
            typeof(Skyline.DataMiner.SDM.DeskDomRepository_Extensions).Assembly,        // SDM.FacilityManagement.Common
            typeof(Skyline.DataMiner.SDM.AssetDomRepository_Extensions).Assembly,       // SDM.AssetManagement.Common
            typeof(Skyline.DataMiner.SDM.JobTypeDomRepository_Extensions).Assembly,     // SDM.PlanAndBuild.Common
        ];

        [TestMethod]
        public void NoPublicTypeShouldExistInMultipleCommonAssemblies()
        {
            var duplicates = CommonAssemblies
                .SelectMany(a => a.GetExportedTypes()
                    .Select(t => new { TypeName = t.FullName, AssemblyName = a.GetName().Name }))
                .GroupBy(x => x.TypeName)
                .Where(g => g.Count() > 1)
                .Select(g => $"  {g.Key}\n    -> {string.Join("\n    -> ", g.Select(x => x.AssemblyName))}")
                .ToList();

            Assert.AreEqual(
                0,
                duplicates.Count,
                $"The following public types are compiled into multiple assemblies (CS0433 risk):\n{string.Join("\n", duplicates)}\n\n" +
                "Fix: move shared types into SDM.InfraOps.Core so each type lives in exactly one assembly.");
        }

        [TestMethod]
        public void AllFourCommonAssembliesShouldBeDistinct()
        {
            var names = CommonAssemblies.Select(a => a.GetName().Name).ToList();
            var distinct = names.Distinct().ToList();

            CollectionAssert.AreEquivalent(
                distinct,
                names,
                "Expected 4 distinct assemblies but got duplicates. Check ProjectReference anchors.");
        }
    }
}
