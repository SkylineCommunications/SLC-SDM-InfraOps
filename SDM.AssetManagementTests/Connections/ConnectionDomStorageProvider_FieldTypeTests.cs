namespace SDM.AssetManagement.Tests.Connections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.Net.Sections;
    using Skyline.DataMiner.Utils.DOM.UnitTesting;

    /// <summary>
    /// Verifies the test DOM mock rejects field values whose CLR type does not match the registered
    /// <see cref="FieldDescriptor.FieldType"/> (ITM-001). Raw <see cref="DomInstance"/>s are used so the
    /// (already fixed) mapper is bypassed.
    /// </summary>
    [TestClass]
    public class ConnectionDomStorageProvider_FieldTypeTests : BaseRepositoryTest
    {
        private DomHelper _dom = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _dom = new DomHelper(((TestApiHelper)Helper).Connection.HandleMessages, SlcAsset_Management.ModuleId);
        }

        [TestMethod]
        public void Create_WithStringInGuidField_ShouldThrowTypeMismatch()
        {
            var instance = NewConnectionInstance();
            SourceInfo(instance).AddOrUpdateValue<string>(SlcAsset_Management.Sections.SourceInfo.SourcePort, "not-a-guid");

            Action act = () => _dom.DomInstances.Create(instance);

            act.Should().Throw<DomFieldValueTypeMismatchException>()
                .Which.Message.Should()
                .Contain("DomInstanceSectionInvalidFieldValueTypes").And
                .Contain("Source Info").And
                .Contain("Source Port").And
                .Contain("System.Guid").And
                .Contain("System.String");
        }

        [TestMethod]
        public void Create_WithGuidInGuidField_ShouldSucceed()
        {
            var instance = NewConnectionInstance();
            var port = Guid.NewGuid();
            SourceInfo(instance).AddOrUpdateValue<Guid>(SlcAsset_Management.Sections.SourceInfo.SourcePort, port);

            _dom.DomInstances.Create(instance);

            var persisted = _dom.DomInstances.Read(DomInstanceExposers.Id.Equal(instance.ID.Id)).Single();
            SourceInfo(persisted).GetValue<Guid>(SlcAsset_Management.Sections.SourceInfo.SourcePort).Value.Should().Be(port);
        }

        [TestMethod]
        public void Update_WithStringInGuidField_ShouldThrowTypeMismatch()
        {
            var instance = NewConnectionInstance();
            SourceInfo(instance).AddOrUpdateValue<Guid>(SlcAsset_Management.Sections.SourceInfo.SourcePort, Guid.NewGuid());
            var created = _dom.DomInstances.Create(instance);

            var cable = new Section(SlcAsset_Management.Sections.CableInformation.Id);
            cable.AddOrUpdateValue<string>(SlcAsset_Management.Sections.CableInformation.CableType, "x");
            created.Sections.Add(cable);

            Action act = () => _dom.DomInstances.Update(created);

            act.Should().Throw<DomFieldValueTypeMismatchException>()
                .Which.Message.Should()
                .Contain("Cable Information").And
                .Contain("Cable Type");
        }

        [TestMethod]
        public void CreateOrUpdate_Bulk_WithOneBadInstance_ShouldThrowBeforePersisting()
        {
            var good = NewConnectionInstance();
            SourceInfo(good).AddOrUpdateValue<Guid>(SlcAsset_Management.Sections.SourceInfo.SourcePort, Guid.NewGuid());

            var bad = NewConnectionInstance();
            var destination = new Section(SlcAsset_Management.Sections.DestinationInfo.Id);
            destination.AddOrUpdateValue<string>(SlcAsset_Management.Sections.DestinationInfo.DestinationPort, "bad");
            bad.Sections.Add(destination);

            Action act = () => _dom.DomInstances.CreateOrUpdate(new List<DomInstance> { good, bad });

            act.Should().Throw<DomFieldValueTypeMismatchException>();
            _dom.DomInstances.Read(DomInstanceExposers.DomDefinitionId.Equal(SlcAsset_Management.Definitions.Connections.Id))
                .Should().BeEmpty("validation runs before the inner mock persists any instance of the batch");
        }

        [TestMethod]
        public void Create_WithNullGuidField_ShouldNotThrow()
        {
            var instance = NewConnectionInstance();
            SourceInfo(instance).AddOrReplaceFieldValue(new FieldValue(SlcAsset_Management.Sections.SourceInfo.SourcePort) { Value = null });

            Action act = () => _dom.DomInstances.Create(instance);

            act.Should().NotThrow();
        }

        [TestMethod]
        public void Create_InSectionWithoutDefinition_ShouldNotValidate()
        {
            var bareHandler = new DomSLNetMessageHandler();
            var connection = ConnectionHelper.CreateConnection(bareHandler);
            var dom = new DomHelper(connection.HandleMessages, SlcAsset_Management.ModuleId);

            var instance = NewConnectionInstance();
            SourceInfo(instance).AddOrUpdateValue<string>(SlcAsset_Management.Sections.SourceInfo.SourcePort, "not-a-guid");

            Action act = () => dom.DomInstances.Create(instance);

            act.Should().NotThrow();
        }

        private static DomInstance NewConnectionInstance()
        {
            var instance = new DomInstance
            {
                ID = new DomInstanceId(Guid.NewGuid()) { ModuleId = SlcAsset_Management.ModuleId },
                DomDefinitionId = SlcAsset_Management.Definitions.Connections,
            };
            instance.Sections.Add(new Section(SlcAsset_Management.Sections.SourceInfo.Id));
            return instance;
        }

        private static Section SourceInfo(DomInstance instance)
        {
            return instance.Sections.Single(s => s.SectionDefinitionID.Equals(SlcAsset_Management.Sections.SourceInfo.Id));
        }
    }
}
