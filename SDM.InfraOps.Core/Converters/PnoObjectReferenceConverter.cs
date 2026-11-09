namespace Skyline.DataMiner.SDM.InfraOps.Core.Converters
{
    using System;
    using Newtonsoft.Json;
    using Skyline.DataMiner.SDM.InfraOps.Core.ApiReferences;
    using Skyline.DataMiner.Solutions.PeopleAndOrganizations.API;

    internal class PnoObjectReferenceConverter<T> : JsonConverter<PnoObjectReference<T>> where T : ApiObject
    {
        public override PnoObjectReference<T> ReadJson(JsonReader reader, Type objectType, PnoObjectReference<T> existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return (PnoObjectReference<T>)(string)reader.Value;
        }

        public override void WriteJson(JsonWriter writer, PnoObjectReference<T> value, JsonSerializer serializer)
        {
            writer.WriteValue((string)value);
        }
    }
}
