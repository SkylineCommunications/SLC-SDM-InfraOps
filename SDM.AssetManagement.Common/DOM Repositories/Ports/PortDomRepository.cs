namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Helper;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Models.Interafaces;
    using SLDataGateway.API.Types.Querying;

    /// <summary>
    /// Repository whose purpose is to read ports from both the DataPort and PowerPort DOM definitions, and split the results per definition.
    /// </summary>
    internal sealed class PortDomRepository : IPortRepository
    {
        private readonly DomHelper helper;
        private readonly IDomInstanceReader<DataPort> dataPortRepository;
        private readonly IDomInstanceReader<PowerPort> powerPortRepository;

        public PortDomRepository(IConnection connection, IDomInstanceReader<DataPort> dataPorts, IDomInstanceReader<PowerPort> powerPorts)
        {
            if (connection is null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            this.helper = new DomHelper(connection.HandleMessages, DataPortDomMapper.ModuleId); // Shares module with PowerPortDomMapper, so either one is fine.
            this.dataPortRepository = dataPorts;
            this.powerPortRepository = powerPorts;
        }

        public IEnumerable<DataPort> Read(FilterElement<DomInstance> filter)
        {
            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            return Read((IQuery<DomInstance>)filter);
        }

        public IEnumerable<DataPort> Read(IQuery<DomInstance> query)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            //domFilter = domFilter.AND(DomInstanceExposers.DomDefinitionId.Equal(AssetManagement.Models.DataPortDomMapper.DomDefinitionId.Id));
            //var domInstances = helper.DomInstances.Read(domFilter);
            //return domInstances.Select(FromInstance);


            //var domFilter = TranslateFullFilter(query.Filter);
            //var domOrder = TranslateFullOrderBy(query.Order);
            //var domQuery = query.WithFilter(domFilter).WithOrder(domOrder);
            //return Read(domQuery);
        }

        //public PortReadResult Read(IEnumerable<Guid> domInstanceIds)
        //{
        //    if (domInstanceIds is null)
        //    {
        //        throw new ArgumentNullException(nameof(domInstanceIds));
        //    }

        //    var dataPorts = new List<DataPort>();
        //    var powerPorts = new List<PowerPort>();
        //    foreach (var batch in domInstanceIds.Distinct().Batch(MaxFilterBatchSize))
        //    {
        //        var idFilters = batch.Select(id => DomInstanceExposers.Id.Equal(new DomInstanceId(id))).ToArray();
        //        if (idFilters.Length == 0)
        //        {
        //            continue;
        //        }

        //        ReadInto(new ORFilterElement<DomInstance>(idFilters), dataPorts, powerPorts);
        //    }

        //    return new PortReadResult(dataPorts, powerPorts);
        //}

        //public PortReadResult Read(IEnumerable<string> identifiers)
        //{
        //    if (identifiers is null)
        //    {
        //        throw new ArgumentNullException(nameof(identifiers));
        //    }

        //    return Read(identifiers.Select(ParseIdentifier));
        //}


        public IEnumerable<IPagedResult<PortReadResult>> ReadPaged(FilterElement<DomInstance> filter)
        {
            return ReadPaged(filter, 500);
        }

        public IEnumerable<IPagedResult<PortReadResult>> ReadPaged(FilterElement<DomInstance> filter, int pageSize)
        {
            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "The page size must be 1 or higher");
            }

            //TODO: Change the filter to be a special exposer that distinguishes between DataPort and PowerPort, so that we can filter on the correct definition id and reduce the numnber of ORs.

            //var domFilter = TranslateFullFilter(filter);
            //var paging = ReadPaged(domFilter, pageSize).GetEnumerator();
            //var moveNext = paging.MoveNext();
            //var i = 0;
            //while (moveNext)
            //{
            //    var page = paging.Current.ToList();
            //    moveNext = paging.MoveNext();
            //    var result = new PagedResult<DataPort>(page, i, pageSize, moveNext);
            //    yield return result;
            //    i++;
            //}
        }

        public IEnumerable<IPagedResult<PortReadResult>> ReadPaged(IQuery<DataPort> query)
        {
            return ReadPaged(query, 500);
        }

        public IEnumerable<IPagedResult<PortReadResult>> ReadPaged(IQuery<DataPort> query, int pageSize)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "The page size must be 1 or higher");
            }

            //var domFilter = TranslateFullFilter(query.Filter);
            //var domOrder = TranslateFullOrderBy(query.Order);
            //var domQuery = query.WithFilter(domFilter).WithOrder(domOrder);
            //var paging = ReadPaged(domQuery, pageSize).GetEnumerator();
            //var moveNext = paging.MoveNext();
            //var i = 0;
            //while (moveNext)
            //{
            //    var page = paging.Current.ToList();
            //    moveNext = paging.MoveNext();
            //    var result = new PagedResult<DataPort>(page, i, pageSize, moveNext);
            //    yield return result;
            //    i++;
            //}
        }


        private static FilterElement<DomInstance> DefinitionFilter()
        {
            return new ORFilterElement<DomInstance>(
                DomInstanceExposers.DomDefinitionId.Equal(DataPortDomMapper.DomDefinitionId.Id),
                DomInstanceExposers.DomDefinitionId.Equal(PowerPortDomMapper.DomDefinitionId.Id));
        }

        private void ReadInto(FilterElement<DomInstance> idFilter, List<DataPort> dataPorts, List<PowerPort> powerPorts)
        {
            var domFilter = idFilter.AND(DefinitionFilter());
            foreach (var instance in helper.DomInstances.Read(domFilter))
            {
                Dispatch(instance, dataPorts, powerPorts);
            }
        }

        private void Dispatch(DomInstance instance, List<DataPort> dataPorts, List<PowerPort> powerPorts)
        {
            if (instance.DomDefinitionId.Id == DataPortDomMapper.DomDefinitionId.Id)
            {
                dataPorts.Add(dataPortRepository.FromDomInstance(instance));
            }
            else if (instance.DomDefinitionId.Id == PowerPortDomMapper.DomDefinitionId.Id)
            {
                powerPorts.Add(powerPortRepository.FromDomInstance(instance));
            }
        }
    }
}
