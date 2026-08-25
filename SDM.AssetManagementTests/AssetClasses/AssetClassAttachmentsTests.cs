namespace SDM.AssetManagement.Tests.AssetClasses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using SDM.AssetManagement.Tests.Setup;

    using SharedMappers.DomIds;

    using Skyline.DataMiner.Net.Messages.SLDataGateway;
    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.AssetManagement.Extensions;
    using Skyline.DataMiner.SDM.AssetManagement.Models;

    /// <summary>
    /// Unit tests for the <see cref="Attachment"/> convenience methods on <see cref="AssetClass"/>
    /// (AddAttachment/RemoveItemFromAttachments/SetAttachments/ClearAttachments), plus a DOM
    /// round-trip test verifying attachments persist, mirroring the JobAttachment implementation.
    /// </summary>
    [TestClass]
    public class AssetClassAttachmentsTests : BaseRepositoryTest
    {
        [TestMethod]
        public void AddAttachment_NewAttachment_ShouldBeAdded()
        {
            var assetClass = new AssetClass();
            var attachment = new Attachment { FilePath = @"C:\file1.pdf" };

            assetClass.AddAttachment(attachment);

            assetClass.Attachments.Should().ContainSingle().Which.Should().Be(attachment);
        }

        [TestMethod]
        public void AddAttachment_Null_ShouldThrow()
        {
            var assetClass = new AssetClass();

            Action act = () => assetClass.AddAttachment(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void AddAttachment_DuplicateFilePath_ShouldThrow()
        {
            var assetClass = new AssetClass();
            assetClass.AddAttachment(new Attachment { FilePath = @"C:\file1.pdf" });

            Action act = () => assetClass.AddAttachment(new Attachment { FilePath = @"C:\file1.pdf" });

            act.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void RemoveItemFromAttachments_ExistingAttachment_ShouldBeRemoved()
        {
            var assetClass = new AssetClass();
            var attachment = new Attachment { FilePath = @"C:\file1.pdf" };
            assetClass.AddAttachment(attachment);

            assetClass.RemoveItemFromAttachments(attachment);

            assetClass.Attachments.Should().BeEmpty();
        }

        [TestMethod]
        public void RemoveItemFromAttachments_Null_ShouldThrow()
        {
            var assetClass = new AssetClass();

            Action act = () => assetClass.RemoveItemFromAttachments(null);

            act.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void RemoveItemFromAttachments_NotFound_ShouldThrow()
        {
            var assetClass = new AssetClass();
            var attachment = new Attachment { FilePath = @"C:\file1.pdf" };

            Action act = () => assetClass.RemoveItemFromAttachments(attachment);

            act.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void SetAttachments_ShouldReplaceExistingList()
        {
            var assetClass = new AssetClass();
            assetClass.AddAttachment(new Attachment { FilePath = @"C:\file1.pdf" });

            var replacement = new List<Attachment>
            {
                new Attachment { FilePath = @"C:\file2.pdf" },
                new Attachment { FilePath = @"C:\file3.pdf" },
            };
            assetClass.SetAttachments(replacement);

            assetClass.Attachments.Should().BeEquivalentTo(replacement);
        }

        [TestMethod]
        public void ClearAttachments_ShouldEmptyList()
        {
            var assetClass = new AssetClass();
            assetClass.AddAttachment(new Attachment { FilePath = @"C:\file1.pdf" });

            assetClass.ClearAttachments();

            assetClass.Attachments.Should().BeEmpty();
        }

        [TestMethod]
        public void SettingFrontImage_ShouldNotAutomaticallyAddToAttachments()
        {
            var assetClass = new AssetClass();

            assetClass.FrontImage = @"C:\images\front.png";

            assetClass.Attachments.Should().BeEmpty();
        }

        [TestMethod]
        public void AddFrontImageAndAttachment_ShouldAddAttachmentAndSetFrontImage()
        {
            var assetClass = new AssetClass();

            assetClass.AddFrontImageAndAttachment(@"C:\images\front.png");

            assetClass.FrontImage.Should().Be(@"C:\images\front.png");
            assetClass.Attachments.Should().ContainSingle()
                .Which.FilePath.Should().Be(@"C:\images\front.png");
        }

        [TestMethod]
        public void AddBackImageAndAttachment_ShouldAddAttachmentAndSetBackImage()
        {
            var assetClass = new AssetClass();

            assetClass.AddBackImageAndAttachment(@"C:\images\back.png");

            assetClass.BackImage.Should().Be(@"C:\images\back.png");
            assetClass.Attachments.Should().ContainSingle()
                .Which.FilePath.Should().Be(@"C:\images\back.png");
        }

        [TestMethod]
        public void AddFrontImageAndAttachment_WhenAlreadyInAttachments_ShouldNotDuplicate()
        {
            var attachedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var attachedBy = Guid.NewGuid();

            var assetClass = new AssetClass();
            assetClass.AddAttachment(new Attachment
            {
                FilePath = @"C:\images\front.png",
                AttachedAt = attachedAt,
                AttachedBy = attachedBy,
            });

            assetClass.AddFrontImageAndAttachment(@"C:\images\front.png");

            assetClass.FrontImage.Should().Be(@"C:\images\front.png");
            var attachment = assetClass.Attachments.Should().ContainSingle().Subject;
            attachment.AttachedAt.Should().Be(attachedAt);
            attachment.AttachedBy.Should().Be(attachedBy);
        }

        [TestMethod]
        public void AddFrontImageAndAttachment_NullOrWhitespace_ShouldThrow()
        {
            var assetClass = new AssetClass();

            Action actNull = () => assetClass.AddFrontImageAndAttachment(null);
            Action actWhitespace = () => assetClass.AddFrontImageAndAttachment("   ");

            actNull.Should().Throw<ArgumentException>();
            actWhitespace.Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void RemoveItemFromAttachments_WhenFrontImage_ShouldThrow()
        {
            var assetClass = new AssetClass();
            assetClass.AddFrontImageAndAttachment(@"C:\images\front.png");
            var attachment = assetClass.Attachments.Single();

            Action act = () => assetClass.RemoveItemFromAttachments(attachment);

            act.Should().Throw<InvalidOperationException>();
            assetClass.Attachments.Should().ContainSingle();
        }

        [TestMethod]
        public void RemoveItemFromAttachments_WhenBackImage_ShouldThrow()
        {
            var assetClass = new AssetClass();
            assetClass.AddBackImageAndAttachment(@"C:\images\back.png");
            var attachment = assetClass.Attachments.Single();

            Action act = () => assetClass.RemoveItemFromAttachments(attachment);

            act.Should().Throw<InvalidOperationException>();
            assetClass.Attachments.Should().ContainSingle();
        }

        [TestMethod]
        public void RemoveAttachmentAndDependencies_WhenFrontImage_ShouldClearImageAndRemove()
        {
            var assetClass = new AssetClass();
            assetClass.AddFrontImageAndAttachment(@"C:\images\front.png");
            var attachment = assetClass.Attachments.Single();

            assetClass.RemoveAttachmentAndDependencies(attachment);

            assetClass.FrontImage.Should().BeNull();
            assetClass.Attachments.Should().BeEmpty();
        }

        [TestMethod]
        public void RemoveAttachmentAndDependencies_WhenBackImage_ShouldClearImageAndRemove()
        {
            var assetClass = new AssetClass();
            assetClass.AddBackImageAndAttachment(@"C:\images\back.png");
            var attachment = assetClass.Attachments.Single();

            assetClass.RemoveAttachmentAndDependencies(attachment);

            assetClass.BackImage.Should().BeNull();
            assetClass.Attachments.Should().BeEmpty();
        }

        [TestMethod]
        public void RemoveAttachmentAndDependencies_NonImageAttachment_ShouldRemoveAndKeepImages()
        {
            var assetClass = new AssetClass();
            assetClass.AddFrontImageAndAttachment(@"C:\images\front.png");
            assetClass.AddAttachment(new Attachment { FilePath = @"C:\docs\manual.pdf" });
            var manual = assetClass.Attachments.Single(a => a.FilePath == @"C:\docs\manual.pdf");

            assetClass.RemoveAttachmentAndDependencies(manual);

            assetClass.FrontImage.Should().Be(@"C:\images\front.png");
            assetClass.Attachments.Select(a => a.FilePath).Should()
                .BeEquivalentTo(new[] { @"C:\images\front.png" });
        }

        [TestMethod]
        public void Create_WithFrontImageNotInAttachments_ShouldFailValidation()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);
            var deviceType = Helper.TestData.DeviceTypes.First();

            var assetClass = new AssetClass
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Class With Orphan Image",
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                Manufacturer = Guid.NewGuid(),
                HeightU = 1.0,
                PowerSupply = SlcAsset_Management.Enums.PowerSupplyEnum.AC,
                FrontImage = @"C:\images\front.png",
            };

            // Act
            Action act = () => Helper.AssetManagement.AssetClasses.Create(assetClass);

            // Assert
            act.Should().Throw<Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions.ValidationException>()
                .WithMessage("*Front Image must be part of the Attachments*");
        }

        [TestMethod]
        public void Create_WithImagesAddedViaHelper_ShouldPersistImagesAsAttachments()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);
            var deviceType = Helper.TestData.DeviceTypes.First();

            var assetClass = new AssetClass
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Class With Image",
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                Manufacturer = Guid.NewGuid(),
                HeightU = 1.0,
                PowerSupply = SlcAsset_Management.Enums.PowerSupplyEnum.AC,
            };
            assetClass.AddFrontImageAndAttachment(@"C:\images\front.png");
            assetClass.AddBackImageAndAttachment(@"C:\images\back.png");

            // Act
            Helper.AssetManagement.AssetClasses.Create(assetClass);
            var created = Helper.AssetManagement.AssetClasses.Read(new TRUEFilterElement<AssetClass>()).Single();

            // Assert
            using (new AssertionScope())
            {
                created.FrontImage.Should().Be(@"C:\images\front.png");
                created.BackImage.Should().Be(@"C:\images\back.png");
                created.Attachments.Select(a => a.FilePath).Should()
                    .BeEquivalentTo(new[] { @"C:\images\front.png", @"C:\images\back.png" });
            }
        }

        [TestMethod]
        public void Create_WithAttachments_ShouldPersistAndReadBackCorrectly()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);
            var deviceType = Helper.TestData.DeviceTypes.First();

            var attachedAt = new DateTime(2024, 1, 15, 8, 30, 0, DateTimeKind.Utc);
            var attachedBy = Guid.NewGuid();

            var assetClass = new AssetClass
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Class With Attachments",
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                Manufacturer = Guid.NewGuid(),
                HeightU = 1.0,
                PowerSupply = SlcAsset_Management.Enums.PowerSupplyEnum.AC,
                Attachments = new List<Attachment>
                {
                    new Attachment { FilePath = @"C:\docs\manual.pdf", AttachedAt = attachedAt, AttachedBy = attachedBy },
                    new Attachment { FilePath = @"C:\docs\datasheet.pdf" },
                },
            };

            // Act
            Helper.AssetManagement.AssetClasses.Create(assetClass);
            var created = Helper.AssetManagement.AssetClasses.Read(new TRUEFilterElement<AssetClass>()).Single();

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
