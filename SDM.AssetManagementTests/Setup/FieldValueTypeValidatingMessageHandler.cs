namespace SDM.AssetManagement.Tests.Setup
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
    using Skyline.DataMiner.Net.ManagerStore;
    using Skyline.DataMiner.Net.Messages;
    using Skyline.DataMiner.Net.Sections;
    using Skyline.DataMiner.Utils.DOM.UnitTesting;

    /// <summary>
    /// Decorates <see cref="DomSLNetMessageHandler"/> and validates, on every instance Create / Update / bulk
    /// CreateOrUpdate, that each written field value's CLR type is assignable to the registered
    /// <see cref="FieldDescriptor.FieldType"/>. Sections or fields without a registered definition are skipped,
    /// as are null values. See docs/specs/SPC-001-dom-mock-field-value-type-validation.md.
    /// </summary>
    internal sealed class FieldValueTypeValidatingMessageHandler
    {
        private readonly DomSLNetMessageHandler _inner;
        private readonly ConcurrentDictionary<string, IReadOnlyDictionary<SectionDefinitionID, SectionDefinition>> _cache = new();

        public FieldValueTypeValidatingMessageHandler(DomSLNetMessageHandler inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public bool TryHandleMessage(DMSMessage message, out DMSMessage response)
        {
            switch (message)
            {
                case ManagerStoreCreateRequest<DomInstance> r:
                    Validate(r.ModuleId, r.Object);
                    break;

                case ManagerStoreUpdateRequest<DomInstance> r:
                    Validate(r.ModuleId, r.Object);
                    break;

                case ManagerStoreBulkCreateOrUpdateRequest<DomInstance> r:
                    foreach (var instance in r.Objects)
                    {
                        Validate(r.ModuleId, instance);
                    }

                    break;

                case ManagerStoreCreateRequest<SectionDefinition> r:
                    _cache.TryRemove(r.ModuleId, out _);
                    break;

                case ManagerStoreUpdateRequest<SectionDefinition> r:
                    _cache.TryRemove(r.ModuleId, out _);
                    break;

                case ManagerStoreDeleteRequest<SectionDefinition> r:
                    _cache.TryRemove(r.ModuleId, out _);
                    break;
            }

            return _inner.TryHandleMessage(message, out response);
        }

        private void Validate(string moduleId, DomInstance instance)
        {
            if (instance?.Sections == null)
            {
                return;
            }

            var sectionDefinitions = GetSectionDefinitions(moduleId);

            foreach (var section in instance.Sections)
            {
                if (!sectionDefinitions.TryGetValue(section.SectionDefinitionID, out var sectionDefinition))
                {
                    continue;
                }

                foreach (var fieldValue in section.FieldValues)
                {
                    var descriptor = sectionDefinition.GetFieldDescriptorById(fieldValue.FieldDescriptorID);
                    var expected = descriptor?.FieldType;
                    var actual = fieldValue.Value?.Value;

                    if (expected == null || actual == null)
                    {
                        continue;
                    }

                    if (!expected.IsInstanceOfType(actual))
                    {
                        throw new DomFieldValueTypeMismatchException(moduleId, sectionDefinition, descriptor!, expected, actual);
                    }
                }
            }
        }

        private IReadOnlyDictionary<SectionDefinitionID, SectionDefinition> GetSectionDefinitions(string moduleId)
        {
            return _cache.GetOrAdd(moduleId, id =>
            {
                var helper = new DomHelper(_inner.HandleMessages, id);
                return helper.SectionDefinitions.ReadAll().ToDictionary(s => s.GetID(), s => s);
            });
        }
    }
}
