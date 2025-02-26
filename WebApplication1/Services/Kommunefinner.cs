using System.Text.Json;
using WebApplication1.API_Models;

public class Kommunefinner
{
    private readonly HttpClient _httpClient; // HttpClient til å lage API-forespørsler
    private readonly ILogger<Kommunefinner> _logger;
    private readonly string _apiBaseUrl; // Base-URL for kommunefinner API

    // Konstruktør for å initialisere tjenesten med avhengigheter
    public Kommunefinner(
        HttpClient httpClient,
        ILogger<Kommunefinner> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiBaseUrl = configuration["ApiSettings:KommuneInfoApiBaseUrl"]; // Hent base-URL fra konfigurasjon
    }

    // (Asynkront) Finner kommunen basert på GeoJSON-informasjon og returnerer Kommunenummer, Kommunenavn og Fylkesnavn
    public async Task<(string Kommunenummer, string Kommunenavn, string Fylkesnavn)> FinnKommuneFraGeoJsonAsync(string geoJson)
    {
        try
        {
            // Analyser GeoJSON for å finne koordinater
            using JsonDocument doc = JsonDocument.Parse(geoJson);
            JsonElement root = doc.RootElement;

            // Array som skal holde koordinatene
            double[] coordinates = null;

            // Sjekk om root er en FeatureCollection
            if (root.GetProperty("type").GetString() == "FeatureCollection")
            {
                if (root.TryGetProperty("features", out JsonElement features) && features.GetArrayLength() > 0)
                {
                    var firstFeature = features[0];
                    if (firstFeature.TryGetProperty("geometry", out JsonElement geometry) &&
                        geometry.TryGetProperty("type", out JsonElement geometryType))
                    {
                        coordinates = ExtractCoordinatesFromGeometry(geometry, geometryType);
                    }
                }
            }
            else if (root.GetProperty("type").GetString() == "Feature")
            {
                // Håndterer enkel Feature
                if (root.TryGetProperty("geometry", out JsonElement geometry) &&
                    geometry.TryGetProperty("type", out JsonElement geometryType))
                {
                    coordinates = ExtractCoordinatesFromGeometry(geometry, geometryType);
                }
            }
            else if (root.GetProperty("type").GetString() == "Point")
            {
                // Håndterer direkte punkt
                coordinates = root.GetProperty("coordinates").EnumerateArray()
                    .Select(x => x.GetDouble())
                    .ToArray();
            }

            if (coordinates == null || coordinates.Length < 2)
            {
                _logger.LogWarning("Could not extract coordinates from GeoJSON");
                return (null, null, null);
            }

            // Antar at koordinat[0] er lengdegrad (øst) og koordinat[1] er breddegrad (nord)
            double longitude = coordinates[0];
            double latitude = coordinates[1];

            // Kaller på API for å finne kommunen
            var response = await _httpClient.GetAsync(
                $"{_apiBaseUrl}/punkt?nord={latitude}&ost={longitude}&koordsys=4258");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                // Deserialiserer ved bruk av KommuneInfo-modellen
                var KommuneInformasjon = JsonSerializer.Deserialize<KommuneInformasjon>(content, new JsonSerializerOptions
                {
                    // case-insensitive matching
                    PropertyNameCaseInsensitive = true
                });

                // Returner verdiene fra det deserialiserte objektet
                return (
                    KommuneInformasjon?.Kommunenummer,
                    KommuneInformasjon?.Kommunenavn,
                    KommuneInformasjon?.Fylkesnavn
                );
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"API call failed with status code: {response.StatusCode}, Response: {errorContent}");

                // Returner null-verdier for alle tre elementene
                return (null, null, null);
            }
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "JSON parsing error while finding municipality from GeoJSON");
            return (null, null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding municipality from GeoJSON");
            return (null, null, null);
        }
    }

    // Metode som extracter koordinater fra et gitt geometri-element basert på typen
    private double[] ExtractCoordinatesFromGeometry(JsonElement geometry, JsonElement geometryType)
    {
        string type = geometryType.GetString();

        try
        {
            // Switch-expression som håndterer forskjellige geometrityper og extracter koordinater
            return type switch
            {
                "Point" => geometry.GetProperty("coordinates").EnumerateArray()
                    .Select(x => x.GetDouble())
                    .ToArray(),

                "LineString" => geometry.GetProperty("coordinates")[0].EnumerateArray()
                    .Select(x => x.GetDouble())
                    .ToArray(),

                "Polygon" => geometry.GetProperty("coordinates")[0][0].EnumerateArray()
                    .Select(x => x.GetDouble())
                    .ToArray(),

                _ => throw new ArgumentException($"Unsupported geometry type: {type}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, $"Could not extract coordinates for geometry type: {type}");
            return null;
        }
    }
}