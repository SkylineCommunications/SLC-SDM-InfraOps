namespace Skyline.DataMiner.SDM.InfraOps.Core.Converters
{
    using Newtonsoft.Json;
    using Skyline.DataMiner.Utils.SecureCoding.SecureSerialization.Json.Newtonsoft;

    public static class InfraOpsJsonConvert
    {
        public static T DeserializeObject<T>(string json)
        {
            return SecureNewtonsoftDeserialization.DeserializeObject<T>(json, InfraOpsSerialization.DefaultSettings);
        }

        public static string SerializeObject<T>(T value)
        {
            return JsonConvert.SerializeObject(value, typeof(T), InfraOpsSerialization.DefaultSettings);
        }
    }
}
