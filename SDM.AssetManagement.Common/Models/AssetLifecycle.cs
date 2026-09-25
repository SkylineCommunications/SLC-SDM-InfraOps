namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;

    using Newtonsoft.Json;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public sealed class AssetLifecycle : ChangeTrackingBase, IEquatable<AssetLifecycle>, ISectionTrackable, ISectionEmptyState
	{
		[JsonIgnore]
		[SdmIgnore]
		Guid? ISectionTrackable.SectionId { get; set; }
		[JsonIgnore]
		[SdmIgnore]
		public bool IsEmpty => PurchaseDate == default &&
			FirstUseDate == default &&
			EndOfWarrantyDate == default &&
			InstallationDate == default &&
			!InstallationUserId.HasValue() &&
			ModificationDate == default &&
			!ModificationUserId.HasValue() &&
			EndOfLife == default;

		public DateTime PurchaseDate
		{
			get => PurchaseDateField.Value;
			set => PurchaseDateField.Value = value;
		}

		public DateTime FirstUseDate
		{
			get => FirstUseDateField.Value;
			set => FirstUseDateField.Value = value;
		}

		public DateTime EndOfWarrantyDate
		{
			get => EndOfWarrantyDateField.Value;
			set => EndOfWarrantyDateField.Value = value;
		}

		public DateTime InstallationDate
		{
			get => InstallationDateField.Value;
			set => InstallationDateField.Value = value;
		}

		public PnoObjectReference<Person> InstallationUserId
		{
			get => InstallationUserIdField.Value;
			set => InstallationUserIdField.Value = value;
		}

		public DateTime ModificationDate
		{
			get => ModificationDateField.Value;
			set => ModificationDateField.Value = value;
		}

		public PnoObjectReference<Person> ModificationUserId
		{
			get => ModificationUserIdField.Value;
			set => ModificationUserIdField.Value = value;
		}

		public DateTime EndOfLife
		{
			get => EndOfLifeField.Value;
			set => EndOfLifeField.Value = value;
		}

		[JsonIgnore]
		[SdmIgnore]
		internal IChangeTrackingField<DateTime> PurchaseDateField => FieldHandler.GetOrCreateField(
			nameof(PurchaseDate),
			() => new ChangeTrackingField<DateTime>(default));

		[JsonIgnore]
		[SdmIgnore]
		internal IChangeTrackingField<DateTime> FirstUseDateField => FieldHandler.GetOrCreateField(
			nameof(FirstUseDate),
			() => new ChangeTrackingField<DateTime>(default));

		[JsonIgnore]
		[SdmIgnore]
		internal IChangeTrackingField<DateTime> EndOfWarrantyDateField => FieldHandler.GetOrCreateField(
			nameof(EndOfWarrantyDate),
			() => new ChangeTrackingField<DateTime>(default));

		[JsonIgnore]
		[SdmIgnore]
		internal IChangeTrackingField<DateTime> InstallationDateField => FieldHandler.GetOrCreateField(
			nameof(InstallationDate),
			() => new ChangeTrackingField<DateTime>(default));

		[JsonIgnore]
		[SdmIgnore]
		internal IChangeTrackingField<PnoObjectReference<Person>> InstallationUserIdField => FieldHandler.GetOrCreateField(
			nameof(InstallationUserId),
			() => new ChangeTrackingField<PnoObjectReference<Person>>(default, reference => reference.Identifier));

		[JsonIgnore]
		[SdmIgnore]
		internal IChangeTrackingField<DateTime> ModificationDateField => FieldHandler.GetOrCreateField(
			nameof(ModificationDate),
			() => new ChangeTrackingField<DateTime>(default));

		[JsonIgnore]
		[SdmIgnore]
		internal IChangeTrackingField<PnoObjectReference<Person>> ModificationUserIdField => FieldHandler.GetOrCreateField(
			nameof(ModificationUserId),
			() => new ChangeTrackingField<PnoObjectReference<Person>>(default, reference => reference.Identifier));

		[JsonIgnore]
		[SdmIgnore]
		internal IChangeTrackingField<DateTime> EndOfLifeField => FieldHandler.GetOrCreateField(
			nameof(EndOfLife),
			() => new ChangeTrackingField<DateTime>(default));

		public static bool operator ==(AssetLifecycle left, AssetLifecycle right)
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

		public static bool operator !=(AssetLifecycle left, AssetLifecycle right)
		{
			return !(left == right);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as AssetLifecycle);
		}

		public bool Equals(AssetLifecycle other)
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
				PurchaseDate.Equals(other.PurchaseDate) &&
				FirstUseDate.Equals(other.FirstUseDate) &&
				EndOfWarrantyDate.Equals(other.EndOfWarrantyDate) &&
				InstallationDate.Equals(other.InstallationDate) &&
				InstallationUserId == other.InstallationUserId &&
				ModificationDate.Equals(other.ModificationDate) &&
				ModificationUserId == other.ModificationUserId &&
				EndOfLife.Equals(other.EndOfLife);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;
				hash = (hash * 23) + PurchaseDate.GetHashCode();
				hash = (hash * 23) + FirstUseDate.GetHashCode();
				hash = (hash * 23) + EndOfWarrantyDate.GetHashCode();
				hash = (hash * 23) + InstallationDate.GetHashCode();
				hash = (hash * 23) + InstallationUserId.GetHashCode();
				hash = (hash * 23) + ModificationDate.GetHashCode();
				hash = (hash * 23) + ModificationUserId.GetHashCode();
				hash = (hash * 23) + EndOfLife.GetHashCode();
				return hash;
			}
		}
	}
}