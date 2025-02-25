using System.Text.Json.Serialization;

namespace WebApplication1.API_Models
{
    public class KommuneInformasjon
    {
        [JsonPropertyName("kommunenavn")]
        public string? Kommunenavn { get; set; }
        [JsonPropertyName("kommunenummer")]
        public string? Kommunenummer { get; set; }
        [JsonPropertyName("fylkesnavn")]
        public string? Fylkesnavn { get; set; }
    }
}