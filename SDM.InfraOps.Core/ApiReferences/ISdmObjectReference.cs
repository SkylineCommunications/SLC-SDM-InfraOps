namespace Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences
{
    using System;

    //
    // Summary:
    //     Represents a reference to an SDM object of type T.
    //
    // Type parameters:
    //   T:
    //     The type of SDM object being referenced.
    public readonly struct ISdmObjectReference<T> : IObjectReference<T>, IEquatable<ISdmObjectReference<T>> where T : ISdmObject
    {
        //
        // Summary:
        //     Gets the identifier of the referenced SDM object.
        public string Identifier { get; }

        //
        // Summary:
        //     Initializes a new instance of the Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.ISdmObjectReference`1
        //     struct.
        //
        // Parameters:
        //   id:
        //     The identifier of the SDM object.
        public ISdmObjectReference(string id)
        {
            Identifier = id;
        }

        //
        // Summary:
        //     Implicitly converts an Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.ISdmObjectReference`1 to a string.
        //
        //
        // Parameters:
        //   id:
        //     The SDM object reference to convert.
        public static implicit operator string(ISdmObjectReference<T> id)
        {
            return id.Identifier;
        }

        //
        // Summary:
        //     Explicitly converts a string to an Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.ISdmObjectReference`1.
        //
        //
        // Parameters:
        //   id:
        //     The identifier string to convert.
        //
        // Returns:
        //     A new Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.ISdmObjectReference`1 if the identifier is not null
        //     or empty; otherwise, the default value.
        public static explicit operator ISdmObjectReference<T>(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return default(ISdmObjectReference<T>);
            }

            return new ISdmObjectReference<T>(id);
        }
        
        //
        // Summary:
        //     Implicitly converts an ISdmObject to an Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.ISdmObjectReference`1.
        //
        //
        // Parameters:
        //   sdmObject:
        //     The SDM object to convert.
        //
        // Returns:
        //     A new Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.ISdmObjectReference`1 if the object and its identifier
        //     are not null; otherwise, the default value.
        public static implicit operator ISdmObjectReference<T>(T sdmObject)
        {
            if (sdmObject?.Identifier == null)
            {
                return default(ISdmObjectReference<T>);
            }

            return new ISdmObjectReference<T>(sdmObject.Identifier);
        }

        //
        // Summary:
        //     Widens an Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.ISdmObjectReference`1 of type T to a reference
        //     of type ISdmObject, e.g. for generic storage across different referenced types.
        //
        // Parameters:
        //   reference:
        //     The reference to widen.
        public static implicit operator ISdmObjectReference<ISdmObject>(ISdmObjectReference<T> reference)
        {
            return new ISdmObjectReference<ISdmObject>(reference.Identifier);
        }

        public static bool operator ==(ISdmObjectReference<T> left, T right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ISdmObjectReference<T> left, T right)
        {
            return !(left == right);
        }

        public static bool operator ==(T left, ISdmObjectReference<T> right)
        {
            return right == left;
        }

        public static bool operator !=(T left, ISdmObjectReference<T> right)
        {
            return !(left == right);
        }

        //
        // Summary:
        //     Determines whether two Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.ISdmObjectReference`1 instances are
        //     equal.
        //
        // Parameters:
        //   left:
        //     The first reference to compare.
        //
        //   right:
        //     The second reference to compare.
        //
        // Returns:
        //     true if the references are equal; otherwise, false.
        public static bool operator ==(ISdmObjectReference<T> left, ISdmObjectReference<T> right)
        {
            return left.Equals(right);
        }

        //
        // Summary:
        //     Determines whether two Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.ISdmObjectReference`1 instances are
        //     not equal.
        //
        // Parameters:
        //   left:
        //     The first reference to compare.
        //
        //   right:
        //     The second reference to compare.
        //
        // Returns:
        //     true if the references are not equal; otherwise, false.
        public static bool operator !=(ISdmObjectReference<T> left, ISdmObjectReference<T> right)
        {
            return !(left == right);
        }

        //
        // Summary:
        //     Converts an object to an Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.ISdmObjectReference`1.
        //
        // Parameters:
        //   obj:
        //     The object to convert. Can be an Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.ISdmObjectReference`1,
        //     Skyline.DataMiner.SDM.ISdmObject`1, or string.
        //
        // Returns:
        //     An Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.ISdmObjectReference`1 representing the object.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     Thrown when the object cannot be converted to an Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.ISdmObjectReference`1.
        public static ISdmObjectReference<R> Convert<R>(object obj) where R : ISdmObject
        {
            if (obj is ISdmObjectReference<R>)
            {
                return (ISdmObjectReference<R>)obj;
            }

            if (!(obj is ISdmObject sdmObject))
            {
                if (obj is string id)
                {
                    return new ISdmObjectReference<R>(id);
                }

                throw new InvalidOperationException("Cannot convert " + obj?.GetType().Name + " to " + typeof(ISdmObjectReference<R>).Name);
            }

            return new ISdmObjectReference<R>(sdmObject.Identifier);
        }

        //
        // Summary:
        //     Determines whether the specified object is equal to the current Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.ISdmObjectReference`1.
        //
        //
        // Parameters:
        //   obj:
        //     The object to compare with the current instance.
        //
        // Returns:
        //     true if the specified object is equal to the current instance; otherwise, false.
        public override bool Equals(object obj)
        {
            if (obj is ISdmObjectReference<T> other)
            {
                return Equals(other);
            }

            return false;
        }

        //
        // Summary:
        //     Determines whether the specified Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.ISdmObjectReference`1 is
        //     equal to the current instance.
        //
        // Parameters:
        //   other:
        //     The reference to compare with the current instance.
        //
        // Returns:
        //     true if the specified reference is equal to the current instance; otherwise,
        //     false.
        public bool Equals(ISdmObjectReference<T> other)
        {
            if (string.IsNullOrEmpty(Identifier) && string.IsNullOrEmpty(other.Identifier))
            {
                return true;
            }

            if (string.IsNullOrEmpty(Identifier) || string.IsNullOrEmpty(other.Identifier))
            {
                return false;
            }

            return Identifier.Equals(other.Identifier);
        }

        //
        // Summary:
        //     Returns the hash code for this instance.
        //
        // Returns:
        //     A 32-bit signed integer hash code.
        public override int GetHashCode()
        {
            return Identifier.GetHashCode();
        }

        //
        // Summary:
        //     Returns a string representation of the Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.ISdmObjectReference`1.
        //
        //
        // Returns:
        //     A string that represents the current reference.
        public override string ToString()
        {
            return "Ref " + typeof(T).Name + " [" + Identifier + "]";
        }

        public static ISdmObjectReference<T> To(T sdmObject)
        {
            if (sdmObject?.Identifier == null)
            {
                return default(ISdmObjectReference<T>);
            }
            return new ISdmObjectReference<T>(sdmObject.Identifier);
        }
    }
}