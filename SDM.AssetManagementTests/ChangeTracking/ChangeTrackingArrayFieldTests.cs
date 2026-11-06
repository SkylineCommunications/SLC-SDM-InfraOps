namespace SDM.AssetManagement.Tests.ChangeTracking
{
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.InfraOps.Common.Validation;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    [TestClass]
    public class ChangeTrackingArrayFieldTests
    {
        [TestMethod]
        public void Changed_ValueTypeList_Untouched_IsFalse()
        {
            var field = new ChangeTrackingArrayField<int>(new[] { 1, 2 });

            field.Changed.Should().BeFalse();
        }

        [TestMethod]
        public void Changed_ReferenceTypeList_Untouched_IsFalse()
        {
            var field = new ChangeTrackingArrayField<string>(new[] { "a", "b" });

            field.Changed.Should().BeFalse();
        }

        [TestMethod]
        public void Changed_ValueTypeList_ItemReplaced_IsTrue()
        {
            var field = new ChangeTrackingArrayField<int>(new[] { 1, 2 });
            field.Value[1] = 3;

            field.Changed.Should().BeTrue();
        }

        [TestMethod]
        public void Asset_OperationalFlags_Untouched_IsNotReportedAsChange()
        {
            var asset = new Asset { Name = "Original" };
            _ = asset.OperationalFlags;
            asset.ResetChangeTracking();

            asset.GetChanges().Select(change => change.FieldName)
                .Should().NotContain(nameof(Asset.OperationalFlags));
        }

        [TestMethod]
        public void Asset_Holders_InPlaceItemMutation_MarksHoldersFieldChangedAndShouldValidate_AndRevertsOnReset()
        {
            var asset = new Asset
            {
                Name = "Original",
                IsNewInternal = false,
            };
            asset.Holders.Add(new AssetHolder { SlotNumber = 1 });
            asset.ResetChangeTracking();

            asset.ShouldValidate(asset.HoldersField).Should().BeFalse();

            // Mutate item in-place to default value 0
            asset.Holders[0].SlotNumber = 0;

            asset.HoldersField.Changed.Should().BeTrue();
            asset.ShouldValidate(asset.HoldersField).Should().BeTrue();

            SdmChangeTracking.RevertFieldChanges(asset);

            asset.Holders[0].SlotNumber.Should().Be(1);
            asset.HoldersField.Changed.Should().BeFalse();
            asset.ShouldValidate(asset.HoldersField).Should().BeFalse();
        }

        [TestMethod]
        public void AssetClass_Ports_InPlaceItemMutation_PreservesDistinctOldAndNewValuesInGetChanges()
        {
            var assetClass = new AssetClass
            {
                Name = "Switch",
                IsNewInternal = false,
            };
            assetClass.DataPorts.Add(new DataPortInfo { Name = "eth0", PortNumber = 1 });
            assetClass.PowerPorts.Add(new PowerPortInfo { Name = "pwr0", PortNumber = 2 });
            assetClass.ResetChangeTracking();

            assetClass.DataPorts[0].PortNumber = -1;
            assetClass.PowerPorts[0].PortNumber = -5;

            assetClass.ShouldValidate(assetClass.DataPortsField).Should().BeTrue();
            assetClass.ShouldValidate(assetClass.PowerPortsField).Should().BeTrue();

            var changes = assetClass.GetChanges().ToList();

            var dataPortsChange = changes.Single(c => c.FieldName == nameof(AssetClass.DataPorts));
            var oldDataPorts = (List<DataPortInfo>)dataPortsChange.OldValue;
            var newDataPorts = (List<DataPortInfo>)dataPortsChange.NewValue;
            oldDataPorts.Should().ContainSingle();
            newDataPorts.Should().ContainSingle();
            oldDataPorts[0].Should().NotBeSameAs(newDataPorts[0]);
            oldDataPorts[0].PortNumber.Should().Be(1);
            oldDataPorts[0].Changed.Should().BeFalse();
            newDataPorts[0].PortNumber.Should().Be(-1);

            var powerPortsChange = changes.Single(c => c.FieldName == nameof(AssetClass.PowerPorts));
            var oldPowerPorts = (List<PowerPortInfo>)powerPortsChange.OldValue;
            var newPowerPorts = (List<PowerPortInfo>)powerPortsChange.NewValue;
            oldPowerPorts.Should().ContainSingle();
            newPowerPorts.Should().ContainSingle();
            oldPowerPorts[0].Should().NotBeSameAs(newPowerPorts[0]);
            oldPowerPorts[0].PortNumber.Should().Be(2);
            oldPowerPorts[0].Changed.Should().BeFalse();
            newPowerPorts[0].PortNumber.Should().Be(-5);
        }

        [TestMethod]
        public void Asset_Holders_InPlaceItemMutation_RevertAndReapply_PreservesMutatedNewValue()
        {
            var source = new Asset
            {
                Name = "Original",
                IsNewInternal = false,
            };
            source.Holders.Add(new AssetHolder { SlotNumber = 1, Label = "Slot-1" });
            source.ResetChangeTracking();

            source.Holders[0].SlotNumber = 9;

            var reverted = SdmChangeTracking.RevertFieldChanges(source);

            source.Holders[0].SlotNumber.Should().Be(1);
            source.HoldersField.Changed.Should().BeFalse();

            var holderChange = reverted.Single(c => c.FieldName == nameof(Asset.Holders));
            ((List<AssetHolder>)holderChange.OldValue)[0].SlotNumber.Should().Be(1);
            ((List<AssetHolder>)holderChange.NewValue)[0].SlotNumber.Should().Be(9);

            var fresh = new Asset
            {
                Name = "Original",
                IsNewInternal = false,
            };
            fresh.Holders.Add(new AssetHolder { SlotNumber = 1, Label = "Slot-1" });
            fresh.ResetChangeTracking();

            var applyResult = SdmChangeTracking.ApplyFieldChanges(fresh, reverted);

            applyResult.Skipped.Should().BeEmpty();
            fresh.Holders[0].SlotNumber.Should().Be(9);
            fresh.HoldersField.Changed.Should().BeTrue();
            fresh.ShouldValidate(fresh.HoldersField).Should().BeTrue();
        }
    }
}
