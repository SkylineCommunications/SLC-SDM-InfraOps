namespace Skyline.DataMiner.SDM.InfraOps.Core.Converters
{
    using System;
    using Newtonsoft.Json;
    using Skyline.DataMiner.Utils.SecureCoding.SecureSerialization.Json;
    using Skyline.DataMiner.Utils.SecureCoding.SecureSerialization.Json.Newtonsoft;

    public static class InfraOpsJsonConvert
    {
        public static T DeserializeObject<T>(string json)
        {
            return SecureNewtonsoftDeserialization.DeserializeObject<T>(json, InfraOpsSerialization.DefaultSettings);
        }

        public static object DeserializeObject(string json, Type type)
        {
            //TODO: verify if there is a SecureNewtonsoftDeserialization alternative.
            if (json == null)
            {
                throw new ArgumentException("'json' cannot be null.", "json");
            }

            var settings = InfraOpsSerialization.DefaultSettings;

            if (settings.TypeNameHandling != TypeNameHandling.None)
            {
                throw new InsecureSerializationSettingsException($"Setting the TypeNameHandling to {settings.TypeNameHandling} may result in insecure deserialization.");
            }

            settings.SerializationBinder = null;

            return JsonConvert.DeserializeObject(json, type, settings);
        }

        public static string SerializeObject<T>(T value)
        {
            return JsonConvert.SerializeObject(value, typeof(T), InfraOpsSerialization.DefaultSettings);
        }

        public static string SerializeObject(object value, Type type)
        {
            return JsonConvert.SerializeObject(value, type, InfraOpsSerialization.DefaultSettings);
        }
    }
}
