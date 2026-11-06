namespace Skyline.DataMiner.SDM.AssetManagement.Extensions
{
    using System;
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// Convenience methods for managing <see cref="Attachment"/> collections on
    /// <see cref="Asset"/> and <see cref="AssetClass"/>, mirroring the JobAttachment API.
    /// </summary>
    public static partial class AttachmentExtensions
    {
        /// <summary>
        /// Adds an <see cref="Attachment"/> to <see cref="Asset.Attachments"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="attachment"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">An attachment with the same <see cref="Attachment.FilePath"/> already exists.</exception>
        /// <remarks>
        /// This only updates the in-memory <see cref="Asset"/> instance. It is not persisted
        /// to the DOM/database until the asset is saved.
        /// </remarks>
        public static void AddAttachment(this Asset asset, Attachment attachment)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            var list = asset.Attachments;

            if (list.Any(a => a.FilePath == attachment.FilePath))
            {
                throw new InvalidOperationException("An Attachment with the same File Path already exists.");
            }

            list.Add(attachment);
            asset.Attachments = list;
        }

        /// <summary>
        /// Removes the <see cref="Attachment"/> matching <paramref name="attachment"/>'s
        /// <see cref="Attachment.FilePath"/> from <see cref="Asset.Attachments"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="attachment"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No matching attachment was found.</exception>
        public static void RemoveItemFromAttachments(this Asset asset, Attachment attachment)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            var list = asset.Attachments;
            var found = list.FirstOrDefault(a => a.FilePath == attachment.FilePath);

            if (found == null)
            {
                throw new ArgumentException("The specified Attachment was not found.");
            }

            list.Remove(found);
            asset.Attachments = list;
        }
    }
}
