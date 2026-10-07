namespace Skyline.DataMiner.SDM.InfraOpsProperties.Common.Mock
{
    using System;
    using System.Collections.Generic;
    using SharedMappers.DomIds;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel.Concatenation;
    using Skyline.DataMiner.Net.Apps.Sections.SectionDefinitions;
    using Skyline.DataMiner.Net.Sections;
    using Skyline.DataMiner.Utils.DOM.Builders;
    using Skyline.DataMiner.Utils.DOM.UnitTesting;

    public static class InfraOpsPropertiesMockMessageHandler
    {
        public static void AddInfraOpsPropertiesModule(this DomSLNetMessageHandler messageHandler)
        {
            var sections = GenerateSectionsDefinitions();
            var behaviorDefinitions = GenerateBehaviorDefinitions();
            var definitions = GenerateDefinitions();

            messageHandler.SetSectionDefinitions(InfraopsProperties.ModuleId, sections.Values);

            messageHandler.SetDefinitions(InfraopsProperties.ModuleId, definitions.Values);

            messageHandler.SetBehaviorDefinitions(InfraopsProperties.ModuleId, behaviorDefinitions.Values);
        }

        private static Dictionary<SectionDefinitionID, SectionDefinition> GenerateSectionsDefinitions()
        {
            return new Dictionary<SectionDefinitionID, SectionDefinition>
            {
                {InfraopsProperties.Sections.Discrete.Id, GenerateSectionsDefinitions_Discrete() },
                {InfraopsProperties.Sections.Layout.Id, GenerateSectionsDefinitions_Layout() },
                {InfraopsProperties.Sections.PropertyInfo.Id, GenerateSectionsDefinitions_PropertyInfo() },
                {InfraopsProperties.Sections.PropertyValue.Id, GenerateSectionsDefinitions_PropertyValue() },
                {InfraopsProperties.Sections.PropertyValueInfo.Id, GenerateSectionsDefinitions_PropertyValueInfo() },
            };
        }

        private static SectionDefinition GenerateSectionsDefinitions_Discrete()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (InfraopsProperties.Sections.Discrete.Option, typeof(string), "Option"),
            };

            return BuildSectionDefinition(InfraopsProperties.Sections.Discrete.Id, "Discrete", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_Layout()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (InfraopsProperties.Sections.Layout.SectionName, typeof(string), "Section Name"),
                (InfraopsProperties.Sections.Layout.Order, typeof(long), "Order"),
            };

            return BuildSectionDefinition(InfraopsProperties.Sections.Layout.Id, "Layout", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_PropertyInfo()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (InfraopsProperties.Sections.PropertyInfo.Name, typeof(string), "Name"),
                (InfraopsProperties.Sections.PropertyInfo.PropertyType, typeof(string), "Property Type"),
                (InfraopsProperties.Sections.PropertyInfo.Scope, typeof(string), "Scope"),
                (InfraopsProperties.Sections.PropertyInfo.Default, typeof(string), "Default"),
                (InfraopsProperties.Sections.PropertyInfo.StringSizeLimit, typeof(long), "String Size Limit"),
                (InfraopsProperties.Sections.PropertyInfo.IsMultiLineString, typeof(bool), "Is Multi Line String"),
            };

            return BuildSectionDefinition(InfraopsProperties.Sections.PropertyInfo.Id, "Property Info", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_PropertyValue()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (InfraopsProperties.Sections.PropertyValue.PropertyName, typeof(string), "Property Name"),
                (InfraopsProperties.Sections.PropertyValue.Value, typeof(string), "Value"),
                (InfraopsProperties.Sections.PropertyValue.PropertyID, typeof(Guid), "Property ID"),
            };

            return BuildSectionDefinition(InfraopsProperties.Sections.PropertyValue.Id, "Property Value", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_PropertyValueInfo()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (InfraopsProperties.Sections.PropertyValueInfo.LinkedObjectID, typeof(string), "Linked Object ID"),
                (InfraopsProperties.Sections.PropertyValueInfo.Scope, typeof(string), "Scope"),
                (InfraopsProperties.Sections.PropertyValueInfo.SubID, typeof(string), "Sub ID"),
            };

            return BuildSectionDefinition(InfraopsProperties.Sections.PropertyValueInfo.Id, "Property Value Info", fields);
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

        private static Dictionary<DomDefinitionId, DomDefinition> GenerateDefinitions()
        {
            return new Dictionary<DomDefinitionId, DomDefinition>
            {
                { InfraopsProperties.Definitions.Property, GenerateDefinitions_Property() },
                { InfraopsProperties.Definitions.PropertyValues, GenerateDefinitions_PropertyValues() },
            };
        }

        private static DomDefinition GenerateDefinitions_Property()
        {
            var builder = new DomDefinitionBuilder()
                .WithID(InfraopsProperties.Definitions.Property.Id)
                .WithName("Property");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(InfraopsProperties.Sections.PropertyInfo.Id),
                new SectionDefinitionLink(InfraopsProperties.Sections.Discrete.Id) { IsOptional = true, AllowMultipleSections = true },
                new SectionDefinitionLink(InfraopsProperties.Sections.Layout.Id) { IsOptional = true },
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
                        new FieldValueConcatenationItem(InfraopsProperties.Sections.PropertyInfo.Name),
                    },
                },
            };

            return definition;
        }

        private static DomDefinition GenerateDefinitions_PropertyValues()
        {
            var builder = new DomDefinitionBuilder()
                .WithID(InfraopsProperties.Definitions.PropertyValues.Id)
                .WithName("Property Values");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(InfraopsProperties.Sections.PropertyValueInfo.Id),
                new SectionDefinitionLink(InfraopsProperties.Sections.PropertyValue.Id) { IsOptional = true, AllowMultipleSections = true },
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
            };
        }
    }
}