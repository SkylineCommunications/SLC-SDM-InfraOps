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
                return new PortReadResult(null, null);
            }

            var dataPorts = new List<DataPort>();
            var powerPorts = new List<PowerPort>();
            foreach (var instance in helper.DomInstances.Read(domFilter))
            {
                ProcessInstances(instance, dataPorts, powerPorts);
            }

            return new PortReadResult(dataPorts, powerPorts);
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

        private IEnumerable<PortReadResult> ReadPagedInternal(FilterElement<DomInstance> domFilter, int pageSize)
        {
            var pagingHelper = helper.DomInstances.PreparePaging(domFilter, pageSize);
            while (pagingHelper.MoveToNextPage())
            {
                var dataPorts = new List<DataPort>();
                var powerPorts = new List<PowerPort>();
                foreach (var instance in pagingHelper.GetCurrentPage())
                {
                    ProcessInstances(instance, dataPorts, powerPorts);
                }

                yield return new PortReadResult(dataPorts, powerPorts);
            }
        }

        private void ProcessInstances(DomInstance instance, List<DataPort> dataPorts, List<PowerPort> powerPorts)
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

        /// <summary>
        /// Translates a shared port filter into a DOM filter targeting both definitions:
        /// (Definition = DataPort AND filter translated with DataPort field descriptors)
        /// OR (Definition = PowerPort AND filter translated with PowerPort field descriptors).
        /// When the filter uses fields exclusive to one definition, the opposite definition cannot
        /// match and its branch is dropped. Returns <c>false</c> when the filter combines exclusive
        /// fields of both definitions, meaning no instance can ever match.
        /// </summary>
        private static FilterElement<DomInstance> TranslateFullFilter(FilterElement<IPort> filter, out bool hasConflictingExclusiveFields)
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

        private static FilterElement<DomInstance> DataPortBranch(FilterElement<IPort> filter)
        {
            return new ANDFilterElement<DomInstance>(
                DomInstanceExposers.DomDefinitionId.Equal(DataPortDomMapper.DomDefinitionId.Id),
                Translate(filter, CreateDataPortFilter));
        }

        private static FilterElement<DomInstance> PowerPortBranch(FilterElement<IPort> filter)
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
                usesDataPortOnly |= DataPortOnlyFields.Contains(fieldName);
                usesPowerPortOnly |= PowerPortOnlyFields.Contains(fieldName);
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

        private static FilterElement<DomInstance> CreateDataPortFilter(string fieldName, Comparer comparer, object value)
        {
            switch (fieldName)
            {
                case "Identifier":
                    return FilterElementFactory.Create<DomInstance>(DomInstanceExposers.Id, comparer, Guid.Parse((string)value));
                case "Asset":
                    return new DynamicManagedListFilter<DomInstance, object>(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.AssetFk.Asset), comparer, Guid.Parse(SdmObjectReference<Asset>.Convert(value).Identifier));
                case "PortInfo.Name":
                    return new DynamicManagedListFilter<DomInstance, object>(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.DataPortInfo.Name), comparer, (string)value);
                case "PortInfo.PortNumber" when (comparer is Comparer.Equals || comparer is Comparer.NotEquals) && value is null:
                    return DomInstanceExposers.FieldValues.KeyExists(DataPortDomMapper.DataPortInfo.PortNumber.Id.ToString()).Equal(comparer == Comparer.NotEquals);
                case "PortInfo.PortNumber":
                    return new DynamicManagedListFilter<DomInstance, object>(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.DataPortInfo.PortNumber), comparer, ((long?)value).Value);
                case "PortInfo.OutputType" when (comparer is Comparer.Equals || comparer is Comparer.NotEquals) && value is null:
                    return DomInstanceExposers.FieldValues.KeyExists(DataPortDomMapper.DataPortInfo.OutputType.Id.ToString()).Equal(comparer == Comparer.NotEquals);
                case "PortInfo.OutputType":
                    return new DynamicManagedListFilter<DomInstance, object>(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.DataPortInfo.OutputType), comparer, (int)((SharedMappers.DomIds.SlcAsset_Management.Enums.Outputtype?)value).Value);
                case "PortInfo.PortExposure":
                    return new DynamicManagedListFilter<DomInstance, object>(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.DataPortInfo.PortExposure), comparer, SharedMappers.DomIds.SlcAsset_Management.Enums.Portexposure.ToValue((SharedMappers.DomIds.SlcAsset_Management.Enums.PortExposureEnum)value));
                case "PortInfo.Type":
                    return new DynamicManagedListFilter<DomInstance, object>(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.DataPortInfo.Type), comparer, Guid.Parse(SdmObjectReference<PortType>.Convert(value).Identifier));
                case "PortInfo.Label":
                    return new DynamicManagedListFilter<DomInstance, object>(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.DataPortInfo.Label), comparer, (string)value);
                case "AddressInfo.Ipv4Address":
                    return new DynamicManagedListFilter<DomInstance, object>(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.AddressInfo.Ipv4Address), comparer, (string)value);
                case "AddressInfo.Ipv6Address":
                    return new DynamicManagedListFilter<DomInstance, object>(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.AddressInfo.Ipv6Address), comparer, (string)value);
                case "AddressInfo.Hostname":
                    return new DynamicManagedListFilter<DomInstance, object>(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.AddressInfo.Hostname), comparer, (string)value);
                case "AddressInfo.DNS":
                    return new DynamicManagedListFilter<DomInstance, object>(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.AddressInfo.DNS), comparer, (bool)value);
                case "PrimaryPortRelation.IsPrimaryIpv6":
                    return new DynamicManagedListFilter<DomInstance, object>(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.PrimaryPortRelation.IsPrimaryIpv6), comparer, (bool)value);
                case "PrimaryPortRelation.IsPrimaryIpv4":
                    return new DynamicManagedListFilter<DomInstance, object>(DomInstanceExposers.FieldValues.DomInstanceField(DataPortDomMapper.PrimaryPortRelation.IsPrimaryIpv4), comparer, (bool)value);
                default:
                    throw new NotSupportedException($"The field '{fieldName}' cannot be used to filter the DataPort definition. Use the exposers from '{nameof(PortExposers)}'.");
            }
        }

        private static FilterElement<DomInstance> CreatePowerPortFilter(string fieldName, Comparer comparer, object value)
        {
            switch (fieldName)
            {
                case "Identifier":
                    return FilterElementFactory.Create<DomInstance>(DomInstanceExposers.Id, comparer, Guid.Parse((string)value));
                case "Asset":
                    return new DynamicManagedListFilter<DomInstance, object>(DomInstanceExposers.FieldValues.DomInstanceField(PowerPortDomMapper.AssetRelationProperties.Asset), comparer, Guid.Parse(SdmObjectReference<Asset>.Convert(value).Identifier));
                case "PortInfo.Name":
                    return new DynamicManagedListFilter<DomInstance, object>(DomInstanceExposers.FieldValues.DomInstanceField(PowerPortDomMapper.PowerPortInfo.Name), comparer, (string)value);
                case "PortInfo.PortNumber" when (comparer is Comparer.Equals || comparer is Comparer.NotEquals) && value is null:
                    return DomInstanceExposers.FieldValues.KeyExists(PowerPortDomMapper.PowerPortInfo.PortNumber.Id.ToString()).Equal(comparer == Comparer.NotEquals);
                case "PortInfo.PortNumber":
                    return new DynamicManagedListFilter<DomInstance, object>(DomInstanceExposers.FieldValues.DomInstanceField(PowerPortDomMapper.PowerPortInfo.PortNumber), comparer, ((long?)value).Value);
                case "PortInfo.OutputType" when (comparer is Comparer.Equals || comparer is Comparer.NotEquals) && value is null:
                    return DomInstanceExposers.FieldValues.KeyExists(PowerPortDomMapper.PowerPortInfo.OutputType.Id.ToString()).Equal(comparer == Comparer.NotEquals);
                case "PortInfo.OutputType":
                    return new DynamicManagedListFilter<DomInstance, object>(DomInstanceExposers.FieldValues.DomInstanceField(PowerPortDomMapper.PowerPortInfo.OutputType), comparer, (int)((SharedMappers.DomIds.SlcAsset_Management.Enums.Outputtype?)value).Value);
                case "PortInfo.PortExposure":
                    return new DynamicManagedListFilter<DomInstance, object>(DomInstanceExposers.FieldValues.DomInstanceField(PowerPortDomMapper.PowerPortInfo.PortExposure), comparer, SharedMappers.DomIds.SlcAsset_Management.Enums.Portexposure.ToValue((SharedMappers.DomIds.SlcAsset_Management.Enums.PortExposureEnum)value));
                case "PortInfo.Type":
                    // The PowerPort definition stores the port type reference as a string, unlike the DataPort definition which stores it as a GUID.
                    return new DynamicManagedListFilter<DomInstance, object>(DomInstanceExposers.FieldValues.DomInstanceField(PowerPortDomMapper.PowerPortInfo.PortType), comparer, SdmObjectReference<PortType>.Convert(value).Identifier);
                case "PortInfo.Label":
                    return new DynamicManagedListFilter<DomInstance, object>(DomInstanceExposers.FieldValues.DomInstanceField(PowerPortDomMapper.PowerPortInfo.Label), comparer, (string)value);
                default:
                    throw new NotSupportedException($"The field '{fieldName}' cannot be used to filter the PowerPort definition. Use the exposers from '{nameof(PortExposers)}'.");
            }
        }
    }
}