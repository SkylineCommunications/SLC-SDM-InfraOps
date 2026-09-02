namespace SDM.AssetManagement.Tests.Assets
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.AssetManagement.Tests.Setup;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Extensions;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// Unit tests for the <see cref="Attachment"/> convenience methods on <see cref="Asset"/>
    /// (AddAttachment/RemoveItemFromAttachments/SetAttachments/ClearAttachments), plus a DOM
    /// round-trip test verifying attachments persist, mirroring the JobAttachment implementation.
    /// </summary>
    [TestClass]
    public class AssetAttachmentsTests : BaseRepositoryTest
    {
        [TestMethod]
        public void AddAttachment_NewAttachment_ShouldBeAdded()
        {
            var asset = new Asset();
            var attachment = new Attachment { FilePath = @"C:\file1.pdf" };

            asset.AddAttachment(attachment);

            asset.Attachments.Should().ContainSingle().Which.Should().Be(attachment);
        }

        [TestMethod]
        public void AddAttachment_Null_ShouldThrow()
        {
            var asset = new Asset();

            Action act = () => asset.AddAttachment(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void AddAttachment_DuplicateFilePath_ShouldThrow()
        {
            var asset = new Asset();
            asset.AddAttachment(new Attachment { FilePath = @"C:\file1.pdf" });

            Action act = () => asset.AddAttachment(new Attachment { FilePath = @"C:\file1.pdf" });

            act.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void RemoveItemFromAttachments_ExistingAttachment_ShouldBeRemoved()
        {
            var asset = new Asset();
            var attachment = new Attachment { FilePath = @"C:\file1.pdf" };
            asset.AddAttachment(attachment);

            asset.RemoveItemFromAttachments(attachment);

            asset.Attachments.Should().BeEmpty();
        }

        [TestMethod]
        public void RemoveItemFromAttachments_Null_ShouldThrow()
        {
            var asset = new Asset();

            Action act = () => asset.RemoveItemFromAttachments(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void RemoveItemFromAttachments_NotFound_ShouldThrow()
        {
            var asset = new Asset();
            var attachment = new Attachment { FilePath = @"C:\file1.pdf" };

            Action act = () => asset.RemoveItemFromAttachments(attachment);

            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void SetAttachments_ShouldReplaceExistingList()
        {
            var asset = new Asset();
            asset.AddAttachment(new Attachment { FilePath = @"C:\file1.pdf" });

            var replacement = new List<Attachment>
            {
                new Attachment { FilePath = @"C:\file2.pdf" },
                new Attachment { FilePath = @"C:\file3.pdf" },
            };
            asset.SetAttachments(replacement);

            asset.Attachments.Should().BeEquivalentTo(replacement);
        }

        [TestMethod]
        public void ClearAttachments_ShouldEmptyList()
        {
            var asset = new Asset();
            asset.AddAttachment(new Attachment { FilePath = @"C:\file1.pdf" });

            asset.ClearAttachments();

            asset.Attachments.Should().BeEmpty();
        }

        [TestMethod]
        public void Create_WithAttachments_ShouldPersistAndReadBackCorrectly()
        {
            // Arrange
            Helper.PopulateWithDemoData(DemoDataLayer.AssetClasses);
            var assetClass = Helper.TestData.AssetClasses.First();

            var attachedAt = new DateTime(2024, 1, 15, 8, 30, 0, DateTimeKind.Utc);
            var attachedBy = Guid.NewGuid();

            var asset = new Asset
            {
                AssetID = Guid.NewGuid().ToString(),
                Name = "Asset With Attachments",
                AssetClassId = new SdmObjectReference<AssetClass>(assetClass.Identifier),
                Attachments = new List<Attachment>
                {
                    new Attachment { FilePath = @"C:\docs\manual.pdf", AttachedAt = attachedAt, AttachedBy = attachedBy },
                    new Attachment { FilePath = @"C:\docs\datasheet.pdf" },
                },
            };

            // Act
            Helper.AssetManagement.Assets.Create(asset);
            var created = Helper.AssetManagement.Assets.Read(new TRUEFilterElement<Asset>()).Single();

            // Assert
            using (new AssertionScope())
            {
                created.Attachments.Should().HaveCount(2);

                var manual = created.Attachments.Single(a => a.FilePath == @"C:\docs\manual.pdf");
                manual.AttachedAt.Should().Be(attachedAt);
                manual.AttachedBy.Should().Be(attachedBy);

                var datasheet = created.Attachments.Single(a => a.FilePath == @"C:\docs\datasheet.pdf");
                datasheet.AttachedAt.Should().BeNull();
                datasheet.AttachedBy.Should().BeNull();
            }
        }
    }
}
