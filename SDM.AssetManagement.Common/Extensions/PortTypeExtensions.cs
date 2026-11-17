namespace Skyline.DataMiner.SDM.AssetManagement.Extensions
{
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// Extension methods for PortType/CableTypeWrapper.
    /// </summary>
    public static class PortTypeExtensions
    {
        /// <summary>
        /// Determines if a PortType is a Data Port Type.
        /// A port type is considered a Data Port Type if:
        /// - It has no categories, OR
        /// - It does not have the "Power" category
        /// 
        /// Inversely, a port type is NOT a Data Port Type if it has the "Power" category (e.g. "Power" + "Data" is not a Data Port Type).
        /// This is an in-memory check; use <see cref="PortTypeFilters.DataPortTypes"/> (or
        /// <see cref="PortTypeDomRepositoryExtensions.ReadDataPortTypes(IReadableRepository{PortType})"/>) for the equivalent server-side filter.
        /// </summary>
        /// <param name="portType">The port type to check.</param>
        /// <returns>True if the port type is a Data Port Type; otherwise, false.</returns>
        public static bool IsDataPortType(this PortType portType)
        {
            if (portType == null)
            {
                return false;
            }

            return !portType.IsPowerPortType();
        }

        /// <summary>
        /// Determines if a PortType is a Power Port Type.
        /// A port type is considered a Power Port Type if it has the "Power" category.
        /// This is an in-memory check; use <see cref="PortTypeFilters.PowerPortTypes"/> (or
        /// <see cref="PortTypeDomRepositoryExtensions.ReadPowerPortTypes(IReadableRepository{PortType})"/>) for the equivalent server-side filter.
        /// </summary>
        /// <param name="portType">The port type to check.</param>
        /// <returns>True if the port type is a Power Port Type; otherwise, false.</returns>
        public static bool IsPowerPortType(this PortType portType)
        {
            if (portType == null)
            {
                return false;
            }

            return portType.CategoryLinks.Categories.Any(category => category == SharedMappers.DomIds.SlcAsset_Management.Enums.CategoriesEnum.Power);
        }
    }
}