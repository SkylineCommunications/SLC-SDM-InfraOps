namespace Skyline.DataMiner.SDM.AssetManagement.Common.Validation
{
    using System.Net;

    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Static validation handler for DataPort business rules.
    /// Contains pure validation logic without data access.
    /// </summary>
    public static class DataPortValidationHandler
    {
        public enum DataPortValidationField
        {
            DataPort,
            Name,
            PortNumber,
            PortType,
            Asset,
            AddressInfo,
            PrimaryPort,
            Ipv4Address,
            Ipv6Address,
            OutputType,
        }

        #region Info Validation

        /// <summary>
        /// Validates mandatory fields (Name, Port Number).
        /// </summary>
        public static bool AreMandatoryFieldsValid(DataPort dataPort, out ValidationResult result)
        {
            result = new ValidationResult();

            if (dataPort == null)
            {
                result.AddFailReason(DataPortValidationField.DataPort, "DataPort cannot be null.");
                return result.IsValid;
            }

            if (string.IsNullOrWhiteSpace(dataPort.DataPortInfo?.Name))
            {
                result.AddFailReason(DataPortValidationField.Name,
                    "DataPort Name cannot be empty.");
                return result.IsValid;
            }

            if (dataPort.DataPortInfo?.PortNumber == null)
            {
                result.AddFailReason(DataPortValidationField.PortNumber,
                    "DataPort Number must be provided.");
                return result.IsValid;
            }

            if (dataPort.DataPortInfo.PortNumber < 0)
            {
                result.AddFailReason(DataPortValidationField.PortNumber,
                    $"DataPort Number cannot be negative. Found: {dataPort.DataPortInfo.PortNumber}");
                return result.IsValid;
            }

            if (dataPort.DataPortInfo?.OutputType == null)
            {
                result.AddFailReason(DataPortValidationField.OutputType,
                    "DataPort Output Type must be provided.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates that DataPort has a valid Asset reference.
        /// </summary>
        public static bool IsAssetLinkValid(DataPort dataPort, out ValidationResult result)
        {
            result = new ValidationResult();

            if (dataPort == null)
            {
                result.AddFailReason(DataPortValidationField.DataPort, "DataPort cannot be null.");
                return result.IsValid;
            }

            if (dataPort.Asset == null || !dataPort.Asset.HasValue())
            {
                result.AddFailReason(DataPortValidationField.Asset,
                    "DataPort must be linked to an Asset.");
                return result.IsValid;
            }

            return result.IsValid;
        }

        #endregion

        #region Address Validation

        /// <summary>
        /// Validates IPv4 and IPv6 addresses format and requirements.
        /// Checks:
        /// - IPv4 address format (if provided)
        /// - IPv6 address format (if provided)
        /// - If marked as primary IPv4, IPv4 address must be populated
        /// - If marked as primary IPv6, IPv6 address must be populated
        /// </summary>
        public static bool IsAddressInfoValid(DataPort dataPort, out ValidationResult result)
        {
            result = new ValidationResult();

            if (dataPort == null)
            {
                result.AddFailReason(DataPortValidationField.DataPort, "DataPort cannot be null.");
                return result.IsValid;
            }

            var addressInfo = dataPort.AddressInfo;
            var primaryPortRelation = dataPort.PrimaryPortRelation;

            // Validate IPv4 Address Format (if provided)
            if (!string.IsNullOrWhiteSpace(addressInfo?.Ipv4Address) && !IsValidIpv4Address(addressInfo.Ipv4Address))
            {
                result.AddFailReason(DataPortValidationField.Ipv4Address,
                    $"Invalid IPv4 address format: '{addressInfo.Ipv4Address}'. Expected format: xxx.xxx.xxx.xxx (e.g., 192.168.1.1)");
            }

            // Validate IPv6 Address Format (if provided)
            if (!string.IsNullOrWhiteSpace(addressInfo?.Ipv6Address) && !IsValidIpv6Address(addressInfo.Ipv6Address))
            {
                result.AddFailReason(DataPortValidationField.Ipv6Address,
                    $"Invalid IPv6 address format: '{addressInfo.Ipv6Address}'. Expected format: xxxx:xxxx:xxxx:xxxx:xxxx:xxxx:xxxx:xxxx (e.g., 2001:0db8:85a3::8a2e:0370:7334)");
            }

            // If marked as Primary IPv4, IPv4 address must be populated
            if (primaryPortRelation?.IsPrimaryIpv4 == true && string.IsNullOrWhiteSpace(addressInfo?.Ipv4Address))
            {
                result.AddFailReason(DataPortValidationField.Ipv4Address,
                    "DataPort marked as Primary IPv4 must have an IPv4 address.");
            }

            // If marked as Primary IPv6, IPv6 address must be populated
            if (primaryPortRelation?.IsPrimaryIpv6 == true && string.IsNullOrWhiteSpace(addressInfo?.Ipv6Address))
            {
                result.AddFailReason(DataPortValidationField.Ipv6Address,
                    "DataPort marked as Primary IPv6 must have an IPv6 address.");
            }

            return result.IsValid;
        }

        /// <summary>
        /// Validates IPv4 address format.
        /// </summary>
        /// <param name="ipv4Address">The IPv4 address string to validate.</param>
        /// <returns>True if valid IPv4 format, false otherwise.</returns>
        public static bool IsValidIpv4Address(string ipv4Address)
        {
            if (string.IsNullOrWhiteSpace(ipv4Address))
            {
                return true;
            }

            var isAddressValid = IPAddress.TryParse(ipv4Address, out IPAddress address);

            return isAddressValid && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork;
        }

        /// <summary>
        /// Validates IPv6 address format.
        /// Supports both standard and compressed IPv6 formats.
        /// </summary>
        /// <param name="ipv6Address">The IPv6 address string to validate.</param>
        /// <returns>True if valid IPv6 format, false otherwise.</returns>
        public static bool IsValidIpv6Address(string ipv6Address)
        {
            if (string.IsNullOrWhiteSpace(ipv6Address))
            {
                return true;
            }

            var isAddressValid = IPAddress.TryParse(ipv6Address, out IPAddress address);

            return isAddressValid && address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6;
        }

        #endregion
    }
}