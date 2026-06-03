namespace Skyline.DataMiner.SDM.Extensions
{
    using System;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;

    /// <summary>
    /// Extension methods that add LessThan/GreaterThan comparison support for nullable-typed exposers.
    /// <para>
    /// The Skyline DataMiner library defines these operators only for non-nullable types; nullable
    /// value types do not satisfy the <c>IEquatable&lt;F&gt;, IComparable&lt;F&gt;</c> constraints.
    /// These extensions accept any <c>Exposer&lt;T, F?&gt;</c> where <c>F : struct</c> and
    /// satisfies those constraints, wrap it in a non-nullable exposer (preserving the field name for
    /// DOM translation), and delegate to the library's built-in generic comparison methods.
    /// </para>
    /// </summary>
    public static class NullableExposerExtensions
    {
        /// <summary>Creates a less-than filter for a nullable-typed exposer.</summary>
        public static ManagedFilter<T, F> LessThan<T, F>(
            this Exposer<T, F?> exposer, F value)
            where F : struct, IComparable<F>, IEquatable<F>
        {
            return CreateNonNullable(exposer).LessThan(value);
        }

        /// <summary>Creates a less-than-or-equal filter for a nullable-typed exposer.</summary>
        public static ManagedFilter<T, F> LessThanOrEqual<T, F>(
            this Exposer<T, F?> exposer, F value)
            where F : struct, IComparable<F>, IEquatable<F>
        {
            return CreateNonNullable(exposer).LessThanOrEqual(value);
        }

        /// <summary>Creates a greater-than filter for a nullable-typed exposer.</summary>
        public static ManagedFilter<T, F> GreaterThan<T, F>(
            this Exposer<T, F?> exposer, F value)
            where F : struct, IComparable<F>, IEquatable<F>
        {
            return CreateNonNullable(exposer).GreaterThan(value);
        }

        /// <summary>Creates a greater-than-or-equal filter for a nullable-typed exposer.</summary>
        public static ManagedFilter<T, F> GreaterThanOrEqual<T, F>(
            this Exposer<T, F?> exposer, F value)
            where F : struct, IComparable<F>, IEquatable<F>
        {
            return CreateNonNullable(exposer).GreaterThanOrEqual(value);
        }

        private static Exposer<T, F> CreateNonNullable<T, F>(Exposer<T, F?> exposer)
            where F : struct
        {
            return new Exposer<T, F>(
                a => exposer.internalFunc(a).GetValueOrDefault(),
                exposer.fieldName);
        }
    }
}
