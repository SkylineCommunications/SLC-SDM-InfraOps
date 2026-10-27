namespace Skyline.DataMiner.SDM.InfraOps.Core.Models
{
    using System;
    using System.Collections.Concurrent;
    using System.Reflection;

    /// <summary>
    /// Declares the InfraOps Properties scope used by an SDM object type when looking up
    /// Properties and PropertyValues.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class PropertyScopeAttribute : Attribute
    {
        private static readonly ConcurrentDictionary<Type, string> ScopeCache = new ConcurrentDictionary<Type, string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyScopeAttribute"/> class.
        /// </summary>
        /// <param name="scope">The Properties scope name.</param>
        /// <exception cref="ArgumentException"><paramref name="scope"/> is null, empty or whitespace.</exception>
        public PropertyScopeAttribute(string scope)
        {
            if (string.IsNullOrWhiteSpace(scope))
            {
                throw new ArgumentException("The scope has to be defined", nameof(scope));
            }

            Scope = scope;
        }

        /// <summary>
        /// Gets the Properties scope name.
        /// </summary>
        public string Scope { get; }

        /// <summary>
        /// Gets the Properties scope declared on <typeparamref name="T"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException"><typeparamref name="T"/> has no <see cref="PropertyScopeAttribute"/>.</exception>
        public static string GetScope<T>()
        {
            return GetScope(typeof(T));
        }

        /// <summary>
        /// Gets the Properties scope declared on <paramref name="type"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="type"/> has no <see cref="PropertyScopeAttribute"/>.</exception>
        public static string GetScope(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (ScopeCache.TryGetValue(type, out var cached))
            {
                return cached;
            }

            var attribute = type.GetCustomAttribute<PropertyScopeAttribute>(inherit: false);
            if (attribute == null)
            {
                throw new InvalidOperationException($"Type '{type.FullName}' is unable to determine its Properties scope.");
            }

            ScopeCache[type] = attribute.Scope;
            return attribute.Scope;
        }
    }
}
