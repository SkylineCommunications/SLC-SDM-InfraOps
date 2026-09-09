namespace SDM.AssetManagement.Tests.Histories
{
    using System;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    [TestClass]
    public class HistoryDomRepositoryTests_CRUD : BaseRepositoryTest
    {
        private History _referenceHistory = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _referenceHistory = CreateHistory(
                "Asset created",
                SlcAsset_Management.Enums.TypeOfHistoryEnum.Add);
        }

        [TestMethod]
        public void Create_WithValidData_PersistsAllFields()
        {
            var created = Helper.AssetManagement.Histories.Create(_referenceHistory);

            AssertHistory(created, _referenceHistory);
            created.IsNew.Should().BeFalse();
            Helper.AssetManagement.Histories.Count(new TRUEFilterElement<History>()).Should().Be(1);
        }

        [TestMethod]
        public void Update_WithChangedData_PersistsChanges()
        {
            var created = Helper.AssetManagement.Histories.Create(_referenceHistory);
            created.HistoryInfo.Description = "Asset modified";
            created.HistoryInfo.Job = Guid.NewGuid();
            created.HistoryInfo.ModifiedInstanceID = Guid.NewGuid().ToString();
            created.HistoryInfo.ModifiedInstanceDefinitionID = Guid.NewGuid().ToString();
            created.HistoryInfo.ExtraInfo = "{\"field\":\"Name\"}";
            created.HistoryInfo.TypeOfHistory = SlcAsset_Management.Enums.TypeOfHistoryEnum.Modification;

            var updated = Helper.AssetManagement.Histories.Update(created);

            AssertHistory(updated, created);
            Helper.AssetManagement.Histories.Count(new TRUEFilterElement<History>()).Should().Be(1);
        }

        [TestMethod]
        public void CreateOrUpdate_WithNewAndExistingHistories_PersistsBoth()
        {
            var existing = Helper.AssetManagement.Histories.Create(_referenceHistory);
            existing.HistoryInfo.Description = "Updated existing history";
            var added = CreateHistory(
                "Asset removed",
                SlcAsset_Management.Enums.TypeOfHistoryEnum.Removal);

            var persisted = Helper.AssetManagement.Histories.CreateOrUpdate([existing, added]);

            persisted.Should().HaveCount(2);
            Helper.AssetManagement.Histories.Count(new TRUEFilterElement<History>()).Should().Be(2);
            Helper.AssetManagement.Histories
                .Read(HistoryExposers.HistoryInfo.Description.Equal("Updated existing history"))
                .Should()
                .ContainSingle();
        }

        [TestMethod]
        public void ReadPaged_WithMultipleHistories_ReturnsExpectedPages()
        {
            var histories = Enumerable.Range(1, 5)
                .Select(index => CreateHistory(
                    $"History {index}",
                    SlcAsset_Management.Enums.TypeOfHistoryEnum.Add))
                .ToArray();
            Helper.AssetManagement.Histories.Create(histories);

            var pages = Helper.AssetManagement.Histories
                .ReadPaged(new TRUEFilterElement<History>(), 2)
                .ToList();

            pages.Should().HaveCount(3);
            pages.SelectMany(page => page).Should().HaveCount(5);
            pages.Should().OnlyContain(page => page.Count <= 2);
        }

        [TestMethod]
        public void Delete_SingleAndBulk_RemovesHistories()
        {
            var histories = Helper.AssetManagement.Histories.Create(
            [
                _referenceHistory,
                CreateHistory("Asset modified", SlcAsset_Management.Enums.TypeOfHistoryEnum.Modification),
                CreateHistory("Asset removed", SlcAsset_Management.Enums.TypeOfHistoryEnum.Removal),
            ]).ToList();

            Helper.AssetManagement.Histories.Delete(histories[0]);
            Helper.AssetManagement.Histories.Delete(histories.Skip(1));

            Helper.AssetManagement.Histories.Count(new TRUEFilterElement<History>()).Should().Be(0);
        }

        private static History CreateHistory(
            string description,
            SlcAsset_Management.Enums.TypeOfHistoryEnum type)
        {
            return new History
            {
                Identifier = Guid.NewGuid().ToString(),
                HistoryInfo =
                {
                    Description = description,
                    Job = Guid.NewGuid(),
                    ModifiedInstanceID = Guid.NewGuid().ToString(),
                    ModifiedInstanceDefinitionID = Guid.NewGuid().ToString(),
                    ExtraInfo = "{\"source\":\"unit-test\"}",
                    TypeOfHistory = type,
                },
            };
        }

        private static void AssertHistory(History actual, History expected)
        {
            using (new AssertionScope())
            {
                actual.Identifier.Should().Be(expected.Identifier);
                actual.HistoryInfo.Description.Should().Be(expected.HistoryInfo.Description);
                actual.HistoryInfo.Job.Should().Be(expected.HistoryInfo.Job);
                actual.HistoryInfo.ModifiedInstanceID.Should().Be(expected.HistoryInfo.ModifiedInstanceID);
                actual.HistoryInfo.ModifiedInstanceDefinitionID.Should().Be(expected.HistoryInfo.ModifiedInstanceDefinitionID);
                actual.HistoryInfo.ExtraInfo.Should().Be(expected.HistoryInfo.ExtraInfo);
                actual.HistoryInfo.TypeOfHistory.Should().Be(expected.HistoryInfo.TypeOfHistory);
            }
        }
    }
}
