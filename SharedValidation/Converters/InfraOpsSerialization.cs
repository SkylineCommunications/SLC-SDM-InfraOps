namespace SharedCommonLibrary.Converters
{
    using Newtonsoft.Json;

    public class InfraOpsSerialization
    {
        public static readonly JsonSerializerSettings DefaultSettings = new JsonSerializerSettings
        {
            ContractResolver = new InfraOpsContractResolver(),
        };
    }
}
