namespace SDM.AssetManagement.Tests.Validation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using FluentAssertions;
    using FluentAssertions.Execution;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    /// <summary>
    /// Tests the null-entity rule of <see cref="ValidatorBase{T}"/> through a minimal test subclass:
    /// a null entity never throws <see cref="ArgumentNullException"/>, it yields the failure of
    /// <c>CreateNullEntityResult</c>, and bulk validation keeps one result per item in the original order.
    /// </summary>
    [TestClass]
    public class ValidatorBaseNullEntityTests
    {
        private const string NullMessage = "Entity cannot be null.";

        private TestValidator _validator = null!;

        [TestInitialize]
        public void Setup()
        {
            _validator = new TestValidator();
        }

        [DataTestMethod]
        [DataRow(RepositoryAction.Create)]
        [DataRow(RepositoryAction.Update)]
        [DataRow(RepositoryAction.Delete)]
        public void Validate_WithNullEntity_ShouldReturnNullEntityFailure(RepositoryAction action)
        {
            var result = _validator.Validate(null!, action);

            using (new AssertionScope())
            {
                result.IsValid.Should().BeFalse();
                result.GetFailReason(TestField.Entity).Should().Be(NullMessage);
                _validator.ReceivedLists.Should().BeEmpty();
            }
        }

        [TestMethod]
        public void ProtectedValidate_WithNullEntity_ShouldReturnNullEntityFailure()
        {
            var result = _validator.CallValidate(null!);

            result.IsValid.Should().BeFalse();
            result.GetFailReason(TestField.Entity).Should().Be(NullMessage);
        }

        [TestMethod]
        public void ProtectedValidateForDelete_WithNullEntity_ShouldReturnNullEntityFailure()
        {
            var result = _validator.CallValidateForDelete(null!);

            result.IsValid.Should().BeFalse();
            result.GetFailReason(TestField.Entity).Should().Be(NullMessage);
        }

        [TestMethod]
        public void ValidateAndThrow_WithNullEntity_ShouldThrowValidationException()
        {
            _validator.Invoking(v => v.ValidateAndThrow(null!, RepositoryAction.Create))
                .Should().Throw<ValidationException>()
                .WithMessage("*" + NullMessage + "*");
        }

        [TestMethod]
        public void ValidateWithHandler_WithNullEntity_ShouldInvokeHandlerWithNullEntityFailure()
        {
            ValidationResult? received = null;

            _validator.ValidateWithHandler(null!, RepositoryAction.Create, r => received = r);

            received.Should().NotBeNull();
            received!.GetFailReason(TestField.Entity).Should().Be(NullMessage);
        }

        [DataTestMethod]
        [DataRow(RepositoryAction.Create)]
        [DataRow(RepositoryAction.Delete)]
        public void ValidateBulk_WithNullItems_ShouldKeepOrderAndOnlyValidateNonNullItems(RepositoryAction action)
        {
            var first = new TestEntity("first");
            var second = new TestEntity("second");

            var results = _validator.ValidateBulk(new List<TestEntity> { null!, first, null!, second }, action);

            using (new AssertionScope())
            {
                results.Should().HaveCount(4);
                results[0].GetFailReason(TestField.Entity).Should().Be(NullMessage);
                results[1].IsValid.Should().BeTrue();
                results[2].GetFailReason(TestField.Entity).Should().Be(NullMessage);
                results[3].IsValid.Should().BeTrue();
                _validator.ReceivedLists.Should().ContainSingle()
                    .Which.Should().Equal(first, second);
            }
        }

        [TestMethod]
        public void ValidateBulk_WithNullItemsAndInvalidItem_ShouldMapResultsBackToOriginalPositions()
        {
            var valid = new TestEntity("valid");
            var invalid = new TestEntity(TestValidator.InvalidName);

            var results = _validator.ValidateBulk(new List<TestEntity> { invalid, null!, valid }, RepositoryAction.Create);

            using (new AssertionScope())
            {
                results.Should().HaveCount(3);
                results[0].GetFailReason(TestField.Name).Should().Be("Name is invalid.");
                results[1].GetFailReason(TestField.Entity).Should().Be(NullMessage);
                results[2].IsValid.Should().BeTrue();
            }
        }

        [TestMethod]
        public void ValidateBulk_WithOnlyNullItems_ShouldNotCallPipeline()
        {
            var results = _validator.ValidateBulk(new List<TestEntity> { null!, null! }, RepositoryAction.Create);

            using (new AssertionScope())
            {
                results.Should().HaveCount(2);
                results.Should().OnlyContain(r => !r.IsValid && r.GetFailReason(TestField.Entity) == NullMessage);
                results[0].Should().NotBeSameAs(results[1]);
                _validator.ReceivedLists.Should().BeEmpty();
            }
        }

        [TestMethod]
        public void ValidateBulk_WithoutNullItems_ShouldPassSameListInstance()
        {
            var entities = new List<TestEntity> { new TestEntity("a"), new TestEntity("b") };

            var results = _validator.ValidateBulk(entities, RepositoryAction.Create);

            using (new AssertionScope())
            {
                results.Should().HaveCount(2);
                _validator.ReceivedLists.Should().ContainSingle()
                    .Which.Should().BeSameAs(entities);
            }
        }

        [TestMethod]
        public void ValidateBulk_WithNullOrEmptyList_ShouldReturnEmpty()
        {
            using (new AssertionScope())
            {
                _validator.ValidateBulk(null!, RepositoryAction.Create).Should().BeEmpty();
                _validator.ValidateBulk(new List<TestEntity>(), RepositoryAction.Create).Should().BeEmpty();
                _validator.ValidateBulk(null!, RepositoryAction.Delete).Should().BeEmpty();
            }
        }

        [TestMethod]
        public void ValidateBulkAndThrow_WithNullItem_ShouldThrowAggregateWithNullFailure()
        {
            Action act = () => _validator.ValidateBulkAndThrow(new List<TestEntity> { new TestEntity("a"), null! }, RepositoryAction.Create);

            act.Should().Throw<ValidationAggregateException>()
                .Which.InnerExceptions.Should().ContainSingle()
                .Which.Message.Should().Contain(NullMessage);
        }

        [TestMethod]
        public void ValidateBulkWithHandler_WithNullItem_ShouldInvokeHandlerWithOrderedResults()
        {
            List<ValidationResult>? received = null;

            _validator.ValidateBulkWithHandler(new List<TestEntity> { new TestEntity("a"), null! }, RepositoryAction.Create, r => received = r);

            using (new AssertionScope())
            {
                received.Should().NotBeNull().And.HaveCount(2);
                received![0].IsValid.Should().BeTrue();
                received[1].GetFailReason(TestField.Entity).Should().Be(NullMessage);
            }
        }

        private enum TestField
        {
            Name,
            Entity,
        }

        private sealed class TestEntity
        {
            public TestEntity(string name)
            {
                Name = name;
            }

            public string Name { get; }
        }

        private sealed class TestValidator : ValidatorBase<TestEntity>
        {
            public const string InvalidName = "invalid";

            public List<List<TestEntity>> ReceivedLists { get; } = new List<List<TestEntity>>();

            public ValidationResult CallValidate(TestEntity entity)
            {
                return Validate(entity);
            }

            public ValidationResult CallValidateForDelete(TestEntity entity)
            {
                return ValidateForDelete(entity);
            }

            protected override ValidationResult CreateNullEntityResult()
            {
                var result = new ValidationResult();
                result.AddFailReason(TestField.Entity, NullMessage);
                return result;
            }

            protected override List<ValidationResult> ValidateBulkNoDatabaseBusinessRules(List<TestEntity> entities)
            {
                ReceivedLists.Add(entities);
                entities?.Should().NotContainNulls();

                return entities.Select(e =>
                {
                    var result = new ValidationResult();
                    if (e.Name == InvalidName)
                    {
                        result.AddFailReason(TestField.Name, "Name is invalid.");
                    }

                    return result;
                }).ToList();
            }

            protected override List<ValidationResult> ValidateBulkNoDatabaseBatchConflicts(List<TestEntity> entities)
            {
                return entities.Select(_ => new ValidationResult()).ToList();
            }

            protected override List<ValidationResult> ValidateBulkWithDatabase(List<TestEntity> entities)
            {
                return entities.Select(_ => new ValidationResult()).ToList();
            }

            protected override List<ValidationResult> ValidateBulkForDelete(List<TestEntity> entities)
            {
                ReceivedLists.Add(entities);
                entities?.Should().NotContainNulls();

                return base.ValidateBulkForDelete(entities);
            }
        }
    }
}
