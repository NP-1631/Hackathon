# AlphaRAG - Team Alpha RAG AI Assistant

This project is organized into two separate directories:

```
Infinite Hackaton/
├── backend/        ← C# ASP.NET Core RAG API (runs on http://localhost:5174)
│   ├── RagService.sln
│   ├── docs/           ← Markdown documents to ingest
│   └── src/
│       ├── RagService.Api/         ← Web API entry point
│       ├── RagService.Core/        ← Domain models, interfaces, DTOs
│       └── RagService.Infrastructure/  ← Service implementations
│
└── frontend/       ← Plain HTML/CSS/JS chatbot UI (runs on http://localhost:3000)
    ├── index.html
    ├── app.js
    ├── style.css
    └── package.json
```

---

## 🚀 Running the Project

### Step 1 — Start the Backend

```bash
cd backend
dotnet run --project src/RagService.Api/RagService.Api.csproj
```

The backend API will be available at: **http://localhost:5174**

> Swagger UI: http://localhost:5174/swagger

### Step 2 — Start the Frontend

Open a **new terminal window** and run:

```bash
cd frontend
npm start
```

The frontend UI will be available at: **http://localhost:3000**

---

## ⚙️ Configuration

Backend settings are in `backend/src/RagService.Api/appsettings.json`:

| Setting | Default | Description |
|---|---|---|
| `LlmProvider` | `Ollama` | LLM provider: `Ollama`, `OpenAI`, or `Groq` |
| `EmbeddingProvider` | `Ollama` | Embedding provider: `Ollama`, `OpenAI`, or `Jina` |
| `DocsFolder` | `docs` | Path to the folder containing documents to index |

### Provider API Keys

- **OpenAI**: Set `RagSettings.OpenAi.ApiKey`
- **Groq**: Set `RagSettings.Groq.ApiKey`
- **Jina**: Set `RagSettings.Jina.ApiKey`
- **Ollama**: Ensure Ollama is running at `http://localhost:11434`

---

## 🔧 Changing the Backend Port

If you change the backend port, update `API_BASE_URL` in `frontend/app.js`:

```js
const API_BASE_URL = 'http://localhost:5174'; // Change to your backend port
```

---

## 📋 API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/health` | Check system health and current provider settings |
| `POST` | `/ingest` | Re-scan and index documents in the docs folder |
| `POST` | `/chat` | Send a message and get a RAG-generated answer |
