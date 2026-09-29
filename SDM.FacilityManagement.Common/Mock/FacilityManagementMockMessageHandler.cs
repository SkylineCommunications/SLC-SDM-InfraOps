namespace Skyline.DataMiner.SDM.FacilityManagement.Common.Mock
{
    using System;
    using System.Collections.Generic;
    using SharedMappers.DomIds;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Concatenation;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Status;
    using Skyline.DataMiner.Net.Apps.Sections.SectionDefinitions;
    using Skyline.DataMiner.Net.Sections;
    using Skyline.DataMiner.Utils.DOM.Builders;
    using Skyline.DataMiner.Utils.DOM.UnitTesting;

    public static class FacilityManagementMockMessageHandler
    {
        public static void AddFacilityManagementModule(this DomSLNetMessageHandler messageHandler)
        {
            var sections = GenerateSectionsDefinitions();
            var behaviorDefinitions = GenerateBehaviorDefinitions();
            var definitions = GenerateDefinitions(behaviorDefinitions);

            messageHandler.SetSectionDefinitions(SlcFacility_Management.ModuleId, sections.Values);

            messageHandler.SetDefinitions(SlcFacility_Management.ModuleId, definitions.Values);

            messageHandler.SetBehaviorDefinitions(SlcFacility_Management.ModuleId, behaviorDefinitions.Values);
        }

        private static Dictionary<SectionDefinitionID, SectionDefinition> GenerateSectionsDefinitions()
        {
            return new Dictionary<SectionDefinitionID, SectionDefinition>
            {
                {SlcFacility_Management.Sections.AppSettings.Id, GenerateSectionsDefinitions_AppSettings() },
                {SlcFacility_Management.Sections.DeskInformation.Id, GenerateSectionsDefinitions_DeskInformation() },
                {SlcFacility_Management.Sections.Facility.Id, GenerateSectionsDefinitions_Facility() },
                {SlcFacility_Management.Sections.FacilityInformation.Id, GenerateSectionsDefinitions_FacilityInformation() },
                {SlcFacility_Management.Sections.Floor.Id, GenerateSectionsDefinitions_Floor() },
                {SlcFacility_Management.Sections.FloorInformation.Id, GenerateSectionsDefinitions_FloorInformation() },
                {SlcFacility_Management.Sections.ImageInfo.Id, GenerateSectionsDefinitions_ImageInfo() },
                {SlcFacility_Management.Sections.RackCapacity.Id, GenerateSectionsDefinitions_RackCapacity() },
                {SlcFacility_Management.Sections.RackInformation.Id, GenerateSectionsDefinitions_RackInformation() },
                {SlcFacility_Management.Sections.Resource.Id, GenerateSectionsDefinitions_Resource() },
                {SlcFacility_Management.Sections.Room.Id, GenerateSectionsDefinitions_Room() },
                {SlcFacility_Management.Sections.RoomInformation.Id, GenerateSectionsDefinitions_RoomInformation() },
                {SlcFacility_Management.Sections.RoomOwnership.Id, GenerateSectionsDefinitions_RoomOwnership() },
                {SlcFacility_Management.Sections.Row.Id, GenerateSectionsDefinitions_Row() },
                {SlcFacility_Management.Sections.RowInformation.Id, GenerateSectionsDefinitions_RowInformation() },
                {SlcFacility_Management.Sections.Site.Id, GenerateSectionsDefinitions_Site() },
                {SlcFacility_Management.Sections.SiteInformation.Id, GenerateSectionsDefinitions_SiteInformation() },
                {SlcFacility_Management.Sections.Zone.Id, GenerateSectionsDefinitions_Zone() },
                {SlcFacility_Management.Sections.ZoneCapacity.Id, GenerateSectionsDefinitions_ZoneCapacity() },
                {SlcFacility_Management.Sections.ZoneInformation.Id, GenerateSectionsDefinitions_ZoneInformation() },
            };
        }

        private static SectionDefinition GenerateSectionsDefinitions_AppSettings()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcFacility_Management.Sections.AppSettings.GoogleMapsAPIKey, typeof(string), "Google Maps API Key"),
            };

            return BuildSectionDefinition(SlcFacility_Management.Sections.AppSettings.Id, "App Settings", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_DeskInformation()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcFacility_Management.Sections.DeskInformation.Name, typeof(string), "Name"),
                (SlcFacility_Management.Sections.DeskInformation.Plan, typeof(string), "Plan"),
                (SlcFacility_Management.Sections.DeskInformation.Description, typeof(string), "Description"),
                (SlcFacility_Management.Sections.DeskInformation.DeskID, typeof(string), "Desk ID"),
            };

            return BuildSectionDefinition(SlcFacility_Management.Sections.DeskInformation.Id, "Desk Information", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_Facility()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcFacility_Management.Sections.Facility.Facility_fc282509, typeof(Guid), "Facility"),
            };

            return BuildSectionDefinition(SlcFacility_Management.Sections.Facility.Id, "Facility", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_FacilityInformation()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcFacility_Management.Sections.FacilityInformation.Name, typeof(string), "Name"),
                (SlcFacility_Management.Sections.FacilityInformation.FacilityType, typeof(string), "Facility Type"),
                (SlcFacility_Management.Sections.FacilityInformation.Description, typeof(string), "Description"),
                (SlcFacility_Management.Sections.FacilityInformation.Address, typeof(string), "Address"),
                (SlcFacility_Management.Sections.FacilityInformation.City, typeof(string), "City"),
                (SlcFacility_Management.Sections.FacilityInformation.ZipCode, typeof(string), "Zip Code"),
                (SlcFacility_Management.Sections.FacilityInformation.Country, typeof(string), "Country"),
                (SlcFacility_Management.Sections.FacilityInformation.Latitude, typeof(double), "Latitude"),
                (SlcFacility_Management.Sections.FacilityInformation.Longitude, typeof(double), "Longitude"),
                (SlcFacility_Management.Sections.FacilityInformation.FacilityID, typeof(string), "Facility ID"),
            };

            return BuildSectionDefinition(SlcFacility_Management.Sections.FacilityInformation.Id, "Facility Information", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_Floor()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcFacility_Management.Sections.Floor.Floor_6a3f0eeb, typeof(Guid), "Floor"),
            };

            return BuildSectionDefinition(SlcFacility_Management.Sections.Floor.Id, "Floor", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_FloorInformation()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcFacility_Management.Sections.FloorInformation.Name, typeof(string), "Name"),
                (SlcFacility_Management.Sections.FloorInformation.Plan, typeof(string), "Plan"),
                (SlcFacility_Management.Sections.FloorInformation.Description, typeof(string), "Description"),
                (SlcFacility_Management.Sections.FloorInformation.FloorID, typeof(string), "Floor ID"),
            };

            return BuildSectionDefinition(SlcFacility_Management.Sections.FloorInformation.Id, "Floor Information", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_ImageInfo()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcFacility_Management.Sections.ImageInfo.ImageFilepath, typeof(string), "Image Filepath"),
                (SlcFacility_Management.Sections.ImageInfo.UploadTimestamp, typeof(DateTime), "Upload Timestamp"),
            };

            return BuildSectionDefinition(SlcFacility_Management.Sections.ImageInfo.Id, "Image Info", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_RackCapacity()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
    {
        (SlcFacility_Management.Sections.RackCapacity.MaximumRackCapacity, typeof(double), "Maximum Rack Capacity"),
        (SlcFacility_Management.Sections.RackCapacity.MaximumPowerCapacity, typeof(double), "Maximum Power Capacity"),
    };

            return BuildSectionDefinition(SlcFacility_Management.Sections.RackCapacity.Id, "Rack Capacity", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_RackInformation()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
    {
        (SlcFacility_Management.Sections.RackInformation.Name, typeof(string), "Name"),
        (SlcFacility_Management.Sections.RackInformation.Model, typeof(string), "Model"),
        (SlcFacility_Management.Sections.RackInformation.Position, typeof(string), "Position"),
        (SlcFacility_Management.Sections.RackInformation.Width, typeof(double), "Width"),
        (SlcFacility_Management.Sections.RackInformation.Depth, typeof(double), "Depth"),
        (SlcFacility_Management.Sections.RackInformation.Height, typeof(double), "Height"),
        (SlcFacility_Management.Sections.RackInformation.Description, typeof(string), "Description"),
        (SlcFacility_Management.Sections.RackInformation.Bookable, typeof(bool), "Bookable"),
        (SlcFacility_Management.Sections.RackInformation.CoolingFlow, typeof(string), "Cooling Flow"),
        (SlcFacility_Management.Sections.RackInformation.X, typeof(double), "X"),
        (SlcFacility_Management.Sections.RackInformation.Y, typeof(double), "Y"),
        (SlcFacility_Management.Sections.RackInformation.Label, typeof(string), "Label"),
        (SlcFacility_Management.Sections.RackInformation.Color, typeof(string), "Color"),
        (SlcFacility_Management.Sections.RackInformation.PlacementOrientation, typeof(int), "Placement Orientation"),
        (SlcFacility_Management.Sections.RackInformation.RackID, typeof(string), "Rack ID"),
    };

            return BuildSectionDefinition(SlcFacility_Management.Sections.RackInformation.Id, "Rack Information", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_Resource()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
    {
        (SlcFacility_Management.Sections.Resource.LinkedResource, typeof(Guid), "Linked Resource"),
    };

            return BuildSectionDefinition(SlcFacility_Management.Sections.Resource.Id, "Resource", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_Room()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
    {
        (SlcFacility_Management.Sections.Room.Room_8d904f8d, typeof(Guid), "Room"),
    };

            return BuildSectionDefinition(SlcFacility_Management.Sections.Room.Id, "Room", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_RoomInformation()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
    {
        (SlcFacility_Management.Sections.RoomInformation.Name, typeof(string), "Name"),
        (SlcFacility_Management.Sections.RoomInformation.Plan, typeof(string), "Plan"),
        (SlcFacility_Management.Sections.RoomInformation.Description, typeof(string), "Description"),
        (SlcFacility_Management.Sections.RoomInformation.Width, typeof(long), "Width"),
        (SlcFacility_Management.Sections.RoomInformation.Depth, typeof(long), "Depth"),
        (SlcFacility_Management.Sections.RoomInformation.RoomID, typeof(string), "Room ID"),
    };

            return BuildSectionDefinition(SlcFacility_Management.Sections.RoomInformation.Id, "Room Information", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_RoomOwnership()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
    {
        (SlcFacility_Management.Sections.RoomOwnership.Team, typeof(Guid), "Team"),
        (SlcFacility_Management.Sections.RoomOwnership.Owner, typeof(Guid), "Owner"),
    };

            return BuildSectionDefinition(SlcFacility_Management.Sections.RoomOwnership.Id, "Room Ownership", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_Row()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
    {
        (SlcFacility_Management.Sections.Row.Row_5d2f9bb3, typeof(Guid), "Row"),
    };

            return BuildSectionDefinition(SlcFacility_Management.Sections.Row.Id, "Row", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_RowInformation()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
    {
        (SlcFacility_Management.Sections.RowInformation.Name, typeof(string), "Name"),
        (SlcFacility_Management.Sections.RowInformation.RowPlan, typeof(string), "Row Plan"),
        (SlcFacility_Management.Sections.RowInformation.RowDescription, typeof(string), "Row Description"),
        (SlcFacility_Management.Sections.RowInformation.Y, typeof(double), "Y"),
        (SlcFacility_Management.Sections.RowInformation.Label, typeof(string), "Label"),
        (SlcFacility_Management.Sections.RowInformation.RowID, typeof(string), "Row ID"),
    };

            return BuildSectionDefinition(SlcFacility_Management.Sections.RowInformation.Id, "Row Information", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_Site()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcFacility_Management.Sections.Site.Site_f08b42c7, typeof(Guid), "Site"),
            };

            return BuildSectionDefinition(SlcFacility_Management.Sections.Site.Id, "Site", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_SiteInformation()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcFacility_Management.Sections.SiteInformation.Name, typeof(string), "Name"),
                (SlcFacility_Management.Sections.SiteInformation.Description, typeof(string), "Description"),
                (SlcFacility_Management.Sections.SiteInformation.Address, typeof(string), "Address"),
                (SlcFacility_Management.Sections.SiteInformation.City, typeof(string), "City"),
                (SlcFacility_Management.Sections.SiteInformation.ZipCode, typeof(string), "Zip Code"),
                (SlcFacility_Management.Sections.SiteInformation.Country, typeof(string), "Country"),
                (SlcFacility_Management.Sections.SiteInformation.Latitude, typeof(double), "Latitude"),
                (SlcFacility_Management.Sections.SiteInformation.Longitude, typeof(double), "Longitude"),
                (SlcFacility_Management.Sections.SiteInformation.SiteID, typeof(string), "Site ID"),
            };

            return BuildSectionDefinition(SlcFacility_Management.Sections.SiteInformation.Id, "Site Information", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_Zone()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
    {
        (SlcFacility_Management.Sections.Zone.Zone_45922fd2, typeof(Guid), "Zone"),
    };

            return BuildSectionDefinition(SlcFacility_Management.Sections.Zone.Id, "Zone", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_ZoneCapacity()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
    {
        (SlcFacility_Management.Sections.ZoneCapacity.CoolingCapacity, typeof(double), "Cooling Capacity"),
    };

            return BuildSectionDefinition(SlcFacility_Management.Sections.ZoneCapacity.Id, "Zone Capacity", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_ZoneInformation()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
    {
        (SlcFacility_Management.Sections.ZoneInformation.Name, typeof(string), "Name"),
        (SlcFacility_Management.Sections.ZoneInformation.Plan, typeof(string), "Plan"),
        (SlcFacility_Management.Sections.ZoneInformation.Description, typeof(string), "Description"),
        (SlcFacility_Management.Sections.ZoneInformation.ThermalType, typeof(int), "Thermal Type"),
        (SlcFacility_Management.Sections.ZoneInformation.X, typeof(double), "X"),
        (SlcFacility_Management.Sections.ZoneInformation.Y, typeof(double), "Y"),
        (SlcFacility_Management.Sections.ZoneInformation.Width, typeof(double), "Width"),
        (SlcFacility_Management.Sections.ZoneInformation.Depth, typeof(double), "Depth"),
        (SlcFacility_Management.Sections.ZoneInformation.ZoneID, typeof(string), "Zone ID"),
    };

            return BuildSectionDefinition(SlcFacility_Management.Sections.ZoneInformation.Id, "Zone Information", fields);
        }

        private static SectionDefinition BuildSectionDefinition(SectionDefinitionID sectionId, string sectionName, List<(FieldDescriptorID id, Type type, string name)> fields)
        {
            var builder = new SectionDefinitionBuilder();
            builder.WithID(sectionId);
            builder.WithName(sectionName);

            foreach (var field in fields)
            {
                builder.AddFieldDescriptor(new FieldDescriptorBuilder()
                    .WithID(field.id)
                    .WithType(field.type)
                    .WithName(field.name)
                    .Build());
            }

            return builder.Build();
        }

        private static Dictionary<DomDefinitionId, DomDefinition> GenerateDefinitions(Dictionary<DomBehaviorDefinitionId, DomBehaviorDefinition> behaviors)
        {
            return new Dictionary<DomDefinitionId, DomDefinition>
            {
                { SlcFacility_Management.Definitions.Desk, GenerateDefinitions_Desk(behaviors) },
                { SlcFacility_Management.Definitions.Facility, GenerateDefinitions_Facility(behaviors) },
                { SlcFacility_Management.Definitions.FacilityManagerAppSettings, GenerateDefinitions_FacilityManagerAppSettings() },
                { SlcFacility_Management.Definitions.Floor, GenerateDefinitions_Floor(behaviors) },
                { SlcFacility_Management.Definitions.Rack, GenerateDefinitions_Rack(behaviors) },
                { SlcFacility_Management.Definitions.Room, GenerateDefinitions_Room(behaviors) },
                { SlcFacility_Management.Definitions.Row, GenerateDefinitions_Row(behaviors) },
                { SlcFacility_Management.Definitions.Site, GenerateDefinitions_Site(behaviors) },
                { SlcFacility_Management.Definitions.Zone, GenerateDefinitions_Zone(behaviors) },
            };
        }

        private static DomDefinition GenerateDefinitions_Desk(Dictionary<DomBehaviorDefinitionId, DomBehaviorDefinition> behaviors)
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcFacility_Management.Definitions.Desk.Id)
                .WithName("Desk");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcFacility_Management.Sections.DeskInformation.Id),
                new SectionDefinitionLink(SlcFacility_Management.Sections.Room.Id),
                new SectionDefinitionLink(SlcFacility_Management.Sections.Resource.Id),
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            builder.WithDomBehaviorDefinition(behaviors[SlcFacility_Management.Behaviors.Desk_Behaviour.Id]);

            var definition = builder.Build();

            definition.ModuleSettingsOverrides = new ModuleSettingsOverrides()
            {
                NameDefinition = new DomInstanceNameDefinition()
                {
                    ConcatenationItems = new List<IDomInstanceConcatenationItem>
                    {
                        new FieldValueConcatenationItem(SlcFacility_Management.Sections.DeskInformation.Name),
                    },
                },
            };

            return definition;
        }

        private static DomDefinition GenerateDefinitions_Facility(Dictionary<DomBehaviorDefinitionId, DomBehaviorDefinition> behaviors)
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcFacility_Management.Definitions.Facility.Id)
                .WithName("Facility");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcFacility_Management.Sections.FacilityInformation.Id),
                new SectionDefinitionLink(SlcFacility_Management.Sections.Site.Id),
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            builder.WithDomBehaviorDefinition(behaviors[SlcFacility_Management.Behaviors.Facility_Behaviour.Id]);

            var definition = builder.Build();

            definition.ModuleSettingsOverrides = new ModuleSettingsOverrides()
            {
                NameDefinition = new DomInstanceNameDefinition()
                {
                    ConcatenationItems = new List<IDomInstanceConcatenationItem>
                    {
                        new FieldValueConcatenationItem(SlcFacility_Management.Sections.FacilityInformation.Name),
                    },
                },
            };

            return definition;
        }

        private static DomDefinition GenerateDefinitions_FacilityManagerAppSettings()
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcFacility_Management.Definitions.FacilityManagerAppSettings.Id)
                .WithName("Facility Manager App Settings");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcFacility_Management.Sections.AppSettings.Id),
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            var definition = builder.Build();

            definition.ModuleSettingsOverrides = new ModuleSettingsOverrides();

            return definition;
        }

        private static DomDefinition GenerateDefinitions_Floor(Dictionary<DomBehaviorDefinitionId, DomBehaviorDefinition> behaviors)
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcFacility_Management.Definitions.Floor.Id)
                .WithName("Floor");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcFacility_Management.Sections.FloorInformation.Id),
                new SectionDefinitionLink(SlcFacility_Management.Sections.Facility.Id),
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            builder.WithDomBehaviorDefinition(behaviors[SlcFacility_Management.Behaviors.Floor_Behaviour.Id]);

            var definition = builder.Build();

            definition.ModuleSettingsOverrides = new ModuleSettingsOverrides()
            {
                NameDefinition = new DomInstanceNameDefinition()
                {
                    ConcatenationItems = new List<IDomInstanceConcatenationItem>
                    {
                        new FieldValueConcatenationItem(SlcFacility_Management.Sections.FloorInformation.Name),
                    },
                },
            };

            return definition;
        }

        private static DomDefinition GenerateDefinitions_Rack(Dictionary<DomBehaviorDefinitionId, DomBehaviorDefinition> behaviors)
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcFacility_Management.Definitions.Rack.Id)
                .WithName("Rack");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcFacility_Management.Sections.RackInformation.Id),
                new SectionDefinitionLink(SlcFacility_Management.Sections.RackCapacity.Id),
                new SectionDefinitionLink(SlcFacility_Management.Sections.Row.Id),
                new SectionDefinitionLink(SlcFacility_Management.Sections.Zone.Id),
                new SectionDefinitionLink(SlcFacility_Management.Sections.Resource.Id),
                new SectionDefinitionLink(SlcFacility_Management.Sections.ImageInfo.Id) { AllowMultipleSections = true },
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            builder.WithDomBehaviorDefinition(behaviors[SlcFacility_Management.Behaviors.Rack_Behaviour.Id]);

            var definition = builder.Build();

            definition.ModuleSettingsOverrides = new ModuleSettingsOverrides()
            {
                NameDefinition = new DomInstanceNameDefinition()
                {
                    ConcatenationItems = new List<IDomInstanceConcatenationItem>
                    {
                        new FieldValueConcatenationItem(SlcFacility_Management.Sections.RackInformation.Name),
                    },
                },
            };

            return definition;
        }

        private static DomDefinition GenerateDefinitions_Room(Dictionary<DomBehaviorDefinitionId, DomBehaviorDefinition> behaviors)
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcFacility_Management.Definitions.Room.Id)
                .WithName("Room");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcFacility_Management.Sections.RoomInformation.Id),
                new SectionDefinitionLink(SlcFacility_Management.Sections.RoomOwnership.Id),
                new SectionDefinitionLink(SlcFacility_Management.Sections.Floor.Id),
                new SectionDefinitionLink(SlcFacility_Management.Sections.Resource.Id),
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            builder.WithDomBehaviorDefinition(behaviors[SlcFacility_Management.Behaviors.Room_Behaviour.Id]);

            var definition = builder.Build();

            definition.ModuleSettingsOverrides = new ModuleSettingsOverrides()
            {
                NameDefinition = new DomInstanceNameDefinition()
                {
                    ConcatenationItems = new List<IDomInstanceConcatenationItem>
                    {
                        new FieldValueConcatenationItem(SlcFacility_Management.Sections.RoomInformation.Name),
                    },
                },
            };

            return definition;
        }

        private static DomDefinition GenerateDefinitions_Row(Dictionary<DomBehaviorDefinitionId, DomBehaviorDefinition> behaviors)
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcFacility_Management.Definitions.Row.Id)
                .WithName("Row");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcFacility_Management.Sections.RowInformation.Id),
                new SectionDefinitionLink(SlcFacility_Management.Sections.Room.Id),
                new SectionDefinitionLink(SlcFacility_Management.Sections.Resource.Id),
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            builder.WithDomBehaviorDefinition(behaviors[SlcFacility_Management.Behaviors.Row_Behaviour.Id]);

            var definition = builder.Build();

            definition.ModuleSettingsOverrides = new ModuleSettingsOverrides()
            {
                NameDefinition = new DomInstanceNameDefinition()
                {
                    ConcatenationItems = new List<IDomInstanceConcatenationItem>
                    {
                        new FieldValueConcatenationItem(SlcFacility_Management.Sections.RowInformation.Name),
                    },
                },
            };

            return definition;
        }

        private static DomDefinition GenerateDefinitions_Site(Dictionary<DomBehaviorDefinitionId, DomBehaviorDefinition> behaviors)
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcFacility_Management.Definitions.Site.Id)
                .WithName("Site");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcFacility_Management.Sections.SiteInformation.Id),
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            builder.WithDomBehaviorDefinition(behaviors[SlcFacility_Management.Behaviors.Site_Behaviour.Id]);

            var definition = builder.Build();

            definition.ModuleSettingsOverrides = new ModuleSettingsOverrides()
            {
                NameDefinition = new DomInstanceNameDefinition()
                {
                    ConcatenationItems = new List<IDomInstanceConcatenationItem>
                    {
                        new FieldValueConcatenationItem(SlcFacility_Management.Sections.SiteInformation.Name),
                    },
                },
            };

            return definition;
        }

        private static DomDefinition GenerateDefinitions_Zone(Dictionary<DomBehaviorDefinitionId, DomBehaviorDefinition> behaviors)
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcFacility_Management.Definitions.Zone.Id)
                .WithName("Zone");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcFacility_Management.Sections.ZoneInformation.Id),
                new SectionDefinitionLink(SlcFacility_Management.Sections.ZoneCapacity.Id),
                new SectionDefinitionLink(SlcFacility_Management.Sections.Room.Id),
                new SectionDefinitionLink(SlcFacility_Management.Sections.Resource.Id),
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            builder.WithDomBehaviorDefinition(behaviors[SlcFacility_Management.Behaviors.Zone_Behaviour.Id]);

            var definition = builder.Build();

            definition.ModuleSettingsOverrides = new ModuleSettingsOverrides()
            {
                NameDefinition = new DomInstanceNameDefinition()
                {
                    ConcatenationItems = new List<IDomInstanceConcatenationItem>
                    {
                        new FieldValueConcatenationItem(SlcFacility_Management.Sections.ZoneInformation.Name),
                    },
                },
            };

            return definition;
        }

        private static Dictionary<DomBehaviorDefinitionId, DomBehaviorDefinition> GenerateBehaviorDefinitions()
        {
            return new Dictionary<DomBehaviorDefinitionId, DomBehaviorDefinition>
            {
                { SlcFacility_Management.Behaviors.Desk_Behaviour.Id, GenerateBehaviorDefinitions_Desk_Behaviour() },
                { SlcFacility_Management.Behaviors.Facility_Behaviour.Id, GenerateBehaviorDefinitions_Facility_Behaviour() },
                { SlcFacility_Management.Behaviors.Floor_Behaviour.Id, GenerateBehaviorDefinitions_Floor_Behaviour() },
                { SlcFacility_Management.Behaviors.Rack_Behaviour.Id, GenerateBehaviorDefinitions_Rack_Behaviour() },
                { SlcFacility_Management.Behaviors.Room_Behaviour.Id, GenerateBehaviorDefinitions_Room_Behaviour() },
                { SlcFacility_Management.Behaviors.Row_Behaviour.Id, GenerateBehaviorDefinitions_Row_Behaviour() },
                { SlcFacility_Management.Behaviors.Site_Behaviour.Id, GenerateBehaviorDefinitions_Site_Behaviour() },
                { SlcFacility_Management.Behaviors.Zone_Behaviour.Id, GenerateBehaviorDefinitions_Zone_Behaviour() },
            };
        }

        private static DomBehaviorDefinition GenerateBehaviorDefinitions_Desk_Behaviour()
        {
            List<DomStatus> status = new List<DomStatus>
            {
                new DomStatus(SlcFacility_Management.Behaviors.Desk_Behaviour.Statuses.Draft, "Draft"),
                new DomStatus(SlcFacility_Management.Behaviors.Desk_Behaviour.Statuses.Active, "Active"),
                new DomStatus(SlcFacility_Management.Behaviors.Desk_Behaviour.Statuses.Deprecated, "Deprecated"),
            };

            List<DomStatusTransition> statusTransitions = new List<DomStatusTransition>
            {
                new DomStatusTransition(SlcFacility_Management.Behaviors.Desk_Behaviour.Transitions.Draft_Active, SlcFacility_Management.Behaviors.Desk_Behaviour.Statuses.Draft, SlcFacility_Management.Behaviors.Desk_Behaviour.Statuses.Active),
                new DomStatusTransition(SlcFacility_Management.Behaviors.Desk_Behaviour.Transitions.Active_Deprecated, SlcFacility_Management.Behaviors.Desk_Behaviour.Statuses.Active, SlcFacility_Management.Behaviors.Desk_Behaviour.Statuses.Deprecated),
            };

            DomBehaviorDefinitionBuilder builder = new DomBehaviorDefinitionBuilder()
                .WithID(SlcFacility_Management.Behaviors.Desk_Behaviour.Id)
                .WithName("Desk_Behaviour")
                .WithInitialStatusId(SlcFacility_Management.Behaviors.Desk_Behaviour.Statuses.Draft)
                .WithStatuses(status)
                .WithStatusTransitions(statusTransitions);

            return builder.Build();
        }

        private static DomBehaviorDefinition GenerateBehaviorDefinitions_Facility_Behaviour()
        {
            List<DomStatus> status = new List<DomStatus>
            {
                new DomStatus(SlcFacility_Management.Behaviors.Facility_Behaviour.Statuses.Draft, "Draft"),
                new DomStatus(SlcFacility_Management.Behaviors.Facility_Behaviour.Statuses.Active, "Active"),
                new DomStatus(SlcFacility_Management.Behaviors.Facility_Behaviour.Statuses.Deprecated, "Deprecated"),
            };

            List<DomStatusTransition> statusTransitions = new List<DomStatusTransition>
            {
                new DomStatusTransition(SlcFacility_Management.Behaviors.Facility_Behaviour.Transitions.Draft_Active, SlcFacility_Management.Behaviors.Facility_Behaviour.Statuses.Draft, SlcFacility_Management.Behaviors.Facility_Behaviour.Statuses.Active),
                new DomStatusTransition(SlcFacility_Management.Behaviors.Facility_Behaviour.Transitions.Active_Deprecated, SlcFacility_Management.Behaviors.Facility_Behaviour.Statuses.Active, SlcFacility_Management.Behaviors.Facility_Behaviour.Statuses.Deprecated),
            };

            DomBehaviorDefinitionBuilder builder = new DomBehaviorDefinitionBuilder()
                .WithID(SlcFacility_Management.Behaviors.Facility_Behaviour.Id)
                .WithName("Facility_Behaviour")
                .WithInitialStatusId(SlcFacility_Management.Behaviors.Facility_Behaviour.Statuses.Draft)
                .WithStatuses(status)
                .WithStatusTransitions(statusTransitions);

            return builder.Build();
        }

        private static DomBehaviorDefinition GenerateBehaviorDefinitions_Floor_Behaviour()
        {
            List<DomStatus> status = new List<DomStatus>
            {
                new DomStatus(SlcFacility_Management.Behaviors.Floor_Behaviour.Statuses.Draft, "Draft"),
                new DomStatus(SlcFacility_Management.Behaviors.Floor_Behaviour.Statuses.Active, "Active"),
                new DomStatus(SlcFacility_Management.Behaviors.Floor_Behaviour.Statuses.Deprecated, "Deprecated"),
            };

            List<DomStatusTransition> statusTransitions = new List<DomStatusTransition>
            {
                new DomStatusTransition(SlcFacility_Management.Behaviors.Floor_Behaviour.Transitions.Draft_Active, SlcFacility_Management.Behaviors.Floor_Behaviour.Statuses.Draft, SlcFacility_Management.Behaviors.Floor_Behaviour.Statuses.Active),
                new DomStatusTransition(SlcFacility_Management.Behaviors.Floor_Behaviour.Transitions.Active_Deprecated, SlcFacility_Management.Behaviors.Floor_Behaviour.Statuses.Active, SlcFacility_Management.Behaviors.Floor_Behaviour.Statuses.Deprecated),
            };

            DomBehaviorDefinitionBuilder builder = new DomBehaviorDefinitionBuilder()
                .WithID(SlcFacility_Management.Behaviors.Floor_Behaviour.Id)
                .WithName("Floor_Behaviour")
                .WithInitialStatusId(SlcFacility_Management.Behaviors.Floor_Behaviour.Statuses.Draft)
                .WithStatuses(status)
                .WithStatusTransitions(statusTransitions);

            return builder.Build();
        }

        private static DomBehaviorDefinition GenerateBehaviorDefinitions_Rack_Behaviour()
        {
            List<DomStatus> status = new List<DomStatus>
            {
                new DomStatus(SlcFacility_Management.Behaviors.Rack_Behaviour.Statuses.Draft, "Draft"),
                new DomStatus(SlcFacility_Management.Behaviors.Rack_Behaviour.Statuses.Active, "Active"),
                new DomStatus(SlcFacility_Management.Behaviors.Rack_Behaviour.Statuses.Deprecated, "Deprecated"),
            };

            List<DomStatusTransition> statusTransitions = new List<DomStatusTransition>
            {
                new DomStatusTransition(SlcFacility_Management.Behaviors.Rack_Behaviour.Transitions.Draft_Active, SlcFacility_Management.Behaviors.Rack_Behaviour.Statuses.Draft, SlcFacility_Management.Behaviors.Rack_Behaviour.Statuses.Active),
                new DomStatusTransition(SlcFacility_Management.Behaviors.Rack_Behaviour.Transitions.Active_Deprecated, SlcFacility_Management.Behaviors.Rack_Behaviour.Statuses.Active, SlcFacility_Management.Behaviors.Rack_Behaviour.Statuses.Deprecated),
            };

            DomBehaviorDefinitionBuilder builder = new DomBehaviorDefinitionBuilder()
                .WithID(SlcFacility_Management.Behaviors.Rack_Behaviour.Id)
                .WithName("Rack_Behaviour")
                .WithInitialStatusId(SlcFacility_Management.Behaviors.Rack_Behaviour.Statuses.Draft)
                .WithStatuses(status)
                .WithStatusTransitions(statusTransitions);

            return builder.Build();
        }

        private static DomBehaviorDefinition GenerateBehaviorDefinitions_Room_Behaviour()
        {
            List<DomStatus> status = new List<DomStatus>
            {
                new DomStatus(SlcFacility_Management.Behaviors.Room_Behaviour.Statuses.Draft, "Draft"),
                new DomStatus(SlcFacility_Management.Behaviors.Room_Behaviour.Statuses.Active, "Active"),
                new DomStatus(SlcFacility_Management.Behaviors.Room_Behaviour.Statuses.Deprecated, "Deprecated"),
            };

            List<DomStatusTransition> statusTransitions = new List<DomStatusTransition>
            {
                new DomStatusTransition(SlcFacility_Management.Behaviors.Room_Behaviour.Transitions.Draft_Active, SlcFacility_Management.Behaviors.Room_Behaviour.Statuses.Draft, SlcFacility_Management.Behaviors.Room_Behaviour.Statuses.Active),
                new DomStatusTransition(SlcFacility_Management.Behaviors.Room_Behaviour.Transitions.Active_Deprecated, SlcFacility_Management.Behaviors.Room_Behaviour.Statuses.Active, SlcFacility_Management.Behaviors.Room_Behaviour.Statuses.Deprecated),
            };

            DomBehaviorDefinitionBuilder builder = new DomBehaviorDefinitionBuilder()
                .WithID(SlcFacility_Management.Behaviors.Room_Behaviour.Id)
                .WithName("Room_Behaviour")
                .WithInitialStatusId(SlcFacility_Management.Behaviors.Room_Behaviour.Statuses.Draft)
                .WithStatuses(status)
                .WithStatusTransitions(statusTransitions);

            return builder.Build();
        }

        private static DomBehaviorDefinition GenerateBehaviorDefinitions_Row_Behaviour()
        {
            List<DomStatus> status = new List<DomStatus>
            {
                new DomStatus(SlcFacility_Management.Behaviors.Row_Behaviour.Statuses.Draft, "Draft"),
                new DomStatus(SlcFacility_Management.Behaviors.Row_Behaviour.Statuses.Active, "Active"),
                new DomStatus(SlcFacility_Management.Behaviors.Row_Behaviour.Statuses.Deprecated, "Deprecated"),
            };

            List<DomStatusTransition> statusTransitions = new List<DomStatusTransition>
            {
                new DomStatusTransition(SlcFacility_Management.Behaviors.Row_Behaviour.Transitions.Draft_Active, SlcFacility_Management.Behaviors.Row_Behaviour.Statuses.Draft, SlcFacility_Management.Behaviors.Row_Behaviour.Statuses.Active),
                new DomStatusTransition(SlcFacility_Management.Behaviors.Row_Behaviour.Transitions.Active_Deprecated, SlcFacility_Management.Behaviors.Row_Behaviour.Statuses.Active, SlcFacility_Management.Behaviors.Row_Behaviour.Statuses.Deprecated),
            };

            DomBehaviorDefinitionBuilder builder = new DomBehaviorDefinitionBuilder()
                .WithID(SlcFacility_Management.Behaviors.Row_Behaviour.Id)
                .WithName("Row_Behaviour")
                .WithInitialStatusId(SlcFacility_Management.Behaviors.Row_Behaviour.Statuses.Draft)
                .WithStatuses(status)
                .WithStatusTransitions(statusTransitions);

            return builder.Build();
        }

        private static DomBehaviorDefinition GenerateBehaviorDefinitions_Site_Behaviour()
        {
            List<DomStatus> status = new List<DomStatus>
            {
                new DomStatus(SlcFacility_Management.Behaviors.Site_Behaviour.Statuses.Draft, "Draft"),
                new DomStatus(SlcFacility_Management.Behaviors.Site_Behaviour.Statuses.Active, "Active"),
                new DomStatus(SlcFacility_Management.Behaviors.Site_Behaviour.Statuses.Deprecated, "Deprecated"),
            };

            List<DomStatusTransition> statusTransitions = new List<DomStatusTransition>
            {
                new DomStatusTransition(SlcFacility_Management.Behaviors.Site_Behaviour.Transitions.Draft_Active, SlcFacility_Management.Behaviors.Site_Behaviour.Statuses.Draft, SlcFacility_Management.Behaviors.Site_Behaviour.Statuses.Active),
                new DomStatusTransition(SlcFacility_Management.Behaviors.Site_Behaviour.Transitions.Active_Deprecated, SlcFacility_Management.Behaviors.Site_Behaviour.Statuses.Active, SlcFacility_Management.Behaviors.Site_Behaviour.Statuses.Deprecated),
            };

            DomBehaviorDefinitionBuilder builder = new DomBehaviorDefinitionBuilder()
                .WithID(SlcFacility_Management.Behaviors.Site_Behaviour.Id)
                .WithName("Site_Behaviour")
                .WithInitialStatusId(SlcFacility_Management.Behaviors.Site_Behaviour.Statuses.Draft)
                .WithStatuses(status)
                .WithStatusTransitions(statusTransitions);

            return builder.Build();
        }

        private static DomBehaviorDefinition GenerateBehaviorDefinitions_Zone_Behaviour()
        {
            List<DomStatus> status = new List<DomStatus>
            {
                new DomStatus(SlcFacility_Management.Behaviors.Zone_Behaviour.Statuses.Draft, "Draft"),
                new DomStatus(SlcFacility_Management.Behaviors.Zone_Behaviour.Statuses.Active, "Active"),
                new DomStatus(SlcFacility_Management.Behaviors.Zone_Behaviour.Statuses.Deprecated, "Deprecated"),
            };

            List<DomStatusTransition> statusTransitions = new List<DomStatusTransition>
            {
                new DomStatusTransition(SlcFacility_Management.Behaviors.Zone_Behaviour.Transitions.Draft_Active, SlcFacility_Management.Behaviors.Zone_Behaviour.Statuses.Draft, SlcFacility_Management.Behaviors.Zone_Behaviour.Statuses.Active),
                new DomStatusTransition(SlcFacility_Management.Behaviors.Zone_Behaviour.Transitions.Active_Deprecated, SlcFacility_Management.Behaviors.Zone_Behaviour.Statuses.Active, SlcFacility_Management.Behaviors.Zone_Behaviour.Statuses.Deprecated),
            };

            DomBehaviorDefinitionBuilder builder = new DomBehaviorDefinitionBuilder()
                .WithID(SlcFacility_Management.Behaviors.Zone_Behaviour.Id)
                .WithName("Zone_Behaviour")
                .WithInitialStatusId(SlcFacility_Management.Behaviors.Zone_Behaviour.Statuses.Draft)
                .WithStatuses(status)
                .WithStatusTransitions(statusTransitions);

            return builder.Build();
        }

    }
}
