namespace Skyline.DataMiner.Utils.InfraOps.Common.Fields
{
    using System;

	internal class ChangeTrackingStringField : ChangeTrackingField<string>
	{
		public ChangeTrackingStringField(string value) : base(value)
		{
		}

		public ChangeTrackingStringField(string value, Action<Action<string>, string> fieldSetter) : base(value, fieldSetter)
		{
		}
	}
}