namespace SDM.AssetManagement.Tests.Histories
{
    using System;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.SDM.InfraOps.Core.Converters;

    [TestClass]
    public class HistoryJsonRoundTripTests
    {
        [TestMethod]
        public void History_RoundTrip_PreservesAllPropertiesAndReferences()
        {
            var history = new History
            {
                Identifier = Guid.NewGuid().ToString(),
            };
            history.HistoryInfo.Description = "Changed";
            history.HistoryInfo.Job = new ISdmObjectReference<ISdmObject>(Guid.NewGuid().ToString());
            history.HistoryInfo.ModifiedInstanceID = new ISdmObjectReference<ISdmObject>(Guid.NewGuid().ToString());
            history.HistoryInfo.ModifiedInstanceDefinitionID = "DefinitionId";
            history.HistoryInfo.ExtraInfo = "Extra";
            history.HistoryInfo.TypeOfHistory = SlcAsset_Management.Enums.TypeOfHistoryEnum.Modification;

            string json = InfraOpsJsonConvert.SerializeObject(history);
            var restored = InfraOpsJsonConvert.DeserializeObject<History>(json);

            restored.Identifier.Should().Be(history.Identifier);
            restored.HistoryInfo.Job.Should().Be(history.HistoryInfo.Job);
            restored.HistoryInfo.ModifiedInstanceID.Should().Be(history.HistoryInfo.ModifiedInstanceID);
            restored.HistoryInfo.TypeOfHistory.Should().Be(SlcAsset_Management.Enums.TypeOfHistoryEnum.Modification);
            restored.Should().Be(history);
            InfraOpsJsonConvert.SerializeObject(restored).Should().Be(json);
        }

        [TestMethod]
        public void History_AuditFields_ExportedButNotImported()
        {
            var history = new History
            {
                CreatedAt = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                CreatedBy = "creator",
                LastModified = new DateTime(2021, 6, 7, 8, 9, 10, DateTimeKind.Utc),
                LastModifiedBy = "modifier",
            };

            string json = InfraOpsJsonConvert.SerializeObject(history);
            var restored = InfraOpsJsonConvert.DeserializeObject<History>(json);

            json.Should().Contain("\"CreatedBy\":\"creator\"");
            json.Should().Contain("\"LastModifiedBy\":\"modifier\"");
            restored.CreatedAt.Should().Be(default(DateTime));
            restored.CreatedBy.Should().BeNull();
            restored.LastModified.Should().Be(default(DateTime));
            restored.LastModifiedBy.Should().BeNull();
        }

        [TestMethod]
        public void History_Serialize_WritesReferencesAsStringsAndEnumsAsNames()
        {
            var jobId = Guid.NewGuid().ToString();
            var instanceId = Guid.NewGuid().ToString();
            var history = new History();
            history.HistoryInfo.Job = new ISdmObjectReference<ISdmObject>(jobId);
            history.HistoryInfo.ModifiedInstanceID = new ISdmObjectReference<ISdmObject>(instanceId);
            history.HistoryInfo.TypeOfHistory = SlcAsset_Management.Enums.TypeOfHistoryEnum.Removal;

            string json = InfraOpsJsonConvert.SerializeObject(history);

            json.Should().Contain($"\"Job\":\"{jobId}\"");
            json.Should().Contain($"\"ModifiedInstanceID\":\"{instanceId}\"");
            json.Should().Contain("\"TypeOfHistory\":\"Removal\"");
        }

        [TestMethod]
        public void History_Serialize_EmptyReferencesAreNull()
        {
            string json = InfraOpsJsonConvert.SerializeObject(new History());

            json.Should().Contain("\"Job\":null");
            json.Should().Contain("\"ModifiedInstanceID\":null");
        }

        [TestMethod]
        public void History_Deserialize_NullAndEmptyReferencesBecomeDefault()
        {
            var restored = InfraOpsJsonConvert.DeserializeObject<History>(
                "{\"HistoryInfo\":{\"Job\":null,\"ModifiedInstanceID\":\"\"}}");

            restored.HistoryInfo.Job.Identifier.Should().BeNullOrEmpty();
            restored.HistoryInfo.ModifiedInstanceID.Identifier.Should().BeNullOrEmpty();
        }
    }
}
