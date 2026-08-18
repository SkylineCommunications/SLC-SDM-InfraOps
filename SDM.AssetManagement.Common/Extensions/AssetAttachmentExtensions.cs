namespace Skyline.DataMiner.SDM.AssetManagement.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// Convenience methods for managing <see cref="AssetAttachment"/> collections on
    /// <see cref="Asset"/> and <see cref="AssetClass"/>, mirroring the JobAttachment API.
    /// </summary>
    public static class AssetAttachmentExtensions
    {
        #region Asset

        /// <summary>
        /// Adds an <see cref="AssetAttachment"/> to <see cref="Asset.Attachments"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="attachment"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">An attachment with the same <see cref="AssetAttachment.FilePath"/> already exists.</exception>
        /// <remarks>
        /// This only updates the in-memory <see cref="Asset"/> instance. It is not persisted
        /// to the DOM/database until the asset is saved.
        /// </remarks>
        public static void AddAttachment(this Asset asset, AssetAttachment attachment)
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
        /// Removes the <see cref="AssetAttachment"/> matching <paramref name="attachment"/>'s
        /// <see cref="AssetAttachment.FilePath"/> from <see cref="Asset.Attachments"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="attachment"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No matching attachment was found.</exception>
        public static void RemoveItemFromAttachments(this Asset asset, AssetAttachment attachment)
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

        /// <summary>
        /// Replaces <see cref="Asset.Attachments"/> with <paramref name="attachments"/>.
        /// </summary>
        public static void SetAttachments(this Asset asset, List<AssetAttachment> attachments)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            asset.Attachments = attachments ?? new List<AssetAttachment>();
        }

        /// <summary>
        /// Clears all entries from <see cref="Asset.Attachments"/>.
        /// </summary>
        public static void ClearAttachments(this Asset asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            if (asset.Attachments.Count == 0)
            {
                return;
            }

            asset.Attachments = new List<AssetAttachment>();
        }

        #endregion

        #region AssetClass

        /// <summary>
        /// Adds an <see cref="AssetAttachment"/> to <see cref="AssetClass.Attachments"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="attachment"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">An attachment with the same <see cref="AssetAttachment.FilePath"/> already exists.</exception>
        /// <remarks>
        /// This only updates the in-memory <see cref="AssetClass"/> instance. It is not persisted
        /// to the DOM/database until the asset class is saved.
        /// </remarks>
        public static void AddAttachment(this AssetClass assetClass, AssetAttachment attachment)
        {
            if (assetClass == null)
            {
                throw new ArgumentNullException(nameof(assetClass));
            }

            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            var list = assetClass.Attachments;

            if (list.Any(a => a.FilePath == attachment.FilePath))
            {
                throw new InvalidOperationException("An Attachment with the same File Path already exists.");
            }

            list.Add(attachment);
            assetClass.Attachments = list;
        }

        /// <summary>
        /// Removes the <see cref="AssetAttachment"/> matching <paramref name="attachment"/>'s
        /// <see cref="AssetAttachment.FilePath"/> from <see cref="AssetClass.Attachments"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="attachment"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No matching attachment was found.</exception>
        public static void RemoveItemFromAttachments(this AssetClass assetClass, AssetAttachment attachment)
        {
            if (assetClass == null)
            {
                throw new ArgumentNullException(nameof(assetClass));
            }

            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            var list = assetClass.Attachments;
            var found = list.FirstOrDefault(a => a.FilePath == attachment.FilePath);

            if (found == null)
            {
                throw new ArgumentException("The specified Attachment was not found.");
            }

            list.Remove(found);
            assetClass.Attachments = list;
        }

        /// <summary>
        /// Replaces <see cref="AssetClass.Attachments"/> with <paramref name="attachments"/>.
        /// </summary>
        public static void SetAttachments(this AssetClass assetClass, List<AssetAttachment> attachments)
        {
            if (assetClass == null)
            {
                throw new ArgumentNullException(nameof(assetClass));
            }

            assetClass.Attachments = attachments ?? new List<AssetAttachment>();
        }

        /// <summary>
        /// Clears all entries from <see cref="AssetClass.Attachments"/>.
        /// </summary>
        public static void ClearAttachments(this AssetClass assetClass)
        {
            if (assetClass == null)
            {
                throw new ArgumentNullException(nameof(assetClass));
            }

            if (assetClass.Attachments.Count == 0)
            {
                return;
            }

            assetClass.Attachments = new List<AssetAttachment>();
        }

        #endregion
    }
}
