namespace Skyline.DataMiner.Utils.InfraOps.Common.Fields
{
    public interface IChangeTrackingReadOnlyField
    {
        bool Changed { get; }

        (object prevVal, object newVal) GetChanges();
    }

    public interface IChangeTrackingField : IChangeTrackingReadOnlyField
    {
        void Reset();
    }

    public interface IChangeTrackingReadOnlyField<out T1> : IChangeTrackingReadOnlyField
    {
        T1 OriginalValue { get; }

        T1 Value { get; }
    }

    public interface IChangeTrackingField<T1> : IChangeTrackingReadOnlyField<T1>, IChangeTrackingField
    {
        new T1 Value { get; set; }
    }
}