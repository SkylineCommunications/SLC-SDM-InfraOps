namespace Skyline.DataMiner.SDM.AssetManagement.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net;
    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.Helper;
    using Skyline.DataMiner.Net.ManagerStore;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.Net.Sections;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.Utils.InfraOps.Common.Fields;

    using SLDataGateway.API.Querying;
    using SLDataGateway.API.Types.Querying;

    internal partial class HistoryDomRepository : IBulkRepository<History>
    {
        private readonly DomHelper _helper;

        public HistoryDomRepository(IConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            _helper = new DomHelper(connection.HandleMessages, HistoryDomMapper.ModuleId);
        }

        public History Create(History createObject)
        {
            if (createObject == null)
            {
                throw new ArgumentNullException(nameof(createObject));
            }

            var instance = ToInstance(createObject);
            instance = _helper.DomInstances.Create(instance);
            return FromInstance(instance);
        }

        public IReadOnlyCollection<History> Create(IEnumerable<History> createObjects)
        {
            if (createObjects == null)
            {
                throw new ArgumentNullException(nameof(createObjects));
            }

            // Check if some of the objects already exist
            var existing = new HashSet<string>();
            foreach (var batch in createObjects.Batch(500))
            {
                existing.UnionWith(Read(new ORFilterElement<History>(batch.Where(obj => !String.IsNullOrWhiteSpace(obj.Identifier)).Select(obj => HistoryExposers.Identifier.Equal(obj.Identifier)).ToArray())).Select(obj => obj.Identifier));
            }

            // Create the remainder
            var SuccessfulItems = new List<History>();
            var failures = new Dictionary<string, Exception>();

            foreach (var batch in createObjects.Select(ToInstance).Batch(_helper.DomInstances.MaxAmountBulkOperation))
            {
                _helper.DomInstances.TryCreateOrUpdate(batch.ToList(), out var result);
                foreach (var failure in result.UnsuccessfulIds)
                {
                    failures.Add(failure.Id.ToString(), new CrudFailedException(result.TraceDataPerItem[failure]));
                }

                foreach (var success in result.SuccessfulItems)
                {
                    SuccessfulItems.Add(FromInstance(success));
                }
            }

            // If everything went fine, return the successful creations
            if (!existing.Any() && !failures.Any())
            {
                return SuccessfulItems;
            }

            // Otherwise, build and throw an exception
            var exceptionBuilder = new SdmBulkCrudException<History>.Builder();
            foreach (var obj in createObjects)
            {
                if (existing.Contains(obj.Identifier))
                {
                    exceptionBuilder.AddFailed(obj, new SdmCrudException<History>(obj, $"Could not create History with guid: '{obj.Identifier}', it already exists."));
                    continue;
                }

                if (failures.ContainsKey(obj.Identifier))
                {
                    exceptionBuilder.AddFailed(obj, failures[obj.Identifier]);
                    continue;
                }

                exceptionBuilder.AddSuccessful(obj);
            }

            throw exceptionBuilder.Build();
        }

        public IReadOnlyCollection<History> CreateOrUpdate(IEnumerable<History> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            var successful = new List<History>();
            var exceptionBuilder = new SdmBulkCrudException<History>.Builder();

            // Convert to instances and build mapping using the RESULT identifier
            var instancesWithOriginals = items
                .Select(obj =>
                {
                    var instance = ToInstance(obj);
                    return new { Original = obj, Instance = instance, Key = instance.ID.Id.ToString() };
                })
                .ToList();

            var objects = instancesWithOriginals.ToDictionary(x => x.Key, x => x.Original);

            foreach (var batch in instancesWithOriginals.Select(x => x.Instance).Batch(_helper.DomInstances.MaxAmountBulkOperation))
            {
                _helper.DomInstances.TryCreateOrUpdate(batch.ToList(), out var result);

                foreach (var failure in result.UnsuccessfulIds)
                {
                    exceptionBuilder.AddFailed(objects[failure.Id.ToString()], new CrudFailedException(result.TraceDataPerItem[failure]));
                }

                foreach (var success in result.SuccessfulItems)
                {
                    var item = FromInstance(success);
                    exceptionBuilder.AddSuccessful(item);
                    successful.Add(item);
                }
            }

            if (exceptionBuilder.HasFailure)
            {
                throw exceptionBuilder.Build();
            }

            return successful;
        }

        public long Count(FilterElement<History> filter)
        {
            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var domFilter = TranslateFullFilter(filter);
            domFilter = domFilter.AND(DomInstanceExposers.DomDefinitionId.Equal(AssetManagement.Models.HistoryDomMapper.DomDefinitionId.Id));
            return _helper.DomInstances.Count(domFilter);
        }

        public long Count(IQuery<History> query)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var domFilter = TranslateFullFilter(query.Filter);
            domFilter = domFilter.AND(DomInstanceExposers.DomDefinitionId.Equal(AssetManagement.Models.HistoryDomMapper.DomDefinitionId.Id));
            var domOrder = TranslateFullOrderBy(query.Order);
            var domQuery = query.WithFilter(domFilter).WithOrder(domOrder);
            return _helper.DomInstances.Count(domQuery);
        }

        public IEnumerable<History> Read(FilterElement<History> filter)
        {
            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            var domFilter = TranslateFullFilter(filter);
            return Read(domFilter);
        }

        public IEnumerable<History> Read(IQuery<History> query)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            var domFilter = TranslateFullFilter(query.Filter);
            var domOrder = TranslateFullOrderBy(query.Order);
            var domQuery = query.WithFilter(domFilter).WithOrder(domOrder);
            return Read(domQuery);
        }

        public IEnumerable<IPagedResult<History>> ReadPaged(FilterElement<History> filter)
        {
            return ReadPaged(filter, 500);
        }

        public IEnumerable<IPagedResult<History>> ReadPaged(FilterElement<History> filter, int pageSize)
        {
            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "The page size must be 1 or higher");
            }

            var domFilter = TranslateFullFilter(filter);
            var paging = ReadPaged(domFilter, pageSize).GetEnumerator();
            var moveNext = paging.MoveNext();
            var i = 0;
            while (moveNext)
            {
                var page = paging.Current.ToList();
                moveNext = paging.MoveNext();
                var result = new PagedResult<History>(page, i, pageSize, moveNext);
                yield return result;
                i++;
            }
        }

        public IEnumerable<IPagedResult<History>> ReadPaged(IQuery<History> query)
        {
            return ReadPaged(query, 500);
        }

        public IEnumerable<IPagedResult<History>> ReadPaged(IQuery<History> query, int pageSize)
        {
            if (query is null)
            {
                throw new ArgumentNullException(nameof(query));
            }

            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), "The page size must be 1 or higher");
            }

            var domFilter = TranslateFullFilter(query.Filter);
            var domOrder = TranslateFullOrderBy(query.Order);
            var domQuery = query.WithFilter(domFilter).WithOrder(domOrder);
            var paging = ReadPaged(domQuery, pageSize).GetEnumerator();
            var moveNext = paging.MoveNext();
            var i = 0;
            while (moveNext)
            {
                var page = paging.Current.ToList();
                moveNext = paging.MoveNext();
                var result = new PagedResult<History>(page, i, pageSize, moveNext);
                yield return result;
                i++;
            }
        }


        public History Update(History updateObject)
        {
            if (updateObject is null)
            {
                throw new ArgumentNullException(nameof(updateObject));
            }

            var instance = ToInstance(updateObject);
            instance = _helper.DomInstances.Update(instance);
            return FromInstance(instance);
        }

        public IReadOnlyCollection<History> Update(IEnumerable<History> updateObjects)
        {
            if (updateObjects is null || !updateObjects.Any())
            {
                return Array.Empty<History>();
            }

            // Check if which objects already exist
            var existing = new HashSet<string>();
            foreach (var batch in updateObjects.Batch(500))
            {
                existing.UnionWith(Read(new ORFilterElement<History>(batch.Select(obj => HistoryExposers.Identifier.Equal(obj.Identifier)).ToArray())).Select(obj => obj.Identifier));
            }

            // Update the existing objects
            var successfulItems = new List<History>();
            var failures = new Dictionary<string, Exception>();
            var objects = updateObjects.Where(obj => existing.Contains(obj.Identifier)).ToDictionary(obj => obj.Identifier);
            foreach (var batch in updateObjects.Select(ToInstance).Batch(_helper.DomInstances.MaxAmountBulkOperation))
            {
                _helper.DomInstances.TryCreateOrUpdate(batch.ToList(), out var result);
                foreach (var failure in result.UnsuccessfulIds)
                {
                    failures.Add(failure.Id.ToString(), new CrudFailedException(result.TraceDataPerItem[failure]));
                }

                foreach (var success in result.SuccessfulItems)
                {
                    successfulItems.Add(FromInstance(success));
                }
            }

            // Check for failures and build exception if needed
            var exceptionBuilder = new SdmBulkCrudException<History>.Builder();
            foreach (var obj in updateObjects)
            {
                if (!existing.Contains(obj.Identifier))
                {
                    exceptionBuilder.AddFailed(obj, new SdmCrudException<History>(obj, "Could not update a non existing History"));
                    continue;
                }

                if (failures.ContainsKey(obj.Identifier))
                {
                    exceptionBuilder.AddFailed(obj, failures[obj.Identifier]);
                    continue;
                }

                exceptionBuilder.AddSuccessful(obj);
            }

            if (exceptionBuilder.HasFailure)
            {
                throw exceptionBuilder.Build();
            }

            return successfulItems;
        }

        public void Delete(History deleteObject)
        {
            if (deleteObject is null)
            {
                throw new ArgumentNullException(nameof(deleteObject));
            }

            var instance = ToInstance(deleteObject);
            _helper.DomInstances.Delete(instance);
        }

        public void Delete(IEnumerable<History> deleteObjects)
        {
            if (deleteObjects is null || !deleteObjects.Any())
            {
                return;
            }

            var exceptionBuilder = new SdmBulkCrudException<History>.Builder();
            var objects = deleteObjects.ToDictionary(obj => obj.Identifier);
            foreach (var batch in deleteObjects.Select(ToInstance).Batch(_helper.DomInstances.MaxAmountBulkOperation))
            {
                _helper.DomInstances.TryDelete(batch.ToList(), out var result);
                foreach (var failure in result.UnsuccessfulIds)
                {
                    exceptionBuilder.AddFailed(objects[failure.Id.ToString()], new CrudFailedException(result.TraceDataPerItem[failure]));
                }

                foreach (var success in result.SuccessfulIds)
                {
                    exceptionBuilder.AddSuccessful(objects[success.Id.ToString()]);
                }
            }

            if (exceptionBuilder.HasFailure)
            {
                throw exceptionBuilder.Build();
            }
        }

        private IEnumerable<History> Read(FilterElement<DomInstance> domFilter)
        {
            if (domFilter is null)
            {
                throw new ArgumentNullException(nameof(domFilter));
            }

            domFilter = domFilter.AND(DomInstanceExposers.DomDefinitionId.Equal(AssetManagement.Models.HistoryDomMapper.DomDefinitionId.Id));
            var domInstances = _helper.DomInstances.Read(domFilter);
            return domInstances.Select(FromInstance);
        }

        private IEnumerable<History> Read(IQuery<DomInstance> domQuery)
        {
            if (domQuery is null)
            {
                throw new ArgumentNullException(nameof(domQuery));
            }

            var domFilter = domQuery.Filter.AND(DomInstanceExposers.DomDefinitionId.Equal(AssetManagement.Models.HistoryDomMapper.DomDefinitionId.Id));
            domQuery = domQuery.WithFilter(domFilter);
            var domInstances = _helper.DomInstances.Read(domQuery);
            return domInstances.Select(FromInstance);
        }

        private IEnumerable<IEnumerable<History>> ReadPaged(FilterElement<DomInstance> domFilter, int pageSize)
        {
            if (domFilter is null)
            {
                throw new ArgumentNullException(nameof(domFilter));
            }

            domFilter = domFilter.AND(DomInstanceExposers.DomDefinitionId.Equal(AssetManagement.Models.HistoryDomMapper.DomDefinitionId.Id));
            var pagingHelper = _helper.DomInstances.PreparePaging(domFilter, pageSize);
            while (pagingHelper.MoveToNextPage())
            {
                yield return pagingHelper.GetCurrentPage().Select(FromInstance);
            }
        }

        private IEnumerable<IEnumerable<History>> ReadPaged(IQuery<DomInstance> domQuery, int pageSize)
        {
            if (domQuery is null)
            {
                throw new ArgumentNullException(nameof(domQuery));
            }

            var domFilter = domQuery.Filter.AND(DomInstanceExposers.DomDefinitionId.Equal(AssetManagement.Models.HistoryDomMapper.DomDefinitionId.Id));
            domQuery = domQuery.WithFilter(domFilter);
            var pagingHelper = _helper.DomInstances.PreparePaging(domQuery, pageSize);
            while (pagingHelper.MoveToNextPage())
            {
                yield return pagingHelper.GetCurrentPage().Select(FromInstance);
            }
        }

        private FilterElement<DomInstance> TranslateFullFilter(FilterElement<History> filter)
        {
            if (filter is null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            FilterElement<DomInstance> translated;
            if (filter is ANDFilterElement<History> and)
            {
                translated = new ANDFilterElement<DomInstance>(and.subFilters.Select(TranslateFullFilter).ToArray());
            }
            else if (filter is ORFilterElement<History> or)
            {
                translated = new ORFilterElement<DomInstance>(or.subFilters.Select(TranslateFullFilter).ToArray());
            }
            else if (filter is NOTFilterElement<History> not)
            {
                translated = new NOTFilterElement<DomInstance>(TranslateFullFilter(not));
            }
            else if (filter is TRUEFilterElement<History>)
            {
                translated = new TRUEFilterElement<DomInstance>();
            }
            else if (filter is FALSEFilterElement<History>)
            {
                translated = new FALSEFilterElement<DomInstance>();
            }
            else if (filter is ManagedFilterIdentifier managedFilter)
            {
                translated = TranslateFilter(managedFilter);
            }
            else
            {
                throw new NotSupportedException($"Unsupported filter: {filter}");
            }

            return translated;
        }

        private IOrderBy TranslateFullOrderBy(IOrderBy order)
        {
            if (order is null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            var translatedElements = new List<IOrderByElement>();
            foreach (var orderByElement in order.Elements)
            {
                var translated = TranslateOrderBy(orderByElement);
                translatedElements.Add(translated);
            }

            return new OrderBy(translatedElements);
        }

        private FilterElement<DomInstance> TranslateFilter(ManagedFilterIdentifier managedFilter)
        {
            if (managedFilter is null)
            {
                throw new ArgumentNullException(nameof(managedFilter));
            }

            var fieldName = managedFilter.getFieldName().fieldName;
            var comparer = managedFilter.getComparer();
            var value = managedFilter.getValue();
            var translated = CreateFilter(fieldName, comparer, value);
            return translated;
        }

        private IOrderByElement TranslateOrderBy(IOrderByElement orderByElement)
        {
            if (orderByElement is null)
            {
                throw new ArgumentNullException(nameof(orderByElement));
            }

            var fieldName = orderByElement.Exposer.fieldName;
            var sortOrder = orderByElement.SortOrder;
            var naturalSort = orderByElement.Options.NaturalSort;
            var translated = CreateOrderBy(fieldName, sortOrder, naturalSort);
            return translated;
        }

        private static History FromInstance(DomInstance instance)
        {
            var obj = new History
            {
                Identifier = instance.ID.Id.ToString(),
                IsNewInternal = false,
            };

            var _historyInfoSection = instance.Sections.FirstOrDefault(s => s.SectionDefinitionID.Equals(AssetManagement.Models.HistoryDomMapper.HistoryInfo.SectionDefinitionId));
            if (_historyInfoSection != default)
            {
                obj.HistoryInfoSectionId = _historyInfoSection.ID.Id;
                ((ISectionTrackable)obj.HistoryInfo).SectionId = _historyInfoSection.ID.Id;
                var _description = _historyInfoSection.GetValue<string>(AssetManagement.Models.HistoryDomMapper.HistoryInfo.Description);
                if (_description != null)
                {
                    obj.HistoryInfo.Description = _description.Value;
                }


                var _job = _historyInfoSection.GetValue<Guid>(AssetManagement.Models.HistoryDomMapper.HistoryInfo.Job);
                if (_job != null)
                {
                    obj.HistoryInfo.Job = _job.Value;
                }

                var _userId = _historyInfoSection.GetValue<Guid>(AssetManagement.Models.HistoryDomMapper.HistoryInfo.UserID);
                if (_userId != null)
                {
                    obj.HistoryInfo.UserID = _userId.Value;
                }

                var _modifiedInstanceId = _historyInfoSection.GetValue<string>(AssetManagement.Models.HistoryDomMapper.HistoryInfo.ModifiedInstanceId);
                if (_modifiedInstanceId != null)
                {
                    obj.HistoryInfo.ModifiedInstanceID = _modifiedInstanceId.Value;
                }

                var _modifiedInstanceDefinitionId = _historyInfoSection.GetValue<string>(AssetManagement.Models.HistoryDomMapper.HistoryInfo.ModifiedInstanceDefinitionId);
                if (_modifiedInstanceDefinitionId != null)
                {
                    obj.HistoryInfo.ModifiedInstanceDefinitionID = _modifiedInstanceDefinitionId.Value;
                }

                var _extraInfo = _historyInfoSection.GetValue<string>(AssetManagement.Models.HistoryDomMapper.HistoryInfo.ExtraInfo);
                if (_extraInfo != null)
                {
                    obj.HistoryInfo.ExtraInfo = _extraInfo.Value;
                }

                var historyType = _historyInfoSection.GetValue<string>(HistoryDomMapper.HistoryInfo.TypeOfHistory);
                if (historyType != null)
                {
                    obj.HistoryInfo.TypeOfHistory = SlcAsset_Management.Enums.Typeofhistory.ToEnum(historyType.Value);
                }
            }

            obj.ResetChangeTracking();

            return obj;
        }

        private static DomInstance ToInstance(History obj)
        {
            //TODO: Reevaluate if we should use is New when Identifier can be changed externally.
            var id = String.IsNullOrWhiteSpace(obj.Identifier)
                ? Guid.NewGuid()
                : Guid.Parse(obj.Identifier);
            var instance = new DomInstance
            {
                DomDefinitionId = HistoryDomMapper.DomDefinitionId,
                ID = new DomInstanceId(id) { ModuleId = HistoryDomMapper.ModuleId },
            };

            if (!obj.HistoryInfo.IsEmpty)
            {
                var info = obj.HistoryInfo;
                var section = new Section(HistoryDomMapper.HistoryInfo.SectionDefinitionId);
                var sectionId = ((ISectionTrackable)info).SectionId;
                if (sectionId.HasValue)
                {
                    section.ID = new SectionID(sectionId.Value);
                }

                if (info.Description != default)
                {
                    section.AddOrUpdateValue<string>(HistoryDomMapper.HistoryInfo.Description, Convert.ToString(info.Description));
                }

                if (info.Job != Guid.Empty)
                {
                    section.AddOrUpdateValue<Guid>(HistoryDomMapper.HistoryInfo.Job, info.Job);
                }

                if (info.UserID != Guid.Empty)
                {
                    section.AddOrUpdateValue<Guid>(HistoryDomMapper.HistoryInfo.UserID, info.UserID);
                }

                if (info.ModifiedInstanceID != default)
                {
                    section.AddOrUpdateValue<string>(HistoryDomMapper.HistoryInfo.ModifiedInstanceId, Convert.ToString(info.ModifiedInstanceID));
                }

                if (info.ModifiedInstanceDefinitionID != default)
                {
                    section.AddOrUpdateValue<string>(HistoryDomMapper.HistoryInfo.ModifiedInstanceDefinitionId, Convert.ToString(info.ModifiedInstanceDefinitionID));
                }

                if (info.ExtraInfo != default)
                {
                    section.AddOrUpdateValue<string>(HistoryDomMapper.HistoryInfo.ExtraInfo, Convert.ToString(info.ExtraInfo));
                }

                if (info.TypeOfHistory.HasValue)
                {
                    section.AddOrUpdateValue<string>(HistoryDomMapper.HistoryInfo.TypeOfHistory, SlcAsset_Management.Enums.Typeofhistory.ToValue(info.TypeOfHistory.Value));
                }

                instance.Sections.Add(section);
            }

            return instance;
        }

        private static FilterElement<DomInstance> CreateFilter(string fieldName, Comparer comparer, object value)
        {
            switch (fieldName)
            {
                case "Identifier":
                    return FilterElementFactory.Create<DomInstance>(DomInstanceExposers.Id, comparer, Guid.Parse((string)value));
                case "HistoryInfo.Description":
                    return FieldFilter(HistoryDomMapper.HistoryInfo.Description, comparer, value);
                case "HistoryInfo.Job":
                    return FieldFilter(HistoryDomMapper.HistoryInfo.Job, comparer, value);
                case "HistoryInfo.UserID":
                    return FieldFilter(HistoryDomMapper.HistoryInfo.UserID, comparer, value);
                case "HistoryInfo.ModifiedInstanceID":
                    return FieldFilter(HistoryDomMapper.HistoryInfo.ModifiedInstanceId, comparer, value);
                case "HistoryInfo.ModifiedInstanceDefinitionID":
                    return FieldFilter(HistoryDomMapper.HistoryInfo.ModifiedInstanceDefinitionId, comparer, value);
                case "HistoryInfo.ExtraInfo":
                    return FieldFilter(HistoryDomMapper.HistoryInfo.ExtraInfo, comparer, value);
                case "HistoryInfo.TypeOfHistory":
                    return FieldFilter(
                        HistoryDomMapper.HistoryInfo.TypeOfHistory,
                        comparer,
                        value == null ? null : SlcAsset_Management.Enums.Typeofhistory.ToValue((SlcAsset_Management.Enums.TypeOfHistoryEnum)value));
                default:
                    throw new NotImplementedException($"Filtering by '{fieldName}' is not supported.");
            }
        }

        private static DynamicManagedListFilter<DomInstance, object> FieldFilter(
            FieldDescriptorID field,
            Comparer comparer,
            object value)
        {
            return new DynamicManagedListFilter<DomInstance, object>(
                DomInstanceExposers.FieldValues.DomInstanceField(field),
                comparer,
                value);
        }

        private static IOrderByElement CreateOrderBy(string fieldName, SortOrder sortOrder, bool naturalSort)
        {
            switch (fieldName)
            {
                case "Identifier":
                    return OrderByElementFactory.Create(DomInstanceExposers.Id, sortOrder, naturalSort);
                case "HistoryInfo.Description":
                    return FieldOrder(HistoryDomMapper.HistoryInfo.Description, sortOrder, naturalSort);
                case "HistoryInfo.Job":
                    return FieldOrder(HistoryDomMapper.HistoryInfo.Job, sortOrder, naturalSort);
                case "HistoryInfo.UserID":
                    return FieldOrder(HistoryDomMapper.HistoryInfo.UserID, sortOrder, naturalSort);
                case "HistoryInfo.ModifiedInstanceID":
                    return FieldOrder(HistoryDomMapper.HistoryInfo.ModifiedInstanceId, sortOrder, naturalSort);
                case "HistoryInfo.ModifiedInstanceDefinitionID":
                    return FieldOrder(HistoryDomMapper.HistoryInfo.ModifiedInstanceDefinitionId, sortOrder, naturalSort);
                case "HistoryInfo.ExtraInfo":
                    return FieldOrder(HistoryDomMapper.HistoryInfo.ExtraInfo, sortOrder, naturalSort);
                case "HistoryInfo.TypeOfHistory":
                    return FieldOrder(HistoryDomMapper.HistoryInfo.TypeOfHistory, sortOrder, naturalSort);
                default:
                    throw new NotImplementedException($"Ordering by '{fieldName}' is not supported.");
            }
        }

        private static IOrderByElement FieldOrder(FieldDescriptorID field, SortOrder sortOrder, bool naturalSort)
        {
            return OrderByElementFactory.Create(
                DomInstanceExposers.FieldValues.DomInstanceField(field),
                sortOrder,
                naturalSort);
        }
    }
}
