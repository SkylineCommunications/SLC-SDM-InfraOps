namespace SDM.AssetManagement.Tests.Setup
{
    using System;

    using Skyline.DataMiner.Net.Sections;

    /// <summary>
    /// Thrown by <see cref="FieldValueTypeValidatingMessageHandler"/> when a written field value's CLR type is not
    /// assignable to the <see cref="FieldDescriptor.FieldType"/> registered for that field.
    /// Mirrors the server-side <c>DomInstanceSectionInvalidFieldValueTypes</c> error.
    /// </summary>
    internal sealed class DomFieldValueTypeMismatchException : InvalidOperationException
    {
        public DomFieldValueTypeMismatchException(string moduleId, SectionDefinition section, FieldDescriptor field, Type expected, object actual)
            : base(
                $"DomInstanceSectionInvalidFieldValueTypes: module '{moduleId}', section '{section.GetName()}' ({section.GetID().Id}), " +
                $"field '{field.Name}' ({field.ID.Id}) expects '{expected.FullName}' but got '{actual.GetType().FullName}' (value: '{actual}').")
        {
            ModuleId = moduleId;
            SectionName = section.GetName();
            FieldName = field.Name;
            ExpectedType = expected;
            ActualType = actual.GetType();
        }

        public string ModuleId { get; }

        public string SectionName { get; }

        public string FieldName { get; }

        public Type ExpectedType { get; }

        public Type ActualType { get; }
    }
}
