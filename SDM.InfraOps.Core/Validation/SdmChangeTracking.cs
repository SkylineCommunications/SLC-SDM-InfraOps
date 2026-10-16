namespace Skyline.DataMiner.Utils.InfraOps.Common.Fields
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A tracked field change of an SDM object, attributed to the (nested) section it belongs to.
    /// </summary>
    public sealed class FieldChange
    {
        public FieldChange(string section, string fieldName, object oldValue, object newValue)
        {
            Section = section;
            FieldName = fieldName;
            OldValue = oldValue;
            NewValue = newValue;
        }

        /// <summary>
        /// Gets the dot separated section property path (for example "Ownership"), or null when the field is on the object itself.
        /// </summary>
        public string Section { get; }

        public string FieldName { get; }

        public object OldValue { get; }

        public object NewValue { get; }
    }

    /// <summary>
    /// A change that <see cref="SdmChangeTracking.ApplyChanges{T}"/> could not apply.
    /// </summary>
    public sealed class SkippedFieldChange
    {
        public SkippedFieldChange(FieldChange change, string reason)
        {
            Change = change;
            Reason = reason;
        }

        public FieldChange Change { get; }

        public string Reason { get; }
    }

    public sealed class ApplyChangesResult
    {
        public List<FieldChange> Applied { get; } = new List<FieldChange>();

        public List<SkippedFieldChange> Skipped { get; } = new List<SkippedFieldChange>();
    }

    /// <summary>
    /// Capture, apply and revert the pending (unsaved) field changes of SDM objects.
    /// </summary>
    /// <remarks>
    /// Typical use: capture the changes of an object, transition it (which returns a fresh object and drops
    /// the pending edits), then apply the captured changes to the new object.
    /// Changes are attributed to the section that holds the field, so equally named fields of different
    /// sections (for example Ownership.Team and Custody.Team) never collide.
    /// </remarks>
    public static class SdmChangeTracking
    {
        private const BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly HashSet<string> IdentityFields = new HashSet<string>(StringComparer.Ordinal)
        {
            "Id", "Identifier", "IsNew", "ModuleId",
        };

        /// <summary>
        /// Returns the fields that changed on <paramref name="obj"/> and on its sections since it was loaded or last reset.
        /// </summary>
        public static IReadOnlyList<FieldChange> GetFieldChanges<T>(T obj)
            where T : class
        {
            var result = new List<FieldChange>();
            if (obj != null)
            {
                Collect(obj, null, result, new HashSet<object>(ReferenceComparer.Instance));
            }

            return result;
        }

        /// <summary>
        /// Writes the changes to <paramref name="target"/> through its public setters, so they are tracked as normal edits.
        /// </summary>
        /// <returns>The changes that were applied and the ones that were skipped, with the reason.</returns>
        public static ApplyChangesResult ApplyFieldChanges<T>(T target, IEnumerable<FieldChange> changes)
            where T : class
        {
            var result = new ApplyChangesResult();
            if (changes == null)
            {
                return result;
            }

            foreach (var change in changes.Where(c => c != null))
            {
                var reason = TryApply(target, change);
                if (reason == null)
                {
                    result.Applied.Add(change);
                }
                else
                {
                    result.Skipped.Add(new SkippedFieldChange(change, reason));
                }
            }

            return result;
        }

        /// <summary>
        /// Reverts every pending change on <paramref name="obj"/> and its sections to the original value.
        /// Unlike <see cref="IChangeTracking.ResetChangeTracking"/>, this restores the values instead of accepting them.
        /// </summary>
        /// <returns>The changes that were reverted.</returns>
        public static IReadOnlyList<FieldChange> RevertFieldChanges<T>(T obj)
            where T : class
        {
            var reverted = GetFieldChanges(obj);
            foreach (var change in reverted)
            {
                var owner = ResolveSection(obj, change.Section);
                var field = owner == null ? null : FindField(owner, change.FieldName);
                if (field is IChangeTrackingField trackingField)
                {
                    trackingField.Reset();
                }
            }

            return reverted;
        }

        private static void Collect(object owner, string path, List<FieldChange> result, HashSet<object> visited)
        {
            if (!visited.Add(owner))
            {
                return;
            }

            var handler = GetHandler(owner);
            if (handler != null)
            {
                foreach (var entry in handler.GetChanges())
                {
                    if (!IdentityFields.Contains(entry.Key))
                    {
                        result.Add(new FieldChange(path, entry.Key, entry.Value.prevVal, entry.Value.newVal));
                    }
                }
            }

            foreach (var section in GetSections(owner))
            {
                Collect(section.Value, path == null ? section.Key : path + "." + section.Key, result, visited);
            }
        }

        private static string TryApply(object target, FieldChange change)
        {
            if (target == null)
            {
                return "Target is null.";
            }

            if (IdentityFields.Contains(change.FieldName))
            {
                return "Identity field.";
            }

            var owner = ResolveSection(target, change.Section);
            if (owner == null)
            {
                return $"Section '{change.Section}' not found on target.";
            }

            var property = owner.GetType().GetProperty(change.FieldName, BindingFlags.Instance | BindingFlags.Public);
            if (property == null)
            {
                return "Field not found on target.";
            }

            if (property.GetSetMethod() == null)
            {
                return "Field is read-only.";
            }

            var value = change.NewValue;
            if (value == null)
            {
                if (property.PropertyType.IsValueType && Nullable.GetUnderlyingType(property.PropertyType) == null)
                {
                    return "Null is not assignable to the field type.";
                }
            }
            else if (!property.PropertyType.IsInstanceOfType(value))
            {
                // Captured values can be the field's stored form (for example a Guid for a reference field).
                if (!TryConvert(value, property.PropertyType, out value))
                {
                    return $"Value of type {change.NewValue.GetType().Name} is not assignable to {property.PropertyType.Name}.";
                }
            }

            property.SetValue(owner, value, null);
            return null;
        }

        private static bool TryConvert(object value, Type targetType, out object converted)
        {
            converted = null;
            var valueType = value.GetType();
            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (underlying.IsEnum && value is string text && Enum.IsDefined(underlying, text))
            {
                converted = Enum.Parse(underlying, text);
                return true;
            }

            foreach (var method in underlying.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                if ((method.Name != "op_Explicit" && method.Name != "op_Implicit") || method.ReturnType != underlying)
                {
                    continue;
                }

                var parameters = method.GetParameters();
                if (parameters.Length == 1 && parameters[0].ParameterType == valueType)
                {
                    converted = method.Invoke(null, new[] { value });
                    return true;
                }
            }

            return false;
        }

        private static object ResolveSection(object root, string path)
        {
            var current = root;
            if (string.IsNullOrEmpty(path))
            {
                return current;
            }

            foreach (var part in path.Split('.'))
            {
                var property = current.GetType().GetProperty(part, BindingFlags.Instance | BindingFlags.Public);
                current = property == null || property.GetIndexParameters().Length > 0 ? null : property.GetValue(current, null);
                if (current == null)
                {
                    return null;
                }
            }

            return current;
        }

        private static IEnumerable<KeyValuePair<string, object>> GetSections(object owner)
        {
            foreach (var property in owner.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (property.GetIndexParameters().Length == 0 && typeof(ChangeTrackingBase).IsAssignableFrom(property.PropertyType))
                {
                    var value = property.GetValue(owner, null);
                    if (value != null)
                    {
                        yield return new KeyValuePair<string, object>(property.Name, value);
                    }
                }
            }
        }

        // The root models declare their own private handler, the sections inherit a private protected one,
        // so the handler can only be reached by reflection.
        private static ChangeTrackingFieldHandler GetHandler(object owner)
        {
            for (var type = owner.GetType(); type != null; type = type.BaseType)
            {
                var property = type.GetProperty("FieldHandler", AnyInstance | BindingFlags.DeclaredOnly);
                if (property != null && typeof(ChangeTrackingFieldHandler).IsAssignableFrom(property.PropertyType))
                {
                    return (ChangeTrackingFieldHandler)property.GetValue(owner, null);
                }
            }

            return null;
        }

        private static object FindField(object owner, string fieldName)
        {
            var handler = GetHandler(owner);
            return handler?.TryGetField(fieldName);
        }

        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceComparer Instance = new ReferenceComparer();

            bool IEqualityComparer<object>.Equals(object x, object y) => ReferenceEquals(x, y);

            int IEqualityComparer<object>.GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}
