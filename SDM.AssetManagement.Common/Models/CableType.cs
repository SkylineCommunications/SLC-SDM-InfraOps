namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;
    using Skyline.DataMiner.SDM.InfraOps.Core.Models;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    //[GenerateExposers]
    //[SdmDomStorage("(slc)asset_management")]
    public sealed class CableType : SdmObjectBase<CableType>, IEquatable<CableType>, IEntityTracking
    {
        [JsonIgnore]
        private ChangeTrackingFieldHandler _fieldHandler;
        [JsonIgnore]
        private CategoryRelation _categoryLinks;
        [JsonIgnore]
        private bool _isNew = true;

        public CableType()
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
            _categoryLinks?.Changed == true;

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

        public CategoryRelation CategoryLinks
        {
            get => _categoryLinks ?? (_categoryLinks = new CategoryRelation());
            set => _categoryLinks = value ?? new CategoryRelation();
        }

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

        public IEnumerable<TrackingFieldValueDifference> GetChanges()
        {
            return FieldHandler.GetChanges()
                .Select(kvp => new TrackingFieldValueDifference
                {
                    FieldName = kvp.Key,
                    OldValue = kvp.Value.prevVal,
                    NewValue = kvp.Value.newVal,
                })
                .Concat(_categoryLinks?.GetChanges() ?? Enumerable.Empty<TrackingFieldValueDifference>());
        }

        public void ResetChangeTracking()
        {
            FieldHandler?.ApplyChanges();
            _categoryLinks?.ResetChangeTracking();
        }

        public static bool operator ==(CableType left, CableType right)
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

        public static bool operator !=(CableType left, CableType right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CableType);
        }

        public bool Equals(CableType other)
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
                Equals(CategoryLinks, other.CategoryLinks);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 23) + (Name != null ? Name.GetHashCode() : 0);
                hash = (hash * 23) + (Description != null ? Description.GetHashCode() : 0);
                hash = (hash * 23) + (CategoryLinks != null ? CategoryLinks.GetHashCode() : 0);
                return hash;
            }
        }

        #region Section Tracking

        [JsonIgnore]
        [SdmIgnore]
        internal Guid? CableTypePropertiesSectionId { get; set; }

        #endregion

    }
}
