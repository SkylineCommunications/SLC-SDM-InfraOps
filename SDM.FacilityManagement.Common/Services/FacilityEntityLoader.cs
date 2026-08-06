namespace Skyline.DataMiner.SDM.FacilityManagement.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.FacilityManagement.Helpers;
    using Skyline.DataMiner.SDM.FacilityManagement.Models;
    using Skyline.DataMiner.SDM.FacilityManagement.Validation;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Extensions;

    /// <summary>
    /// Dedicated service for loading and querying Facility Manager entities.
    /// Centralizes read access (Count, Read) used by the Facility Manager validators.
    /// <para>
    /// Note: This class only performs read operations on the repositories. Queries are issued
    /// through the pass-through repositories on the helper to avoid a chicken-and-egg dependency
    /// with the validation middleware.
    /// </para>
    /// </summary>
    public class FacilityEntityLoader
    {
        private readonly IFacilityManagementApiHelper _helper;

        public FacilityEntityLoader(IFacilityManagementApiHelper helper)
        {
            _helper = helper ?? throw new ArgumentNullException(nameof(helper));
        }

        public IFacilityManagementExternalReferenceChecker ExternalReferenceChecker => _helper.ExternalReferenceChecker;

        #region Site

        /// <summary>
        /// Counts the Sites whose SiteId matches <paramref name="siteId"/>, optionally excluding a single identifier.
        /// </summary>
        public long CountSitesBySiteId(string siteId, string exceptIdentifier = null)
        {
            if (_helper?.Sites == null || string.IsNullOrWhiteSpace(siteId))
            {
                return 0;
            }

            FilterElement<Site> filter = SiteExposers.SiteProperties.SiteId.Equal(siteId);

            if (!string.IsNullOrWhiteSpace(exceptIdentifier))
            {
                filter = filter.AND(SiteExposers.Identifier.NotEqual(exceptIdentifier));
            }

            return _helper.Sites.Count(filter);
        }

        /// <summary>
        /// Retrieves all Sites whose SiteId matches any of the provided ids.
        /// Uses a single big-OR query (internally batched) to safely handle large sets without
        /// creating an oversized OR filter in a single call.
        /// </summary>
        public List<Site> GetSitesBySiteIds(List<string> siteIds)
        {
            if (_helper?.Sites == null || siteIds == null || !siteIds.Any())
            {
                return new List<Site>();
            }

            return _helper.Sites.ReadByBigOrFilter(
                siteIds,
                id => SiteExposers.SiteProperties.SiteId.Equal(id));
        }

        public List<Site> GetSitesByIdentifiers(List<string> identifiers)
        {
            if (_helper?.Sites == null || identifiers == null || !identifiers.Any())
            {
                return new List<Site>();
            }

            return _helper.Sites.ReadByBigOrFilter(
                identifiers,
                id => SiteExposers.Identifier.Equal(id));
        }

        #endregion

        #region Facility

        public long CountFacilitiesByFacilityId(string facilityId, string exceptIdentifier = null)
        {
            if (_helper?.Facilities == null || string.IsNullOrWhiteSpace(facilityId))
            {
                return 0;
            }

            FilterElement<Facility> filter = FacilityExposers.FacilityId.Equal(facilityId);

            if (!string.IsNullOrWhiteSpace(exceptIdentifier))
            {
                filter = filter.AND(FacilityExposers.Identifier.NotEqual(exceptIdentifier));
            }

            return _helper.Facilities.Count(filter);
        }

        public List<Facility> GetFacilitiesByFacilityIds(List<string> facilityIds)
        {
            if (_helper?.Facilities == null || facilityIds == null || !facilityIds.Any())
            {
                return new List<Facility>();
            }

            return _helper.Facilities.ReadByBigOrFilter(
                facilityIds,
                id => FacilityExposers.FacilityId.Equal(id));
        }

        public List<Facility> GetFacilitiesByIdentifiers(List<string> identifiers)
        {
            if (_helper?.Facilities == null || identifiers == null || !identifiers.Any())
            {
                return new List<Facility>();
            }

            return _helper.Facilities.ReadByBigOrFilter(
                identifiers,
                id => FacilityExposers.Identifier.Equal(id));
        }

        public List<Facility> GetFacilitiesBySiteIdentifiers(List<string> siteIdentifiers)
        {
            if (_helper?.Facilities == null || siteIdentifiers == null || !siteIdentifiers.Any())
            {
                return new List<Facility>();
            }

            return _helper.Facilities.ReadByBigOrFilter(
                siteIdentifiers,
                id => FacilityExposers.SiteFk.Site.Equal(new SdmObjectReference<Site>(id)));
        }

        #endregion

        #region Floor

        public long CountFloorsByFloorId(string floorId, string exceptIdentifier = null)
        {
            if (_helper?.Floors == null || string.IsNullOrWhiteSpace(floorId))
            {
                return 0;
            }

            FilterElement<Floor> filter = FloorExposers.FloorProperties.FloorId.Equal(floorId);

            if (!string.IsNullOrWhiteSpace(exceptIdentifier))
            {
                filter = filter.AND(FloorExposers.Identifier.NotEqual(exceptIdentifier));
            }

            return _helper.Floors.Count(filter);
        }

        public List<Floor> GetFloorsByFloorIds(List<string> floorIds)
        {
            if (_helper?.Floors == null || floorIds == null || !floorIds.Any())
            {
                return new List<Floor>();
            }

            return _helper.Floors.ReadByBigOrFilter(
                floorIds,
                id => FloorExposers.FloorProperties.FloorId.Equal(id));
        }

        public List<Floor> GetFloorsByIdentifiers(List<string> identifiers)
        {
            if (_helper?.Floors == null || identifiers == null || !identifiers.Any())
            {
                return new List<Floor>();
            }

            return _helper.Floors.ReadByBigOrFilter(
                identifiers,
                id => FloorExposers.Identifier.Equal(id));
        }

        public List<Floor> GetFloorsByFacilityIdentifiers(List<string> facilityIdentifiers)
        {
            if (_helper?.Floors == null || facilityIdentifiers == null || !facilityIdentifiers.Any())
            {
                return new List<Floor>();
            }

            return _helper.Floors.ReadByBigOrFilter(
                facilityIdentifiers,
                id => FloorExposers.FacilityFk.Facility.Equal(new SdmObjectReference<Facility>(id)));
        }

        #endregion

        #region Room

        public long CountRoomsByRoomId(string roomId, string exceptIdentifier = null)
        {
            if (_helper?.Rooms == null || string.IsNullOrWhiteSpace(roomId))
            {
                return 0;
            }

            FilterElement<Room> filter = RoomExposers.RoomProperties.RoomId.Equal(roomId);

            if (!string.IsNullOrWhiteSpace(exceptIdentifier))
            {
                filter = filter.AND(RoomExposers.Identifier.NotEqual(exceptIdentifier));
            }

            return _helper.Rooms.Count(filter);
        }

        public List<Room> GetRoomsByRoomIds(List<string> roomIds)
        {
            if (_helper?.Rooms == null || roomIds == null || !roomIds.Any())
            {
                return new List<Room>();
            }

            return _helper.Rooms.ReadByBigOrFilter(
                roomIds,
                id => RoomExposers.RoomProperties.RoomId.Equal(id));
        }

        public List<Room> GetRoomsByIdentifiers(List<string> identifiers)
        {
            if (_helper?.Rooms == null || identifiers == null || !identifiers.Any())
            {
                return new List<Room>();
            }

            return _helper.Rooms.ReadByBigOrFilter(
                identifiers,
                id => RoomExposers.Identifier.Equal(id));
        }

        public List<Room> GetRoomsByFloorIdentifiers(List<string> floorIdentifiers)
        {
            if (_helper?.Rooms == null || floorIdentifiers == null || !floorIdentifiers.Any())
            {
                return new List<Room>();
            }

            return _helper.Rooms.ReadByBigOrFilter(
                floorIdentifiers,
                id => RoomExposers.FloorFk.Floor.Equal(new SdmObjectReference<Floor>(id)));
        }

        #endregion

        #region Row

        public long CountRowsByRowId(string rowId, string exceptIdentifier = null)
        {
            if (_helper?.Rows == null || string.IsNullOrWhiteSpace(rowId))
            {
                return 0;
            }

            FilterElement<Row> filter = RowExposers.RowProperties.RowId.Equal(rowId);

            if (!string.IsNullOrWhiteSpace(exceptIdentifier))
            {
                filter = filter.AND(RowExposers.Identifier.NotEqual(exceptIdentifier));
            }

            return _helper.Rows.Count(filter);
        }

        public List<Row> GetRowsByRowIds(List<string> rowIds)
        {
            if (_helper?.Rows == null || rowIds == null || !rowIds.Any())
            {
                return new List<Row>();
            }

            return _helper.Rows.ReadByBigOrFilter(
                rowIds,
                id => RowExposers.RowProperties.RowId.Equal(id));
        }

        public List<Row> GetRowsByIdentifiers(List<string> identifiers)
        {
            if (_helper?.Rows == null || identifiers == null || !identifiers.Any())
            {
                return new List<Row>();
            }

            return _helper.Rows.ReadByBigOrFilter(
                identifiers,
                id => RowExposers.Identifier.Equal(id));
        }

        public List<Row> GetRowsByRoomIdentifiers(List<string> roomIdentifiers)
        {
            if (_helper?.Rows == null || roomIdentifiers == null || !roomIdentifiers.Any())
            {
                return new List<Row>();
            }

            return _helper.Rows.ReadByBigOrFilter(
                roomIdentifiers,
                id => RowExposers.RoomFk.Room.Equal(new SdmObjectReference<Room>(id)));
        }

        #endregion

        #region Zone

        public long CountZonesByZoneId(string zoneId, string exceptIdentifier = null)
        {
            if (_helper?.Zones == null || string.IsNullOrWhiteSpace(zoneId))
            {
                return 0;
            }

            FilterElement<Zone> filter = ZoneExposers.ZoneProperties.ZoneId.Equal(zoneId);

            if (!string.IsNullOrWhiteSpace(exceptIdentifier))
            {
                filter = filter.AND(ZoneExposers.Identifier.NotEqual(exceptIdentifier));
            }

            return _helper.Zones.Count(filter);
        }

        public List<Zone> GetZonesByZoneIds(List<string> zoneIds)
        {
            if (_helper?.Zones == null || zoneIds == null || !zoneIds.Any())
            {
                return new List<Zone>();
            }

            return _helper.Zones.ReadByBigOrFilter(
                zoneIds,
                id => ZoneExposers.ZoneProperties.ZoneId.Equal(id));
        }

        public List<Zone> GetZonesByIdentifiers(List<string> identifiers)
        {
            if (_helper?.Zones == null || identifiers == null || !identifiers.Any())
            {
                return new List<Zone>();
            }

            return _helper.Zones.ReadByBigOrFilter(
                identifiers,
                id => ZoneExposers.Identifier.Equal(id));
        }

        public List<Zone> GetZonesByRoomIdentifiers(List<string> roomIdentifiers)
        {
            if (_helper?.Zones == null || roomIdentifiers == null || !roomIdentifiers.Any())
            {
                return new List<Zone>();
            }

            return _helper.Zones.ReadByBigOrFilter(
                roomIdentifiers,
                id => ZoneExposers.RoomFk.Room.Equal(new SdmObjectReference<Room>(id)));
        }

        #endregion

        #region Desk

        public long CountDesksByDeskId(string deskId, string exceptIdentifier = null)
        {
            if (_helper?.Desks == null || string.IsNullOrWhiteSpace(deskId))
            {
                return 0;
            }

            FilterElement<Desk> filter = DeskExposers.DeskInformation.DeskID.Equal(deskId);

            if (!string.IsNullOrWhiteSpace(exceptIdentifier))
            {
                filter = filter.AND(DeskExposers.Identifier.NotEqual(exceptIdentifier));
            }

            return _helper.Desks.Count(filter);
        }

        public List<Desk> GetDesksByDeskIds(List<string> deskIds)
        {
            if (_helper?.Desks == null || deskIds == null || !deskIds.Any())
            {
                return new List<Desk>();
            }

            return _helper.Desks.ReadByBigOrFilter(
                deskIds,
                id => DeskExposers.DeskInformation.DeskID.Equal(id));
        }

        public List<Desk> GetDesksByRoomIdentifiers(List<string> roomIdentifiers)
        {
            if (_helper?.Desks == null || roomIdentifiers == null || !roomIdentifiers.Any())
            {
                return new List<Desk>();
            }

            return _helper.Desks.ReadByBigOrFilter(
                roomIdentifiers,
                id => DeskExposers.RoomFk.Room.Equal(new SdmObjectReference<Room>(id)));
        }

        #endregion

        #region Rack

        public long CountRacksByRackId(string rackId, string exceptIdentifier = null)
        {
            if (_helper?.Racks == null || string.IsNullOrWhiteSpace(rackId))
            {
                return 0;
            }

            FilterElement<Rack> filter = RackExposers.RackProperties.RackId.Equal(rackId);

            if (!string.IsNullOrWhiteSpace(exceptIdentifier))
            {
                filter = filter.AND(RackExposers.Identifier.NotEqual(exceptIdentifier));
            }

            return _helper.Racks.Count(filter);
        }

        public List<Rack> GetRacksByRackIds(List<string> rackIds)
        {
            if (_helper?.Racks == null || rackIds == null || !rackIds.Any())
            {
                return new List<Rack>();
            }

            return _helper.Racks.ReadByBigOrFilter(
                rackIds,
                id => RackExposers.RackProperties.RackId.Equal(id));
        }

        public List<Rack> GetRacksByRowIdentifiers(List<string> rowIdentifiers)
        {
            if (_helper?.Racks == null || rowIdentifiers == null || !rowIdentifiers.Any())
            {
                return new List<Rack>();
            }

            return _helper.Racks.ReadByBigOrFilter(
                rowIdentifiers,
                id => RackExposers.RowFk.Row.Equal(new SdmObjectReference<Row>(id)));
        }

        public List<Rack> GetRacksByZoneIdentifiers(List<string> zoneIdentifiers)
        {
            if (_helper?.Racks == null || zoneIdentifiers == null || !zoneIdentifiers.Any())
            {
                return new List<Rack>();
            }

            return _helper.Racks.ReadByBigOrFilter(
                zoneIdentifiers,
                id => RackExposers.ZoneFk.Zone.Equal(new SdmObjectReference<Zone>(id)));
        }

        #endregion
    }
}
