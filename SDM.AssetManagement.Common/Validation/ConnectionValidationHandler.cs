namespace Skyline.DataMiner.SDM.AssetManagement.Common.Validation
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.All.Validations;
	using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.Asset_Manager.Wrappers;
	using Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.DomIds;

	public static class ConnectionValidationHandler
	{
		public enum ConnectionValidationField
		{
			CableLength,

			SourcePort,
			SourceAsset,
			DestinationPort,
			DestinationAsset,
		}

		public static ValidationResult ValidateConnection(ConnectionWrapper connection, ValidatorContext<ConnectionWrapper> context)
		{
			List<Func<ValidationResult>> validations = new List<Func<ValidationResult>>()
			{
				() => ValidateConnectionInfo(connection, context),
				() => ValidateConnectionCableInfo(connection, context),
				() => ValidateSourceInfo(connection),
				() => ValidateDestinationInfo(connection),
				() => ValidateConnectionConnectionRelation(connection, context),
			};

			ValidationResult result = new ValidationResult();
			foreach (var validation in validations)
			{
				result.CombineResults(validation());

				if (context.ReturnWhenInvalid && !result.IsValid)
				{
					return result;
				}
			}

			return result;
		}

		#region Info

		private static ValidationResult ValidateConnectionInfo(ConnectionWrapper connection, ValidatorContext<ConnectionWrapper> context)
		{
			ValidationResult result = new ValidationResult();

			return result;
		}

		#endregion

		#region Cable Information

		private static ValidationResult ValidateConnectionCableInfo(ConnectionWrapper connection, ValidatorContext<ConnectionWrapper> context)
		{
			ValidationResult result = new ValidationResult();

			if(connection.CableLengthField.Changed && !IsCableLengthValid(connection, context, out var cableLengthResult))
			{
				result.CombineResults(cableLengthResult);
			}

			return result;
		}

		public static bool IsCableLengthValid(ConnectionWrapper connection, ValidatorContext<ConnectionWrapper> context, out ValidationResult result)
		{
			result = new ValidationResult();

			if(connection.CableLengthOrDefault < 0)
			{
				result.AddFailReason(ConnectionValidationField.CableLength, "Cable length cannot be negative.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		#endregion

		#region Source Info

		private static ValidationResult ValidateSourceInfo(ConnectionWrapper connection)
		{
			ValidationResult result = new ValidationResult();
			if (connection.SourcePortIdField.Changed && !IsConnectionSourcePortValid(connection, out var nameResult))
			{
				result.CombineResults(nameResult);
			}

			return result;
		}

		private static bool IsConnectionSourcePortValid(ConnectionWrapper connection, out ValidationResult result)
		{
			result = new ValidationResult();

			if (!connection.HasSource)
			{
				result.AddFailReason(ConnectionValidationField.SourcePort, "A source port must be selected.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		#endregion

		#region Destination

		private static ValidationResult ValidateDestinationInfo(ConnectionWrapper connection)
		{
			ValidationResult result = new ValidationResult();
			if (connection.DestinationPortIdField.Changed && !IsConnectionDestinationPortValid(connection, out var nameResult))
			{
				result.CombineResults(nameResult);
			}

			return result;
		}

		private static bool IsConnectionDestinationPortValid(ConnectionWrapper connection, out ValidationResult result)
		{
			result = new ValidationResult();

			if (!connection.HasDestination)
			{
				result.AddFailReason(ConnectionValidationField.DestinationPort, "A destination port must be selected.");
				return result.IsValid;
			}

			return result.IsValid;
		}

		#endregion

		#region Connection Relation

		private static ValidationResult ValidateConnectionConnectionRelation(ConnectionWrapper connection, ValidatorContext<ConnectionWrapper> context)
		{
			ValidationResult result;
			switch (connection.Type)
			{
				case SlcAsset_Management.Enums.ConnectionType.Data:
					result = ValidateDataConnection(connection.ModuleHandlers, connection.DataSourcePort, connection.DataDestinationPort, context);
					break;
				case SlcAsset_Management.Enums.ConnectionType.Power:
					result = ValidatePowerConnection(connection.ModuleHandlers, connection.PowerSourcePort, connection.PowerDestinationPort, context);
					break;
				default:
					throw new InvalidOperationException($"Unknown connection type {connection.Type}");
			}

			return result;
		}

		#endregion

		#region Data Connection

		public static ValidationResult ValidateDataConnection(GlobalInfraOpsModuleHandler moduleHandlers, DataPortWrapper sourceDataPort, DataPortWrapper destinationDataPort, ValidatorContext<ConnectionWrapper> context)
		{
			var result = IsValidDataPortSource(sourceDataPort);

			result.CombineResults(IsValidDataPortDestination(moduleHandlers, destinationDataPort));

			if (!result.IsValid)
			{
				return result;
			}

			result.CombineResults(IsValidDataConnectionAsset(sourceDataPort.Asset, ConnectionValidationField.SourceAsset));
			result.CombineResults(IsValidDataConnectionAsset(destinationDataPort.Asset, ConnectionValidationField.DestinationAsset));

			if (sourceDataPort.Asset == destinationDataPort.Asset && sourceDataPort == destinationDataPort)
			{
				result.AddFailReason(ConnectionValidationField.SourcePort, "Destination Port is the same as source.");
				result.AddFailReason(ConnectionValidationField.DestinationPort, "Source Port is the same as destination.");
			}

			return result;
		}

		private static ValidationResult IsValidDataPortSource(DataPortWrapper sourceDataPort)
		{
			ValidationResult result = new ValidationResult();
			if (sourceDataPort == null)
			{
				result.AddFailReason(ConnectionValidationField.SourcePort, "The source port must be provided");
				return result;
			}

			if (sourceDataPort.OutputType.Equals(SlcAsset_Management.Enums.Outputtype.In))
			{
				result.AddFailReason(ConnectionValidationField.SourcePort, "The source port must be of type Output or I/O.");
				return result;
			}

			return result;
		}

		private static ValidationResult IsValidDataPortDestination(GlobalInfraOpsModuleHandler moduleHandlers, DataPortWrapper destinationDataPort)
		{
			ValidationResult result = new ValidationResult();
			if (destinationDataPort == null)
			{
				result.AddFailReason(ConnectionValidationField.DestinationPort, "The destination port must be provided");
				return result;
			}

			if (destinationDataPort.OutputType.Equals(SlcAsset_Management.Enums.Outputtype.Out))
			{
				result.AddFailReason(ConnectionValidationField.DestinationPort, "The destination port must be of type Input or I/O.");
				return result;
			}

			var maxConnectionsCount = destinationDataPort.Asset.AssetClass.DeviceType.IsConnectionPanel() ? 2 : 1;
			var connections = moduleHandlers.ConnectionHandler.GetAllDataConnectionsFromPort(destinationDataPort);
			if (connections.Count() >= maxConnectionsCount)
			{
				result.AddFailReason(ConnectionValidationField.DestinationPort, $"Port {destinationDataPort.PortNumber} is already in use.");
				return result;
			}

			return result;
		}

		private static ValidationResult IsValidDataConnectionAsset(AssetWrapper asset, ConnectionValidationField field)
		{
			var result = new ValidationResult();
			if (asset == null)
			{
				result.AddFailReason(field, "The asset must be provided.");
				return result;
			}

			if (asset.Status == AssetStatus.NotAvailable || asset.Status == AssetStatus.Disposed)
			{
				result.AddFailReason(field, "The asset must not be in the 'Not Available' or 'Disposed' state.");
				return result;
			}

			var assetClass = asset.AssetClass;

			if (assetClass == null)
			{
				result.AddFailReason(field, "The asset must have an asset class.");
				return result;
			}

			if (assetClass.Status != AssetClassStatus.Active)
			{
				result.AddFailReason(field, "The asset's Asset Class must be active.");
				return result;
			}

			if (!assetClass.DeviceType.HasTag(SlcAsset_Management.Enums.TagOption.AcceptsDataConnection))
			{
				result.AddFailReason(field, "The asset must accept data connections.");
				return result;
			}

			return result;
		}

		#endregion

		#region Power Connection

		public static ValidationResult ValidatePowerConnection(GlobalInfraOpsModuleHandler moduleHandlers, PowerPortWrapper sourcePowerPort, PowerPortWrapper destinationPowerPort, ValidatorContext<ConnectionWrapper> context)
		{
			var result = IsValidPowerPortSource(moduleHandlers, sourcePowerPort, context);

			result.CombineResults(IsValidPowerPortDestination(moduleHandlers, destinationPowerPort, context));

			if (!result.IsValid)
			{
				return result;
			}

			result.CombineResults(IsValidPowerConnectionSourceAsset(sourcePowerPort.Asset));
			result.CombineResults(IsValidPowerConnectionDestinationAsset(destinationPowerPort.Asset));

			if (sourcePowerPort.Asset == destinationPowerPort.Asset && sourcePowerPort == destinationPowerPort)
			{
				result.AddFailReason(ConnectionValidationField.SourcePort, "Destination Port is the same as source.");
				result.AddFailReason(ConnectionValidationField.DestinationPort, "Source Port is the same as destination.");//$"Cannot select the same port within the same asset.");
			}

			return result;
		}

		private static ValidationResult IsValidPowerConnectionSourceAsset(AssetWrapper sourceAsset)
		{
			var result = new ValidationResult();
			if (sourceAsset == null)
			{
				result.AddFailReason(ConnectionValidationField.SourceAsset, "The asset must be provided.");
				return result;
			}

			if (sourceAsset.Status == AssetStatus.NotAvailable || sourceAsset.Status == AssetStatus.Disposed)
			{
				result.AddFailReason(ConnectionValidationField.SourceAsset, "The asset must not be in the 'Not Available' or 'Disposed' state.");
				return result;
			}

			var assetClass = sourceAsset.AssetClass;
			if (assetClass == null)
			{
				result.AddFailReason(ConnectionValidationField.SourceAsset, "The asset must have an asset class.");
				return result;
			}

			if (assetClass.Status != AssetClassStatus.Active)
			{
				result.AddFailReason(ConnectionValidationField.SourceAsset, "The asset's Asset Class must be active.");
				return result;
			}

			if (!assetClass.DeviceType.HasTag(SlcAsset_Management.Enums.TagOption.PowerProvider))
			{
				result.AddFailReason(ConnectionValidationField.SourceAsset, "The asset must be a Power Provider.");
				return result;
			}

			return result;
		}

		private static ValidationResult IsValidPowerConnectionDestinationAsset(AssetWrapper destinationAsset)
		{
			var result = new ValidationResult();
			if (destinationAsset == null)
			{
				result.AddFailReason(ConnectionValidationField.DestinationAsset, "The asset must be provided.");
				return result;
			}

			if (destinationAsset.Status == AssetStatus.NotAvailable || destinationAsset.Status == AssetStatus.Disposed)
			{
				result.AddFailReason(ConnectionValidationField.DestinationAsset, "The asset must not be in the 'Not Available' or 'Disposed' state.");
				return result;
			}

			var assetClass = destinationAsset.AssetClass;
			if (assetClass == null)
			{
				result.AddFailReason(ConnectionValidationField.DestinationAsset, "The asset must have an asset class.");
				return result;
			}

			if (assetClass.Status != AssetClassStatus.Active)
			{
				result.AddFailReason(ConnectionValidationField.DestinationAsset, "The asset's Asset Class must be active.");
				return result;
			}

			return result;
		}

		private static ValidationResult IsValidPowerPortSource(GlobalInfraOpsModuleHandler moduleHandlers, PowerPortWrapper sourcePowerPort, ValidatorContext<ConnectionWrapper> context)
		{
			ValidationResult result = new ValidationResult();
			if (sourcePowerPort == null)
			{
				result.AddFailReason(ConnectionValidationField.SourcePort, "The source port must be provided");
				return result;
			}

			if (sourcePowerPort.OutputType.Equals(SlcAsset_Management.Enums.Outputtype.In))
			{
				result.AddFailReason(ConnectionValidationField.SourcePort, "The source port must be of type Output or I/O.");
				return result;
			}

			var connections = moduleHandlers.ConnectionHandler.GetAllPowerConnectionsFromPort(sourcePowerPort);

			if (connections.Any(entry => entry != context.BaseEntry))
			{
				result.AddFailReason(ConnectionValidationField.SourcePort, $"Port {sourcePowerPort.PortNumber} is already in use.");
				return result;
			}

			return result;
		}

		private static ValidationResult IsValidPowerPortDestination(GlobalInfraOpsModuleHandler moduleHandlers, PowerPortWrapper destinationPowerPort, ValidatorContext<ConnectionWrapper> context)
		{
			ValidationResult result = new ValidationResult();
			if (destinationPowerPort == null)
			{
				result.AddFailReason(ConnectionValidationField.DestinationPort, "The destination port must be provided");
				return result;
			}

			if (destinationPowerPort.OutputType.Equals(SlcAsset_Management.Enums.Outputtype.Out))
			{
				result.AddFailReason(ConnectionValidationField.DestinationPort, "The destination port must be of type Input or I/O.");
				return result;
			}

			var connections = moduleHandlers.ConnectionHandler.GetAllPowerConnectionsFromPort(destinationPowerPort);
			if (connections.Any(entry => entry != context.BaseEntry))
			{
				result.AddFailReason(ConnectionValidationField.DestinationPort, $"Port {destinationPowerPort.PortNumber} is already in use.");
				return result;
			}

			return result;
		}

		#endregion
	}
}