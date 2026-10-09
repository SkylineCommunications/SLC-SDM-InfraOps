namespace SDM.AssetManagement.Tests.Histories
{
    using System;
    using System.Linq;

    using FluentAssertions;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.Net.Sections;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;

    [TestClass]
    public class HistoryDomRepositoryTests_StringInstanceId : BaseRepositoryTest
    {
        private DomHelper _dom = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _dom = new DomHelper(((TestApiHelper)Helper).Connection.HandleMessages, SlcAsset_Management.ModuleId);
        }

        [DataTestMethod]
        [DataRow("A10588B4-848F-45B1-B2BA-8B2C27D9BFA3")]
        [DataRow("{a10588b4-848f-45b1-b2ba-8b2c27d9bfa3}")]
        public void Create_ModifiedInstanceId_PersistsStringWrapper(string identifier)
        {
            var history = new History
            {
                HistoryInfo = { ModifiedInstanceID = new ISdmObjectReference<ISdmObject>(identifier) },
            };

            var created = Helper.AssetManagement.Histories.Create(history);
            AssertStoredIdentifier(created.Identifier, identifier);
            created.HistoryInfo.ModifiedInstanceID.Identifier.Should().Be(identifier);
        }

        [TestMethod]
        public void Update_ModifiedInstanceId_PersistsStringWrapper()
        {
            var created = Helper.AssetManagement.Histories.Create(new History
            {
                HistoryInfo = { ModifiedInstanceID = new ISdmObjectReference<ISdmObject>(Guid.NewGuid().ToString()) },
            });
            const string updatedIdentifier = "{A10588B4-848F-45B1-B2BA-8B2C27D9BFA3}";
            created.HistoryInfo.ModifiedInstanceID = new ISdmObjectReference<ISdmObject>(updatedIdentifier);

            var updated = Helper.AssetManagement.Histories.Update(created);

            AssertStoredIdentifier(updated.Identifier, updatedIdentifier);
            updated.HistoryInfo.ModifiedInstanceID.Identifier.Should().Be(updatedIdentifier);
        }

        [DataTestMethod]
        [DataRow("A10588B4-848F-45B1-B2BA-8B2C27D9BFA3")]
        [DataRow("asset/external-42")]
        public void Read_StringBackedHistory_PreservesReferenceIdentifier(string identifier)
        {
            var instance = SeedHistory(identifier);

            var history = Helper.AssetManagement.Histories
                .Read(HistoryExposers.Identifier.Equal(instance.ID.Id.ToString()))
                .Single();

            history.HistoryInfo.ModifiedInstanceID.Identifier.Should().Be(identifier);
            history.IsNew.Should().BeFalse();
            history.Changed.Should().BeFalse();
        }

        [DataTestMethod]
        [DataRow("a10588b4-848f-45b1-b2ba-8b2c27d9bfa3")]
        [DataRow("{A10588B4-848F-45B1-B2BA-8B2C27D9BFA3}")]
        public void Read_ModifiedInstanceIdFilters_MatchStringBackedField(string identifier)
        {
            var matching = SeedHistory(identifier);
            var other = SeedHistory(Guid.NewGuid().ToString());
            var missing = SeedHistory(null);
            var reference = new ISdmObjectReference<ISdmObject>(identifier);

            Helper.AssetManagement.Histories
                .Read(HistoryExposers.HistoryInfo.ModifiedInstanceID.Equal(reference))
                .Select(history => history.Identifier)
                .Should().Equal(matching.ID.Id.ToString());

            Helper.AssetManagement.Histories
                .Read(HistoryExposers.HistoryInfo.ModifiedInstanceID.Equal(identifier))
                .Select(history => history.Identifier)
                .Should().Equal(matching.ID.Id.ToString());

            Helper.AssetManagement.Histories
                .Read(HistoryExposers.HistoryInfo.ModifiedInstanceID.NotEqual(reference))
                .Select(history => history.Identifier)
                .Should().Equal(other.ID.Id.ToString());

            Helper.AssetManagement.Histories
                .Read(HistoryExposers.HistoryInfo.ModifiedInstanceID.Equal(String.Empty))
                .Select(history => history.Identifier)
                .Should().Equal(missing.ID.Id.ToString());

            Helper.AssetManagement.Histories
                .Read(HistoryExposers.HistoryInfo.ModifiedInstanceID.NotEqual(String.Empty))
                .Select(history => history.Identifier)
                .Should().BeEquivalentTo(matching.ID.Id.ToString(), other.ID.Id.ToString());
        }

        private DomInstance SeedHistory(string? identifier)
        {
            var section = new Section(SlcAsset_Management.Sections.HistoryInfo.Id);
            section.AddOrUpdateValue<string>(SlcAsset_Management.Sections.HistoryInfo.Description, "String-backed history");
            if (identifier != null)
            {
                section.AddOrUpdateValue<string>(SlcAsset_Management.Sections.HistoryInfo.InstanceID, identifier);
            }

            var instance = new DomInstance
            {
                ID = new DomInstanceId(Guid.NewGuid()) { ModuleId = SlcAsset_Management.ModuleId },
                DomDefinitionId = SlcAsset_Management.Definitions.History,
            };
            instance.Sections.Add(section);
            return _dom.DomInstances.Create(instance);
        }

        private void AssertStoredIdentifier(string historyId, string identifier)
        {
            var instance = _dom.DomInstances.Read(DomInstanceExposers.Id.Equal(Guid.Parse(historyId))).Single();
            var section = instance.Sections.Single(s => s.SectionDefinitionID.Equals(SlcAsset_Management.Sections.HistoryInfo.Id));
            var field = section.FieldValues.Single(f => f.FieldDescriptorID.Equals(SlcAsset_Management.Sections.HistoryInfo.InstanceID));

            field.Value.Should().BeOfType<ValueWrapper<string>>();
            section.GetValue<string>(SlcAsset_Management.Sections.HistoryInfo.InstanceID).Value.Should().Be(identifier);
        }
    }
}
