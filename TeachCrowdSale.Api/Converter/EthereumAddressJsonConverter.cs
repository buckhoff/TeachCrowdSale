using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TeachCrowdSale.Api.Converter;

public class EthereumAddressJsonConverter : JsonConverter<string>
{
    private static readonly Regex AddressRegex = new Regex("^0x[0-9a-fA-F]{40}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }
        
        if (!AddressRegex.IsMatch(value))
        {
            throw new JsonException("Invalid Ethereum address format");
        }
        
        // Normalize to lowercase
        return value.ToLowerInvariant();
    }
    
    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}