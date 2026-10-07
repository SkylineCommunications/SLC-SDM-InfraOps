namespace SDM.InfraOpsProperties.Tests
{
	using Moq;
	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Common.Mock;
	using Skyline.DataMiner.SDM.InfraOpsProperties.Helpers;
	using Skyline.DataMiner.Utils.DOM.UnitTesting;


	internal static class ConnectionHelper
	{
		internal static IConnection CreateConnection()
		{
			var messageHandler = new DomSLNetMessageHandler();
			messageHandler.AddInfraOpsPropertiesModule();
			return CreateConnection(messageHandler);
		}

		internal static IInfraOpsPropertiesApiHelper GetMockedHelper(this IConnection connection, bool cascadeDeleteOnProperty = true)
		{
			return new InfraOpsPropertiesApiHelper(connection, cascadeDeleteOnProperty);
		}

		private static IConnection CreateConnection(DomSLNetMessageHandler messageHandler)
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

		private static DMSMessage[] HandleSLNetMessages(DomSLNetMessageHandler messageHandler, DMSMessage[] messages)
		{
			if (messages is null)
			{
				throw new ArgumentNullException(nameof(messages));
			}

			return messages.Select(m => HandleSLNetMessage(messageHandler, m)).SelectMany(m => m).ToArray();
		}

		private static DMSMessage[] HandleSLNetMessage(DomSLNetMessageHandler messageHandler, DMSMessage message)
		{
			if (!TryHandleSLNetMessage(messageHandler, message, out var response))
			{
				throw new NotSupportedException($"Message of type {message.GetType().Name} is not supported by the mock.");
			}

			return response;
		}

		private static bool TryHandleSLNetMessage(DomSLNetMessageHandler messageHandler, DMSMessage message, out DMSMessage[] responses)
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
