namespace Skyline.DataMiner.SDM.AssetManagement.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// Convenience methods for managing <see cref="AssetAttachment"/> collections on
    /// <see cref="AssetClass"/>, including keeping the Front/Back images in sync with the attachments.
    /// </summary>
    public static partial class AttachmentExtensions
    {
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
        /// <exception cref="InvalidOperationException">
        /// The attachment is currently used as the <see cref="AssetClass.FrontImage"/> or
        /// <see cref="AssetClass.BackImage"/>. Use <see cref="RemoveAttachmentAndDependencies"/> instead.
        /// </exception>
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

            if (!string.IsNullOrEmpty(attachment.FilePath)
                && (attachment.FilePath == assetClass.FrontImage || attachment.FilePath == assetClass.BackImage))
            {
                throw new InvalidOperationException(
                    "Cannot remove an Attachment that is used as the Front or Back Image of the Asset Class. Use RemoveAttachmentAndDependencies instead.");
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

        /// <summary>
        /// Sets <see cref="AssetClass.FrontImage"/> to <paramref name="imagePath"/>, first adding it to
        /// <see cref="AssetClass.Attachments"/> when it is not already present. This guarantees the
        /// image satisfies the "Front Image must be part of the Attachments" validation rule.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="assetClass"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="imagePath"/> is <c>null</c> or whitespace.</exception>
        public static void AddFrontImageAndAttachment(this AssetClass assetClass, string imagePath)
        {
            EnsureImageAttachment(assetClass, imagePath);
            assetClass.FrontImage = imagePath;
        }

        /// <summary>
        /// Sets <see cref="AssetClass.BackImage"/> to <paramref name="imagePath"/>, first adding it to
        /// <see cref="AssetClass.Attachments"/> when it is not already present. This guarantees the
        /// image satisfies the "Back Image must be part of the Attachments" validation rule.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="assetClass"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="imagePath"/> is <c>null</c> or whitespace.</exception>
        public static void AddBackImageAndAttachment(this AssetClass assetClass, string imagePath)
        {
            EnsureImageAttachment(assetClass, imagePath);
            assetClass.BackImage = imagePath;
        }

        /// <summary>
        /// Removes <paramref name="attachment"/> from <see cref="AssetClass.Attachments"/>, first clearing
        /// <see cref="AssetClass.FrontImage"/> and/or <see cref="AssetClass.BackImage"/> if they reference it.
        /// This is the safe counterpart to <see cref="RemoveItemFromAttachments(AssetClass, AssetAttachment)"/>,
        /// which refuses to remove an attachment still in use as an image.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="attachment"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">No matching attachment was found.</exception>
        public static void RemoveAttachmentAndDependencies(this AssetClass assetClass, AssetAttachment attachment)
        {
            if (assetClass == null)
            {
                throw new ArgumentNullException(nameof(assetClass));
            }

            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            if (!string.IsNullOrEmpty(attachment.FilePath))
            {
                if (assetClass.FrontImage == attachment.FilePath)
                {
                    assetClass.FrontImage = null;
                }

                if (assetClass.BackImage == attachment.FilePath)
                {
                    assetClass.BackImage = null;
                }
            }

            assetClass.RemoveItemFromAttachments(attachment);
        }

        private static void EnsureImageAttachment(AssetClass assetClass, string imagePath)
        {
            if (assetClass == null)
            {
                throw new ArgumentNullException(nameof(assetClass));
            }

            if (string.IsNullOrWhiteSpace(imagePath))
            {
                throw new ArgumentException("Image path must be provided.", nameof(imagePath));
            }

            if (!assetClass.Attachments.Any(a => a?.FilePath == imagePath))
            {
                assetClass.AddAttachment(new AssetAttachment { FilePath = imagePath });
            }
        }
    }
}
