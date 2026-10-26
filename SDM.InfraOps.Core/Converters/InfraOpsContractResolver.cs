namespace Skyline.DataMiner.SDM.InfraOps.Core.Converters
{
    using System;
    using System.Reflection;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    using Skyline.DataMiner.SDM;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;

    internal class InfraOpsContractResolver : DefaultContractResolver
    {
        protected override JsonConverter ResolveContractConverter(Type objectType)
        {
            if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(SdmObjectReference<>))
            {
                Type type = objectType.GenericTypeArguments[0];
                return (JsonConverter)Activator.CreateInstance(typeof(SdmObjectReferenceConverter<>).MakeGenericType(type));
            }

            if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(PnoObjectReference<>))
            {
                Type type = objectType.GenericTypeArguments[0];
                return (JsonConverter)Activator.CreateInstance(typeof(PnoObjectReferenceConverter<>).MakeGenericType(type));
            }

            if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(ISdmObjectReference<>))
            {
                Type type = objectType.GenericTypeArguments[0];
                return (JsonConverter)Activator.CreateInstance(typeof(ISdmObjectReferenceConverter<>).MakeGenericType(type));
            }

            return base.ResolveContractConverter(objectType);
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty jsonProperty = base.CreateProperty(member, memberSerialization);
            if (jsonProperty.PropertyName == "Reference" && member.DeclaringType.IsGenericType && member.DeclaringType.GetGenericTypeDefinition() == typeof(SdmObject<>))
            {
                jsonProperty.ShouldSerialize = (object _) => false;
            }

            if (jsonProperty.PropertyName == "Identifier" && member.DeclaringType.IsGenericType && member.DeclaringType.GetGenericTypeDefinition() == typeof(SdmObject<>))
            {
                jsonProperty.Writable = true;
            }

            return jsonProperty;
        }
    }
}
