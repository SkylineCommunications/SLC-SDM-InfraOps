namespace Skyline.DataMiner.SDM.AssetManagement.Common.Mock
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

    public static class AssetManagementMockMessageHandler
    {
        public static void AddAssetManagementModule(this DomSLNetMessageHandler messageHandler)
        {
            var sections = GenerateSectionsDefinitions();
            var behaviorDefinitions = GenerateBehaviorDefinitions();
            var definitions = GenerateDefinitions(behaviorDefinitions);

            messageHandler.SetSectionDefinitions(SlcAsset_Management.ModuleId, sections.Values);

            messageHandler.SetDefinitions(SlcAsset_Management.ModuleId, definitions.Values);

            messageHandler.SetBehaviorDefinitions(SlcAsset_Management.ModuleId, behaviorDefinitions.Values);
        }

        private static Dictionary<SectionDefinitionID, SectionDefinition> GenerateSectionsDefinitions()
        {
            return new Dictionary<SectionDefinitionID, SectionDefinition>
            {
                {SlcAsset_Management.Sections.AddressInfo.Id, GenerateSectionsDefinitions_AddressInfo() },
                {SlcAsset_Management.Sections.AppSettings.Id, GenerateSectionsDefinitions_AppSettings() },
                {SlcAsset_Management.Sections.Asset.Id, GenerateSectionsDefinitions_Asset() },
                {SlcAsset_Management.Sections.AssetClassInfo.Id, GenerateSectionsDefinitions_AssetClassInfo() },
                {SlcAsset_Management.Sections.AssetClassLifecycleInformation.Id, GenerateSectionsDefinitions_AssetClassLifecycleInformation() },
                {SlcAsset_Management.Sections.AssetCustody.Id, GenerateSectionsDefinitions_AssetCustody() },
                {SlcAsset_Management.Sections.AssetInformation.Id, GenerateSectionsDefinitions_AssetInformation() },
                {SlcAsset_Management.Sections.AssetLifecycle.Id, GenerateSectionsDefinitions_AssetLifecycle() },
                {SlcAsset_Management.Sections.AssetLocation.Id, GenerateSectionsDefinitions_AssetLocation() },
                {SlcAsset_Management.Sections.AssetLocationDestination.Id, GenerateSectionsDefinitions_AssetLocationDestination() },
                {SlcAsset_Management.Sections.AssetNetworkDetails.Id, GenerateSectionsDefinitions_AssetNetworkDetails() },
                {SlcAsset_Management.Sections.AssetOwnership.Id, GenerateSectionsDefinitions_AssetOwnership() },
                {SlcAsset_Management.Sections.Attachment.Id, GenerateSectionsDefinitions_Attachment() },
                {SlcAsset_Management.Sections.CableCompatibility.Id, GenerateSectionsDefinitions_CableCompatibility() },
                {SlcAsset_Management.Sections.CableInformation.Id, GenerateSectionsDefinitions_CableInformation() },
                {SlcAsset_Management.Sections.CableTypeInformation.Id, GenerateSectionsDefinitions_CableTypeInformation() },
                {SlcAsset_Management.Sections.CategoryLink.Id, GenerateSectionsDefinitions_CategoryLink() },
                {SlcAsset_Management.Sections.ConnectionInfo.Id, GenerateSectionsDefinitions_ConnectionInfo() },
                {SlcAsset_Management.Sections.DataPortInfo.Id, GenerateSectionsDefinitions_DataPortInfo() },
                {SlcAsset_Management.Sections.DestinationInfo.Id, GenerateSectionsDefinitions_DestinationInfo() },
                {SlcAsset_Management.Sections.DeviceTypeInformation.Id, GenerateSectionsDefinitions_DeviceTypeInformation() },
                {SlcAsset_Management.Sections.ElementLink.Id, GenerateSectionsDefinitions_ElementLink() },
                {SlcAsset_Management.Sections.HierarchyInfo.Id, GenerateSectionsDefinitions_HierarchyInfo() },
                {SlcAsset_Management.Sections.HistoryInfo.Id, GenerateSectionsDefinitions_HistoryInfo() },
                {SlcAsset_Management.Sections.Holders.Id, GenerateSectionsDefinitions_Holders() },
                {SlcAsset_Management.Sections.LinkedJob.Id, GenerateSectionsDefinitions_LinkedJob() },
                {SlcAsset_Management.Sections.PortTypeInformation.Id, GenerateSectionsDefinitions_PortTypeInformation() },
                {SlcAsset_Management.Sections.PowerPortInfo.Id, GenerateSectionsDefinitions_PowerPortInfo() },
                {SlcAsset_Management.Sections.PrimaryPortRelation.Id, GenerateSectionsDefinitions_PrimaryPortRelation() },
                {SlcAsset_Management.Sections.ProtocolLink.Id, GenerateSectionsDefinitions_ProtocolLink() },
                {SlcAsset_Management.Sections.ReservationInfo.Id, GenerateSectionsDefinitions_ReservationInfo() },
                {SlcAsset_Management.Sections.ReservationRack.Id, GenerateSectionsDefinitions_ReservationRack() },
                {SlcAsset_Management.Sections.ReservedPositions.Id, GenerateSectionsDefinitions_ReservedPositions() },
                {SlcAsset_Management.Sections.SourceInfo.Id, GenerateSectionsDefinitions_SourceInfo() },
                {SlcAsset_Management.Sections.TagsInfo.Id, GenerateSectionsDefinitions_TagsInfo() },
            };
        }

        private static SectionDefinition GenerateSectionsDefinitions_AddressInfo()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.AddressInfo.IPV4Address, typeof(string), "IPV4 Address"),
                (SlcAsset_Management.Sections.AddressInfo.IPV6Address, typeof(string), "IPV6 Address"),
                (SlcAsset_Management.Sections.AddressInfo.Hostname, typeof(string), "Hostname"),
                (SlcAsset_Management.Sections.AddressInfo.DNS, typeof(bool), "DNS"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.AddressInfo.Id, "Address Info", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_AppSettings()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.AppSettings.EnableAssetHistory, typeof(bool), "Enable Asset History"),
                (SlcAsset_Management.Sections.AppSettings.PlanAndBuildJobPrompt, typeof(int), "Plan and Build Job Prompt"),
                (SlcAsset_Management.Sections.AppSettings.EnableConnectionHistory, typeof(bool), "Enable Connection History"),
                (SlcAsset_Management.Sections.AppSettings.HistoryTTL, typeof(TimeSpan), "History TTL"),
                (SlcAsset_Management.Sections.AppSettings.HistoryLimit, typeof(long), "History Limit"),
                (SlcAsset_Management.Sections.AppSettings.EnableResourceLink, typeof(bool), "Enable Resource Link"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.AppSettings.Id, "App Settings", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_Asset()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.Asset.AssetID, typeof(Guid), "Asset ID"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.Asset.Id, "Asset", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_AssetClassInfo()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.AssetClassInfo.Name, typeof(string), "Name"),
                (SlcAsset_Management.Sections.AssetClassInfo.DeviceType, typeof(Guid), "Device Type"),
                (SlcAsset_Management.Sections.AssetClassInfo.Description, typeof(string), "Description"),
                (SlcAsset_Management.Sections.AssetClassInfo.Manufacturer, typeof(Guid), "Manufacturer"),
                (SlcAsset_Management.Sections.AssetClassInfo.Height, typeof(double), "Height"),
                (SlcAsset_Management.Sections.AssetClassInfo.Depth, typeof(double), "Depth"),
                (SlcAsset_Management.Sections.AssetClassInfo.Width, typeof(double), "Width"),
                (SlcAsset_Management.Sections.AssetClassInfo.HeightU, typeof(double), "Height (U)"),
                (SlcAsset_Management.Sections.AssetClassInfo.Weight, typeof(double), "Weight"),
                (SlcAsset_Management.Sections.AssetClassInfo.MaximumPowerConsumption, typeof(double), "Maximum Power Consumption"),
                (SlcAsset_Management.Sections.AssetClassInfo.TypicalPowerConsumption, typeof(double), "Typical Power Consumption"),
                (SlcAsset_Management.Sections.AssetClassInfo.FrontImage, typeof(string), "Front Image"),
                (SlcAsset_Management.Sections.AssetClassInfo.BackImage, typeof(string), "Back Image"),
                (SlcAsset_Management.Sections.AssetClassInfo.PowerSupply, typeof(string), "Power Supply"),
                (SlcAsset_Management.Sections.AssetClassInfo.Citype, typeof(string), "CIType"),
                (SlcAsset_Management.Sections.AssetClassInfo.Isbookable, typeof(bool), "IsBookable"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.AssetClassInfo.Id, "Asset Class Info", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_AssetClassLifecycleInformation()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.AssetClassLifecycleInformation.EOLDate, typeof(DateTime), "EOL Date"),
                (SlcAsset_Management.Sections.AssetClassLifecycleInformation.EOSDate, typeof(DateTime), "EOS Date"),
                (SlcAsset_Management.Sections.AssetClassLifecycleInformation.NominalLifetime, typeof(TimeSpan), "Nominal Lifetime"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.AssetClassLifecycleInformation.Id, "Asset Class Lifecycle Information", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_AssetCustody()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.AssetCustody.From, typeof(DateTime), "From"),
                (SlcAsset_Management.Sections.AssetCustody.Till, typeof(DateTime), "Till"),
                (SlcAsset_Management.Sections.AssetCustody.ContactPerson, typeof(Guid), "Contact person"),
                (SlcAsset_Management.Sections.AssetCustody.Team, typeof(Guid), "Team"),
                (SlcAsset_Management.Sections.AssetCustody.Organization, typeof(Guid), "Organization"),
                (SlcAsset_Management.Sections.AssetCustody.ContactPersonRole, typeof(Guid), "Contact person role"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.AssetCustody.Id, "Asset Custody", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_AssetInformation()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.AssetInformation.AssetName, typeof(string), "Asset Name"),
                (SlcAsset_Management.Sections.AssetInformation.SerialNumber, typeof(string), "Serial number"),
                (SlcAsset_Management.Sections.AssetInformation.AssetClass, typeof(Guid), "Asset Class"),
                (SlcAsset_Management.Sections.AssetInformation.AssetDescription, typeof(string), "Asset Description"),
                (SlcAsset_Management.Sections.AssetInformation.FWOS, typeof(string), "FW/OS"),
                (SlcAsset_Management.Sections.AssetInformation.AssetID, typeof(string), "Asset ID"),
                (SlcAsset_Management.Sections.AssetInformation.HardwareVersion, typeof(string), "Hardware Version"),
                (SlcAsset_Management.Sections.AssetInformation.OperationalFlags, typeof(List<int>), "Operational Flags"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.AssetInformation.Id, "Asset Information", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_AssetLifecycle()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.AssetLifecycle.PurchaseDate, typeof(DateTime), "Purchase date"),
                (SlcAsset_Management.Sections.AssetLifecycle.FirstUseDate, typeof(DateTime), "First-use date"),
                (SlcAsset_Management.Sections.AssetLifecycle.EndOfWarrantyDate, typeof(DateTime), "End of warranty date"),
                (SlcAsset_Management.Sections.AssetLifecycle.InstallationDate, typeof(DateTime), "Installation Date"),
                (SlcAsset_Management.Sections.AssetLifecycle.InstallationUser, typeof(Guid), "Installation User"),
                (SlcAsset_Management.Sections.AssetLifecycle.ModificationDate, typeof(DateTime), "Modification Date"),
                (SlcAsset_Management.Sections.AssetLifecycle.ModificationUser, typeof(Guid), "Modification User"),
                (SlcAsset_Management.Sections.AssetLifecycle.EndOfLife, typeof(DateTime), "End Of Life"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.AssetLifecycle.Id, "Asset Lifecycle", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_AssetLocation()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.AssetLocation.RackPosition, typeof(long), "Rack Position"),
                (SlcAsset_Management.Sections.AssetLocation.Side, typeof(string), "Side"),
                (SlcAsset_Management.Sections.AssetLocation.Rack, typeof(Guid), "Rack"),
                (SlcAsset_Management.Sections.AssetLocation.Desk, typeof(Guid), "Desk"),
                (SlcAsset_Management.Sections.AssetLocation.Container, typeof(Guid), "Container"),
                (SlcAsset_Management.Sections.AssetLocation.Room, typeof(Guid), "Room"),
                (SlcAsset_Management.Sections.AssetLocation.PowerSupplyRackPosition, typeof(long), "Power Supply Rack Position"),
                (SlcAsset_Management.Sections.AssetLocation.ParentAsset, typeof(Guid), "Parent Asset"),
                (SlcAsset_Management.Sections.AssetLocation.HolderNumber, typeof(long), "Holder Number"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.AssetLocation.Id, "Asset Location", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_AssetLocationDestination()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.AssetLocationDestination.RackPosition, typeof(long), "Rack Position"),
                (SlcAsset_Management.Sections.AssetLocationDestination.Side, typeof(string), "Side"),
                (SlcAsset_Management.Sections.AssetLocationDestination.Rack, typeof(Guid), "Rack"),
                (SlcAsset_Management.Sections.AssetLocationDestination.Desk, typeof(Guid), "Desk"),
                (SlcAsset_Management.Sections.AssetLocationDestination.Container, typeof(Guid), "Container"),
                (SlcAsset_Management.Sections.AssetLocationDestination.Room, typeof(Guid), "Room"),
                (SlcAsset_Management.Sections.AssetLocationDestination.PowerSupplyRackPosition, typeof(long), "Power Supply Rack Position"),
                (SlcAsset_Management.Sections.AssetLocationDestination.ParentAsset, typeof(Guid), "Parent Asset"),
                (SlcAsset_Management.Sections.AssetLocationDestination.HolderNumber, typeof(long), "Holder Number"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.AssetLocationDestination.Id, "Asset Location Destination", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_AssetNetworkDetails()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.AssetNetworkDetails.MACAddress, typeof(string), "MAC Address"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.AssetNetworkDetails.Id, "Asset Network Details", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_AssetOwnership()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.AssetOwnership.Organization, typeof(Guid), "Organization"),
                (SlcAsset_Management.Sections.AssetOwnership.ContactPerson, typeof(Guid), "Contact person"),
                (SlcAsset_Management.Sections.AssetOwnership.ContactPersonRole, typeof(Guid), "Contact person role"),
                (SlcAsset_Management.Sections.AssetOwnership.Team, typeof(Guid), "Team"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.AssetOwnership.Id, "Asset Ownership", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_Attachment()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.Attachment.Path, typeof(string), "Path"),
                (SlcAsset_Management.Sections.Attachment.AttachedAt, typeof(DateTime), "Attached At"),
                (SlcAsset_Management.Sections.Attachment.AttachedBy, typeof(string), "Attached By"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.Attachment.Id, "Attachment", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_CableCompatibility()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.CableCompatibility.CableTypes, typeof(List<Guid>), "Cable Types"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.CableCompatibility.Id, "Cable Compatibility", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_CableInformation()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.CableInformation.CableLength, typeof(double), "Cable Length"),
                (SlcAsset_Management.Sections.CableInformation.CableType, typeof(Guid), "Cable Type"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.CableInformation.Id, "Cable Information", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_CableTypeInformation()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.CableTypeInformation.Name, typeof(string), "Name"),
                (SlcAsset_Management.Sections.CableTypeInformation.Description, typeof(string), "Description"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.CableTypeInformation.Id, "Cable Type Information", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_CategoryLink()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.CategoryLink.Categories, typeof(List<string>), "Categories"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.CategoryLink.Id, "Category Link", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_ConnectionInfo()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.ConnectionInfo.Notes, typeof(string), "Notes"),
                (SlcAsset_Management.Sections.ConnectionInfo.Description, typeof(string), "Description"),
                (SlcAsset_Management.Sections.ConnectionInfo.ConnectionType, typeof(int), "Connection Type"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.ConnectionInfo.Id, "Connection Info", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_DataPortInfo()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.DataPortInfo.PortNumber, typeof(long), "Port Number"),
                (SlcAsset_Management.Sections.DataPortInfo.PortName, typeof(string), "Port Name"),
                (SlcAsset_Management.Sections.DataPortInfo.PortExposure, typeof(string), "Port Exposure"),
                (SlcAsset_Management.Sections.DataPortInfo.OutputType, typeof(int), "Output Type"),
                (SlcAsset_Management.Sections.DataPortInfo.PortType, typeof(Guid), "Port Type"),
                (SlcAsset_Management.Sections.DataPortInfo.Label, typeof(string), "Label"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.DataPortInfo.Id, "Data Port Info", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_DestinationInfo()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.DestinationInfo.CableDestinationTag, typeof(string), "Cable Destination Tag"),
                (SlcAsset_Management.Sections.DestinationInfo.DestinationPort, typeof(Guid), "Destination Port"),
                (SlcAsset_Management.Sections.DestinationInfo.DestinationPortType, typeof(Guid), "Destination Port Type"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.DestinationInfo.Id, "Destination Info", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_DeviceTypeInformation()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.DeviceTypeInformation.Name, typeof(string), "Name"),
                (SlcAsset_Management.Sections.DeviceTypeInformation.Description, typeof(string), "Description"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.DeviceTypeInformation.Id, "Device Type Information", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_ElementLink()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.ElementLink.ElementID, typeof(string), "Element ID"),
                (SlcAsset_Management.Sections.ElementLink.IsPrimary, typeof(bool), "Is Primary"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.ElementLink.Id, "Element Link", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_HierarchyInfo()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.HierarchyInfo.HierarchyRole, typeof(string), "Hierarchy Role"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.HierarchyInfo.Id, "Hierarchy Info", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_HistoryInfo()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.HistoryInfo.Description, typeof(string), "Description"),
                (SlcAsset_Management.Sections.HistoryInfo.Job, typeof(Guid), "Job"),
                (SlcAsset_Management.Sections.HistoryInfo.UserID, typeof(Guid), "User ID"),
                (SlcAsset_Management.Sections.HistoryInfo.InstanceDefinitionID, typeof(string), "Instance Definition ID"),
                (SlcAsset_Management.Sections.HistoryInfo.InstanceID, typeof(string), "Instance ID"),
                (SlcAsset_Management.Sections.HistoryInfo.ExtraInfo, typeof(string), "Extra Info"),
                (SlcAsset_Management.Sections.HistoryInfo.TypeOfHistory, typeof(string), "Type of History"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.HistoryInfo.Id, "History Info", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_Holders()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.Holders.SlotNumber, typeof(long), "Slot Number"),
                (SlcAsset_Management.Sections.Holders.HierarchyRole, typeof(string), "Hierarchy Role"),
                (SlcAsset_Management.Sections.Holders.Label, typeof(string), "Label"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.Holders.Id, "Holders", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_LinkedJob()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.LinkedJob.Job, typeof(Guid), "Job"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.LinkedJob.Id, "Linked Job", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_PortTypeInformation()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.PortTypeInformation.Name, typeof(string), "Name"),
                (SlcAsset_Management.Sections.PortTypeInformation.Description, typeof(string), "Description"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.PortTypeInformation.Id, "Port Type Information", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_PowerPortInfo()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.PowerPortInfo.PortNumber, typeof(long), "Port Number"),
                (SlcAsset_Management.Sections.PowerPortInfo.PortName, typeof(string), "Port Name"),
                (SlcAsset_Management.Sections.PowerPortInfo.PortExposure, typeof(string), "Port Exposure"),
                (SlcAsset_Management.Sections.PowerPortInfo.OutputType, typeof(int), "Output Type"),
                (SlcAsset_Management.Sections.PowerPortInfo.PortType, typeof(Guid), "Port Type"),
                (SlcAsset_Management.Sections.PowerPortInfo.Label, typeof(string), "Label"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.PowerPortInfo.Id, "Power Port Info", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_PrimaryPortRelation()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.PrimaryPortRelation.Isprimaryipv4, typeof(bool), "Is primary ipv4"),
                (SlcAsset_Management.Sections.PrimaryPortRelation.Isprimaryipv6, typeof(bool), "Is primary ipv6"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.PrimaryPortRelation.Id, "Primary Port Relation", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_ProtocolLink()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.ProtocolLink.Protocol, typeof(string), "Protocol"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.ProtocolLink.Id, "Protocol Link", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_ReservationInfo()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.ReservationInfo.Description, typeof(string), "Description"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.ReservationInfo.Id, "Reservation Info", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_ReservationRack()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.ReservationRack.Rack, typeof(Guid), "Rack"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.ReservationRack.Id, "Reservation Rack", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_ReservedPositions()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.ReservedPositions.ReservedPositionsLowerBound, typeof(long), "Reserved Positions Lower Bound"),
                (SlcAsset_Management.Sections.ReservedPositions.ReservedPositionsUpperBound, typeof(long), "Reserved Positions Upper Bound"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.ReservedPositions.Id, "Reserved Positions", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_SourceInfo()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.SourceInfo.CableSourceTag, typeof(string), "Cable Source Tag"),
                (SlcAsset_Management.Sections.SourceInfo.SourcePort, typeof(Guid), "Source Port"),
                (SlcAsset_Management.Sections.SourceInfo.SourcePortType, typeof(Guid), "Source Port Type"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.SourceInfo.Id, "Source Info", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_TagsInfo()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcAsset_Management.Sections.TagsInfo.Tags, typeof(List<int>), "Tags"),
            };

            return BuildSectionDefinition(SlcAsset_Management.Sections.TagsInfo.Id, "Tags Info", fields);
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
                { SlcAsset_Management.Definitions.Asset, GenerateDefinitions_Asset(behaviors) },
                { SlcAsset_Management.Definitions.AssetClass, GenerateDefinitions_AssetClass(behaviors) },
                { SlcAsset_Management.Definitions.AssetManagerAppSettings, GenerateDefinitions_AssetManagerAppSettings() },
                { SlcAsset_Management.Definitions.CableType, GenerateDefinitions_CableType() },
                { SlcAsset_Management.Definitions.Connections, GenerateDefinitions_Connections() },
                { SlcAsset_Management.Definitions.DataPort, GenerateDefinitions_DataPort() },
                { SlcAsset_Management.Definitions.DeviceType, GenerateDefinitions_DeviceType() },
                { SlcAsset_Management.Definitions.History, GenerateDefinitions_History() },
                { SlcAsset_Management.Definitions.PortType, GenerateDefinitions_PortType() },
                { SlcAsset_Management.Definitions.PowerPort, GenerateDefinitions_PowerPort() },
                { SlcAsset_Management.Definitions.Reservations, GenerateDefinitions_Reservations() },
            };
        }

        private static DomDefinition GenerateDefinitions_Asset(Dictionary<DomBehaviorDefinitionId, DomBehaviorDefinition> behaviors)
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcAsset_Management.Definitions.Asset.Id)
                .WithName("Asset");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcAsset_Management.Sections.AssetInformation.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.AssetLocation.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.AssetLocationDestination.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.AssetLifecycle.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.AssetOwnership.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.AssetCustody.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.Holders.Id) { AllowMultipleSections = true },
                new SectionDefinitionLink(SlcAsset_Management.Sections.ElementLink.Id) { AllowMultipleSections = true },
                new SectionDefinitionLink(SlcAsset_Management.Sections.AssetNetworkDetails.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.Attachment.Id) { AllowMultipleSections = true },
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            builder.WithDomBehaviorDefinition(behaviors[SlcAsset_Management.Behaviors.Asset_Behavior.Id]);

            var definition = builder.Build();

            definition.ModuleSettingsOverrides = new ModuleSettingsOverrides()
            {
                NameDefinition = new DomInstanceNameDefinition()
                {
                    ConcatenationItems = new List<IDomInstanceConcatenationItem>
                    {
                        new FieldValueConcatenationItem(SlcAsset_Management.Sections.AssetInformation.AssetName),
                    },
                },
            };

            return definition;
        }

        private static DomDefinition GenerateDefinitions_AssetClass(Dictionary<DomBehaviorDefinitionId, DomBehaviorDefinition> behaviors)
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcAsset_Management.Definitions.AssetClass.Id)
                .WithName("Asset Class");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcAsset_Management.Sections.AssetClassInfo.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.AssetClassLifecycleInformation.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.Holders.Id){ AllowMultipleSections = true },
                new SectionDefinitionLink(SlcAsset_Management.Sections.DataPortInfo.Id){ IsOptional = true, AllowMultipleSections = true },
                new SectionDefinitionLink(SlcAsset_Management.Sections.PowerPortInfo.Id){ IsOptional = true, AllowMultipleSections = true },
                new SectionDefinitionLink(SlcAsset_Management.Sections.ProtocolLink.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.Attachment.Id){ AllowMultipleSections = true },
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            builder.WithDomBehaviorDefinition(behaviors[SlcAsset_Management.Behaviors.Asset_Class_Behavior.Id]);

            var definition = builder.Build();

            definition.ModuleSettingsOverrides = new ModuleSettingsOverrides()
            {
                NameDefinition = new DomInstanceNameDefinition()
                {
                    ConcatenationItems = new List<IDomInstanceConcatenationItem>
                    {
                        new FieldValueConcatenationItem(SlcAsset_Management.Sections.AssetClassInfo.Name),
                    },
                },
            };

            return definition;
        }

        private static DomDefinition GenerateDefinitions_AssetManagerAppSettings()
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcAsset_Management.Definitions.AssetManagerAppSettings.Id)
                .WithName("Asset Manager App Settings");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcAsset_Management.Sections.AppSettings.Id),
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            return builder.Build();
        }

        private static DomDefinition GenerateDefinitions_CableType()
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcAsset_Management.Definitions.CableType.Id)
                .WithName("Cable Type");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcAsset_Management.Sections.CableTypeInformation.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.CategoryLink.Id),
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            var definition = builder.Build();

            definition.ModuleSettingsOverrides = new ModuleSettingsOverrides()
            {
                NameDefinition = new DomInstanceNameDefinition()
                {
                    ConcatenationItems = new List<IDomInstanceConcatenationItem>
                    {
                        new FieldValueConcatenationItem(SlcAsset_Management.Sections.CableTypeInformation.Name),
                    },
                },
            };

            return definition;
        }

        private static DomDefinition GenerateDefinitions_Connections()
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcAsset_Management.Definitions.Connections.Id)
                .WithName("Connections");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcAsset_Management.Sections.ConnectionInfo.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.CableInformation.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.SourceInfo.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.DestinationInfo.Id),
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            return builder.Build();
        }

        private static DomDefinition GenerateDefinitions_DataPort()
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcAsset_Management.Definitions.DataPort.Id)
                .WithName("Data Port");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcAsset_Management.Sections.DataPortInfo.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.AddressInfo.Id) { IsOptional = true },
                new SectionDefinitionLink(SlcAsset_Management.Sections.Asset.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.PrimaryPortRelation.Id) { IsOptional = true },
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            return builder.Build();
        }

        private static DomDefinition GenerateDefinitions_DeviceType()
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcAsset_Management.Definitions.DeviceType.Id)
                .WithName("Device Type");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcAsset_Management.Sections.DeviceTypeInformation.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.TagsInfo.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.HierarchyInfo.Id),
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            var definition = builder.Build();

            definition.ModuleSettingsOverrides = new ModuleSettingsOverrides()
            {
                NameDefinition = new DomInstanceNameDefinition()
                {
                    ConcatenationItems = new List<IDomInstanceConcatenationItem>
                    {
                        new FieldValueConcatenationItem(SlcAsset_Management.Sections.DeviceTypeInformation.Name),
                    },
                },
            };

            return definition;
        }

        private static DomDefinition GenerateDefinitions_History()
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcAsset_Management.Definitions.History.Id)
                .WithName("History");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcAsset_Management.Sections.HistoryInfo.Id),
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            return builder.Build();
        }

        private static DomDefinition GenerateDefinitions_PortType()
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcAsset_Management.Definitions.PortType.Id)
                .WithName("Port Type");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcAsset_Management.Sections.PortTypeInformation.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.CategoryLink.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.CableCompatibility.Id),
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            var definition = builder.Build();

            definition.ModuleSettingsOverrides = new ModuleSettingsOverrides()
            {
                NameDefinition = new DomInstanceNameDefinition()
                {
                    ConcatenationItems = new List<IDomInstanceConcatenationItem>
                    {
                        new FieldValueConcatenationItem(SlcAsset_Management.Sections.PortTypeInformation.Name),
                    },
                },
            };

            return definition;
        }

        private static DomDefinition GenerateDefinitions_PowerPort()
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcAsset_Management.Definitions.PowerPort.Id)
                .WithName("Power Port");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcAsset_Management.Sections.PowerPortInfo.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.Asset.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.PrimaryPortRelation.Id) { IsOptional = true },
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            return builder.Build();
        }

        private static DomDefinition GenerateDefinitions_Reservations()
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcAsset_Management.Definitions.Reservations.Id)
                .WithName("Reservations");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcAsset_Management.Sections.ReservationInfo.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.ReservationRack.Id),
                new SectionDefinitionLink(SlcAsset_Management.Sections.ReservedPositions.Id) { AllowMultipleSections = true },
                new SectionDefinitionLink(SlcAsset_Management.Sections.LinkedJob.Id) { IsOptional = true },
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            return builder.Build();
        }

        private static Dictionary<DomBehaviorDefinitionId, DomBehaviorDefinition> GenerateBehaviorDefinitions()
        {
            return new Dictionary<DomBehaviorDefinitionId, DomBehaviorDefinition>
            {
                { SlcAsset_Management.Behaviors.Asset_Behavior.Id, GenerateBehaviorDefinitions_AssetBehavior() },
                { SlcAsset_Management.Behaviors.Asset_Class_Behavior.Id, GenerateBehaviorDefinitions_AssetClassBehavior() },
            };
        }

        private static DomBehaviorDefinition GenerateBehaviorDefinitions_AssetBehavior()
        {
            List<DomStatus> status = new List<DomStatus>
            {
                new DomStatus(SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.NotAvailable, "Not Available"),
                new DomStatus(SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Available, "Available"),
                new DomStatus(SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.BuildPlanReady, "Build Plan Ready"),
                new DomStatus(SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Installed, "Installed"),
                new DomStatus(SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InService, "In Service"),
                new DomStatus(SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Disposed, "Disposed"),
                new DomStatus(SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning, "In Planning"),
                new DomStatus(SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit, "In Transit"),
                new DomStatus(SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair, "In Repair"),
            };

            List<DomStatusTransition> statusTransitions = new List<DomStatusTransition>
            {
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Notavailable_To_Available, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.NotAvailable, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Available),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Notavailable_To_Disposed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.NotAvailable, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Disposed),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Available_To_Notavailable, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Available, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.NotAvailable),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Buildplanready_To_Installed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.BuildPlanReady, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Installed),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Installed_To_Inservice, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Installed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InService),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inservice_To_Notavailable, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InService, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.NotAvailable),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inservice_To_Buildplanready, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InService, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.BuildPlanReady),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inservice_To_Available, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InService, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Available),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inservice_To_Installed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InService, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Installed),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inplanning_To_Available, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Available),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inplanning_To_Buildplanready, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.BuildPlanReady),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Buildplanready_To_Inplanning, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.BuildPlanReady, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inservice_To_Inplanning, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InService, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Available_To_Inplanning, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Available, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Installed_To_Inplanning, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Installed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Notavailable_To_Intransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.NotAvailable, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Available_To_Intransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Available, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Buildplanready_To_Intransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.BuildPlanReady, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Installed_To_Intransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Installed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inservice_To_Intransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InService, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inplanning_To_Intransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inrepair_To_Intransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Notavailable_To_Inrepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.NotAvailable, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Available_To_Inrepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Available, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Buildplanready_To_Inrepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.BuildPlanReady, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Installed_To_Inrepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Installed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inservice_To_Inrepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InService, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inplanning_To_Inrepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Intransit_To_Inrepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Intransit_To_Notavailable, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.NotAvailable),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Intransit_To_Available, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Available),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Intransit_To_Buildplanready, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.BuildPlanReady),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Intransit_To_Installed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Installed),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Intransit_To_Disposed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Disposed),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Intransit_To_Inplanning, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InTransit, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inrepair_To_Notavailable, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.NotAvailable),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inrepair_To_Available, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Available),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inrepair_To_Buildplanready, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.BuildPlanReady),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inrepair_To_Installed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Installed),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inrepair_To_Disposed, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.Disposed),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Behavior.Transitions.Inrepair_To_Inplanning, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InRepair, SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.InPlanning),
            };

            DomBehaviorDefinitionBuilder builder = new DomBehaviorDefinitionBuilder()
                .WithID(SlcAsset_Management.Behaviors.Asset_Behavior.Id)
                .WithName("Asset_Behavior")
                .WithInitialStatusId(SlcAsset_Management.Behaviors.Asset_Behavior.Statuses.NotAvailable)
                .WithStatuses(status)
                .WithStatusTransitions(statusTransitions);

            return builder.Build();
        }

        private static DomBehaviorDefinition GenerateBehaviorDefinitions_AssetClassBehavior()
        {
            List<DomStatus> status = new List<DomStatus>
            {
                new DomStatus(SlcAsset_Management.Behaviors.Asset_Class_Behavior.Statuses.Draft, "Draft"),
                new DomStatus(SlcAsset_Management.Behaviors.Asset_Class_Behavior.Statuses.Active, "Active"),
                new DomStatus(SlcAsset_Management.Behaviors.Asset_Class_Behavior.Statuses.Deprecated, "Deprecated"),
            };

            List<DomStatusTransition> statusTransitions = new List<DomStatusTransition>
            {
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Class_Behavior.Transitions.Draft_Active, SlcAsset_Management.Behaviors.Asset_Class_Behavior.Statuses.Draft, SlcAsset_Management.Behaviors.Asset_Class_Behavior.Statuses.Active),
                new DomStatusTransition(SlcAsset_Management.Behaviors.Asset_Class_Behavior.Transitions.Active_Deprecated, SlcAsset_Management.Behaviors.Asset_Class_Behavior.Statuses.Active, SlcAsset_Management.Behaviors.Asset_Class_Behavior.Statuses.Deprecated),
            };

            DomBehaviorDefinitionBuilder builder = new DomBehaviorDefinitionBuilder()
                .WithID(SlcAsset_Management.Behaviors.Asset_Class_Behavior.Id)
                .WithName("Asset_Class_Behavior")
                .WithInitialStatusId(SlcAsset_Management.Behaviors.Asset_Class_Behavior.Statuses.Draft)
                .WithStatuses(status)
                .WithStatusTransitions(statusTransitions);

            return builder.Build();
        }
    }
}
