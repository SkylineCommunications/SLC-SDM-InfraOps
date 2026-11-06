namespace Skyline.DataMiner.SDM.InfraOps.Core.Converters
{
    using System;
    using Newtonsoft.Json;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;

    internal class ISdmObjectReferenceConverter<T> : JsonConverter<ISdmObjectReference<T>> where T : ISdmObject
    {
        public override ISdmObjectReference<T> ReadJson(JsonReader reader, Type objectType, ISdmObjectReference<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return (ISdmObjectReference<T>)(string)reader.Value;
        }

        public override void WriteJson(JsonWriter writer, ISdmObjectReference<T> value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Identifier);
        }
    }
}
