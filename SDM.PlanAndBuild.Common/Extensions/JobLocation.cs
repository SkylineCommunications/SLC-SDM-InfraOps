namespace Skyline.DataMiner.SDM.PlanAndBuild.Extensions
{
    using System;

    using Skyline.DataMiner.SDM.FacilityManagement.Models;

    /// <summary>
    /// Identifies which Facility Management DOM type a <see cref="JobLocation"/> resolved to.
    /// </summary>
    public enum FacilityLocationKind
    {
        /// <summary>The Guid did not resolve to any known Facility Management DOM instance.</summary>
        Unknown,

        /// <summary>The Guid resolved to a <see cref="FacilityManagement.Models.Facility"/>.</summary>
        Facility,

        /// <summary>The Guid resolved to a <see cref="FacilityManagement.Models.Floor"/>.</summary>
        Floor,

        /// <summary>The Guid resolved to a <see cref="FacilityManagement.Models.Room"/>.</summary>
        Room,

        /// <summary>The Guid resolved to a <see cref="FacilityManagement.Models.Zone"/>.</summary>
        Zone,

        /// <summary>The Guid resolved to a <see cref="FacilityManagement.Models.Row"/>.</summary>
        Row,

        /// <summary>The Guid resolved to a <see cref="FacilityManagement.Models.Desk"/>.</summary>
        Desk,

        /// <summary>The Guid resolved to a <see cref="FacilityManagement.Models.Rack"/>.</summary>
        Rack,
    }

    /// <summary>
    /// Represents a single entry of <see cref="Models.PlanAndBuildJob.Locations"/> resolved to its
    /// concrete, strongly-typed Facility Management DOM instance (Facility, Floor, Room, Zone, Row, Desk or Rack).
    /// </summary>
    public class JobLocation
    {
        /// <summary>
        /// Gets the DOM instance identifier, as it appears in <see cref="Models.PlanAndBuildJob.Locations"/>.
        /// </summary>
        public Guid Id { get; internal set; }

        /// <summary>
        /// Gets a value indicating which Facility Management DOM type <see cref="Id"/> resolved to.
        /// </summary>
        public FacilityLocationKind Kind { get; internal set; } = FacilityLocationKind.Unknown;

        /// <summary>
        /// Gets the resolved <see cref="FacilityManagement.Models.Facility"/>, or <c>null</c> if
        /// <see cref="Kind"/> is not <see cref="FacilityLocationKind.Facility"/>.
        /// </summary>
        public Facility Facility { get; internal set; }

        /// <summary>
        /// Gets the resolved <see cref="FacilityManagement.Models.Floor"/>, or <c>null</c> if
        /// <see cref="Kind"/> is not <see cref="FacilityLocationKind.Floor"/>.
        /// </summary>
        public Floor Floor { get; internal set; }

        /// <summary>
        /// Gets the resolved <see cref="FacilityManagement.Models.Room"/>, or <c>null</c> if
        /// <see cref="Kind"/> is not <see cref="FacilityLocationKind.Room"/>.
        /// </summary>
        public Room Room { get; internal set; }

        /// <summary>
        /// Gets the resolved <see cref="FacilityManagement.Models.Zone"/>, or <c>null</c> if
        /// <see cref="Kind"/> is not <see cref="FacilityLocationKind.Zone"/>.
        /// </summary>
        public Zone Zone { get; internal set; }

        /// <summary>
        /// Gets the resolved <see cref="FacilityManagement.Models.Row"/>, or <c>null</c> if
        /// <see cref="Kind"/> is not <see cref="FacilityLocationKind.Row"/>.
        /// </summary>
        public Row Row { get; internal set; }

        /// <summary>
        /// Gets the resolved <see cref="FacilityManagement.Models.Desk"/>, or <c>null</c> if
        /// <see cref="Kind"/> is not <see cref="FacilityLocationKind.Desk"/>.
        /// </summary>
        public Desk Desk { get; internal set; }

        /// <summary>
        /// Gets the resolved <see cref="FacilityManagement.Models.Rack"/>, or <c>null</c> if
        /// <see cref="Kind"/> is not <see cref="FacilityLocationKind.Rack"/>.
        /// </summary>
        public Rack Rack { get; internal set; }

        /// <summary>
        /// Gets the resolved object as <see cref="object"/> (whichever of <see cref="Facility"/>,
        /// <see cref="Floor"/>, <see cref="Room"/>, <see cref="Zone"/>, <see cref="Row"/>, <see cref="Desk"/> or
        /// <see cref="Rack"/> is set), or <c>null</c> if <see cref="Kind"/> is <see cref="FacilityLocationKind.Unknown"/>.
        /// </summary>
        public object Value =>
            (object)Facility ?? (object)Floor ?? (object)Room ?? (object)Zone ?? (object)Row ?? (object)Desk ?? (object)Rack;
    }
}
