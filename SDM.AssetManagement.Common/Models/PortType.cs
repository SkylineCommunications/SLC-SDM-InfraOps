namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using Newtonsoft.Json;

    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public sealed class PortType : SdmObject<PortType>, IEquatable<PortType>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private CategoryRelation _categoryLinks;
        [JsonIgnore]
        private CableRelation _cableFKs;
        [JsonIgnore]
        private bool _isNew = true;

        public PortType()
        {
            _fieldHandler = new ChangeTrackingFieldHandler();
        }

        [JsonIgnore]
        [SdmIgnore]
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

        [JsonIgnore]
        [SdmIgnore]
        public bool IsNew => _isNew;

        /// <summary>
        /// Sets the IsNew flag. Used internally when loading from database.
        /// </summary>
        [JsonIgnore]
        [SdmIgnore]
        internal bool IsNewInternal
        {
            get => _isNew;
            set => _isNew = value;
        }

        [JsonIgnore]
        [SdmIgnore]
        public bool Changed =>
            FieldHandler.HasChanges ||
            _categoryLinks?.Changed == true ||
            _cableFKs?.Changed == true;

        public string Name
        {
            get => NameField.Value;
            set => NameField.Value = value;
        }

        public string Description
        {
            get => DescriptionField.Value;
            set => DescriptionField.Value = value;
        }

        public CategoryRelation CategoryLinks => _categoryLinks ?? (_categoryLinks = new CategoryRelation());

        public CableRelation CableFKs => _cableFKs ?? (_cableFKs = new CableRelation());

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> NameField => FieldHandler.GetOrCreateField(
            nameof(Name),
            () => new ChangeTrackingStringField(null));

        [JsonIgnore]
        [SdmIgnore]
        internal IChangeTrackingField<string> DescriptionField => FieldHandler.GetOrCreateField(
            nameof(Description),
            () => new ChangeTrackingStringField(null));

        public void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();
            _categoryLinks?.ResetChangeTracking();
            _cableFKs?.ResetChangeTracking();
        }

        public static bool operator ==(PortType left, PortType right)
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

        public static bool operator !=(PortType left, PortType right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PortType);
        }

        public bool Equals(PortType other)
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
                string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(Description, other.Description, StringComparison.OrdinalIgnoreCase) &&
                Equals(CategoryLinks, other.CategoryLinks) &&
                Equals(CableFKs, other.CableFKs);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (Name != null ? Name.GetHashCode() : 0);
                hash = (hash * 23) + (Description != null ? Description.GetHashCode() : 0);
                hash = (hash * 23) + (CategoryLinks != null ? CategoryLinks.GetHashCode() : 0);
                hash = (hash * 23) + (CableFKs != null ? CableFKs.GetHashCode() : 0);
                return hash;
            }
        }

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? PortTypePropertiesSectionId { get; set; }

        #endregion

    }
}
