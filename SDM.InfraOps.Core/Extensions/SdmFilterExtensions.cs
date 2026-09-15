namespace Skyline.DataMiner.SDM.Extensions
{
    using System;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;

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

        public static ManagedFilter<TFilter, TField?> Equal<TFilter, TField>(
            this Exposer<TFilter, TField?> exposer,
            TField? value)
            where TFilter : class
            where TField : struct
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

        //
        // Summary:
        //     Creates a filter that checks if the exposed Reference field has a value.
        //
        // Parameters:
        //   exposer:
        //     The exposer that identifies the field to filter on.
        //
        // Type parameters:
        //   TFilter:
        //     The type of the filter.
        //
        //   TField:
        //     The non-nullable value type being compared.
        //
        // Returns:
        //     A Skyline.DataMiner.Net.Messages.SLDataGateway.ManagedFilter`2 that matches when
        //     the field is not null.
        public static ManagedFilter<TFilter, SdmObjectReference<TField>> HasValue<TFilter, TField>(this Exposer<TFilter, SdmObjectReference<TField>> exposer) where TFilter : class where TField : SdmObject<TField>
        {
            return new ManagedFilter<TFilter, SdmObjectReference<TField>>(exposer, Comparer.NotEquals, default, (TFilter obj) => exposer.internalFunc(obj).HasValue());
        }

        //
        // Summary:
        //     Creates a filter that checks if the exposed Reference field has a value.
        //
        // Parameters:
        //   exposer:
        //     The exposer that identifies the field to filter on.
        //
        // Type parameters:
        //   TFilter:
        //     The type of the filter.
        //
        //   TField:
        //     The non-nullable value type being compared.
        //
        // Returns:
        //     A Skyline.DataMiner.Net.Messages.SLDataGateway.ManagedFilter`2 that matches when
        //     the field is not null.
        public static ManagedFilter<TFilter, IObjectReference<TField>> HasValue<TFilter, TField>(this Exposer<TFilter, IObjectReference<TField>> exposer) where TFilter : class
        {
            return new ManagedFilter<TFilter, IObjectReference<TField>>(exposer, Comparer.NotEquals, default, (TFilter obj) => exposer.internalFunc(obj).HasValue());
        }

        //
        // Summary:
        //     Creates a filter that checks if the exposed Reference field has no value.
        //
        // Parameters:
        //   exposer:
        //     The exposer that identifies the field to filter on.
        //
        // Type parameters:
        //   TFilter:
        //     The type of the filter.
        //
        //   TField:
        //     The non-nullable value type being compared.
        //
        // Returns:
        //     A Skyline.DataMiner.Net.Messages.SLDataGateway.ManagedFilter`2 that matches when
        //     the field is null.
        public static ManagedFilter<TFilter, SdmObjectReference<TField>> HasNoValue<TFilter, TField>(this Exposer<TFilter, SdmObjectReference<TField>> exposer) where TFilter : class where TField : SdmObject<TField>
        {
            return new ManagedFilter<TFilter, SdmObjectReference<TField>>(exposer, Comparer.Equals, default, (TFilter obj) => !exposer.internalFunc(obj).HasValue());
        }

        //
        // Summary:
        //     Creates a filter that checks if the exposed Reference field has no value.
        //
        // Parameters:
        //   exposer:
        //     The exposer that identifies the field to filter on.
        //
        // Type parameters:
        //   TFilter:
        //     The type of the filter.
        //
        //   TField:
        //     The non-nullable value type being compared.
        //
        // Returns:
        //     A Skyline.DataMiner.Net.Messages.SLDataGateway.ManagedFilter`2 that matches when
        //     the field is null.
        public static ManagedFilter<TFilter, IObjectReference<TField>> HasNoValue<TFilter, TField>(this Exposer<TFilter, IObjectReference<TField>> exposer) where TFilter : class
        {
            return new ManagedFilter<TFilter, IObjectReference<TField>>(exposer, Comparer.Equals, default, (TFilter obj) => !exposer.internalFunc(obj).HasValue());
        }
    }
}