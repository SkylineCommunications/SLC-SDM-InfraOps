namespace Skyline.DataMiner.Utils.InfraOps.Common.Fields
{
    using System;
	using System.Collections.Generic;
	using System.Linq;

	public class DomInstanceFieldHandler
	{
		private readonly Dictionary<string, IDomInstanceFieldApplyChanges> _fields;

		public DomInstanceFieldHandler()
		{
			_fields = new Dictionary<string, IDomInstanceFieldApplyChanges>();
		}

		public bool HasChanges
		{
			get
			{
				return _fields.Values.Any(field => field.Changed);
			}
		}

		public ChangeTrackingField<T1> GetOrCreateField<T1>(string fieldName, Func<ChangeTrackingField<T1>> creator)
		{
			if (!_fields.TryGetValue(fieldName, out var field))
			{
				field = creator();
				_fields.Add(fieldName, field);
			}

			if(field is ChangeTrackingField<T1> typedField)
			{
				return typedField;
			}

			throw new InvalidOperationException($"Field '{fieldName}' is not of type '{typeof(ChangeTrackingField<T1>).FullName}', expected '{field.GetType().FullName}'.");
		}

		public ChangeTrackingArrayField<T1> GetOrCreateArrayField<T1>(string fieldName, Func<ChangeTrackingArrayField<T1>> creator)
		{
			if (!_fields.TryGetValue(fieldName, out var field))
			{
				field = creator();
				_fields.Add(fieldName, field);
			}

			if (field is ChangeTrackingArrayField<T1> typedField)
			{
				return typedField;
			}

			throw new InvalidOperationException($"Field '{fieldName}' is not of type '{typeof(ChangeTrackingField<T1>).FullName}', expected '{field.GetType().FullName}'.");
		}

		public Dictionary<string, (object prevVal, object newVal)> GetChanges()
		{
			return _fields.ToDictionary(entry => entry.Key, entry => entry.Value.GetChanges());
		}

		public void ApplyChanges()
		{
			foreach (var field in _fields.Values)
			{
				field.ApplyChanges();
			}
		}
	}
}