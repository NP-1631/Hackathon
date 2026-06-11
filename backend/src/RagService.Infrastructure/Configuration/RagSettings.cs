namespace RagService.Infrastructure.Configuration
{
    public class RagSettings
    {
        public string LlmProvider { get; set; } = "Ollama"; // "Ollama" or "OpenAI"
        public string EmbeddingProvider { get; set; } = "Ollama"; // "Ollama", "OpenAI", or "Jina"

        public OpenAiSettings OpenAi { get; set; } = new();
        public OllamaSettings Ollama { get; set; } = new();
        public GroqSettings Groq { get; set; } = new();
        public JinaSettings Jina { get; set; } = new();

        public string DocsFolder { get; set; } = "docs";
        public int ChunkSize { get; set; } = 500;
        public int ChunkOverlap { get; set; } = 100;
    }

    public class OpenAiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string LlmModel { get; set; } = "gpt-4o-mini";
        public string EmbeddingModel { get; set; } = "text-embedding-3-small";
    }

    public class OllamaSettings
    {
        public string BaseUrl { get; set; } = "http://localhost:11434";
        public string LlmModel { get; set; } = "llama3";
        public string EmbeddingModel { get; set; } = "nomic-embed-text";
    }

    public class GroqSettings
    {
        // Groq API is OpenAI-compatible. Base URL: https://api.groq.com/openai/v1
        // Get free key at: https://console.groq.com
        public string ApiKey { get; set; } = string.Empty;
        public string LlmModel { get; set; } = "llama-3.1-8b-instant";
        // Note: Groq does not provide embedding models. Use Jina or Ollama for embeddings.
    }

    public class JinaSettings
    {
        // Free tier: 1M tokens/month, no credit card needed.
        // Get free key at: https://jina.ai
        public string ApiKey { get; set; } = string.Empty;
        public string EmbeddingModel { get; set; } = "jina-embeddings-v2-base-en";
    }
}
