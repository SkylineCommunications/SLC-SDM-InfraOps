namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Models.Interfaces;

    internal partial class PowerPortDomRepository : IBulkRepository<PowerPort>, IDomInstanceReader<PowerPort>
    {
        PowerPort IDomInstanceReader<PowerPort>.FromDomInstance(DomInstance instance)
        {
            return FromInstance(instance);
        }

        FilterElement<DomInstance> IDomInstanceReader<PowerPort>.CreateDomFilter(string fieldName, Comparer comparer, object value)
        {
            return CreateFilter(fieldName, comparer, value);
        }

    }
}