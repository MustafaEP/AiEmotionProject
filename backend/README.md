# Backend API - AI Emotion Project

.NET 8.0 based RESTful API service.

## 🌐 Live Link

**Swagger UI**: [https://aiemotionproject.onrender.com/swagger/index.html](https://aiemotionproject.onrender.com/swagger/index.html)

You can view and test the API documentation by opening the link above in your browser.

## 🚀 Local Setup

### Requirements

- .NET 8.0 SDK
- SQLite (automatically created)

### Installation

```bash
dotnet restore
```

### Database Migration

Migrations are automatically run when the application starts.

### Running

```bash
dotnet run
```

The API will run at `http://localhost:5000` (or `https://localhost:5001`).

Swagger UI: `http://localhost:5000/swagger`

## 📡 API Endpoints

### Health Check

- `GET /health` - System health check

### Emotion Analysis

- `POST /api/emotion/analyze` - Text emotion analysis
- `POST /api/SyncAnalyze` - Synchronous analysis and save

### Emotion Records

- `GET /api/EmotionRecords` - List all records (supports filtering and pagination)
- `GET /api/EmotionRecords/{id}` - Get single record
- `DELETE /api/EmotionRecords/{id}` - Delete record

### Query Parameters (GET /api/EmotionRecords)

- `username` - Filter by username
- `label` - Filter by emotion label (positive/negative/neutral)
- `fromUtc` - Start date
- `toUtc` - End date
- `page` - Page number (default: 1)
- `pageSize` - Page size (default: 20, maximum: 100)

## 🔧 Configuration

### Environment Variables

- `SELF_BASE_URL` - Own API base URL (default: `https://aiemotionproject.onrender.com/`)
- `EMOTION_SERVICE_BASE_URL` - AI service base URL (default: `https://mustafaep-emotion-analyzer.hf.space`)
- `PORT` - Port to listen on (automatic for Render)

### appsettings.json

```json
{
  "Self": {
    "BaseUrl": "https://aiemotionproject.onrender.com/"
  },
  "Cors": {
    "AllowedOrigins": [
      "https://ai-emotion-project-llej9t1cm-mustafa-erhans-projects.vercel.app"
    ]
  },
  "EmotionService": {
    "BaseUrl": "https://mustafaep-emotion-analyzer.hf.space",
    "MaxRetries": 3,
    "RetryDelayMs": 700
  }
}
```

## 🗄️ Database

SQLite database is used:
- Development: `emotiondata.db` (in project directory)
- Production: `/var/data/emotiondata.db` (Render Persistent Disk)

## 📦 Technologies

- .NET 8.0
- Entity Framework Core
- SQLite
- Swagger/OpenAPI
- HttpClient Factory

## 🔒 Security Features

- ✅ CORS policy (Specific origins in production)
- ✅ Swagger only active in Development mode
- ✅ Input validation (Data Annotations)
- ✅ Error handling middleware
- ✅ Structured logging
- ✅ Health check endpoint

## 🐳 Docker

```bash
docker build -t ai-emotion-backend .
docker run -p 5000:5000 ai-emotion-backend
```

## 📝 Recent Changes

- Input validation added (Data Annotations)
- CORS policy restricted for production
- Swagger only active in development mode
- Error handling middleware added
- Health check endpoint added
- Hardcoded URLs moved to configuration
- Retry mechanism improved with exponential backoff
- Logging configuration added
- Max length constraints added to models
- Database indexes added
