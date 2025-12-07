# Backend API - AI Emotion Project

.NET 8.0 tabanlı RESTful API servisi.

## 🌐 Canlı Link

**Swagger UI**: [https://aiemotionproject.onrender.com/swagger/index.html](https://aiemotionproject.onrender.com/swagger/index.html)

Tarayıcıdan yukarıdaki linki açarak API dokümantasyonunu görüntüleyebilir ve test edebilirsiniz.

## 🚀 Yerel Çalıştırma

### Gereksinimler

- .NET 8.0 SDK
- SQLite (otomatik oluşturulur)

### Kurulum

```bash
dotnet restore
```

### Veritabanı Migration

Migration'lar uygulama başlatıldığında otomatik olarak çalıştırılır.

### Çalıştırma

```bash
dotnet run
```

API `http://localhost:5000` (veya `https://localhost:5001`) adresinde çalışacaktır.

Swagger UI: `http://localhost:5000/swagger`

## 📡 API Endpoints

### Health Check

- `GET /health` - Sistem sağlık kontrolü

### Emotion Analysis

- `POST /api/emotion/analyze` - Metin duygu analizi
- `POST /api/SyncAnalyze` - Senkron analiz ve kayıt

### Emotion Records

- `GET /api/EmotionRecords` - Tüm kayıtları listele (filtreleme ve sayfalama destekler)
- `GET /api/EmotionRecords/{id}` - Tek kayıt getir
- `DELETE /api/EmotionRecords/{id}` - Kayıt sil

### Query Parameters (GET /api/EmotionRecords)

- `username` - Kullanıcı adına göre filtrele
- `label` - Duygu etiketine göre filtrele (positive/negative/neutral)
- `fromUtc` - Başlangıç tarihi
- `toUtc` - Bitiş tarihi
- `page` - Sayfa numarası (varsayılan: 1)
- `pageSize` - Sayfa boyutu (varsayılan: 20, maksimum: 100)

## 🔧 Yapılandırma

### Environment Variables

- `SELF_BASE_URL` - Kendi API base URL'i (varsayılan: `https://aiemotionproject.onrender.com/`)
- `EMOTION_SERVICE_BASE_URL` - AI servis base URL'i (varsayılan: `https://mustafaep-emotion-analyzer.hf.space`)
- `PORT` - Dinlenecek port (Render için otomatik)

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

## 🗄️ Veritabanı

SQLite veritabanı kullanılır:
- Development: `emotiondata.db` (proje dizininde)
- Production: `/var/data/emotiondata.db` (Render Persistent Disk)

## 📦 Teknolojiler

- .NET 8.0
- Entity Framework Core
- SQLite
- Swagger/OpenAPI
- HttpClient Factory

## 🔒 Güvenlik Özellikleri

- ✅ CORS policy (Production'da spesifik origin'ler)
- ✅ Swagger sadece Development modunda aktif
- ✅ Input validation (Data Annotations)
- ✅ Error handling middleware
- ✅ Structured logging
- ✅ Health check endpoint

## 🐳 Docker

```bash
docker build -t ai-emotion-backend .
docker run -p 5000:5000 ai-emotion-backend
```

## 📝 Son Değişiklikler

- Input validation eklendi (Data Annotations)
- CORS policy production için kısıtlandı
- Swagger sadece development modunda aktif
- Error handling middleware eklendi
- Health check endpoint eklendi
- Hardcoded URL'ler configuration'a taşındı
- Retry mekanizması exponential backoff ile iyileştirildi
- Logging yapılandırması eklendi
- Model'lere max length constraint'leri eklendi
- Database index'leri eklendi

