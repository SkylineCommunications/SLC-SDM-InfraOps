namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations
{
	using System.Collections.Generic;
	using System.Linq;

	internal class ValidatorContext<T1> where T1 : class
	{
		private readonly T1 _baseEntry;
		private readonly List<T1> _otherChangedEntries;
		private readonly List<T1> _allChangedEntries;

		public ValidatorContext()
		{
			this._baseEntry = null;
			this._otherChangedEntries = new List<T1>();
			_allChangedEntries = new List<T1>();
		}

		public ValidatorContext(T1 baseEntry)
		{
			this._baseEntry = baseEntry;
			this._otherChangedEntries = new List<T1>();
			_allChangedEntries = new List<T1>
            {
                baseEntry
            };
		}

		public ValidatorContext(T1 baseEntry, List<T1> otherChangedEntries)
		{
			_baseEntry = baseEntry;
			_otherChangedEntries = otherChangedEntries.Except(new List<T1> { baseEntry }).ToList();
			_allChangedEntries = new List<T1>
            {
                baseEntry
            };
			_allChangedEntries.AddRange(_otherChangedEntries);
		}

		public T1 BaseEntry
		{
			get
			{
				return _baseEntry;
			}
		}

		public List<T1> OtherChangedEntries
		{
			get
			{
				return _otherChangedEntries;
			}
		}

		public List<T1> ChangedEntries
		{
			get
			{
				return _allChangedEntries;
			}
		}

		public bool ReturnWhenInvalid { get; set; } = true;
	}
}