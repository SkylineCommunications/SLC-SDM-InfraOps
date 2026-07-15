namespace SharedCommonLibrary.Converters
{
    using Newtonsoft.Json;

    internal class InfraOpsSerialization
    {
        public static readonly JsonSerializerSettings DefaultSettings = new JsonSerializerSettings
        {
            ContractResolver = new InfraOpsContractResolver(),
            Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() },
        };
    }
}
