namespace SDM.AssetManagement.Tests.Histories
{
    using System;
    using System.Linq;

    using FluentAssertions;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;

    [TestClass]
    public class HistoryDomRepositoryTests_Filters : BaseRepositoryTest
    {
        private readonly ISdmObjectReference<ISdmObject> _targetJob = new ISdmObjectReference<ISdmObject>(Convert.ToString(Guid.NewGuid()));
        private readonly ISdmObjectReference<ISdmObject> _targetInstanceId = new ISdmObjectReference<ISdmObject>(Convert.ToString(Guid.NewGuid()));
        private readonly string _targetDefinitionId = Guid.NewGuid().ToString();

        [TestInitialize]
        public void TestInitialize()
        {
            Helper.AssetManagement.Histories.Create(
            [
                CreateHistory(
                    "Asset Alpha created",
                    _targetJob,
                    _targetInstanceId,
                    _targetDefinitionId,
                    "alpha details",
                    SlcAsset_Management.Enums.TypeOfHistoryEnum.Add),
                CreateHistory(
                    "Asset Beta modified",
                    new ISdmObjectReference<ISdmObject>(Convert.ToString(Guid.NewGuid())),
                    new ISdmObjectReference<ISdmObject>(Convert.ToString(Guid.NewGuid())),
                    Convert.ToString(Guid.NewGuid()),
                    "beta details",
                    SlcAsset_Management.Enums.TypeOfHistoryEnum.Modification),
                CreateHistory(
                    "Asset Gamma removed",
                    new ISdmObjectReference<ISdmObject>(Convert.ToString(Guid.NewGuid())),
                    new ISdmObjectReference<ISdmObject>(Convert.ToString(Guid.NewGuid())),
                    Convert.ToString(Guid.NewGuid()),
                    "gamma details",
                    SlcAsset_Management.Enums.TypeOfHistoryEnum.Removal),
            ]);
        }

        [TestMethod]
        public void Read_DescriptionContains_ReturnsMatchingHistory()
        {
            var results = Helper.AssetManagement.Histories
                .Read(HistoryExposers.HistoryInfo.Description.Contains("Beta"))
                .ToList();

            results.Should().ContainSingle();
            results[0].HistoryInfo.Description.Should().Be("Asset Beta modified");
        }

        [TestMethod]
        public void Read_JobEqual_ReturnsMatchingHistory()
        {
            var results = Helper.AssetManagement.Histories
                .Read(HistoryExposers.HistoryInfo.Job.Equal(_targetJob))
                .ToList();

            results.Should().ContainSingle();
            results[0].HistoryInfo.Job.Should().Be(_targetJob);
        }

        [TestMethod]
        public void Read_ModifiedInstanceEqual_ReturnsMatchingHistory()
        {
            var filter = HistoryExposers.HistoryInfo.ModifiedInstanceID.Equal(_targetInstanceId)
                .AND(HistoryExposers.HistoryInfo.ModifiedInstanceDefinitionID.Equal(_targetDefinitionId));

            var results = Helper.AssetManagement.Histories.Read(filter).ToList();

            results.Should().ContainSingle();
            results[0].HistoryInfo.ModifiedInstanceID.Should().Be(_targetInstanceId);
            results[0].HistoryInfo.ModifiedInstanceDefinitionID.Should().Be(_targetDefinitionId);
        }

        [TestMethod]
        public void Read_ExtraInfoContains_ReturnsMatchingHistory()
        {
            var results = Helper.AssetManagement.Histories
                .Read(HistoryExposers.HistoryInfo.ExtraInfo.Contains("gamma"))
                .ToList();

            results.Should().ContainSingle();
            results[0].HistoryInfo.ExtraInfo.Should().Be("gamma details");
        }

        [TestMethod]
        public void Read_TypeOfHistoryEqual_ReturnsMatchingHistory()
        {
            var results = Helper.AssetManagement.Histories
                .Read(HistoryExposers.HistoryInfo.TypeOfHistory.Equal(
                    SlcAsset_Management.Enums.TypeOfHistoryEnum.Modification))
                .ToList();

            results.Should().ContainSingle();
            results[0].HistoryInfo.TypeOfHistory
                .Should()
                .Be(SlcAsset_Management.Enums.TypeOfHistoryEnum.Modification);
        }

        private static History CreateHistory(
            string description,
            ISdmObjectReference<ISdmObject> job,
            ISdmObjectReference<ISdmObject> instanceId,
            string definitionId,
            string extraInfo,
            SlcAsset_Management.Enums.TypeOfHistoryEnum type)
        {
            return new History
            {
                Identifier = Guid.NewGuid().ToString(),
                HistoryInfo =
                {
                    Description = description,
                    Job = job,
                    ModifiedInstanceID = instanceId,
                    ModifiedInstanceDefinitionID = definitionId,
                    ExtraInfo = extraInfo,
                    TypeOfHistory = type,
                },
            };
        }
    }
}