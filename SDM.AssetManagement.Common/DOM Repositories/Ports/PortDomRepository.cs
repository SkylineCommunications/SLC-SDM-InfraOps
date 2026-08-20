namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models.Interafaces;
    using SLDataGateway.API.Querying;
    using SLDataGateway.API.Types.Querying;

    /// <summary>
    /// Repository whose purpose is to read ports from both the DataPort and PowerPort DOM definitions, and split the results per definition.
    /// Filters built with <see cref="PortExposers"/> are translated per definition and combined as:
    /// (Definition = DataPort AND translated filter) OR (Definition = PowerPort AND translated filter).
    /// When the filter uses definition-exclusive fields (<see cref="PortExposers.DataPortOnly"/> / <see cref="PortExposers.PowerPortOnly"/>),
    /// the opposite definition is useless to filter and is skipped. When exclusive fields of both definitions
    /// are combined, no instance can match and an empty result is returned without querying.
    /// </summary>
    internal sealed class PortDomRepository : IPortRepository
    {
        private const int DefaultPageSize = 500;

        private static readonly HashSet<string> DataPortOnlyFields = new HashSet<string>(StringComparer.Ordinal)
        {
            "AddressInfo.Ipv4Address",
            "AddressInfo.Ipv6Address",
            "AddressInfo.Hostname",
            "AddressInfo.DNS",
            "PrimaryPortRelation.IsPrimaryIpv6",
            "PrimaryPortRelation.IsPrimaryIpv4",
        };

        // The PowerPort definition currently has no fields that the DataPort definition lacks.
        private static readonly HashSet<string> PowerPortOnlyFields = new HashSet<string>(StringComparer.Ordinal);

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
            this.dataPortRepository = dataPorts ?? throw new ArgumentNullException(nameof(dataPorts));
            this.powerPortRepository = powerPorts ?? throw new ArgumentNullException(nameof(powerPorts));
        }

        public PortReadResult Read()
        {
            return Read(new TRUEFilterElement<IPort>());
        }

        public PortReadResult Read(FilterElement<IPort> filter)
        {
            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var domFilter = TranslateFullFilter(filter, out var hasConflictingExclusiveFields);
            if (hasConflictingExclusiveFields)
            {
                // The filter combines DataPort-exclusive and PowerPort-exclusive fields; no instance can match.
                return new PortReadResult(null);
            }

            var ports = new List<IPort>();
            foreach (var instance in helper.DomInstances.Read(domFilter))
            {
                ProcessInstance(instance, ports);
            }

            return new PortReadResult(ports);
        }

        public PortReadResult Read(IQuery<IPort> query)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var domFilter = TranslateFullFilter(query.Filter, out var hasConflictingExclusiveFields);
            if (hasConflictingExclusiveFields)
            {
                // The filter combines DataPort-exclusive and PowerPort-exclusive fields; no instance can match.
                return new PortReadResult(null);
            }

            var domOrder = TranslateFullOrderBy(query.Order);
            var domQuery = query.WithFilter(domFilter).WithOrder(domOrder);
            var ports = new List<IPort>();
            foreach (var instance in helper.DomInstances.Read(domQuery))
            {
                ProcessInstance(instance, ports);
            }

            return new PortReadResult(ports);
        }

        public IEnumerable<PortReadResult> ReadPaged(int pageSize = DefaultPageSize)
        {
            return ReadPaged(new TRUEFilterElement<IPort>(), pageSize);
        }

        public IEnumerable<PortReadResult> ReadPaged(FilterElement<IPort> filter, int pageSize = DefaultPageSize)
        {
            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "The page size must be 1 or higher");
            }

            var domFilter = TranslateFullFilter(filter, out var hasConflictingExclusiveFields);
            if (hasConflictingExclusiveFields)
            {
                // The filter combines DataPort-exclusive and PowerPort-exclusive fields; no instance can match.
                return Enumerable.Empty<PortReadResult>();
            }

            return ReadPagedInternal(domFilter, pageSize);
        }

        public IEnumerable<PortReadResult> ReadPaged(IQuery<IPort> query, int pageSize = DefaultPageSize)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "The page size must be 1 or higher");
            }

            var domFilter = TranslateFullFilter(query.Filter, out var hasConflictingExclusiveFields);
            if (hasConflictingExclusiveFields)
            {
                // The filter combines DataPort-exclusive and PowerPort-exclusive fields; no instance can match.
                return Enumerable.Empty<PortReadResult>();
            }

            var domOrder = TranslateFullOrderBy(query.Order);
            var domQuery = query.WithFilter(domFilter).WithOrder(domOrder);
            return ReadPagedInternal(domQuery, pageSize);
        }

        private IEnumerable<PortReadResult> ReadPagedInternal(FilterElement<DomInstance> domFilter, int pageSize)
        {
            var pagingHelper = helper.DomInstances.PreparePaging(domFilter, pageSize);
            var hasPage = pagingHelper.MoveToNextPage();
            var pageNumber = 0;
            while (hasPage)
            {
                var ports = new List<IPort>();
                foreach (var instance in pagingHelper.GetCurrentPage())
                {
                    ProcessInstance(instance, ports);
                }

                hasPage = pagingHelper.MoveToNextPage();
                yield return new PortReadResult(ports, pageNumber, hasPage);
                pageNumber++;
            }
        }

        private IEnumerable<PortReadResult> ReadPagedInternal(IQuery<DomInstance> domQuery, int pageSize)
        {
            var pagingHelper = helper.DomInstances.PreparePaging(domQuery, pageSize);
            var hasPage = pagingHelper.MoveToNextPage();
            var pageNumber = 0;
            while (hasPage)
            {
                var ports = new List<IPort>();
                foreach (var instance in pagingHelper.GetCurrentPage())
                {
                    ProcessInstance(instance, ports);
                }

                hasPage = pagingHelper.MoveToNextPage();
                yield return new PortReadResult(ports, pageNumber, hasPage);
                pageNumber++;
            }
        }

        private void ProcessInstance(DomInstance instance, List<IPort> ports)
        {
            if (instance.DomDefinitionId.Id == DataPortDomMapper.DomDefinitionId.Id)
            {
                ports.Add(dataPortRepository.FromDomInstance(instance));
            }
            else if (instance.DomDefinitionId.Id == PowerPortDomMapper.DomDefinitionId.Id)
            {
                ports.Add(powerPortRepository.FromDomInstance(instance));
            }
        }

        /// <summary>
        /// Translates a shared port filter into a DOM filter targeting both definitions:
        /// (Definition = DataPort AND filter translated with DataPort field descriptors)
        /// OR (Definition = PowerPort AND filter translated with PowerPort field descriptors).
        /// When the filter uses fields exclusive to one definition, the opposite definition cannot
        /// match and its branch is dropped. Returns <c>false</c> when the filter combines exclusive
        /// fields of both definitions, meaning no instance can ever match.
        /// </summary>
        private FilterElement<DomInstance> TranslateFullFilter(FilterElement<IPort> filter, out bool hasConflictingExclusiveFields)
        {
            var usesDataPortOnly = false;
            var usesPowerPortOnly = false;
            CollectExclusiveFieldUsage(filter, ref usesDataPortOnly, ref usesPowerPortOnly);

            if (usesDataPortOnly && usesPowerPortOnly)
            {
                hasConflictingExclusiveFields = true;
                return null;
            }
            hasConflictingExclusiveFields = false;

            if (usesDataPortOnly)
            {
                return DataPortBranch(filter);
            }

            if (usesPowerPortOnly)
            {
                return PowerPortBranch(filter);
            }

            return new ORFilterElement<DomInstance>(DataPortBranch(filter), PowerPortBranch(filter));
        }

        /// <summary>
        /// Translates a shared port order-by into a DOM order-by. Each element on a field whose
        /// descriptor differs between the definitions expands into two elements — the DataPort
        /// field followed by the PowerPort field — so OrderBy(PortExposers.PortInfo.PortNumber)
        /// behaves as OrderBy(DataPort.PortNumber).ThenBy(PowerPort.PortNumber). Fields sharing
        /// the same descriptor (Identifier, Asset) and definition-exclusive fields expand to a
        /// single element.
        /// </summary>
        private static IOrderBy TranslateFullOrderBy(IOrderBy order)
        {
            if (order is null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            var translatedElements = new List<IOrderByElement>();
            foreach (var orderByElement in order.Elements)
            {
                var fieldName = orderByElement.Exposer.fieldName;
                var sortOrder = orderByElement.SortOrder;
                var naturalSort = orderByElement.Options.NaturalSort;
                translatedElements.AddRange(CreateOrderByElements(fieldName, sortOrder, naturalSort));
            }

            return new OrderBy(translatedElements);
        }

        private static IEnumerable<IOrderByElement> CreateOrderByElements(string fieldName, SortOrder sortOrder, bool naturalSort)
        {
            switch (fieldName)
            {
                case "Identifier":
                    yield return OrderByElementFactory.Create(DomInstanceExposers.Id, sortOrder, naturalSort);
                    break;
                case "Asset":
                    // Both definitions share the same field descriptor for the asset reference, so one element covers both.
                    yield return OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.AssetFk.Asset), sortOrder, naturalSort);
                    break;
                case "PortInfo.Name":
                    yield return OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.DataPortInfo.Name), sortOrder, naturalSort);
                    yield return OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(PowerPortDomMapper.PowerPortInfo.Name), sortOrder, naturalSort);
                    break;
                case "PortInfo.PortNumber":
                    yield return OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.DataPortInfo.PortNumber), sortOrder, naturalSort);
                    yield return OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(PowerPortDomMapper.PowerPortInfo.PortNumber), sortOrder, naturalSort);
                    break;
                case "PortInfo.OutputType":
                    yield return OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.DataPortInfo.OutputType), sortOrder, naturalSort);
                    yield return OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(PowerPortDomMapper.PowerPortInfo.OutputType), sortOrder, naturalSort);
                    break;
                case "PortInfo.PortExposure":
                    yield return OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.DataPortInfo.PortExposure), sortOrder, naturalSort);
                    yield return OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(PowerPortDomMapper.PowerPortInfo.PortExposure), sortOrder, naturalSort);
                    break;
                case "PortInfo.Type":
                    yield return OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.DataPortInfo.Type), sortOrder, naturalSort);
                    yield return OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(PowerPortDomMapper.PowerPortInfo.PortType), sortOrder, naturalSort);
                    break;
                case "PortInfo.Label":
                    yield return OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.DataPortInfo.Label), sortOrder, naturalSort);
                    yield return OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(PowerPortDomMapper.PowerPortInfo.Label), sortOrder, naturalSort);
                    break;
                case "AddressInfo.Ipv4Address":
                    yield return OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.AddressInfo.Ipv4Address), sortOrder, naturalSort);
                    break;
                case "AddressInfo.Ipv6Address":
                    yield return OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.AddressInfo.Ipv6Address), sortOrder, naturalSort);
                    break;
                case "AddressInfo.Hostname":
                    yield return OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.AddressInfo.Hostname), sortOrder, naturalSort);
                    break;
                case "AddressInfo.DNS":
                    yield return OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.AddressInfo.DNS), sortOrder, naturalSort);
                    break;
                case "PrimaryPortRelation.IsPrimaryIpv6":
                    yield return OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.PrimaryPortRelation.IsPrimaryIpv6), sortOrder, naturalSort);
                    break;
                case "PrimaryPortRelation.IsPrimaryIpv4":
                    yield return OrderByElementFactory.Create(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.PrimaryPortRelation.IsPrimaryIpv4), sortOrder, naturalSort);
                    break;
                default:
                    throw new NotSupportedException($"The field '{fieldName}' cannot be used to order ports. Use the exposers from '{nameof(PortExposers)}'.");
            }
        }

        private FilterElement<DomInstance> DataPortBranch(FilterElement<IPort> filter)
        {
            return new ANDFilterElement<DomInstance>(
                DomInstanceExposers.DomDefinitionId.Equal(DataPortDomMapper.DomDefinitionId.Id),
                Translate(filter, CreateDataPortFilter));
        }

        private FilterElement<DomInstance> PowerPortBranch(FilterElement<IPort> filter)
        {
            return new ANDFilterElement<DomInstance>(
                DomInstanceExposers.DomDefinitionId.Equal(PowerPortDomMapper.DomDefinitionId.Id),
                Translate(filter, CreatePowerPortFilter));
        }

        private static void CollectExclusiveFieldUsage(FilterElement<IPort> filter, ref bool usesDataPortOnly, ref bool usesPowerPortOnly)
        {
            if (filter is ANDFilterElement<IPort> and)
            {
                foreach (var sub in and.subFilters)
                {
                    CollectExclusiveFieldUsage(sub, ref usesDataPortOnly, ref usesPowerPortOnly);
                }
            }
            else if (filter is ORFilterElement<IPort> or)
            {
                foreach (var sub in or.subFilters)
                {
                    CollectExclusiveFieldUsage(sub, ref usesDataPortOnly, ref usesPowerPortOnly);
                }
            }
            else if (filter is NOTFilterElement<IPort> not)
            {
                CollectExclusiveFieldUsage(not.original, ref usesDataPortOnly, ref usesPowerPortOnly);
            }
            else if (filter is ManagedFilterIdentifier managedFilter)
            {
                var fieldName = managedFilter.getFieldName().fieldName;
                if (fieldName == "Type")
                {
                    var targetsDataPort = TypeFilterTargetsDataPort(managedFilter.getComparer(), managedFilter.getValue());
                    usesDataPortOnly |= targetsDataPort;
                    usesPowerPortOnly |= !targetsDataPort;
                }
                else
                {
                    usesDataPortOnly |= DataPortOnlyFields.Contains(fieldName);
                    usesPowerPortOnly |= PowerPortOnlyFields.Contains(fieldName);
                }
            }
        }

        /// <summary>
        /// Interprets a filter on the "Type" discriminator field. Returns <c>true</c> when the
        /// filter targets the DataPort definition, <c>false</c> when it targets the PowerPort definition.
        /// </summary>
        private static bool TypeFilterTargetsDataPort(Comparer comparer, object value)
        {
            var type = value as string;
            if (type != "Data" && type != "Power")
            {
                throw new NotSupportedException($"The value '{value}' is not supported for the Type filter. Use \"Data\" or \"Power\".");
            }

            switch (comparer)
            {
                case Comparer.Equals:
                    return type == "Data";
                case Comparer.NotEquals:
                    return type != "Data";
                default:
                    throw new NotSupportedException($"The comparer '{comparer}' is not supported for the Type filter. Use Equals or NotEquals.");
            }
        }

        private static FilterElement<DomInstance> Translate(FilterElement<IPort> filter, Func<string, Comparer, object, FilterElement<DomInstance>> createFilter)
        {
            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            FilterElement<DomInstance> translated;
            if (filter is ANDFilterElement<IPort> and)
            {
                translated = new ANDFilterElement<DomInstance>(and.subFilters.Select(sub => Translate(sub, createFilter)).ToArray());
            }
            else if (filter is ORFilterElement<IPort> or)
            {
                translated = new ORFilterElement<DomInstance>(or.subFilters.Select(sub => Translate(sub, createFilter)).ToArray());
            }
            else if (filter is NOTFilterElement<IPort> not)
            {
                translated = new NOTFilterElement<DomInstance>(Translate(not.original, createFilter));
            }
            else if (filter is TRUEFilterElement<IPort>)
            {
                translated = new TRUEFilterElement<DomInstance>();
            }
            else if (filter is FALSEFilterElement<IPort>)
            {
                translated = new FALSEFilterElement<DomInstance>();
            }
            else if (filter is ManagedFilterIdentifier managedFilter)
            {
                var fieldName = managedFilter.getFieldName().fieldName;
                var comparer = managedFilter.getComparer();
                var value = managedFilter.getValue();
                translated = createFilter(fieldName, comparer, value);
            }
            else
            {
                throw new NotSupportedException($"Unsupported filter: {filter}");
            }

            return translated;
        }

        private FilterElement<DomInstance> CreateDataPortFilter(string fieldName, Comparer comparer, object value)
        {
            if (fieldName == "Type")
            {
                // The definition discriminator: within the DataPort branch the filter is already satisfied or contradicted.
                return TypeFilterTargetsDataPort(comparer, value)
                    ? (FilterElement<DomInstance>)new TRUEFilterElement<DomInstance>()
                    : new FALSEFilterElement<DomInstance>();
            }

            if(fieldName.StartsWith("PortInfo.", StringComparison.Ordinal))
            {
                fieldName = $"Data{fieldName}";
            }

            return dataPortRepository.CreatePortFilter(fieldName, comparer, value);
        }

        private FilterElement<DomInstance> CreatePowerPortFilter(string fieldName, Comparer comparer, object value)
        {
            if (fieldName == "Type")
            {
                // The definition discriminator: within the PowerPort branch the filter is already satisfied or contradicted.
                return TypeFilterTargetsDataPort(comparer, value)
                    ? (FilterElement<DomInstance>)new FALSEFilterElement<DomInstance>()
                    : new TRUEFilterElement<DomInstance>();
            }

            if (fieldName.StartsWith("PortInfo.", StringComparison.Ordinal))
            {
                fieldName = $"Power{fieldName}";
            }

            return powerPortRepository.CreatePortFilter(fieldName, comparer, value);
        }
    }
}