namespace Skyline.DataMiner.Utils.InfraOps.Common.Fields
{
    using System;

	public class ChangeTrackingStringField : ChangeTrackingField<string>
	{
		public ChangeTrackingStringField(string value) : base(value)
		{
		}

		public ChangeTrackingStringField(string value, Action<Action<string>, string> fieldSetter) : base(value, fieldSetter)
		{
		}
	}
}