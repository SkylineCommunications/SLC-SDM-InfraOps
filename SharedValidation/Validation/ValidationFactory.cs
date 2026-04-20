namespace Skyline.DataMiner.Utils.InfraOps.Common.Validation
{
    using System;
    using System.Collections.Generic;
    using Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Validations;

    public sealed class ValidationFactory<T1> where T1 : class
    {
        private readonly List<ValidationFieldEntry> _validations = new List<ValidationFieldEntry>();

        private ValidationFactory(Func<ValidationData, ValidationResult> validateAction)
        {
            _validations.Add(new ValidationFieldEntry
            {
                HasChanged = (dat) => true,
                ValidateAction = validateAction,
            });
        }

        private ValidationFactory(Func<ValidationData, bool> hasChanged, Func<ValidationData, ValidationResult> validateAction)
        {
            _validations.Add(new ValidationFieldEntry
            {
                HasChanged = hasChanged,
                ValidateAction = validateAction,
            });
        }

        public ValidationFactory<T1> AddValidation(Func<ValidationData, bool> hasChanged, Func<ValidationData, ValidationResult> validateAction)
        {
            _validations.Add(new ValidationFieldEntry
            {
                HasChanged = hasChanged,
                ValidateAction = validateAction,
            });

            return this;
        }

        public ValidationFactory<T1> AddValidation(Func<ValidationData, ValidationResult> validateAction)
        {
            _validations.Add(new ValidationFieldEntry
            {
                HasChanged = (dat) => true,
                ValidateAction = validateAction,
            });

            return this;
        }

        public bool Validate(T1 obj, ValidatorContext<T1> context, out ValidationResult result)
        {
            var dat = new ValidationData(obj, context);
            result = new ValidationResult();
            foreach (var validation in _validations)
            {
                if (validation.HasChanged(dat))
                {
                    result.CombineResults(validation.ValidateAction(dat));
                    if (context.ReturnWhenInvalid && !result.IsValid)
                    {
                        break;
                    }
                }
            }

            return result.IsValid;
        }

        public static ValidationFactory<T1> PrepareValidation(Func<ValidationData, bool> hasChanged, Func<ValidationData, ValidationResult> validateAction)
        {
            return new ValidationFactory<T1>(hasChanged, validateAction);
        }

        public static ValidationFactory<T1> PrepareValidation(Func<ValidationData, ValidationResult> validateAction)
        {
            return new ValidationFactory<T1>(validateAction);
        }

        public class ValidationData
        {
            public ValidationData(T1 obj, ValidatorContext<T1> context)
            {
                Object = obj;
                Context = context;
            }

            public T1 Object { get; }

            public ValidatorContext<T1> Context { get; }
        }

        private sealed class ValidationFieldEntry
        {
            public Func<ValidationData, bool> HasChanged { get; set; }

            public Func<ValidationData, ValidationResult> ValidateAction { get; set; }
        }
    }
}