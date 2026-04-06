namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations
{
	using System;
	using System.Collections.Generic;

	public class ValidationResult
	{
		private readonly Dictionary<string, string> _failReasons;
		private readonly Dictionary<string, string> _displayKey;

		private bool _isValid;

		public ValidationResult()
		{
			_failReasons = new Dictionary<string, string>();
			_displayKey = new Dictionary<string, string>();
			_isValid = true;
		}

		public bool IsValid
		{
			get
			{
				return _isValid;
			}
		}

		public IReadOnlyDictionary<string, string> FailureReasons => _failReasons;

		public bool TryGetFailReason<T>(T field, out string reason) where T : Enum
		{
			return _failReasons.TryGetValue(GetFieldToId(field), out reason);
		}

		public string GetFailReason<T>(T field) where T : Enum
		{
			if(!TryGetFailReason(field, out string reason))
			{
				reason = string.Empty;
			}

			return reason;
		}

		public void AddFailReason<T>(T field, string reason) where T : Enum
		{
			AddFailReason(GetFieldToId(field), Convert.ToString(field), reason);
		}

		public void AddFailReason(string field, string displayFieldName, string reason)
		{
			_isValid = false;
			if (_failReasons.ContainsKey(field))
			{
				throw new InvalidOperationException($"Fail Reason for field '{field}' already exists. Cannot add multiple reasons for the same field.");
			}

			_failReasons[field] = reason;
			_displayKey[field] = displayFieldName;
		}

		public ValidationResult CombineResults(ValidationResult otherResult)
		{
			var combinedResult = new ValidationResult();

			foreach (var entry in _failReasons)
			{
				combinedResult.AddFailReason(entry.Key, _displayKey[entry.Key], entry.Value);
			}

			foreach (var entry in otherResult.FailureReasons)
			{
				combinedResult.AddFailReason(entry.Key, otherResult._displayKey[entry.Key], entry.Value);
			}

			return combinedResult;
		}

		public string GetCombinedFailureReasons(string separator)
		{
			List<string> reasons = new List<string>();
			foreach (var entry in _failReasons)
			{
				reasons.Add($"{_displayKey[entry.Key]}: {entry.Value}");
			}

			return string.Join(separator, reasons);
		}

		private string GetFieldToId<T>(T field) where T : Enum
		{
			return $"{typeof(T).Name}_{Convert.ToString(field)}";
		}
	}
}