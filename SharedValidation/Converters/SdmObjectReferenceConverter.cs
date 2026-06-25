namespace SharedCommonLibrary.Converters
{
    using System;

    using Newtonsoft.Json;

    using Skyline.DataMiner.SDM;

    internal class SdmObjectReferenceConverter<T> : JsonConverter<SdmObjectReference<T>> where T : SdmObject<T>
    {
        public override SdmObjectReference<T> ReadJson(JsonReader reader, Type objectType, SdmObjectReference<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return new SdmObjectReference<T>((string)reader.Value);
        }

        public override void WriteJson(JsonWriter writer, SdmObjectReference<T> value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Identifier);
        }
    }
}
