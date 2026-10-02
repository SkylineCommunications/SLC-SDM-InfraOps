namespace Skyline.DataMiner.SDM.AssetManagement.Helpers
{
    using Skyline.DataMiner.SDM.AssetManagement.Deletion;
    using Skyline.DataMiner.SDM.AssetManagement.Common.Validation.Reservations;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    using Skyline.DataMiner.SDM.AssetManagement.Validation;

    using Connection = Skyline.DataMiner.SDM.AssetManagement.Models.Connection;

    public interface IAssetManagementApiHelper
	{
    	IAssetRepository Assets { get; }

        IBulkRepository<AssetManagerAppSettings> AppSettings { get; }

		IAssetClassRepository AssetClasses { get; }

		IBulkRepository<PowerPort> PowerPorts { get; }

		IBulkRepository<DataPort> DataPorts { get; }

		IPortReader Ports { get; }

		IBulkRepository<DeviceType> DeviceTypes { get; }

        IBulkRepository<PortType> PortTypes { get; }

        IBulkRepository<Connection> Connections { get; }

        IBulkRepository<CableType> CableTypes { get; }

        IBulkRepository<InfraopsReservation> Reservations { get; }

        IBulkRepository<History> Histories { get; }

        AssetClassValidator AssetClassValidator { get; }

        AssetValidator AssetValidator { get; }

        DataPortValidator DataPortValidator { get; }

        PowerPortValidator PowerPortValidator { get; }

        DeviceTypeValidator DeviceTypeValidator { get; }

        PortTypeValidator PortTypeValidator { get; }

        CableTypeValidator CableTypeValidator { get; }

        ConnectionValidator ConnectionValidator { get; }

        InfraopsReservationValidator InfraopsReservationValidator { get; }

        AssetManagerAppSettingsValidator AppSettingsValidator { get; }

        HistoryValidator HistoryValidator { get; }

        /// <summary>
        /// Retries cleanup after an Asset DOM deletion has already succeeded.
        /// </summary>
        /// <param name="assetIdentifier">The Asset DOM identifier.</param>
        /// <remarks>
        /// Identifier-only recovery supports only the default deletion policy, which removes Asset Job
        /// entries and reads Connection snapshot cable tags from Connections that still exist.
        /// It cannot reconstruct snapshots of already-deleted Connections.
        /// Prefer <see cref="RecoverAssetDeletion(AssetDeletionRecoveryContext)"/> with the original failure
        /// context whenever available. Custom policies require that context.
        /// </remarks>
        void RecoverAssetDeletion(string assetIdentifier);

        /// <summary>
        /// Retries dependency cleanup after successful Asset DOM deletion using captured recovery data.
        /// </summary>
        /// <param name="context">The original recovery context from the failed deletion outcome.</param>
        /// <remarks>
        /// Preferred for both default and custom policies because captured Asset metadata and Connection
        /// snapshots remain available even after their source objects have been deleted.
        /// The context policy must match this helper's deletion policy, and the Asset must no longer exist.
        /// If recovery fails, use the context from the new failure outcome for the next attempt.
        /// </remarks>
        void RecoverAssetDeletion(AssetDeletionRecoveryContext context);
    }
}