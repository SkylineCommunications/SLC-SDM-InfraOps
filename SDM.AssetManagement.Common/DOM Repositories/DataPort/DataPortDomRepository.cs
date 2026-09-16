namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models.Interafaces;

    internal partial class DataPortDomRepository : IBulkRepository<DataPort>, IDomInstanceReader<DataPort>
    {
        DataPort IDomInstanceReader<DataPort>.FromDomInstance(DomInstance instance)
        {
            return FromInstance(instance);
        }

        FilterElement<DomInstance> IDomInstanceReader<DataPort>.CreateDomFilter(string fieldName, Comparer comparer, object value)
        {
            return CreateFilter(fieldName, comparer, value);
        }
    }
}