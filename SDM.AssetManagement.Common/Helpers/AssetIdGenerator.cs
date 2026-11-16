namespace Skyline.DataMiner.SDM.AssetManagement.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Models;
    using Skyline.DataMiner.SDM.Extensions;

    /// <summary>
    /// Generates sequential Asset IDs based on the Asset Manager app settings.
    /// The ID is composed as <c>{AssetClassName}-{sequence}</c>, where the sequence is zero-padded to the configured minimum digits.
    /// </summary>
    public static class AssetIdGenerator
    {
        public const string Separator = "-";

        public const long DefaultNextSequence = 1;
        public const long DefaultIncrement = 1;
        public const long DefaultMinimumDigits = 4;

        private const int MaxAttempts = 10000;

        private static readonly object Lock = new object();

        /// <summary>
        /// Checks if automatic Asset ID generation is enabled in the Asset Manager app settings.
        /// </summary>
        public static bool IsAutoAssetIdEnabled(this IAssetManagementApiHelper api)
        {
            if (api == null)
            {
                throw new ArgumentNullException(nameof(api));
            }

            return api.AppSettings.Read(new TRUEFilterElement<AssetManagerAppSettings>()).FirstOrDefault()?.EnableAutoAssetId ?? false;
        }

        /// <summary>
        /// Generates the next free Asset ID for the given Asset Class name and persists the advanced sequence.
        /// IDs already in use are skipped, so calling this again (for example to regenerate after a duplicate was detected) always moves forward.
        /// </summary>
        /// <param name="api">The Asset Management API helper.</param>
        /// <param name="assetClassName">The name of the Asset Class used as prefix.</param>
        /// <returns>The generated Asset ID, or <see langword="null"/> if automatic generation is not enabled.</returns>
        public static string GenerateNextAssetId(this IAssetManagementApiHelper api, string assetClassName)
        {
            if (api == null)
            {
                throw new ArgumentNullException(nameof(api));
            }

            lock (Lock)
            {
                var settings = api.AppSettings.Read(new TRUEFilterElement<AssetManagerAppSettings>()).FirstOrDefault();
                if (settings == null || !settings.EnableAutoAssetId)
                {
                    return null;
                }

                long increment = settings.AssetIdIncrement > 0 ? settings.AssetIdIncrement : DefaultIncrement;
                long sequence = settings.AssetIdNextSequence > 0 ? settings.AssetIdNextSequence : DefaultNextSequence;
                int minimumDigits = (int)Math.Max(0, Math.Min(settings.AssetIdMinimumDigits, 18));

                for (int attempt = 0; attempt < MaxAttempts; attempt++)
                {
                    string assetId = Compose(assetClassName, sequence, minimumDigits);
                    sequence += increment;

                    if (api.Assets.Count(AssetExposers.AssetId.Equal(assetId)) > 0)
                    {
                        continue;
                    }

                    settings.AssetIdNextSequence = sequence;
                    api.AppSettings.CreateOrUpdate(new List<AssetManagerAppSettings> { settings });

                    return assetId;
                }

                throw new InvalidOperationException($"Could not find a free Asset ID for '{assetClassName}' after {MaxAttempts} attempts.");
            }
        }

        /// <summary>
        /// Composes an Asset ID from the Asset Class name and a sequence number.
        /// </summary>
        public static string Compose(string assetClassName, long sequence, int minimumDigits)
        {
            return $"{SanitizePrefix(assetClassName)}{Separator}{sequence.ToString($"D{minimumDigits}")}";
        }

        /// <summary>
        /// Removes every character that is not a letter or digit, so the Asset Class name can be safely used as ID prefix.
        /// </summary>
        public static string SanitizePrefix(string assetClassName)
        {
            if (string.IsNullOrWhiteSpace(assetClassName))
            {
                return string.Empty;
            }

            var sb = new StringBuilder(assetClassName.Length);
            foreach (char c in assetClassName)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }
    }
}
