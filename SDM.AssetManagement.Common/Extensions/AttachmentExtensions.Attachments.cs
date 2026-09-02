namespace Skyline.DataMiner.SDM.AssetManagement.Common.Extensions
{
    using System.IO;

    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// Convenience methods for managing <see cref="Attachment"/> collections on
    /// <see cref="Asset"/> and <see cref="AssetClass"/>, mirroring the JobAttachment API.
    /// </summary>
    public static partial class AttachmentExtensions
    {
        public static bool IsImage(this Attachment attachment)
        {
            var extension = Path.GetExtension(attachment?.FilePath?.Replace('\\', '/'));

            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }

            switch (extension.TrimStart('.').ToLowerInvariant())
            {
                case "png":
                case "jpg":
                case "jpeg":
                case "bmp":
                case "webp":
                    return true;
                default:
                    return false;
            }
        }
    }
}
