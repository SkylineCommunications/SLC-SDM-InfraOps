namespace SDM.AssetManagement.Tests.ChangeTracking
{
    using FluentAssertions;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    [TestClass]
    public class SdmChangeTrackingTests
    {
        private static Asset CreateLoadedAsset(string? name = "Original")
        {
            var asset = new Asset { Name = name };
            asset.ResetChangeTracking();
            return asset;
        }

        [TestMethod]
        public void GetFieldChanges_NoChanges_ReturnsEmpty()
        {
            var asset = CreateLoadedAsset();

            SdmChangeTracking.GetFieldChanges(asset).Should().BeEmpty();
        }

        [TestMethod]
        public void GetFieldChanges_SingleFieldChange_ReturnsOldAndNewValue()
        {
            var asset = CreateLoadedAsset();
            asset.Name = "Changed";

            var changes = SdmChangeTracking.GetFieldChanges(asset);

            changes.Should().ContainSingle();
            changes[0].Section.Should().BeNull();
            changes[0].FieldName.Should().Be(nameof(Asset.Name));
            changes[0].OldValue.Should().Be("Original");
            changes[0].NewValue.Should().Be("Changed");
        }

        [TestMethod]
        public void GetFieldChanges_SectionFieldChange_IsAttributedToSection()
        {
            var asset = CreateLoadedAsset();
            asset.Location.RackPosition = 7;

            var changes = SdmChangeTracking.GetFieldChanges(asset);

            changes.Should().ContainSingle();
            changes[0].Section.Should().Be(nameof(Asset.Location));
            changes[0].FieldName.Should().Be("RackPosition");
        }

        [TestMethod]
        public void GetFieldChanges_NullToValue_IsReported()
        {
            var asset = new Asset();
            asset.ResetChangeTracking();
            asset.Name = "Value";

            var changes = SdmChangeTracking.GetFieldChanges(asset);

            changes.Should().ContainSingle(c => c.FieldName == nameof(Asset.Name) && (string)c.NewValue == "Value");
        }

        [TestMethod]
        public void GetFieldChanges_ValueToNull_IsReported()
        {
            var asset = CreateLoadedAsset();
            asset.Name = null;

            var changes = SdmChangeTracking.GetFieldChanges(asset);

            changes.Should().ContainSingle(c => c.FieldName == nameof(Asset.Name) && (string)c.OldValue == "Original" && c.NewValue == null);
        }

        [TestMethod]
        public void GetFieldChanges_ValueSetBackToOriginal_ReturnsEmpty()
        {
            var asset = CreateLoadedAsset();
            asset.Name = "Changed";
            asset.Name = "Original";

            SdmChangeTracking.GetFieldChanges(asset).Should().BeEmpty();
        }

        [TestMethod]
        public void GetFieldChanges_ListChange_IsReported()
        {
            var asset = CreateLoadedAsset();
            asset.Holders = new List<AssetHolder> { new AssetHolder() };

            SdmChangeTracking.GetFieldChanges(asset).Should().ContainSingle(c => c.FieldName == nameof(Asset.Holders));
        }

        [TestMethod]
        public void ApplyFieldChanges_CapturedChanges_AreAppliedToFreshObject()
        {
            var source = CreateLoadedAsset();
            source.Name = "Changed";
            source.Location.RackPosition = 7;
            var changes = SdmChangeTracking.GetFieldChanges(source);

            var fresh = CreateLoadedAsset();
            var result = SdmChangeTracking.ApplyFieldChanges(fresh, changes);

            result.Skipped.Should().BeEmpty();
            result.Applied.Should().HaveCount(2);
            fresh.Name.Should().Be("Changed");
            fresh.Location.RackPosition.Should().Be(7);
            SdmChangeTracking.GetFieldChanges(fresh).Should().HaveCount(2);
        }

        [TestMethod]
        public void ApplyFieldChanges_ValueToNull_SetsNull()
        {
            var source = CreateLoadedAsset();
            source.Name = null;

            var fresh = CreateLoadedAsset();
            SdmChangeTracking.ApplyFieldChanges(fresh, SdmChangeTracking.GetFieldChanges(source));

            fresh.Name.Should().BeNull();
        }

        [TestMethod]
        public void ApplyFieldChanges_FieldMissingOnTarget_IsSkipped()
        {
            var fresh = CreateLoadedAsset();

            var result = SdmChangeTracking.ApplyFieldChanges(fresh, new[] { new FieldChange(null, "DoesNotExist", null, "x") });

            result.Applied.Should().BeEmpty();
            result.Skipped.Should().ContainSingle().Which.Reason.Should().Contain("not found");
        }

        [TestMethod]
        public void ApplyFieldChanges_SectionMissingOnTarget_IsSkipped()
        {
            var fresh = CreateLoadedAsset();

            var result = SdmChangeTracking.ApplyFieldChanges(fresh, new[] { new FieldChange("NoSuchSection", "Name", null, "x") });

            result.Skipped.Should().ContainSingle().Which.Reason.Should().Contain("Section");
        }

        [TestMethod]
        public void ApplyFieldChanges_ReadOnlyOrIdentityField_IsSkipped()
        {
            var fresh = CreateLoadedAsset();

            var result = SdmChangeTracking.ApplyFieldChanges(fresh, new[]
            {
                new FieldChange(null, "Id", null, Guid.NewGuid()),
                new FieldChange(null, "Changed", false, true),
            });

            result.Applied.Should().BeEmpty();
            result.Skipped.Should().HaveCount(2);
        }

        [TestMethod]
        public void ApplyFieldChanges_IncompatibleValueType_IsSkipped()
        {
            var fresh = CreateLoadedAsset();

            var result = SdmChangeTracking.ApplyFieldChanges(fresh, new[] { new FieldChange(null, nameof(Asset.Name), "a", 42) });

            result.Skipped.Should().ContainSingle().Which.Reason.Should().Contain("not assignable");
        }

        [TestMethod]
        public void RevertFieldChanges_RestoresOriginalValuesAndLeavesNoChanges()
        {
            var asset = CreateLoadedAsset();
            asset.Name = "Changed";
            asset.Location.RackPosition = 7;
            asset.Holders = new List<AssetHolder> { new AssetHolder() };

            var reverted = SdmChangeTracking.RevertFieldChanges(asset);

            reverted.Should().HaveCount(3);
            asset.Name.Should().Be("Original");
            asset.Location.RackPosition.Should().BeNull();
            asset.Holders.Should().BeEmpty();
            SdmChangeTracking.GetFieldChanges(asset).Should().BeEmpty();
        }

        [TestMethod]
        public void TeamOnOwnershipAndCustody_AreTrackedAppliedAndRevertedIndependently()
        {
            var ownerTeam = new PnoObjectReference<Team>(Guid.NewGuid());
            var custodyTeam = new PnoObjectReference<Team>(Guid.NewGuid());
            var source = CreateLoadedAsset();
            source.Ownership.Team = ownerTeam;
            source.Custody.Team = custodyTeam;

            var changes = SdmChangeTracking.GetFieldChanges(source);

            changes.Should().HaveCount(2);
            changes.Single(c => c.Section == nameof(Asset.Ownership)).NewValue.Should().Be(ownerTeam.Identifier);
            changes.Single(c => c.Section == nameof(Asset.Custody)).NewValue.Should().Be(custodyTeam.Identifier);

            var target = CreateLoadedAsset();
            var result = SdmChangeTracking.ApplyFieldChanges(target, changes);

            result.Skipped.Should().BeEmpty();
            target.Ownership.Team.Should().Be(ownerTeam);
            target.Custody.Team.Should().Be(custodyTeam);

            SdmChangeTracking.RevertFieldChanges(target);

            target.Ownership.Team.Should().Be(default(PnoObjectReference<Team>));
            target.Custody.Team.Should().Be(default(PnoObjectReference<Team>));
            SdmChangeTracking.GetFieldChanges(target).Should().BeEmpty();
        }

        [TestMethod]
        public void PortTypeOnPowerPortInfo_IsTrackedAndAppliedUnderItsPropertyName()
        {
            var source = new PowerPortInfo();
            source.ResetChangeTracking();
            source.PortType = (SdmObjectReference<PortType>)Guid.NewGuid().ToString();

            var changes = SdmChangeTracking.GetFieldChanges(source);

            changes.Should().ContainSingle();
            changes[0].FieldName.Should().Be(nameof(PowerPortInfo.PortType));

            var target = new PowerPortInfo();
            target.ResetChangeTracking();
            var result = SdmChangeTracking.ApplyFieldChanges(target, changes);

            result.Skipped.Should().BeEmpty();
            target.PortType.Should().Be(source.PortType);
        }

        [TestMethod]
        public void RevertFieldChanges_NoChanges_ReturnsEmpty()
        {
            SdmChangeTracking.RevertFieldChanges(CreateLoadedAsset()).Should().BeEmpty();
        }

        [TestMethod]
        public void RevertFieldChanges_WhenLoadedWithDefaultBool_PreservesIsSetAndIsNotEmpty()
        {
            var port = new DataPort
            {
                Identifier = Guid.NewGuid().ToString(),
                AddressInfo = { DNS = false },
            };
            port.ResetChangeTracking();

            port.AddressInfo.DNS = true;
            var reverted = SdmChangeTracking.RevertFieldChanges(port);

            reverted.Should().ContainSingle(c => c.Section == nameof(DataPort.AddressInfo) && c.FieldName == nameof(AddressInfo.DNS));
            port.AddressInfo.DNS.Should().BeFalse();
            port.AddressInfo.IsEmpty.Should().BeFalse();
            port.AddressInfo.Changed.Should().BeFalse();
        }

        [TestMethod]
        public void RevertFieldChanges_WhenUnsetBoolExplicitlySetToFalse_RevertsToEmpty()
        {
            var port = new DataPort
            {
                Identifier = Guid.NewGuid().ToString(),
            };
            port.ResetChangeTracking();

            port.AddressInfo.IsEmpty.Should().BeTrue();
            port.AddressInfo.DNS = false;
            port.AddressInfo.IsEmpty.Should().BeFalse();
            port.AddressInfo.Changed.Should().BeTrue();

            var reverted = SdmChangeTracking.RevertFieldChanges(port);

            reverted.Should().ContainSingle(c => c.Section == nameof(DataPort.AddressInfo) && c.FieldName == nameof(AddressInfo.DNS));
            port.AddressInfo.IsEmpty.Should().BeTrue();
            port.AddressInfo.Changed.Should().BeFalse();
        }
    }
}

