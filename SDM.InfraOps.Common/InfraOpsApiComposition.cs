namespace Skyline.DataMiner.SDM.InfraOps.Orchestration
{
    using System;

    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.SDM.AssetManagement.Deletion;
    using Skyline.DataMiner.SDM.AssetManagement.Helpers;
    using Skyline.DataMiner.SDM.FacilityManagement.Helpers;
    using Skyline.DataMiner.SDM.InfraOps.Orchestration.AssetDeletion;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Helpers;
    using Skyline.DataMiner.SDM.PlanAndBuild.Deletion;
    using Skyline.DataMiner.SDM.PlanAndBuild.Helpers;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

    /// <summary>
    /// Fully composed InfraOps helpers sharing the Asset deletion participants.
    /// </summary>
    public sealed class InfraOpsApiComposition
    {
        private InfraOpsApiComposition(
            IAssetManagementApiHelper assetManagement,
            IFacilityManagementApiHelper facilityManagement,
            IPlanAndBuildApiHelper planAndBuild,
            IInfraOpsPropertiesApiHelper infraOpsProperties)
        {
            AssetManagement = assetManagement;
            FacilityManagement = facilityManagement;
            PlanAndBuild = planAndBuild;
            InfraOpsProperties = infraOpsProperties;
        }

        /// <summary>
        /// Gets the composed Asset Management helper.
        /// </summary>
        public IAssetManagementApiHelper AssetManagement { get; }

        /// <summary>
        /// Gets the Facility Management helper shared with Asset Management.
        /// </summary>
        public IFacilityManagementApiHelper FacilityManagement { get; }

        /// <summary>
        /// Gets the Plan &amp; Build helper sharing the composed Asset Management helper.
        /// </summary>
        public IPlanAndBuildApiHelper PlanAndBuild { get; }

        /// <summary>
        /// Gets the InfraOps Properties helper participating in Asset deletion cleanup.
        /// </summary>
        public IInfraOpsPropertiesApiHelper InfraOpsProperties { get; }

        /// <summary>
        /// Creates all InfraOps helpers and binds every required Asset deletion participant before returning.
        /// </summary>
        public static InfraOpsApiComposition Create(IConnection connection)
            => Create(connection, AssetDeletionPolicy.Default);

        public static InfraOpsApiComposition Create(IConnection connection, AssetDeletionPolicy deletionPolicy)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            return Create(connection, connection.GetPeopleAndOrganizationsApi(), deletionPolicy);
        }

        /// <summary>
        /// Creates all InfraOps helpers with an explicitly supplied People &amp; Organizations API.
        /// </summary>
        public static InfraOpsApiComposition Create(
            IConnection connection,
            IPeopleAndOrganizationsApi peopleApi)
            => Create(connection, peopleApi, AssetDeletionPolicy.Default);

        public static InfraOpsApiComposition Create(
            IConnection connection,
            IPeopleAndOrganizationsApi peopleApi,
            AssetDeletionPolicy deletionPolicy)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (peopleApi == null)
            {
                throw new ArgumentNullException(nameof(peopleApi));
            }

            var assetDeletionMiddleware = new AssetDeletionMiddleware(deletionPolicy);
            var facilityManagement = new FacilityManagementApiHelper(connection);
            var assetManagement = new AssetManagementApiHelper(
                connection,
                facilityManagement,
                peopleApi,
                assetDeletionMiddleware);
            var planAndBuild = new PlanAndBuildApiHelper(connection);
            var infraOpsProperties = new InfraOpsPropertiesApiHelper(connection);

            assetDeletionMiddleware.Configure(assetManagement, (IAssetDeletionJobCleanup)planAndBuild, infraOpsProperties);

            return new InfraOpsApiComposition(
                assetManagement,
                facilityManagement,
                planAndBuild,
                infraOpsProperties);
        }

        /// <summary>
        /// Retries dependency cleanup for an Asset whose DOM deletion has already succeeded.
        /// </summary>
        public void RecoverAssetDeletion(string assetIdentifier)
        {
            AssetManagement.RecoverAssetDeletion(assetIdentifier);
        }

        public void RecoverAssetDeletion(AssetDeletionRecoveryContext context)
        {
            AssetManagement.RecoverAssetDeletion(context);
        }
    }

    /// <summary>
    /// Factory methods for complete InfraOps helper composition.
    /// </summary>
    public static class InfraOpsApiHelperFactory
    {
        /// <summary>
        /// Creates an Asset Management helper with Plan &amp; Build and Properties deletion participants configured.
        /// </summary>
        public static IAssetManagementApiHelper CreateAssetManagementApiHelper(IConnection connection)
            => InfraOpsApiComposition.Create(connection).AssetManagement;

        /// <summary>
        /// Creates a Plan &amp; Build helper using the Asset Management helper from a complete composition.
        /// </summary>
        public static IPlanAndBuildApiHelper CreatePlanAndBuildApiHelper(IConnection connection)
            => InfraOpsApiComposition.Create(connection).PlanAndBuild;
    }
}
