namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.Runtime.Serialization;

    using Newtonsoft.Json;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    public class AssetCustody
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;

        public AssetCustody()
        {
            _fieldHandler = new ChangeTrackingFieldHandler();
        }

        [JsonIgnore]
        private ChangeTrackingFieldHandler FieldHandler
        {
            get
            {
                if (_fieldHandler == null)
                {
                    _fieldHandler = new ChangeTrackingFieldHandler();
                }
                return _fieldHandler;
            }
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            ResetChangeTracking();
        }

        #region Public Properties

        public DateTime? From
        {
            get => FromField.Value;
            set => FromField.Value = value;
        }

        public DateTime? Till
        {
            get => TillField.Value;
            set => TillField.Value = value;
        }

        public Guid ContactPerson
        {
            get => ContactPersonField.Value;
            set => ContactPersonField.Value = value;
        }

        public Guid Team
        {
            get => TeamField.Value;
            set => TeamField.Value = value;
        }

        public Guid Organization
        {
            get => OrganizationField.Value;
            set => OrganizationField.Value = value;
        }

        public Guid ContactPersonRole
        {
            get => ContactPersonRoleField.Value;
            set => ContactPersonRoleField.Value = value;
        }

        #endregion

        #region Internal Tracking Fields

        [JsonIgnore]
        internal IChangeTrackingField<DateTime?> FromField => FieldHandler.GetOrCreateField(
            nameof(From),
            () => new ChangeTrackingField<DateTime?>(null));

        [JsonIgnore]
        internal IChangeTrackingField<DateTime?> TillField => FieldHandler.GetOrCreateField(
            nameof(Till),
            () => new ChangeTrackingField<DateTime?>(null));

        [JsonIgnore]
        internal IChangeTrackingField<Guid> ContactPersonField => FieldHandler.GetOrCreateField(
            nameof(ContactPerson),
            () => new ChangeTrackingField<Guid>(default));

        [JsonIgnore]
        internal IChangeTrackingField<Guid> TeamField => FieldHandler.GetOrCreateField(
            nameof(Team),
            () => new ChangeTrackingField<Guid>(default));

        [JsonIgnore]
        internal IChangeTrackingField<Guid> OrganizationField => FieldHandler.GetOrCreateField(
            nameof(Organization),
            () => new ChangeTrackingField<Guid>(default));

        [JsonIgnore]
        internal IChangeTrackingField<Guid> ContactPersonRoleField => FieldHandler.GetOrCreateField(
            nameof(ContactPersonRole),
            () => new ChangeTrackingField<Guid>(default));

        #endregion

        public void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();
        }
    }
}