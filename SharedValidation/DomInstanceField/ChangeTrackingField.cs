namespace Skyline.DataMiner.Utils.InfraOps.Common.Fields
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ChangeTrackingField<T1> : IChangeTrackingField<T1>, IEquatable<ChangeTrackingField<T1>>, IEquatable<T1>
	{
		private readonly Func<T1, object> _getChangesConverter;

		private readonly Action<Action<T1>, T1> _fieldSetter;

		private T1 _originalValue;
		private object _originalValueChanges;
		private T1 _currentValue;

        public ChangeTrackingField(T1 value, Func<T1, object> getChangesConverter = null)
        {
            if (!typeof(T1).IsValueType && typeof(T1) != typeof(string))
            {
                throw new InvalidOperationException("ChangeTrackingField does not support reference types.");
            }

            _fieldSetter = null;

            //_applyChanges = applyChanges ?? throw new ArgumentNullException(nameof(applyChanges));
            _getChangesConverter = getChangesConverter;
            //_valueIsNullChecker = valueIsNullChecker ?? ((val) => val == null);

            _originalValue = value;
            _originalValueChanges = getChangesConverter?.Invoke(_originalValue) ?? _originalValue;
            _currentValue = value;
        }

        public ChangeTrackingField(T1 value, Action<Action<T1>, T1> fieldSetter, Func<T1, object> getChangesConverter = null) : this(value,  getChangesConverter)
        {
            _fieldSetter = fieldSetter ?? throw new ArgumentNullException(nameof(fieldSetter));
        }

        //      public ChangeTrackingField(T1 value, Action<T1, T1> applyChanges, Func<T1, bool> valueIsNullChecker = null, Func<T1, object> getChangesConverter = null)
        //{
        //	if (!typeof(T1).IsValueType && typeof(T1) != typeof(string))
        //	{
        //		throw new InvalidOperationException("DomInstanceField does not support reference types.");
        //	}

        //	_fieldSetter = null;

        //	//_applyChanges = applyChanges ?? throw new ArgumentNullException(nameof(applyChanges));
        //	_getChangesConverter = getChangesConverter;
        //	_valueIsNullChecker = valueIsNullChecker ?? ((val) => val == null);

        //	_originalValue = value;
        //	_originalValueChanges = getChangesConverter?.Invoke(_originalValue) ?? _originalValue;
        //	_currentValue = value;
        //}

  //      public ChangeTrackingField(T1 value, Action<Action<T1>, T1> fieldSetter, Action<T1, T1> applyChanges, Func<T1, bool> valueIsNullChecker = null, Func<T1, object> getChangesConverter = null) : this(value, applyChanges, valueIsNullChecker, getChangesConverter)
		//{
		//	_fieldSetter = fieldSetter ?? throw new ArgumentNullException(nameof(fieldSetter));
		//}

		public T1 OriginalValue
		{
			get
			{
				return _originalValue;
			}
		}

		public T1 Value
		{
			get
			{
				return _currentValue;
			}

			set
			{
				if(_fieldSetter == null)
				{
					_currentValue = value;
				}
				else
				{
					_fieldSetter.Invoke((val) => _currentValue = val, value);
				}
			}
		}

		public bool Changed
		{
			get
			{
                bool originalIsNull = IsValueNull(_originalValue);
				bool currentIsNull = IsValueNull(_currentValue);

				if (originalIsNull)
				{
					return !currentIsNull;
				}

				if (currentIsNull)
				{
					return !originalIsNull;
				}

				var (prevVal, newVal) = GetChanges();

				bool changed;
				if (prevVal is IEnumerable<object> prevValList)
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

		public (object prevVal, object newVal) GetChanges()
		{
			return (_originalValueChanges, _getChangesConverter?.Invoke(Value) ?? Value);
		}

		public void Reset()
		{
			Value = _originalValue;
		}

		//public void ApplyChanges()
		//{
		//	_applyChanges.Invoke(_originalValue, _currentValue);
		//	_originalValue = _currentValue;
		//	_originalValueChanges = _getChangesConverter?.Invoke(_originalValue) ?? _originalValue;
		//}

		#region Equatable

		public override bool Equals(object obj)
		{
			if (obj is ChangeTrackingField<T1> otherField)
			{
				return Equals(otherField);
			}

			if (obj is T1 otherValue)
			{
				return Equals(otherValue);
			}

			return false;
		}

		public bool Equals(ChangeTrackingField<T1> other)
		{
			if (this == other)
			{
				return true;
			}

			return Equals(other.Value);
		}

		public bool Equals(T1 other)
		{
			return Equals(Value, other);
		}

		public override int GetHashCode()
		{
			return Value != null ? Value.GetHashCode() : 0;
		}

		public static implicit operator T1(ChangeTrackingField<T1> field)
		{
			return field.Value;
		}

		public static bool operator ==(ChangeTrackingField<T1> field, T1 value)
		{
			return field.Equals(value);
		}

		public static bool operator !=(ChangeTrackingField<T1> field, T1 value)
		{
			return !field.Equals(value);
		}

        private bool IsValueNull(T1 value)
        {
            if(value is string str)
            {
                return String.IsNullOrWhiteSpace(str);
            }

            return value == null;
        }

		#endregion
	}
}