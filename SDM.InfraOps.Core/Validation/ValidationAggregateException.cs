namespace Skyline.DataMiner.Utils.InfraOps.SharedCommonLibrary.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents errors that occur during validation in the Validation process.
    /// </summary>
    [Serializable]
    public class ValidationAggregateException : AggregateException
    {
        public ValidationAggregateException()
        {
        }

        public ValidationAggregateException(string message) : base(message)
        {
        }

        public ValidationAggregateException(IEnumerable<ValidationException> innerExceptions) : base(innerExceptions)
        {
        }

        public ValidationAggregateException(params ValidationException[] innerExceptions) : base(innerExceptions)
        {
        }

        public ValidationAggregateException(string message, ValidationException innerException) : base(message, innerException)
        {
        }

        public ValidationAggregateException(string message, IEnumerable<ValidationException> innerExceptions) : base(message, innerExceptions)
        {
        }

        public ValidationAggregateException(string message, params ValidationException[] innerExceptions) : base(message, innerExceptions)
        {
        }

        protected ValidationAggregateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public new ReadOnlyCollection<ValidationException> InnerExceptions
        {
            get
            {
                return new ReadOnlyCollection<ValidationException>(base.InnerExceptions.Cast<ValidationException>().ToList());
            }
        }
    }
}
