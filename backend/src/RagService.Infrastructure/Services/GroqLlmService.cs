using System;
using System.Collections.Generic;
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
    /// LLM service that uses the Groq API (https://api.groq.com/openai/v1).
    /// Groq's API is fully OpenAI-compatible but runs on custom LPU hardware,
    /// delivering significantly faster inference than standard cloud GPU providers.
    /// Get your free API key at: https://console.groq.com
    /// </summary>
    public class GroqLlmService : ILlmService
    {
        private const string GroqBaseUrl = "https://api.groq.com/openai/v1/chat/completions";

        private readonly HttpClient _httpClient;
        private readonly RagSettings _settings;

        public GroqLlmService(HttpClient httpClient, IOptions<RagSettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<string> GenerateCompletionAsync(string prompt, string systemInstruction = "")
        {
            if (string.IsNullOrWhiteSpace(_settings.Groq.ApiKey))
            {
                Console.WriteLine("[Warning] Groq API key is missing. Returning fallback response.");
                return SmartFallbackGenerator.GenerateSmartFallback(prompt, "Groq", _settings.Groq.LlmModel, GroqBaseUrl);
            }

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, GroqBaseUrl);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.Groq.ApiKey);

            var messages = new List<GroqChatMessage>();
            if (!string.IsNullOrWhiteSpace(systemInstruction))
            {
                messages.Add(new GroqChatMessage { Role = "system", Content = systemInstruction });
            }
            messages.Add(new GroqChatMessage { Role = "user", Content = prompt });

            var payload = new GroqChatRequest
            {
                Model = _settings.Groq.LlmModel,
                Messages = messages,
                Stream = false
            };

            requestMessage.Content = JsonContent.Create(payload);

            try
            {
                var response = await _httpClient.SendAsync(requestMessage);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<GroqChatResponse>();
                if (result?.Choices != null && result.Choices.Length > 0)
                {
                    return result.Choices[0].Message?.Content ?? string.Empty;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Warning] Failed to fetch Groq completion: {ex.Message}. Returning fallback message.");
                return SmartFallbackGenerator.GenerateSmartFallback(prompt, "Groq", _settings.Groq.LlmModel, GroqBaseUrl);
            }
        }

        private class GroqChatRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } = string.Empty;

            [JsonPropertyName("messages")]
            public List<GroqChatMessage> Messages { get; set; } = new();

            [JsonPropertyName("stream")]
            public bool Stream { get; set; } = false;
        }

        private class GroqChatMessage
        {
            [JsonPropertyName("role")]
            public string Role { get; set; } = string.Empty;

            [JsonPropertyName("content")]
            public string Content { get; set; } = string.Empty;
        }

        private class GroqChatResponse
        {
            [JsonPropertyName("choices")]
            public GroqChatChoice[] Choices { get; set; } = Array.Empty<GroqChatChoice>();
        }

        private class GroqChatChoice
        {
            [JsonPropertyName("message")]
            public GroqChatMessage? Message { get; set; }
        }
    }
}
