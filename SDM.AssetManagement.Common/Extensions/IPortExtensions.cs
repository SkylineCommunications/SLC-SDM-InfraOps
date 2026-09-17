namespace Skyline.DataMiner.SDM.AssetManagement.Common.Extensions
{
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;

    public static class IPortExtensions
    {
        public static ManagedFilter<TFilter, ISdmObjectReference<IPort>> Equal<TFilter>(
            this Exposer<TFilter, ISdmObjectReference<IPort>> exposer,
            IPort value)
            where TFilter : class
        {
            var reference = ISdmObjectReference<IPort>.To(value);
            return new ManagedFilter<TFilter, ISdmObjectReference<IPort>>(exposer, Comparer.Equals, reference, delegate (TFilter obj)
            {
                var val = exposer.internalFunc(obj);
                return val.HasValue() && val.Equals(reference);
            });
        }
    }
}