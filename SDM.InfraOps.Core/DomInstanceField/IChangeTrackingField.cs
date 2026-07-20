namespace Skyline.DataMiner.Utils.InfraOps.Common.Fields
{
    internal interface IChangeTrackingReadOnlyField
    {
        bool Changed { get; }

        (object prevVal, object newVal) GetChanges();
    }

    internal interface IChangeTrackingField : IChangeTrackingReadOnlyField
    {
        void Reset();
    }

    internal interface IChangeTrackingReadOnlyField<out T1> : IChangeTrackingReadOnlyField
    {
        T1 OriginalValue { get; }

        T1 Value { get; }
    }

    internal interface IChangeTrackingField<T1> : IChangeTrackingReadOnlyField<T1>, IChangeTrackingField
    {
        new T1 Value { get; set; }
    }
}