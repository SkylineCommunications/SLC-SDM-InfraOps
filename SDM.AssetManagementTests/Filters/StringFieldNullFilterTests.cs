namespace SDM.AssetManagement.Tests.Filters
{
    using System;
    using System.Collections;
    using System.Linq;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SDM.AssetManagement.Tests.Setup;

    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    /// <summary>
    /// Null / "has value" filters on plain string exposers. The generated DOM repositories translate nullable,
    /// reference and file-reference fields into a DOM KeyExists filter, but plain string fields are translated into
    /// a DynamicManagedListFilter holding a null value. Real SLNet calls ToString() on every outgoing request
    /// (ConnectionInfo.AddPendingMessage), which throws a NullReferenceException inside DynamicManagedListFilter,
    /// so the read fails before it reaches the DMA. The default mock connection never calls ToString(), which is
    /// why the other filter tests do not catch it.
    /// </summary>
    [TestClass]
    public class StringFieldNullFilterTests
    {
        private ITestApiHelper _helper = null!;

        [TestInitialize]
        public void Initialize()
        {
            _helper = new TestApiHelper(ToStringCallingConnection.Wrap(ConnectionHelper.CreateConnection()));
            _helper.CleanupAllTestData();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _helper?.CleanupAllTestData();
        }

        #region Unguarded string fields (fail until the code generator emits a null guard)

        [TestMethod]
        public void DataPort_Ipv4Address_HasValue_ShouldNotThrow()
        {
            AssertReadDoesNotThrow(() => _helper.AssetManagement.DataPorts.Read(DataPortExposers.AddressInfo.Ipv4Address.HasValue()));
        }

        [TestMethod]
        public void DataPort_Ipv6Address_HasValue_ShouldNotThrow()
        {
            AssertReadDoesNotThrow(() => _helper.AssetManagement.DataPorts.Read(DataPortExposers.AddressInfo.Ipv6Address.HasValue()));
        }

        [TestMethod]
        public void DataPort_Hostname_HasNoValue_ShouldNotThrow()
        {
            AssertReadDoesNotThrow(() => _helper.AssetManagement.DataPorts.Read(DataPortExposers.AddressInfo.Hostname.HasNoValue()));
        }

        [TestMethod]
        public void DataPort_Label_NotEqualNull_ShouldNotThrow()
        {
            AssertReadDoesNotThrow(() => _helper.AssetManagement.DataPorts.Read(DataPortExposers.DataPortInfo.Label.NotEqual(null)));
        }

        [TestMethod]
        public void DataPort_Name_EqualNull_ShouldNotThrow()
        {
            AssertReadDoesNotThrow(() => _helper.AssetManagement.DataPorts.Read(DataPortExposers.DataPortInfo.Name.Equal(null)));
        }

        [TestMethod]
        public void Asset_Description_HasValue_ShouldNotThrow()
        {
            AssertReadDoesNotThrow(() => _helper.AssetManagement.Assets.Read(AssetExposers.AssetDescription.HasValue()));
        }

        [TestMethod]
        public void Site_Description_HasValue_ShouldNotThrow()
        {
            AssertReadDoesNotThrow(() => _helper.FacilityManagement.Sites.Read(Skyline.DataMiner.SDM.FacilityManagement.Models.SiteExposers.SiteProperties.Description.HasValue()));
        }

        #endregion

        #region Guarded fields (controls, pass today)

        [TestMethod]
        public void DataPort_PortNumber_HasValue_ShouldNotThrow()
        {
            AssertReadDoesNotThrow(() => _helper.AssetManagement.DataPorts.Read(DataPortExposers.DataPortInfo.PortNumber.HasValue()));
        }

        [TestMethod]
        public void DataPort_OutputType_HasNoValue_ShouldNotThrow()
        {
            AssertReadDoesNotThrow(() => _helper.AssetManagement.DataPorts.Read(DataPortExposers.DataPortInfo.OutputType.HasNoValue()));
        }

        [TestMethod]
        public void AssetClass_FrontImage_NotEqualNull_ShouldNotThrow()
        {
            AssertReadDoesNotThrow(() => _helper.AssetManagement.AssetClasses.Read(AssetClassExposers.FrontImage.NotEqual(null)));
        }

        #endregion

        private static void AssertReadDoesNotThrow(Func<IEnumerable> read)
        {
            Action act = () => read().Cast<object>().ToList();

            act.Should().NotThrow("a null filter on a string field must be translated to a DOM filter whose ToString() does not throw");
        }

        /// <summary>
        /// Wraps a connection and calls ToString() on every outgoing message, like SLNet's
        /// ConnectionInfo.AddPendingMessage does on a real DMA. The SLNet types used in tests do not format the query\n        /// inside ManagerStoreReadRequest.ToString(), so the query is stringified explicitly.
        /// </summary>
        private static class ToStringCallingConnection
        {
            public static IConnection Wrap(IConnection inner)
            {
                var mock = new Mock<IConnection>();

                mock.Setup(c => c.HandleMessages(It.IsAny<DMSMessage[]>()))
                    .Returns((DMSMessage[] messages) =>
                    {
                        Touch(messages);
                        return inner.HandleMessages(messages);
                    });
                mock.Setup(c => c.HandleMessage(It.IsAny<DMSMessage>()))
                    .Returns((DMSMessage message) =>
                    {
                        Touch(message);
                        return inner.HandleMessage(message);
                    });
                mock.Setup(c => c.HandleSingleResponseMessage(It.IsAny<DMSMessage>()))
                    .Returns((DMSMessage message) =>
                    {
                        Touch(message);
                        return inner.HandleSingleResponseMessage(message);
                    });
                mock.Setup(c => c.UserDomainName).Returns(() => inner.UserDomainName);

                return mock.Object;
            }

            private static void Touch(params DMSMessage[] messages)
            {
                foreach (var message in messages)
                {
                    _ = message.ToString();
                    if (message is ManagerStoreReadRequest<DomInstance> read)
                    {
                        _ = read.Query?.ToString();
                    }
                }
            }
        }
    }
}
