namespace Skyline.DataMiner.SDM.Extensions
{
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;

    public static class ISdmObjectReferenceExtensions
    {
        public static ManagedFilter<TFilter, ISdmObjectReference<TField>> Equal<TFilter, TField>(
            this Exposer<TFilter, ISdmObjectReference<TField>> exposer,
            TField value)
            where TFilter : class
            where TField : ISdmObject
        {
            var reference = ISdmObjectReference<TField>.To(value);
            return new ManagedFilter<TFilter, ISdmObjectReference<TField>>(exposer, Comparer.Equals, reference, delegate (TFilter obj)
            {
                var val = exposer.internalFunc(obj);
                return val.HasValue() && val.Equals(reference);
            });
        }

        public static ManagedFilter<TFilter, ISdmObjectReference<TField>> NotEqual<TFilter, TField>(
            this Exposer<TFilter, ISdmObjectReference<TField>> exposer,
            TField value)
            where TFilter : class
            where TField : ISdmObject
        {
            var reference = ISdmObjectReference<TField>.To(value);
            return new ManagedFilter<TFilter, ISdmObjectReference<TField>>(exposer, Comparer.Equals, reference, delegate (TFilter obj)
            {
                var val = exposer.internalFunc(obj);
                return val.HasValue() && !val.Equals(reference);
            });
        }
    }
}