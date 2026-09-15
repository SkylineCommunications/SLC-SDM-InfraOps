namespace Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences
{
    using System;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

    public readonly struct PnoObjectReference<T> : IObjectReference<T>, IEquatable<PnoObjectReference<T>> where T : ApiObject
    {
        //
        // Summary:
        //     Gets the identifier of the referenced SDM object.
        public Guid Identifier { get; }

        string IObjectReference<T>.Identifier => (string)this;

        //
        // Summary:
        //     Initializes a new instance of the Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.PnoObjectReference`1
        //     struct.
        //
        // Parameters:
        //   id:
        //     The identifier of the SDM object.
        public PnoObjectReference(Guid id)
        {
            Identifier = id;
        }

        //
        // Summary:
        //     Implicitly converts an Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.PnoObjectReference`1 to a string.
        //
        //
        // Parameters:
        //   id:
        //     The SDM object reference to convert.
        public static implicit operator string(PnoObjectReference<T> id)
        {
            if(id == Guid.Empty)
            {
                return null;
            }

            return System.Convert.ToString(id.Identifier);
        }

        //
        // Summary:
        //     Implicitly converts an Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.PnoObjectReference`1 to a string.
        //
        //
        // Parameters:
        //   id:
        //     The SDM object reference to convert.
        public static implicit operator Guid(PnoObjectReference<T> id)
        {
            return id.Identifier;
        }

        //
        // Summary:
        //     Explicitly converts a string to an Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.PnoObjectReference`1.
        //
        //
        // Parameters:
        //   id:
        //     The identifier string to convert.
        //
        // Returns:
        //     A new Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.PnoObjectReference`1 if the identifier is not null
        //     or empty; otherwise, the default value.
        public static explicit operator PnoObjectReference<T>(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return default(PnoObjectReference<T>);
            }

            if (!Guid.TryParse(id, out var result))
            {
                throw new InvalidOperationException($"The string '{id}' is not a valid GUID.");
            }

            if (result == Guid.Empty)
            {
                return default(PnoObjectReference<T>);
            }

            return new PnoObjectReference<T>(result);
        }

        //
        // Summary:
        //     Explicitly converts a string to an Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.PnoObjectReference`1.
        //
        //
        // Parameters:
        //   id:
        //     The identifier string to convert.
        //
        // Returns:
        //     A new Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.PnoObjectReference`1 if the identifier is not null
        //     or empty; otherwise, the default value.
        public static explicit operator PnoObjectReference<T>(Guid id)
        {
            if (id == Guid.Empty)
            {
                return default(PnoObjectReference<T>);
            }

            return new PnoObjectReference<T>(id);
        }

        //
        // Summary:
        //     Implicitly converts an Skyline.DataMiner.SDM.SdmObject`1 to an Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.PnoObjectReference`1.
        //
        //
        // Parameters:
        //   sdmObject:
        //     The SDM object to convert.
        //
        // Returns:
        //     A new Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.PnoObjectReference`1 if the object and its identifier
        //     are not null; otherwise, the default value.
        public static implicit operator PnoObjectReference<T>(ApiObject pnoObject)
        {
            if (pnoObject?.Id == Guid.Empty)
            {
                return default(PnoObjectReference<T>);
            }

            return new PnoObjectReference<T>(pnoObject.Id);
        }

        //
        // Summary:
        //     Determines whether two Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.PnoObjectReference`1 instances are
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
        public static bool operator ==(PnoObjectReference<T> left, PnoObjectReference<T> right)
        {
            return left.Equals(right);
        }

        //
        // Summary:
        //     Determines whether two Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.PnoObjectReference`1 instances are
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
        public static bool operator !=(PnoObjectReference<T> left, PnoObjectReference<T> right)
        {
            return !(left == right);
        }

        //
        // Summary:
        //     Converts an object to an Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.PnoObjectReference`1.
        //
        // Parameters:
        //   obj:
        //     The object to convert. Can be an Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.PnoObjectReference`1,
        //     Skyline.DataMiner.SDM.ISdmObject`1, or string.
        //
        // Returns:
        //     An Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.PnoObjectReference`1 representing the object.
        //
        // Exceptions:
        //   T:System.InvalidOperationException:
        //     Thrown when the object cannot be converted to an Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.PnoObjectReference`1.
        public static PnoObjectReference<T> Convert(object obj)
        {
            if (obj is PnoObjectReference<T>)
            {
                return (PnoObjectReference<T>)obj;
            }

            if (!(obj is ApiObject pnoObject))
            {
                if (obj is Guid id)
                {
                    return new PnoObjectReference<T>(id);
                }

                throw new InvalidOperationException("Cannot convert " + obj?.GetType().Name + " to " + typeof(PnoObjectReference<T>).Name);
            }

            return new PnoObjectReference<T>(pnoObject.Id);
        }

        //
        // Summary:
        //     Determines whether the specified object is equal to the current Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.PnoObjectReference`1.
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
            if (obj is PnoObjectReference<T> other)
            {
                return Equals(other);
            }

            return false;
        }

        //
        // Summary:
        //     Determines whether the specified Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.PnoObjectReference`1 is
        //     equal to the current instance.
        //
        // Parameters:
        //   other:
        //     The reference to compare with the current instance.
        //
        // Returns:
        //     true if the specified reference is equal to the current instance; otherwise,
        //     false.
        public bool Equals(PnoObjectReference<T> other)
        {
            if (Identifier == Guid.Empty && other.Identifier == Guid.Empty)
            {
                return true;
            }

            if (Identifier == Guid.Empty || other.Identifier == Guid.Empty)
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
        //     Returns a string representation of the Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences.PnoObjectReference`1.
        //
        //
        // Returns:
        //     A string that represents the current reference.
        public override string ToString()
        {
            return "Ref " + typeof(T).Name + " [" + Identifier + "]";
        }
    }
}
