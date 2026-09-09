namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
	using System;

	using Newtonsoft.Json;

	using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

	public sealed class ElementLink : ChangeTrackingBase, IEquatable<ElementLink>, ISectionTrackable, ISectionEmptyState
	{
		[JsonIgnore]
		[SdmIgnore]
		Guid? ISectionTrackable.SectionId { get; set; }
		[JsonIgnore]
		[SdmIgnore]
		public bool IsEmpty => ElementID == default &&
			IsPrimary == default;

		public string ElementID
		{
			get => ElementIDField.Value;
			set => ElementIDField.Value = value;
		}

		public bool IsPrimary
		{
			get => IsPrimaryField.Value;
			set => IsPrimaryField.Value = value;
		}

		[JsonIgnore]
		[SdmIgnore]
		internal IChangeTrackingField<string> ElementIDField => FieldHandler.GetOrCreateField(
			nameof(ElementID),
			() => new ChangeTrackingStringField(null));

		[JsonIgnore]
		[SdmIgnore]
		internal IChangeTrackingField<bool> IsPrimaryField => FieldHandler.GetOrCreateField(
			nameof(IsPrimary),
			() => new ChangeTrackingField<bool>(default));

		public static bool operator ==(ElementLink left, ElementLink right)
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

		public static bool operator !=(ElementLink left, ElementLink right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as ElementLink);
		}

		public bool Equals(ElementLink other)
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
				string.Equals(ElementID, other.ElementID, StringComparison.OrdinalIgnoreCase) &&
				IsPrimary == other.IsPrimary;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = (hash * 23) + (ElementID != null ? ElementID.GetHashCode() : 0);
				hash = (hash * 23) + IsPrimary.GetHashCode();
				return hash;
			}
		}
	}
}