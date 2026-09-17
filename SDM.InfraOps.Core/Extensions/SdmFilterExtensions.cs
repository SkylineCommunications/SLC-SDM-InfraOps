namespace Skyline.DataMiner.SDM.Extensions
{
    using System;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;

    public static class SdmFilterExtensions
    {
        /// <summary>
        /// Creates a filter that checks if the exposed nullable field equals the specified value.
        /// A null field does not satisfy this filter.
        /// </summary>
        /// <typeparam name="TFilter">The type of the filter.</typeparam>
        /// <typeparam name="TField">The non-nullable value type being compared. Must be a struct.</typeparam>
        /// <param name="exposer">The exposer that identifies the nullable field to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <returns>
        /// A <see cref="ManagedFilter{TFilter, TField}"/> that matches when the field has a value equal to <paramref name="value"/>.
        /// </returns>
        
        public static ManagedFilter<TFilter, TField?> Equal<TFilter, TField>(
            this Exposer<TFilter, TField?> exposer,
            TField value)
            where TFilter : class
            where TField : struct, Enum
        {
            return new ManagedFilter<TFilter, TField?>(exposer, Comparer.Equals, value, delegate (TFilter obj)
            {
                TField? val = exposer.internalFunc(obj);
                return val.HasValue && val.GetValueOrDefault().Equals(value);
            });
        }

        /// <summary>
        /// Creates a filter that checks if the exposed nullable field does not equal the specified value.
        /// A null field does not satisfy this filter.
        /// </summary>
        /// <typeparam name="TFilter">The type of the filter.</typeparam>
        /// <typeparam name="TField">The non-nullable value type being compared. Must be a struct.</typeparam>
        /// <param name="exposer">The exposer that identifies the nullable field to filter on.</param>
        /// <param name="value">The value to compare against.</param>
        /// <returns>
        /// A <see cref="ManagedFilter{TFilter, TField}"/> that matches when the field has a value that is not equal to <paramref name="value"/>.
        /// </returns>
        public static ManagedFilter<TFilter, TField?> NotEqual<TFilter, TField>(
            this Exposer<TFilter, TField?> exposer,
            TField value)
            where TFilter : class
            where TField : struct, Enum
        {
            return new ManagedFilter<TFilter, TField?>(exposer, Comparer.NotEquals, value, delegate (TFilter obj)
            {
                TField? val = exposer.internalFunc(obj);
                return val.HasValue && !val.GetValueOrDefault().Equals(value);
            });
        }
    }
}