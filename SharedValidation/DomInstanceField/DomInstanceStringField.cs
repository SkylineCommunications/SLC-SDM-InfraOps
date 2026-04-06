namespace Skyline.DataMiner.Utils.InfraOps.Common.DOM_Classes.DOM.Applications.All.Fields
{
	using System;

	public class DomInstanceStringField : ChangeTrackingField<string>
	{
		public DomInstanceStringField(string value, Action<string, string> applyChanges, Func<string, object> getChangesConverter = null) : base(value, applyChanges, String.IsNullOrWhiteSpace, getChangesConverter)
		{
		}

		public DomInstanceStringField(string value, Action<Action<string>, string> fieldSetter, Action<string, string> applyChanges, Func<string, object> getChangesConverter = null) : base(value, fieldSetter, applyChanges, String.IsNullOrWhiteSpace, getChangesConverter)
		{
		}
	}
}