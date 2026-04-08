namespace Skyline.DataMiner.Utils.InfraOps.Common.Fields
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class ChangeTrackingArrayField<T1> : IChangeTrackingField<List<T1>>, IDomInstanceFieldApplyChanges, IList<T1>, IReadOnlyList<T1>
	{
		//private readonly Action<List<T1>, List<T1>> _applyChanges;

		private readonly Func<List<T1>, object> _getChangesConverter;

		private List<T1> _originalValue;
		private object _originalValueChanges;
		private List<T1> _currentValue;

		public ChangeTrackingArrayField(IEnumerable<T1> value, Func<List<T1>, object> getChangesConverter = null)
		{
			_originalValue = value.ToList();
			_originalValueChanges = getChangesConverter?.Invoke(_originalValue) ?? _originalValue;
			_currentValue = value.ToList();
			//_applyChanges = applyChanges ?? throw new ArgumentNullException(nameof(applyChanges));
			_getChangesConverter = getChangesConverter;
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
				return _originalValue.ToList();
			}
		}

		public List<T1> Value
		{
			get
			{
				return _currentValue;
			}

			set
			{
				_currentValue = value;
			}
		}

		public bool Changed
		{
			get
			{
				bool originalIsNull = _originalValue == null;
				bool currentIsNull = _currentValue == null;

				if (originalIsNull)
				{
					return !currentIsNull;
				}

				if (currentIsNull)
				{
					return !originalIsNull;
				}

				if (_originalValue.Count != _currentValue.Count)
				{
					return true;
				}

				var (prevVal, newVal) = GetChanges();

				bool changed;
				if(prevVal is IEnumerable<object> prevValList)
				{
					changed = Enumerable.SequenceEqual(prevValList, newVal as IEnumerable<object>);
				}
				else
				{
					changed = Equals(prevVal, newVal);
				}

				return !changed;
			}
		}

		public int Count => Value.Count;

		bool ICollection<T1>.IsReadOnly => ((ICollection<T1>)Value).IsReadOnly;

		public (object prevVal, object newVal) GetChanges()
		{
			return (_originalValueChanges, _getChangesConverter?.Invoke(Value) ?? Value);
		}

		public void ApplyChanges()
		{
			//_applyChanges.Invoke(_originalValue, _currentValue);

			_originalValue = _currentValue.ToList();
			_originalValueChanges = _getChangesConverter?.Invoke(_originalValue) ?? _originalValue;
		}

		public void Reset()
		{
			_currentValue = _originalValue.ToList();
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