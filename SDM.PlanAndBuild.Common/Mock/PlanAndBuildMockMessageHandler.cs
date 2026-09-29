namespace Skyline.DataMiner.SDM.PlanAndBuild.Common.Mock
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

    public static class PlanAndBuildMockMessageHandler
    {
        public static void AddPlanAndBuildModule(this DomSLNetMessageHandler messageHandler)
        {
            var sections = GenerateSectionsDefinitions();
            var behaviorDefinitions = GenerateBehaviorDefinitions();
            var definitions = GenerateDefinitions(behaviorDefinitions);

            messageHandler.SetSectionDefinitions(SlcPlan_And_Build.ModuleId, sections.Values);

            messageHandler.SetDefinitions(SlcPlan_And_Build.ModuleId, definitions.Values);

            messageHandler.SetBehaviorDefinitions(SlcPlan_And_Build.ModuleId, behaviorDefinitions.Values);
        }

        private static Dictionary<SectionDefinitionID, SectionDefinition> GenerateSectionsDefinitions()
        {
            return new Dictionary<SectionDefinitionID, SectionDefinition>
            {
                {SlcPlan_And_Build.Sections.AssetsUsed.Id, GenerateSectionsDefinitions_AssetsUsed() },
                {SlcPlan_And_Build.Sections.ConnectionsOnJob.Id, GenerateSectionsDefinitions_ConnectionsOnJob() },
                {SlcPlan_And_Build.Sections.JobAttachment.Id, GenerateSectionsDefinitions_JobAttachment() },
                {SlcPlan_And_Build.Sections.JobInformation.Id, GenerateSectionsDefinitions_JobInformation() },
                {SlcPlan_And_Build.Sections.JobOwnership.Id, GenerateSectionsDefinitions_JobOwnership() },
                {SlcPlan_And_Build.Sections.JobSettings.Id, GenerateSectionsDefinitions_JobSettings() },
                {SlcPlan_And_Build.Sections.JobTypeInfo.Id, GenerateSectionsDefinitions_JobTypeInfo() },
            };
        }

        private static SectionDefinition GenerateSectionsDefinitions_AssetsUsed()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcPlan_And_Build.Sections.AssetsUsed.AssetID, typeof(Guid), "Asset ID"),
                (SlcPlan_And_Build.Sections.AssetsUsed.Action, typeof(string), "Action"),
                (SlcPlan_And_Build.Sections.AssetsUsed.AssetName, typeof(string), "Asset Name"),
                (SlcPlan_And_Build.Sections.AssetsUsed.AssetClassName, typeof(string), "Asset Class Name"),
                (SlcPlan_And_Build.Sections.AssetsUsed.IPAddress, typeof(string), "IP Address"),
            };

            return BuildSectionDefinition(SlcPlan_And_Build.Sections.AssetsUsed.Id, "Assets Used", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_ConnectionsOnJob()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcPlan_And_Build.Sections.ConnectionsOnJob.Source, typeof(string), "Source"),
                (SlcPlan_And_Build.Sections.ConnectionsOnJob.Destination, typeof(string), "Destination"),
                (SlcPlan_And_Build.Sections.ConnectionsOnJob.Status, typeof(string), "Status"),
                (SlcPlan_And_Build.Sections.ConnectionsOnJob.Cabletype, typeof(string), "CableType"),
                (SlcPlan_And_Build.Sections.ConnectionsOnJob.CableLength, typeof(double), "Cable Length"),
                (SlcPlan_And_Build.Sections.ConnectionsOnJob.ConnectionID, typeof(Guid), "Connection ID"),
            };

            return BuildSectionDefinition(SlcPlan_And_Build.Sections.ConnectionsOnJob.Id, "Connections on Job", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_JobAttachment()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcPlan_And_Build.Sections.JobAttachment.FilePath, typeof(string), "File Path"),
                (SlcPlan_And_Build.Sections.JobAttachment.AttachedAt, typeof(DateTime), "Attached At"),
                (SlcPlan_And_Build.Sections.JobAttachment.AttachedBy, typeof(string), "Attached By"),
            };

            return BuildSectionDefinition(SlcPlan_And_Build.Sections.JobAttachment.Id, "Job Attachment", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_JobInformation()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcPlan_And_Build.Sections.JobInformation.JobID, typeof(string), "Job ID"),
                (SlcPlan_And_Build.Sections.JobInformation.JobName, typeof(string), "Job Name"),
                (SlcPlan_And_Build.Sections.JobInformation.Start, typeof(DateTime), "Start"),
                (SlcPlan_And_Build.Sections.JobInformation.JobType, typeof(string), "Job Type"),
                (SlcPlan_And_Build.Sections.JobInformation.End, typeof(DateTime), "End"),
                (SlcPlan_And_Build.Sections.JobInformation.JobDescription, typeof(string), "Job Description"),
                (SlcPlan_And_Build.Sections.JobInformation.Remarks, typeof(string), "Remarks"),
                (SlcPlan_And_Build.Sections.JobInformation.Priority, typeof(string), "Priority"),
                (SlcPlan_And_Build.Sections.JobInformation.SubState, typeof(string), "Sub State"),
                (SlcPlan_And_Build.Sections.JobInformation.Locations, typeof(List<Guid>), "Locations"),
                (SlcPlan_And_Build.Sections.JobInformation.Type, typeof(Guid), "Type"),
            };

            return BuildSectionDefinition(SlcPlan_And_Build.Sections.JobInformation.Id, "Job Information", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_JobOwnership()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcPlan_And_Build.Sections.JobOwnership.AssignedTo, typeof(Guid), "Assigned To"),
                (SlcPlan_And_Build.Sections.JobOwnership.AssignmentGroup, typeof(Guid), "Assignment Group"),
            };

            return BuildSectionDefinition(SlcPlan_And_Build.Sections.JobOwnership.Id, "Job Ownership", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_JobSettings()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcPlan_And_Build.Sections.JobSettings.JobIDPrefix, typeof(string), "Job ID prefix"),
                (SlcPlan_And_Build.Sections.JobSettings.JobIDNextSequence, typeof(long), "Job ID next sequence"),
                (SlcPlan_And_Build.Sections.JobSettings.JobIDIncrement, typeof(long), "Job ID increment"),
                (SlcPlan_And_Build.Sections.JobSettings.JobIDStartingSeed, typeof(long), "Job ID starting seed"),
                (SlcPlan_And_Build.Sections.JobSettings.JobIDMinimumDigits, typeof(long), "Job ID minimum digits"),
            };

            return BuildSectionDefinition(SlcPlan_And_Build.Sections.JobSettings.Id, "Job Settings", fields);
        }

        private static SectionDefinition GenerateSectionsDefinitions_JobTypeInfo()
        {
            List<(FieldDescriptorID id, Type type, string name)> fields = new List<(FieldDescriptorID id, Type type, string name)>
            {
                (SlcPlan_And_Build.Sections.JobTypeInfo.Name, typeof(string), "Name"),
                (SlcPlan_And_Build.Sections.JobTypeInfo.Description, typeof(string), "Description"),
                (SlcPlan_And_Build.Sections.JobTypeInfo.Icon, typeof(string), "Icon"),
            };

            return BuildSectionDefinition(SlcPlan_And_Build.Sections.JobTypeInfo.Id, "Job Type Info", fields);
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
                { SlcPlan_And_Build.Definitions.AppSettings, GenerateDefinitions_App_Settings() },
                { SlcPlan_And_Build.Definitions.Job, GenerateDefinitions_Job(behaviors) },
                { SlcPlan_And_Build.Definitions.JobType, GenerateDefinitions_JobType() },
            };
        }

        private static DomDefinition GenerateDefinitions_App_Settings()
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcPlan_And_Build.Definitions.AppSettings.Id)
                .WithName("App Settings");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcPlan_And_Build.Sections.JobSettings.Id),
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            return builder.Build();
        }

        private static DomDefinition GenerateDefinitions_Job(Dictionary<DomBehaviorDefinitionId, DomBehaviorDefinition> behaviors)
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcPlan_And_Build.Definitions.Job.Id)
                .WithName("Job");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcPlan_And_Build.Sections.JobInformation.Id),
                new SectionDefinitionLink(SlcPlan_And_Build.Sections.JobOwnership.Id),
                new SectionDefinitionLink(SlcPlan_And_Build.Sections.AssetsUsed.Id) { AllowMultipleSections = true },
                new SectionDefinitionLink(SlcPlan_And_Build.Sections.JobAttachment.Id) { AllowMultipleSections = true },
                new SectionDefinitionLink(SlcPlan_And_Build.Sections.ConnectionsOnJob.Id) { AllowMultipleSections = true },
            };

            foreach (var link in sectionLinks)
            {
                builder.AddSectionDefinitionLink(link);
            }

            builder.WithDomBehaviorDefinition(behaviors[SlcPlan_And_Build.Behaviors.Job_Behavior.Id]);

            var definition = builder.Build();

            definition.ModuleSettingsOverrides = new ModuleSettingsOverrides()
            {
                NameDefinition = new DomInstanceNameDefinition()
                {
                    ConcatenationItems = new List<IDomInstanceConcatenationItem>
                    {
                        new FieldValueConcatenationItem(SlcPlan_And_Build.Sections.JobInformation.JobName),
                    },
                },
            };

            return definition;
        }

        private static DomDefinition GenerateDefinitions_JobType()
        {
            var builder = new DomDefinitionBuilder()
                .WithID(SlcPlan_And_Build.Definitions.JobType.Id)
                .WithName("Job Type");

            List<SectionDefinitionLink> sectionLinks = new List<SectionDefinitionLink>
            {
                new SectionDefinitionLink(SlcPlan_And_Build.Sections.JobTypeInfo.Id),
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
                        new FieldValueConcatenationItem(SlcPlan_And_Build.Sections.JobTypeInfo.Name),
                    },
                },
            };

            return definition;
        }

        private static Dictionary<DomBehaviorDefinitionId, DomBehaviorDefinition> GenerateBehaviorDefinitions()
        {
            return new Dictionary<DomBehaviorDefinitionId, DomBehaviorDefinition>
            {
                { SlcPlan_And_Build.Behaviors.Job_Behavior.Id, GenerateBehaviorDefinitions_Job_Behavior() },
            };
        }

        private static DomBehaviorDefinition GenerateBehaviorDefinitions_Job_Behavior()
        {
            List<DomStatus> status = new List<DomStatus>
            {
                new DomStatus(SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.New, "New"),
                new DomStatus(SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Active, "Active"),
                new DomStatus(SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Assigned, "Assigned"),
                new DomStatus(SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Canceled, "Canceled"),
                new DomStatus(SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Review, "Review"),
                new DomStatus(SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Resolved, "Resolved"),
            };

            List<DomStatusTransition> statusTransitions = new List<DomStatusTransition>
            {
                new DomStatusTransition(SlcPlan_And_Build.Behaviors.Job_Behavior.Transitions.New_Assigned, SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.New, SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Assigned),
                new DomStatusTransition(SlcPlan_And_Build.Behaviors.Job_Behavior.Transitions.Assigned_Active, SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Assigned, SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Active),
                new DomStatusTransition(SlcPlan_And_Build.Behaviors.Job_Behavior.Transitions.Active_Review, SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Active, SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Review),
                new DomStatusTransition(SlcPlan_And_Build.Behaviors.Job_Behavior.Transitions.New_Canceled, SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.New, SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Canceled),
                new DomStatusTransition(SlcPlan_And_Build.Behaviors.Job_Behavior.Transitions.Assigned_Canceled, SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Assigned, SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Canceled),
                new DomStatusTransition(SlcPlan_And_Build.Behaviors.Job_Behavior.Transitions.Active_Canceled, SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Active, SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Canceled),
                new DomStatusTransition(SlcPlan_And_Build.Behaviors.Job_Behavior.Transitions.Review_Resolved, SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Review, SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.Resolved),
            };

            DomBehaviorDefinitionBuilder builder = new DomBehaviorDefinitionBuilder()
                .WithID(SlcPlan_And_Build.Behaviors.Job_Behavior.Id)
                .WithName("Job_Behavior")
                .WithInitialStatusId(SlcPlan_And_Build.Behaviors.Job_Behavior.Statuses.New)
                .WithStatuses(status)
                .WithStatusTransitions(statusTransitions);

            return builder.Build();
        }
    }
}
