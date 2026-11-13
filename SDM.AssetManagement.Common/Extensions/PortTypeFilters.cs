namespace Skyline.DataMiner.SDM.AssetManagement.Extensions
{
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;

    using CategoriesEnum = SharedMappers.DomIds.SlcAsset_Management.Enums.CategoriesEnum;

    /// <summary>
    /// Server-side (repository Read) filters for <see cref="PortType"/>.
    /// These filters are the query equivalents of the in-memory checks in <see cref="PortTypeExtensions"/>
    /// and can be combined with other <see cref="FilterElement{T}"/> instances.
    /// </summary>
    public static class PortTypeFilters
    {
        /// <summary>
        /// Gets a filter matching all Data Port Types, equivalent to <see cref="PortTypeExtensions.IsDataPortType(PortType)"/>:
        /// port types without categories, or without the <see cref="CategoriesEnum.Power"/> category.
        /// </summary>
        /// <remarks>
        /// <see cref="WithoutCategories"/> is required because NotContains does not match port types without categories.
        /// </remarks>
        /// <returns>A filter selecting all Data Port Types.</returns>
        public static FilterElement<PortType> DataPortTypes()
        {
            return new ORFilterElement<PortType>(
                WithoutCategories(),
                PortTypeExposers.CategoryLinks.Categories.NotContains(CategoriesEnum.Power));
        }

        /// <summary>
        /// Gets a filter matching all Power Port Types, equivalent to <see cref="PortTypeExtensions.IsPowerPortType(PortType)"/>:
        /// port types that have the <see cref="CategoriesEnum.Power"/> category.
        /// </summary>
        /// <returns>A filter selecting all Power Port Types.</returns>
        public static FilterElement<PortType> PowerPortTypes()
        {
            return PortTypeExposers.CategoryLinks.Categories.Contains(CategoriesEnum.Power);
        }

        /// <summary>
        /// Gets a filter matching port types without any category.
        /// </summary>
        /// <returns>A filter selecting port types without categories.</returns>
        public static FilterElement<PortType> WithoutCategories()
        {
            return PortTypeExposers.CategoryLinks.Categories.HasNoValue();
        }
    }
}
