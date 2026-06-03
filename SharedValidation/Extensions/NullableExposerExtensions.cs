namespace Skyline.DataMiner.SDM.Extensions
{
    using System;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;

    /// <summary>
    /// Extension methods that add LessThan/GreaterThan comparison support for nullable-typed exposers.
    /// <para>
    /// The Skyline DataMiner library only defines these comparison operators for non-nullable types.
    /// These extensions wrap a nullable exposer in a non-nullable one (preserving the field name for
    /// DOM translation) and delegate to the library's built-in comparison methods.
    /// </para>
    /// </summary>
    public static class NullableExposerExtensions
    {
        /// <summary>Creates a less-than filter for a nullable DateTime exposer.</summary>
        public static ManagedFilter<T, DateTime> LessThan<T>(
            this Exposer<T, DateTime?> exposer, DateTime value)
        {
            return CreateNonNullable(exposer).LessThan(value);
        }

        /// <summary>Creates a less-than-or-equal filter for a nullable DateTime exposer.</summary>
        public static ManagedFilter<T, DateTime> LessThanOrEqual<T>(
            this Exposer<T, DateTime?> exposer, DateTime value)
        {
            return CreateNonNullable(exposer).LessThanOrEqual(value);
        }

        /// <summary>Creates a greater-than filter for a nullable DateTime exposer.</summary>
        public static ManagedFilter<T, DateTime> GreaterThan<T>(
            this Exposer<T, DateTime?> exposer, DateTime value)
        {
            return CreateNonNullable(exposer).GreaterThan(value);
        }

        /// <summary>Creates a greater-than-or-equal filter for a nullable DateTime exposer.</summary>
        public static ManagedFilter<T, DateTime> GreaterThanOrEqual<T>(
            this Exposer<T, DateTime?> exposer, DateTime value)
        {
            return CreateNonNullable(exposer).GreaterThanOrEqual(value);
        }

        private static Exposer<T, DateTime> CreateNonNullable<T>(Exposer<T, DateTime?> exposer)
        {
            return new Exposer<T, DateTime>(
                a => exposer.internalFunc(a).GetValueOrDefault(),
                exposer.fieldName);
        }
    }
}
