namespace SDM.AssetManagement.Tests.Assets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FluentAssertions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SDM.AssetManagement.Tests.Setup;
    using SharedMappers.DomIds;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Deletion;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.InfraOps.Orchestration;
    using Skyline.DataMiner.SDM.InfraOps.Orchestration.AssetDeletion;
    using Skyline.DataMiner.SDM.InfraOpsProperties.Models;
    using Skyline.DataMiner.SDM.PlanAndBuild.Deletion;
    using Skyline.DataMiner.SDM.PlanAndBuild.Models;

    [TestClass]
    public class AssetDeletionCascadeTests : BaseRepositoryTest
    {
        [TestMethod]
        public void Delete_ShouldRemoveOnlyExactAssetPropertyValuesAcrossAllSubIds()
        {
            Helper.PopulateWithDemoData(DemoDataLayer.Assets);
            var asset = Helper.TestData.Assets.First();
            var linkedObjectId = Guid.Parse(asset.Identifier);

            var assetProperty = Helper.InfraOpsProperties.Properties.Create(new Property
            {
                Name = "Deletion cascade asset property",
                Scope = "Asset",
            });
            var facilityProperty = Helper.InfraOpsProperties.Properties.Create(new Property
            {
                Name = "Deletion cascade facility property",
                Scope = "Facility",
            });

            var ownedValues = new[]
            {
                CreatePropertyValues(linkedObjectId, "Asset", null, assetProperty),
                CreatePropertyValues(linkedObjectId, "Asset", "Port1", assetProperty),
                CreatePropertyValues(linkedObjectId, "Asset", "Port2", assetProperty),
            };

            Helper.InfraOpsProperties.PropertyValues.Create(ownedValues);



            var differentScope = Helper.InfraOpsProperties.PropertyValues.Create(
                CreatePropertyValues(linkedObjectId, "Facility", null, facilityProperty));
            var differentLinkedId = Helper.InfraOpsProperties.PropertyValues.Create(
                CreatePropertyValues(Guid.NewGuid(), "Asset", null, assetProperty));
            var unrelatedAsset = Helper.InfraOpsProperties.PropertyValues.Create(
                CreatePropertyValues(Guid.NewGuid(), "Asset", "Port1", assetProperty));

            asset.State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable;
            Helper.AssetManagement.Assets.Delete(asset);

            var remainingValues = Helper.InfraOpsProperties.PropertyValues
                .Read(new TRUEFilterElement<PropertyValues>())
                .ToList();

            ownedValues.Should().NotContain(value => remainingValues.Any(item => item.Identifier == value.Identifier));
            remainingValues.Select(value => value.Identifier)
                .Should().Contain(new[] { differentScope.Identifier, differentLinkedId.Identifier, unrelatedAsset.Identifier });
            Helper.InfraOpsProperties.Properties
                .Read(new TRUEFilterElement<Property>())
                .Select(property => property.Identifier)
                .Should().Contain(new[] { assetProperty.Identifier, facilityProperty.Identifier });
        }

        [TestMethod]
        public void Delete_ShouldRemoveAssetFromItsPlanAndBuildJob()
        {
            Helper.PopulateWithDemoData(DemoDataLayer.Assets);
            var asset = Helper.TestData.Assets.First();
            Helper.PlanAndBuild.AppSettings.Create(new PlanAndBuildAppSettings
            {
                JobIDPrefix = "DEL",
                JobIDNextSequence = 1,
                JobIDIncrement = 1,
                JobIDMinimumDigits = 4,
            });
            var jobType = Helper.PlanAndBuild.JobTypes.Create(new JobType
            {
                Name = $"Asset deletion job type {Guid.NewGuid():N}",
            });
            var job = Helper.PlanAndBuild.Jobs.Create(new PlanAndBuildJob
            {
                JobName = $"Asset deletion job {Guid.NewGuid():N}",
                Type = new SdmObjectReference<JobType>(jobType.Identifier),
                AssetsUsed = new List<JobAsset>
                {
                    new JobAsset { AssetId = new SdmObjectReference<Asset>(asset.Identifier) },
                },
            });

            asset.State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable;
            Helper.AssetManagement.Assets.Delete(asset);

            Helper.PlanAndBuild.Jobs.Read(new TRUEFilterElement<PlanAndBuildJob>())
                .Single(item => item.Identifier == job.Identifier)
                .AssetsUsed.Should().BeEmpty();
        }

        [TestMethod]
        public void AssetManagementFactory_ShouldInstallCompleteDeletionComposition()
        {
            Helper.PopulateWithDemoData(DemoDataLayer.Assets);
            var asset = Helper.TestData.Assets.First();
            asset.State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable;
            var factoryHelper = InfraOpsApiHelperFactory.CreateAssetManagementApiHelper(
                ((TestApiHelper)Helper).Connection);

            factoryHelper.Assets.Delete(asset);

            Helper.AssetManagement.Assets.Read(new TRUEFilterElement<Asset>())
                .Should().NotContain(item => item.Identifier == asset.Identifier);
        }

        [TestMethod]
        public void InfraOpsComposition_ShouldExposeAllDomainHelpers()
        {
            var composition = InfraOpsApiComposition.Create(
                ((TestApiHelper)Helper).Connection,
                PeopleApiMock.CreateDefault());

            composition.AssetManagement.Should().NotBeNull();
            composition.FacilityManagement.Should().NotBeNull();
            composition.PlanAndBuild.Should().NotBeNull();
            composition.InfraOpsProperties.Should().NotBeNull();
        }

        [TestMethod]
        public void Delete_ShouldRemoveOwnedPowerPorts()
        {
            Helper.PopulateWithDemoData(DemoDataLayer.PowerPorts);
            var port = Helper.TestData.PowerPorts.First();
            var asset = Helper.AssetManagement.Assets
                .Read(new TRUEFilterElement<Asset>())
                .Single(item => item.Identifier == port.Asset.Identifier);
            asset.State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable;

            Helper.AssetManagement.Assets.Delete(asset);

            Helper.AssetManagement.PowerPorts
                .Read(new TRUEFilterElement<PowerPort>())
                .Should().NotContain(item => item.Asset.Identifier == asset.Identifier);
        }

        [TestMethod]
        public void Delete_ShouldDetachDirectChildrenWithoutDeletingThem()
        {
            Helper.PopulateWithDemoData(DemoDataLayer.Assets);
            var assets = Helper.TestData.Assets.Take(3).ToList();
            var parent = assets[0];
            var locationChild = assets[1];
            var destinationChild = assets[2];
            locationChild.Location.ParentAsset = new SdmObjectReference<Asset>(parent.Identifier);
            locationChild.Location.HolderNumber = 3;
            destinationChild.DestinationLocation.ParentAsset = new SdmObjectReference<Asset>(parent.Identifier);
            destinationChild.DestinationLocation.HolderNumber = 4;
            var rawAssets = new AssetDomRepository(((TestApiHelper)Helper).Connection);
            rawAssets.Update(new[] { locationChild, destinationChild });

            parent.State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable;
            Helper.AssetManagement.Assets.Delete(parent);

            var remainingAssets = Helper.AssetManagement.Assets.Read(new TRUEFilterElement<Asset>()).ToList();
            remainingAssets.Should().Contain(item => item.Identifier == locationChild.Identifier);
            remainingAssets.Should().Contain(item => item.Identifier == destinationChild.Identifier);
            remainingAssets.Single(item => item.Identifier == locationChild.Identifier)
                .Location.ParentAsset.HasValue().Should().BeFalse();
            remainingAssets.Single(item => item.Identifier == locationChild.Identifier)
                .Location.HolderNumber.Should().BeNull();
            remainingAssets.Single(item => item.Identifier == destinationChild.Identifier)
                .DestinationLocation.ParentAsset.HasValue().Should().BeFalse();
            remainingAssets.Single(item => item.Identifier == destinationChild.Identifier)
                .DestinationLocation.HolderNumber.Should().BeNull();
        }

        [TestMethod]
        public void Delete_WhenDomDeleteFails_ShouldLeaveDependenciesUntouched()
        {
            Helper.PopulateWithDemoData(DemoDataLayer.Assets);
            var asset = Helper.TestData.Assets.First();
            var property = Helper.InfraOpsProperties.Properties.Create(new Property
            {
                Name = "DOM failure property",
                Scope = "Asset",
            });
            var propertyValue = Helper.InfraOpsProperties.PropertyValues.Create(
                CreatePropertyValues(Guid.Parse(asset.Identifier), "Asset", null, property));
            asset.State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable;
            var cascade = CreateCascade();

            Action delete = () => cascade.OnDelete(
                asset,
                _ => throw new InvalidOperationException("Simulated Asset DOM failure."));

            delete.Should().Throw<AssetDeletionCascadeException>()
                .Which.Outcomes.Should().ContainSingle(outcome =>
                    outcome.AssetIdentifier == asset.Identifier &&
                    !outcome.DomDeletionSucceeded &&
                    outcome.FailedStage == "Asset DOM delete");
            Helper.AssetManagement.Assets.Read(new TRUEFilterElement<Asset>())
                .Should().ContainSingle(item => item.Identifier == asset.Identifier);
            Helper.InfraOpsProperties.PropertyValues.Read(new TRUEFilterElement<PropertyValues>())
                .Should().ContainSingle(item => item.Identifier == propertyValue.Identifier);
        }

        [TestMethod]
        public void Delete_WhenAssetWasAlreadyMissing_ShouldNotCleanDependencies()
        {
            var identifier = Guid.NewGuid();
            var property = Helper.InfraOpsProperties.Properties.Create(new Property
            {
                Name = "Missing Asset property",
                Scope = "Asset",
            });
            var propertyValue = Helper.InfraOpsProperties.PropertyValues.Create(
                CreatePropertyValues(identifier, "Asset", null, property));

            CreateCascade().OnDelete(new Asset { Identifier = identifier.ToString() }, _ => { });

            Helper.InfraOpsProperties.PropertyValues.Read(new TRUEFilterElement<PropertyValues>())
                .Should().ContainSingle(item => item.Identifier == propertyValue.Identifier);
        }

        [DataTestMethod]
        [DataRow(JobEntryRemovalMode.Remove)]
        [DataRow(JobEntryRemovalMode.KeepRemovedSnapshot)]
        public void Delete_MixedRawBatchOutcome_ShouldCleanOnlyAssetsDeletedByDom(JobEntryRemovalMode assetMode)
        {
            Helper.PopulateWithDemoData(DemoDataLayer.Assets);
            var assets = Helper.TestData.Assets.Take(2).ToList();
            var property = Helper.InfraOpsProperties.Properties.Create(new Property
            {
                Name = "Partial batch property",
                Scope = "Asset",
            });
            var values = assets.Select(asset => Helper.InfraOpsProperties.PropertyValues.Create(
                CreatePropertyValues(Guid.Parse(asset.Identifier), "Asset", null, property))).ToList();
            var rawAssets = new AssetDomRepository(((TestApiHelper)Helper).Connection);
            var cascade = CreateCascade(policy: new AssetDeletionPolicy(assetMode));

            Action delete = () => cascade.OnDelete(
                assets,
                batch =>
                {
                    rawAssets.Delete(batch.Take(1));
                    throw new InvalidOperationException("Simulated partial DOM batch failure.");
                });

            var exception = delete.Should().Throw<AssetDeletionCascadeException>().Which;
            exception.Outcomes.Should().ContainSingle(outcome =>
                outcome.AssetIdentifier == assets[0].Identifier &&
                outcome.DomDeletionSucceeded &&
                outcome.FailedStage == null);
            exception.Outcomes.Should().ContainSingle(outcome =>
                outcome.AssetIdentifier == assets[1].Identifier &&
                !outcome.DomDeletionSucceeded &&
                outcome.FailedStage == "Asset DOM delete");

            var remainingAssets = Helper.AssetManagement.Assets.Read(new TRUEFilterElement<Asset>()).ToList();
            remainingAssets.Should().NotContain(item => item.Identifier == assets[0].Identifier);
            remainingAssets.Should().Contain(item => item.Identifier == assets[1].Identifier);
            var remainingValues = Helper.InfraOpsProperties.PropertyValues
                .Read(new TRUEFilterElement<PropertyValues>())
                .Select(item => item.Identifier)
                .ToList();
            remainingValues.Should().NotContain(values[0].Identifier);
            remainingValues.Should().Contain(values[1].Identifier);
        }

        [TestMethod]
        public void RecoverAssetDeletion_WhenJobCleanupFails_ShouldFinishAndBeIdempotent()
        {
            Helper.PopulateWithDemoData(DemoDataLayer.Assets);
            var asset = Helper.TestData.Assets.First();
            Helper.PlanAndBuild.AppSettings.Create(new PlanAndBuildAppSettings
            {
                JobIDPrefix = "REC",
                JobIDNextSequence = 1,
                JobIDIncrement = 1,
                JobIDMinimumDigits = 4,
            });
            var jobType = Helper.PlanAndBuild.JobTypes.Create(new JobType
            {
                Name = $"Asset recovery job type {Guid.NewGuid():N}",
            });
            var job = Helper.PlanAndBuild.Jobs.Create(new PlanAndBuildJob
            {
                JobName = $"Asset recovery job {Guid.NewGuid():N}",
                Type = new SdmObjectReference<JobType>(jobType.Identifier),
                AssetsUsed = new List<JobAsset>
                {
                    new JobAsset { AssetId = new SdmObjectReference<Asset>(asset.Identifier) },
                },
            });
            asset.State = SlcAsset_Management.Behaviors.Asset_Behavior.StatusesEnum.NotAvailable;
            var actualCleanup = (IAssetDeletionJobCleanup)Helper.PlanAndBuild;
            var jobCleanup = new FailOnceJobCleanup(actualCleanup);
            var cascade = CreateCascade(jobCleanup);
            var rawAssets = new AssetDomRepository(((TestApiHelper)Helper).Connection);

            Action delete = () => cascade.OnDelete(asset, item => rawAssets.Delete(item));

            delete.Should().Throw<AssetDeletionCascadeException>()
                .Which.Outcomes.Should().ContainSingle(outcome =>
                    outcome.AssetIdentifier == asset.Identifier &&
                    outcome.DomDeletionSucceeded &&
                    outcome.FailedStage == "PlanAndBuild Jobs");
            Helper.PlanAndBuild.Jobs.Read(new TRUEFilterElement<PlanAndBuildJob>())
                .Single(item => item.Identifier == job.Identifier)
                .AssetsUsed.Should().ContainSingle();

            cascade.RecoverAssetDeletion(asset.Identifier);
            cascade.RecoverAssetDeletion(asset.Identifier);

            Helper.PlanAndBuild.Jobs.Read(new TRUEFilterElement<PlanAndBuildJob>())
                .Single(item => item.Identifier == job.Identifier)
                .AssetsUsed.Should().BeEmpty();
        }

        [DataTestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void RecoverAssetDeletion_CustomPolicy_ShouldRetainMetadataAndRejectDefaultRecovery(bool bulk)
        {
            Helper.PopulateWithDemoData(DemoDataLayer.Assets);
            var asset = Helper.TestData.Assets.First();
            var className = Helper.AssetManagement.AssetClasses
                .Read(AssetClassExposers.Identifier.Equal(asset.AssetClassId.Identifier)).Single().Name;
            Helper.PlanAndBuild.AppSettings.Create(new PlanAndBuildAppSettings
            {
                JobIDPrefix = "REC", JobIDNextSequence = 1, JobIDIncrement = 1, JobIDMinimumDigits = 4,
            });
            var jobType = Helper.PlanAndBuild.JobTypes.Create(new JobType { Name = "Custom recovery type" });
            var job = Helper.PlanAndBuild.Jobs.Create(new PlanAndBuildJob
            {
                JobName = "Custom recovery job",
                Type = new SdmObjectReference<JobType>(jobType.Identifier),
                AssetsUsed = new List<JobAsset> { new JobAsset { AssetId = new SdmObjectReference<Asset>(asset.Identifier) } },
            });
            var cascade = CreateCascade(
                new FailOnceJobCleanup((IAssetDeletionJobCleanup)Helper.PlanAndBuild),
                new AssetDeletionPolicy(JobEntryRemovalMode.KeepRemovedSnapshot));
            var rawAssets = new AssetDomRepository(((TestApiHelper)Helper).Connection);
            Action delete = () =>
            {
                if (bulk)
                {
                    cascade.OnDelete(new[] { asset }, items => rawAssets.Delete(items));
                }
                else
                {
                    cascade.OnDelete(asset, item => rawAssets.Delete(item));
                }
            };
            var outcome = delete.Should().Throw<AssetDeletionCascadeException>().Which.Outcomes.Single();
            var context = outcome.RecoveryContext;
            context.Should().NotBeNull();
            context.AssetName.Should().Be(asset.Name);
            context.AssetClassName.Should().Be(className);
            Action identifierRecovery = () => cascade.RecoverAssetDeletion(asset.Identifier);
            identifierRecovery.Should().Throw<InvalidOperationException>();
            Action mismatchedRecovery = () => CreateCascade().RecoverAssetDeletion(context);
            mismatchedRecovery.Should().Throw<InvalidOperationException>();

            var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(context);
            var restored = Newtonsoft.Json.JsonConvert.DeserializeObject<AssetDeletionRecoveryContext>(serialized)!;
            cascade.RecoverAssetDeletion(restored);
            cascade.RecoverAssetDeletion(restored);

            var entry = Helper.PlanAndBuild.Jobs.Read(new TRUEFilterElement<PlanAndBuildJob>())
                .Single(item => item.Identifier == job.Identifier).AssetsUsed.Should().ContainSingle().Which;
            entry.Action.Should().Be(SlcPlan_And_Build.Enums.ActionforassetenumEnum.Removed);
            entry.AssetName.Should().Be(asset.Name);
            entry.AssetClassName.Should().Be(className);
        }

        [TestMethod]
        public void AssetDeletionPolicy_ShouldRejectInvalidEnumValues()
        {
            Action invalidAsset = () => new AssetDeletionPolicy((JobEntryRemovalMode)99);
            Action invalidConnection = () => new AssetDeletionPolicy(connectionJobEntryMode: (JobEntryRemovalMode)99);
            Action invalidFormat = () => new AssetDeletionPolicy(connectionSnapshotFormat: (ConnectionSnapshotFormat)99);
            invalidAsset.Should().Throw<ArgumentOutOfRangeException>();
            invalidConnection.Should().Throw<ArgumentOutOfRangeException>();
            invalidFormat.Should().Throw<ArgumentOutOfRangeException>();
        }

        private AssetDeletionMiddleware CreateCascade(IAssetDeletionJobCleanup jobCleanup = null, AssetDeletionPolicy policy = null)
        {
            var cascade = new AssetDeletionMiddleware(policy ?? AssetDeletionPolicy.Default);
            cascade.Configure(
                Helper.AssetManagement,
                jobCleanup ?? (IAssetDeletionJobCleanup)Helper.PlanAndBuild,
                Helper.InfraOpsProperties);
            return cascade;
        }

        private static PropertyValues CreatePropertyValues(
            Guid linkedObjectId,
            string scope,
            string? subId,
            Property property)
        {
            return new PropertyValues
            {
                LinkedObjectID = linkedObjectId,
                Scope = scope,
                SubID = subId,
                Values = new List<PropertyValue>
                {
                    new PropertyValue
                    {
                        PropertyName = property.Name,
                        PropertyId = new SdmObjectReference<Property>(property.Identifier),
                        Value = "test",
                    },
                },
            };
        }

        private sealed class FailOnceJobCleanup : IAssetDeletionJobCleanup
        {
            private readonly IAssetDeletionJobCleanup _inner;
            private bool _failed;

            public FailOnceJobCleanup(IAssetDeletionJobCleanup inner)
            {
                _inner = inner;
            }

            public void RemoveDeletedAssetReferences(
                Skyline.DataMiner.SDM.AssetManagement.Deletion.AssetDeletionRecoveryContext context)
            {
                if (!_failed)
                {
                    _failed = true;
                    throw new InvalidOperationException("Simulated Plan & Build cleanup failure.");
                }

                _inner.RemoveDeletedAssetReferences(context);
            }
        }
    }
}
