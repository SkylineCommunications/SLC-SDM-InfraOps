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
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

    /// <summary>
    /// Unit tests for the <see cref="Attachment"/> convenience methods on <see cref="AssetClass"/>
    /// (AddAttachment/RemoveItemFromAttachments/RemoveAttachmentAndDependencies), plus a DOM
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
        public void SettingAttachments_ShouldReplaceExistingList()
        {
            var assetClass = new AssetClass();
            assetClass.AddAttachment(new Attachment { FilePath = @"C:\file1.pdf" });

            var replacement = new List<Attachment>
            {
                new Attachment { FilePath = @"C:\file2.pdf" },
                new Attachment { FilePath = @"C:\file3.pdf" },
            };
            assetClass.Attachments = replacement;

            assetClass.Attachments.Should().BeEquivalentTo(replacement);
        }

        [TestMethod]
        public void ClearingAttachments_ShouldEmptyList()
        {
            var assetClass = new AssetClass();
            assetClass.AddAttachment(new Attachment { FilePath = @"C:\file1.pdf" });

            assetClass.Attachments.Clear();

            assetClass.Attachments.Should().BeEmpty();
        }

        [TestMethod]
        public void SettingFrontImage_ShouldAddAttachmentAndSetFrontImage()
        {
            var assetClass = new AssetClass();

            assetClass.FrontImage = @"C:\images\front.png";

            assetClass.FrontImage.Should().Be(@"C:\images\front.png");
            assetClass.Attachments.Should().ContainSingle()
                .Which.FilePath.Should().Be(@"C:\images\front.png");
        }

        [TestMethod]
        public void SettingBackImage_ShouldAddAttachmentAndSetBackImage()
        {
            var assetClass = new AssetClass();

            assetClass.BackImage = @"C:\images\back.png";

            assetClass.BackImage.Should().Be(@"C:\images\back.png");
            assetClass.Attachments.Should().ContainSingle()
                .Which.FilePath.Should().Be(@"C:\images\back.png");
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void SettingImage_WhenAlreadyInAttachments_ShouldPreserveAttachment(bool front)
        {
            var attachedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var attachedBy = Convert.ToString(Guid.NewGuid());

            var assetClass = new AssetClass();
            assetClass.AddAttachment(new Attachment
            {
                FilePath = @"C:\images\front.png",
                AttachedAt = attachedAt,
                AttachedBy = attachedBy,
            });

            if (front)
            {
                assetClass.FrontImage = @"C:\images\front.png";
            }
            else
            {
                assetClass.BackImage = @"C:\images\front.png";
            }

            (front ? assetClass.FrontImage : assetClass.BackImage).Should().Be(@"C:\images\front.png");
            var attachment = assetClass.Attachments.Should().ContainSingle().Subject;
            attachment.AttachedAt.Should().Be(attachedAt);
            attachment.AttachedBy.Should().Be(attachedBy);
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void SettingEmptyImages_ShouldNotAddAttachments(string imagePath)
        {
            var assetClass = new AssetClass();

            assetClass.FrontImage = imagePath;
            assetClass.BackImage = imagePath;

            assetClass.FrontImage.Should().Be(imagePath);
            assetClass.BackImage.Should().Be(imagePath);
            assetClass.Attachments.Should().BeEmpty();
        }

        [TestMethod]
        public void SettingImagesToSamePathRepeatedly_ShouldShareOneAttachment()
        {
            var assetClass = new AssetClass();

            assetClass.FrontImage = "shared.png";
            assetClass.BackImage = "shared.png";
            assetClass.FrontImage = "shared.png";
            assetClass.BackImage = "shared.png";

            assetClass.Attachments.Should().ContainSingle()
                .Which.FilePath.Should().Be("shared.png");
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void ReassigningImage_AfterAttachmentRemoval_ShouldRestoreAttachment(bool front)
        {
            var assetClass = new AssetClass();
            if (front)
            {
                assetClass.FrontImage = "image.png";
            }
            else
            {
                assetClass.BackImage = "image.png";
            }
            assetClass.Attachments.Clear();

            if (front)
            {
                assetClass.FrontImage = "image.png";
            }
            else
            {
                assetClass.BackImage = "image.png";
            }

            assetClass.Attachments.Should().ContainSingle()
                .Which.FilePath.Should().Be("image.png");
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void ChangingAndClearingImage_ShouldRetainExistingAttachments(bool front)
        {
            var assetClass = new AssetClass();
            if (front)
            {
                assetClass.FrontImage = "old.png";
                assetClass.FrontImage = "new.png";
                assetClass.FrontImage = null;
            }
            else
            {
                assetClass.BackImage = "old.png";
                assetClass.BackImage = "new.png";
                assetClass.BackImage = null;
            }

            (front ? assetClass.FrontImage : assetClass.BackImage).Should().BeNull();
            assetClass.Attachments.Select(a => a.FilePath).Should()
                .BeEquivalentTo("old.png", "new.png");
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void SettingImage_ShouldTrackImageAndAttachmentChanges(bool front)
        {
            var assetClass = new AssetClass();
            assetClass.ResetChangeTracking();

            if (front)
            {
                assetClass.FrontImage = "image.png";
            }
            else
            {
                assetClass.BackImage = "image.png";
            }

            assetClass.Changed.Should().BeTrue();
            assetClass.GetChanges().Select(c => c.FieldName).Should()
                .BeEquivalentTo(front ? nameof(AssetClass.FrontImage) : nameof(AssetClass.BackImage),
                    nameof(AssetClass.Attachments));

            assetClass.ResetChangeTracking();
            assetClass.Changed.Should().BeFalse();
            assetClass.GetChanges().Should().BeEmpty();
        }

        [TestMethod]
        public void RemoveItemFromAttachments_WhenFrontImage_ShouldThrow()
        {
            var assetClass = new AssetClass();
            assetClass.FrontImage = @"C:\images\front.png";
            var attachment = assetClass.Attachments.Single();

            Action act = () => assetClass.RemoveItemFromAttachments(attachment);

            act.Should().Throw<InvalidOperationException>();
            assetClass.Attachments.Should().ContainSingle();
        }

        [TestMethod]
        public void RemoveItemFromAttachments_WhenBackImage_ShouldThrow()
        {
            var assetClass = new AssetClass();
            assetClass.BackImage = @"C:\images\back.png";
            var attachment = assetClass.Attachments.Single();

            Action act = () => assetClass.RemoveItemFromAttachments(attachment);

            act.Should().Throw<InvalidOperationException>();
            assetClass.Attachments.Should().ContainSingle();
        }

        [TestMethod]
        public void RemoveAttachmentAndDependencies_WhenFrontImage_ShouldClearImageAndRemove()
        {
            var assetClass = new AssetClass();
            assetClass.FrontImage = @"C:\images\front.png";
            var attachment = assetClass.Attachments.Single();

            assetClass.RemoveAttachmentAndDependencies(attachment);

            assetClass.FrontImage.Should().BeNull();
            assetClass.Attachments.Should().BeEmpty();
        }

        [TestMethod]
        public void RemoveAttachmentAndDependencies_WhenBackImage_ShouldClearImageAndRemove()
        {
            var assetClass = new AssetClass();
            assetClass.BackImage = @"C:\images\back.png";
            var attachment = assetClass.Attachments.Single();

            assetClass.RemoveAttachmentAndDependencies(attachment);

            assetClass.BackImage.Should().BeNull();
            assetClass.Attachments.Should().BeEmpty();
        }

        [TestMethod]
        public void RemoveAttachmentAndDependencies_NonImageAttachment_ShouldRemoveAndKeepImages()
        {
            var assetClass = new AssetClass();
            assetClass.FrontImage = @"C:\images\front.png";
            assetClass.AddAttachment(new Attachment { FilePath = @"C:\docs\manual.pdf" });
            var manual = assetClass.Attachments.Single(a => a.FilePath == @"C:\docs\manual.pdf");

            assetClass.RemoveAttachmentAndDependencies(manual);

            assetClass.FrontImage.Should().Be(@"C:\images\front.png");
            assetClass.Attachments.Select(a => a.FilePath).Should()
                .BeEquivalentTo(new[] { @"C:\images\front.png" });
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Create_WithImageAttachmentRemoved_ShouldFailValidation(bool front)
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);
            var deviceType = Helper.TestData.DeviceTypes.First();

            var assetClass = new AssetClass
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Class With Orphan Image",
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                Manufacturer = new PnoObjectReference<Organization>(Guid.NewGuid()),
                HeightU = 1.0,
                PowerSupply = SlcAsset_Management.Enums.PowerSupplyEnum.AC,
            };
            if (front)
            {
                assetClass.FrontImage = "image.png";
            }
            else
            {
                assetClass.BackImage = "image.png";
            }
            assetClass.Attachments.Clear();

            // Act
            Action act = () => Helper.AssetManagement.AssetClasses.Create(assetClass);

            // Assert
            act.Should().Throw<Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions.ValidationException>()
                .WithMessage(front
                    ? "*Front Image must be part of the Attachments*"
                    : "*Back Image must be part of the Attachments*");
        }

        [TestMethod]
        public void Create_WithImagesAssignedDirectly_ShouldPersistImagesAsAttachments()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);
            var deviceType = Helper.TestData.DeviceTypes.First();

            var assetClass = new AssetClass
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Class With Image",
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                Manufacturer = new PnoObjectReference<Organization>(Guid.NewGuid()),
                HeightU = 1.0,
                PowerSupply = SlcAsset_Management.Enums.PowerSupplyEnum.AC,
            };
            assetClass.FrontImage = @"C:\images\front.png";
            assetClass.BackImage = @"C:\images\back.png";

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
                created.Changed.Should().BeFalse();
            }

            created.FrontImage = "updated-front.png";
            created.BackImage = "updated-back.png";
            Helper.AssetManagement.AssetClasses.Update(created);
            var updated = Helper.AssetManagement.AssetClasses.Read(new TRUEFilterElement<AssetClass>()).Single();

            using (new AssertionScope())
            {
                updated.FrontImage.Should().Be("updated-front.png");
                updated.BackImage.Should().Be("updated-back.png");
                updated.Attachments.Select(a => a.FilePath).Should()
                    .BeEquivalentTo(@"C:\images\front.png", @"C:\images\back.png",
                        "updated-front.png", "updated-back.png");
                updated.Changed.Should().BeFalse();
            }
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Deserialize_WithImagesAndAttachments_ShouldPreserveMetadataWithoutDuplicates(bool imagesFirst)
        {
            const string images = @"""FrontImage"":""image.png"",""BackImage"":""image.png""";
            const string attachments = @"""Attachments"":[{""FilePath"":""image.png"",""AttachedBy"":""user""}]";
            var json = imagesFirst
                ? "{" + images + "," + attachments + "}"
                : "{" + attachments + "," + images + "}";

            var assetClass = Newtonsoft.Json.JsonConvert.DeserializeObject<AssetClass>(json);

            assetClass.FrontImage.Should().Be("image.png");
            assetClass.BackImage.Should().Be("image.png");
            var attachment = assetClass.Attachments.Should().ContainSingle().Subject;
            attachment.FilePath.Should().Be("image.png");
            attachment.AttachedBy.Should().Be("user");
        }

        [TestMethod]
        public void Deserialize_WithImagesOnly_ShouldAddAttachments()
        {
            var assetClass = Newtonsoft.Json.JsonConvert.DeserializeObject<AssetClass>(
                @"{""FrontImage"":""front.png"",""BackImage"":""back.png""}");

            assetClass.Attachments.Select(a => a.FilePath).Should()
                .BeEquivalentTo("front.png", "back.png");
        }

        [TestMethod]
        public void RemoveAttachmentAndDependencies_WhenSharedImage_ShouldClearBothImages()
        {
            var assetClass = new AssetClass
            {
                FrontImage = "shared.png",
                BackImage = "shared.png",
            };

            assetClass.RemoveAttachmentAndDependencies(assetClass.Attachments.Single());

            assetClass.FrontImage.Should().BeNull();
            assetClass.BackImage.Should().BeNull();
            assetClass.Attachments.Should().BeEmpty();
        }

        [TestMethod]
        public void Create_WithAttachments_ShouldPersistAndReadBackCorrectly()
        {
            // Arrange
            Helper.PopulateWithDemoData(upTo: DemoDataLayer.DeviceTypes);
            var deviceType = Helper.TestData.DeviceTypes.First();

            var attachedAt = new DateTime(2024, 1, 15, 8, 30, 0, DateTimeKind.Utc);
            var attachedBy = Convert.ToString(Guid.NewGuid());

            var assetClass = new AssetClass
            {
                Identifier = Guid.NewGuid().ToString(),
                Name = "Class With Attachments",
                DeviceTypeId = new SdmObjectReference<DeviceType>(deviceType.Identifier),
                Manufacturer = new PnoObjectReference<Organization>(Guid.NewGuid()),
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
