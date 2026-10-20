namespace Skyline.DataMiner.Utils.InfraOps.Common.Fields
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class ChangeTrackingArrayField<T1> : IChangeTrackingField<List<T1>>, IDomInstanceFieldApplyChanges, IList<T1>, IReadOnlyList<T1>
	{
		//private readonly Action<List<T1>, List<T1>> _applyChanges;

		private readonly Func<List<T1>, object> _getChangesConverter;

		private List<T1> _originalValue;
		private object _originalValueChanges;

		public ChangeTrackingArrayField(IEnumerable<T1> value, Func<List<T1>, object> getChangesConverter = null)
		{
			_getChangesConverter = getChangesConverter;
			Value = value.ToList();
			_originalValue = CloneList(Value);
			_originalValueChanges = _getChangesConverter?.Invoke(_originalValue) ?? _originalValue;
		}

		public T1 this[int index]
		{
			get
			{
				return Value[index];
			}
		}

		T1 IList<T1>.this[int index]
		{
			get => Value[index];
			set => Value[index] = value;
		}

		public List<T1> OriginalValue
		{
			get
			{
				return _originalValue;
			}
		}

		public List<T1> Value { get; set; }

		public bool IsSet => Changed || (Value != null && Value.Count > 0);

		public bool Changed
		{
			get
			{
				bool originalIsNull = _originalValue == null;
				bool currentIsNull = Value == null;

				if (originalIsNull)
				{
					return !currentIsNull;
				}

				if (currentIsNull)
				{
					return !originalIsNull;
				}

				if (_originalValue.Count != Value.Count)
				{
					return true;
				}

				if (Value.OfType<IChangeTracking>().Any(item => item != null && item.Changed))
				{
					return true;
				}

				var prevVal = _originalValueChanges;
				var newVal = _getChangesConverter?.Invoke(Value) ?? Value;

				return !AreEqual(prevVal, newVal);
			}
		}

		// The non-generic IEnumerable is needed: List<valueType> is not an IEnumerable<object>.
		private static bool AreEqual(object prevVal, object newVal)
		{
			if (prevVal is IEnumerable prevItems && !(prevVal is string) && newVal is IEnumerable newItems && !(newVal is string))
			{
				return prevItems.Cast<object>().SequenceEqual(newItems.Cast<object>());
			}

			return Equals(prevVal, newVal);
		}

		private static T1 CloneItem(T1 item)
		{
			if (item is ChangeTrackingBase trackable)
			{
				return (T1)(object)trackable.Clone();
			}

			return item;
		}

		private static List<T1> CloneList(IEnumerable<T1> source)
		{
			return source?.Select(CloneItem).ToList();
		}

		public int Count => Value.Count;

		bool ICollection<T1>.IsReadOnly => ((ICollection<T1>)Value).IsReadOnly;

		public (object prevVal, object newVal) GetChanges()
		{
			var prevVal = _getChangesConverter != null
				? (_originalValue != null ? _getChangesConverter.Invoke(_originalValue) : null)
				: CloneList(_originalValue);
			var newVal = _getChangesConverter != null
				? (Value != null ? _getChangesConverter.Invoke(Value) : null)
				: CloneList(Value);
			return (prevVal, newVal);
		}

		public void ApplyChanges()
		{
			if (Value != null)
			{
				foreach (var item in Value.OfType<IChangeTracking>())
				{
					item?.ResetChangeTracking();
				}
			}

			_originalValue = CloneList(Value);
			_originalValueChanges = _getChangesConverter != null && _originalValue != null
				? _getChangesConverter.Invoke(_originalValue)
				: _originalValue;
		}

		public void Reset()
		{
			if (Value != null)
			{
				foreach (var item in Value.OfType<IChangeTracking>())
				{
					SdmChangeTracking.RevertFieldChanges(item);
				}
			}

			Value = CloneList(_originalValue);
		}

		public IDomInstanceFieldApplyChanges Clone()
		{
			var clone = (ChangeTrackingArrayField<T1>)MemberwiseClone();
			clone._originalValue = CloneList(_originalValue);
			clone._originalValueChanges = _getChangesConverter != null && clone._originalValue != null
				? _getChangesConverter.Invoke(clone._originalValue)
				: clone._originalValue;
			clone.Value = CloneList(Value);
			return clone;
		}

		public void Add(T1 item)
		{
			Value.Add(item);
		}

		public void Clear()
		{
			Value.Clear();
		}

		public bool Contains(T1 item)
		{
			return Value.Contains(item);
		}

		public void CopyTo(T1[] array, int arrayIndex)
		{
			Value.CopyTo(array, arrayIndex);
		}

		public int IndexOf(T1 item)
		{
			return Value.IndexOf(item);
		}

		public void Insert(int index, T1 item)
		{
			Value.Insert(index, item);
		}

		public bool Remove(T1 item)
		{
			return Value.Remove(item);
		}

		public void RemoveAt(int index)
		{
			Value.RemoveAt(index);
		}

		public IEnumerator<T1> GetEnumerator()
		{
			return Value.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}