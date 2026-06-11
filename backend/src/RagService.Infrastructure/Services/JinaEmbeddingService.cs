using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RagService.Core.Interfaces;
using RagService.Infrastructure.Configuration;

namespace RagService.Infrastructure.Services
{
    /// <summary>
    /// Embedding service that uses the Jina AI Embeddings API.
    /// Free tier: 1,000,000 tokens/month — no credit card required.
    /// Get your free API key at: https://jina.ai
    /// </summary>
    public class JinaEmbeddingService : IEmbeddingService
    {
        private const string JinaEmbeddingUrl = "https://api.jina.ai/v1/embeddings";

        private readonly HttpClient _httpClient;
        private readonly RagSettings _settings;

        public JinaEmbeddingService(HttpClient httpClient, IOptions<RagSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<float[]> GetEmbeddingAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(_settings.Jina.ApiKey))
            {
                Console.WriteLine("[Warning] Jina API key is missing. Using fallback mock embedding.");
                return GenerateMockEmbedding(text);
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, JinaEmbeddingUrl);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.Jina.ApiKey);

            var payload = new JinaEmbeddingRequest
            {
                Model = _settings.Jina.EmbeddingModel,
                Input = new[] { text }
            };

            requestMessage.Content = JsonContent.Create(payload);

            try
            {
                var response = await _httpClient.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<JinaEmbeddingResponse>();
                if (result?.Data != null && result.Data.Length > 0)
                {
                    return result.Data[0].Embedding ?? Array.Empty<float>();
                }
                return Array.Empty<float>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Warning] Failed to fetch Jina embedding: {ex.Message}. Using fallback mock embedding.");
                return GenerateMockEmbedding(text);
            }
        }

        /// <summary>
        /// Deterministic mock embedding for fallback when API is unreachable.
        /// </summary>
        private static float[] GenerateMockEmbedding(string text)
        {
            var mockEmbedding = new float[768]; // jina-embeddings-v2-base-en uses 768 dimensions
            var rand = new Random(text.GetHashCode());
            for (int i = 0; i < mockEmbedding.Length; i++)
            {
                mockEmbedding[i] = (float)rand.NextDouble();
            }
            return mockEmbedding;
        }

        // --- Private DTOs ---

        private class JinaEmbeddingRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("input")]
            public string[] Input { get; set; } = Array.Empty<string>();
        }

        private class JinaEmbeddingResponse
        {
            [JsonPropertyName("data")]
            public JinaEmbeddingData[] Data { get; set; } = Array.Empty<JinaEmbeddingData>();
        }

        private class JinaEmbeddingData
        {
            [JsonPropertyName("embedding")]
            public float[]? Embedding { get; set; }
        }
    }
}
