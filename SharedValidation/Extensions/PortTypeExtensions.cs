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
        /// - It has at least one category that is NOT "Power"
        /// 
        /// Inversely, a port type is NOT a Data Port Type if it has ONLY the "Power" category.
        /// </summary>
        /// <param name="portType">The port type to check.</param>
        /// <returns>True if the port type is a Data Port Type; otherwise, false.</returns>
        public static bool IsDataPortType(this PortType portType)
        {
            if (portType == null)
            {
                return false;
            }

            return !portType.CategoryLinks.Categories.Any() ||
                   portType.CategoryLinks.Categories.Any(category => category != SharedMappers.DomIds.SlcAsset_Management.Enums.CategoriesEnum.Power);
        }

        /// <summary>
        /// Determines if a PortType is a Power Port Type.
        /// A port type is considered a Power Port Type if it has the "Power" category.
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