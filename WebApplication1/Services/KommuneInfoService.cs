﻿using WebApplication1.API_Models;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace WebApplication1.Services
{
    public class KommuneInfoService : IKommuneInfoService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<KommuneInfoService> _logger;
        private readonly ApiSettings _apiSettings;
        private readonly string _apiBaseUrl; // Base URL for the municipality information API


        // Base URL for the API
        public KommuneInfoService(HttpClient httpClient, ILogger<KommuneInfoService> logger, IOptions<ApiSettings> apisettings, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiSettings = apisettings.Value;
            _apiBaseUrl = configuration["ApiSettings:KommuneInfoApiBaseUrl"];
        }
        public async Task<KommuneInfo> GetKommuneInfoAsync(string kommuneNr)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_apiSettings.KommuneInfoApiBaseUrl}/kommuner/{kommuneNr}");
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"KommuneInfo Response: {json}");
                var kommuneInfo = JsonSerializer.Deserialize<KommuneInfo>(json);
                return kommuneInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching KommuneInfo for {kommuneNr}: {ex.Message}");
                return null;
            }

        }
        // Asynchronously finds the municipality based on GeoJSON input
        public async Task<(string MunicipalityNumber, string MunicipalityName, string CountyName)> FindMunicipalityFromGeoJsonAsync(string geoJson)
        {
            try
            {
                // Parse the GeoJSON to extract coordinates
                using JsonDocument doc = JsonDocument.Parse(geoJson);
                JsonElement root = doc.RootElement;

                // Array to hold extracted coordinates
                double[] coordinates = null;

                // Check if the root is a FeatureCollection
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
                    // Handle single Feature
                    if (root.TryGetProperty("geometry", out JsonElement geometry) &&
                        geometry.TryGetProperty("type", out JsonElement geometryType))
                    {
                        coordinates = ExtractCoordinatesFromGeometry(geometry, geometryType);
                    }
                }
                else if (root.GetProperty("type").GetString() == "Point")
                {
                    // Handle direct Point
                    coordinates = root.GetProperty("coordinates").EnumerateArray()
                        .Select(x => x.GetDouble())
                        .ToArray();
                }

                if (coordinates == null || coordinates.Length < 2)
                {
                    _logger.LogWarning("Could not extract coordinates from GeoJSON");
                    return (null, null, null);
                }

                // Assuming coordinates[0] is longitude (ost) and coordinates[1] is latitude (nord)
                double longitude = coordinates[0];
                double latitude = coordinates[1];

                // Make API call to find municipality
                var response = await _httpClient.GetAsync(
                    $"{_apiBaseUrl}/punkt?nord={latitude}&ost={longitude}&koordsys=4258");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    // Deserialize using the KommuneInfo model
                    var kommuneInfo = JsonSerializer.Deserialize<KommuneInfo>(content, new JsonSerializerOptions
                    {
                        // case-insensitive matching
                        PropertyNameCaseInsensitive = true
                    });

                    // Return the values from the deserialized object
                    return (
                        kommuneInfo?.Kommunenummer,
                        kommuneInfo?.Kommunenavn,
                        kommuneInfo?.Fylkesnavn
                    );
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"API call failed with status code: {response.StatusCode}, Response: {errorContent}");

                    // Return null values for all three elements
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

        // Method to extract coordinates from a given geometry element based on its type
        private double[] ExtractCoordinatesFromGeometry(JsonElement geometry, JsonElement geometryType)
        {
            string type = geometryType.GetString();

            try
            {
                // Switch expression to handle different geometry types and extract coordinates
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
}