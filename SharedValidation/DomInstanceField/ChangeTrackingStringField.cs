namespace Skyline.DataMiner.Utils.InfraOps.Common.Fields
{
    using System;

	public class ChangeTrackingStringField : ChangeTrackingField<string>
	{
		public ChangeTrackingStringField(string value, Action<string, string> applyChanges, Func<string, object> getChangesConverter = null) : base(value, applyChanges, String.IsNullOrWhiteSpace, getChangesConverter)
		{
		}

		public ChangeTrackingStringField(string value, Action<Action<string>, string> fieldSetter, Action<string, string> applyChanges, Func<string, object> getChangesConverter = null) : base(value, fieldSetter, applyChanges, String.IsNullOrWhiteSpace, getChangesConverter)
		{
		}
	}
}