namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;

    using Newtonsoft.Json;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class AssetHolder : ChangeTrackingBase, IEquatable<AssetHolder>, ISectionTrackable, ISectionEmptyState
	{
		[JsonIgnore]
		[SdmIgnore]
		Guid? ISectionTrackable.SectionId { get; set; }
		[JsonIgnore]
		[SdmIgnore]
		public bool IsEmpty => SlotNumber == default &&
			Label == default &&
			HierarchyRole == default;

		public long SlotNumber
		{
			get => SlotNumberField.Value;
			set => SlotNumberField.Value = value;
		}

		public string Label
		{
			get => LabelField.Value;
			set => LabelField.Value = value;
		}

		public SlcAsset_Management.Enums.HierarchyRoleEnum HierarchyRole
		{
			get => HierarchyRoleField.Value;
			set => HierarchyRoleField.Value = value;
		}

		[JsonIgnore]
		[SdmIgnore]
		internal IChangeTrackingField<long> SlotNumberField => FieldHandler.GetOrCreateField(
			nameof(SlotNumber),
			() => new ChangeTrackingField<long>(default));

		[JsonIgnore]
		[SdmIgnore]
		internal IChangeTrackingField<string> LabelField => FieldHandler.GetOrCreateField(
			nameof(Label),
			() => new ChangeTrackingStringField(null));

		[JsonIgnore]
		[SdmIgnore]
		internal IChangeTrackingField<SlcAsset_Management.Enums.HierarchyRoleEnum> HierarchyRoleField => FieldHandler.GetOrCreateField(
			nameof(HierarchyRole),
			() => new ChangeTrackingField<SlcAsset_Management.Enums.HierarchyRoleEnum>(default));

		public static bool operator ==(AssetHolder left, AssetHolder right)
		{
			if (ReferenceEquals(left, right))
			{
				return true;
			}

			if (left is null || right is null)
			{
				return false;
			}

			return left.Equals(right);
		}

		public static bool operator !=(AssetHolder left, AssetHolder right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as AssetHolder);
		}

		public bool Equals(AssetHolder other)
		{
			if (other is null)
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return
				SlotNumber == other.SlotNumber &&
				HierarchyRole == other.HierarchyRole;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = (hash * 23) + SlotNumber.GetHashCode();
				hash = (hash * 23) + HierarchyRole.GetHashCode();
				return hash;
			}
		}
	}
}