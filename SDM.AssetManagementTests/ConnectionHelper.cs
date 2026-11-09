namespace SDM.AssetManagement.Tests
{
    using Moq;
    using SDM.AssetManagement.Tests.Setup;
    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Mock;
    using Skyline.DataMiner.SDM.FacilityManagement.Common.Mock;
    using Skyline.DataMiner.Utils.DOM.UnitTesting;

    internal static class ConnectionHelper
	{
		internal static IConnection CreateConnection()
		{
			var messageHandler = new DomSLNetMessageHandler();
			messageHandler.AddAssetManagementModule();
			messageHandler.AddFacilityManagementModule();
			return CreateConnection(new FieldValueTypeValidatingMessageHandler(messageHandler));
		}

		/// <summary>
		/// Creates a mocked connection around an arbitrary <see cref="DomSLNetMessageHandler"/>, still guarded by
		/// field-value-type validation. Used by tests that need a handler without the default module seeding.
		/// </summary>
		internal static IConnection CreateConnection(DomSLNetMessageHandler messageHandler)
		{
			return CreateConnection(new FieldValueTypeValidatingMessageHandler(messageHandler));
		}

		private static IConnection CreateConnection(FieldValueTypeValidatingMessageHandler messageHandler)
		{
			var connectionMock = new Mock<IConnection>();
			connectionMock.Setup(c => c.HandleMessages(It.IsAny<DMSMessage[]>()))
				.Returns((DMSMessage[] messages) => HandleSLNetMessages(messageHandler, messages));
			connectionMock.Setup(c => c.HandleMessage(It.IsAny<DMSMessage>()))
				.Returns((DMSMessage message) => HandleSLNetMessage(messageHandler, message));
			connectionMock.Setup(c => c.HandleSingleResponseMessage(It.IsAny<DMSMessage>()))
				.Returns((DMSMessage message) => HandleSLNetMessage(messageHandler, message)[0]);
			connectionMock.Setup(c => c.UserDomainName)
				.Returns("Mocked User");

			return connectionMock.Object;
		}


		private static DMSMessage[] HandleSLNetMessages(FieldValueTypeValidatingMessageHandler messageHandler, DMSMessage[] messages)
		{
			if (messages is null)
			{
				throw new ArgumentNullException(nameof(messages));
			}

			return messages.Select(m => HandleSLNetMessage(messageHandler, m)).SelectMany(m => m).ToArray();
		}

		private static DMSMessage[] HandleSLNetMessage(FieldValueTypeValidatingMessageHandler messageHandler, DMSMessage message)
		{
			if (!TryHandleSLNetMessage(messageHandler, message, out var response))
			{
				throw new NotSupportedException($"Message of type {message.GetType().Name} is not supported by the mock.");
			}

			return response;
		}

		private static bool TryHandleSLNetMessage(FieldValueTypeValidatingMessageHandler messageHandler, DMSMessage message, out DMSMessage[] responses)
		{
			responses = Array.Empty<DMSMessage>();
			if (messageHandler.TryHandleMessage(message, out var domMessage))
			{
				responses = [domMessage];
				return true;
			}

			return false;
		}
	}
}