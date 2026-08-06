namespace Skyline.DataMiner.SDM.PlanAndBuild.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Helpers;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Helpers;
    using Skyline.DataMiner.SDM.PlanAndBuild.Extensions;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Extensions;

    /// <summary>
    /// Default cross-module reference checker backed by Facility Management and Asset Management helpers.
    /// </summary>
    public sealed class PlanAndBuildExternalReferenceChecker : IPlanAndBuildExternalReferenceChecker
    {
        private readonly IFacilityManagementApiHelper _facilityManagementHelper;
        private readonly IAssetManagementApiHelper _assetManagementHelper;

        public PlanAndBuildExternalReferenceChecker(
            IFacilityManagementApiHelper facilityManagementHelper = null,
            IAssetManagementApiHelper assetManagementHelper = null)
        {
            _facilityManagementHelper = facilityManagementHelper;
            _assetManagementHelper = assetManagementHelper;
        }

        public IReadOnlyCollection<Guid> GetExistingLocationIds(IReadOnlyCollection<Guid> locationIds)
        {
            var ids = locationIds?.Distinct().ToList() ?? new List<Guid>();
            if (_facilityManagementHelper == null || ids.Count == 0)
            {
                // No facility helper available: treat every referenced id as existing so
                // reference validation is effectively skipped rather than reporting false errors.
                return ids;
            }

            return new PlanAndBuildJob { Locations = ids }
                .ResolveLocations(_facilityManagementHelper)
                .Where(location => location.Kind != FacilityLocationKind.Unknown)
                .Select(location => location.Id)
                .ToList();
        }

        public IReadOnlyCollection<string> GetExistingAssetIds(IReadOnlyCollection<string> assetIds)
        {
            var keys = Normalize(assetIds);
            if (_assetManagementHelper == null || keys.Count == 0)
            {
                return keys;
            }

            return _assetManagementHelper.Assets
                .ReadByBigOrFilter(keys, id => AssetExposers.Identifier.Equal(id))
                .Select(asset => asset.Identifier)
                .ToList();
        }

        public IReadOnlyCollection<string> GetExistingConnectionIds(IReadOnlyCollection<string> connectionIds)
        {
            var keys = Normalize(connectionIds);
            if (_assetManagementHelper == null || keys.Count == 0)
            {
                return keys;
            }

            return _assetManagementHelper.Connections
                .ReadByBigOrFilter(keys, id => ConnectionExposers.Identifier.Equal(id))
                .Select(connection => connection.Identifier)
                .ToList();
        }

        public IReadOnlyCollection<string> GetExistingCableTypeIds(IReadOnlyCollection<string> cableTypeIds)
        {
            var keys = Normalize(cableTypeIds);
            if (_assetManagementHelper == null || keys.Count == 0)
            {
                return keys;
            }

            return _assetManagementHelper.CableTypes
                .ReadByBigOrFilter(keys, id => CableTypeExposers.Identifier.Equal(id))
                .Select(cableType => cableType.Identifier)
                .ToList();
        }

        private static List<string> Normalize(IEnumerable<string> identifiers)
        {
            return identifiers?
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList() ?? new List<string>();
        }
    }
}
