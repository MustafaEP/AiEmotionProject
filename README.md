# AI Emotion Project

AI tabanlı duygu analizi projesi. Bu proje, metinlerin duygusal tonunu analiz eden bir full-stack uygulamadır.

## 🌐 Canlı Linkler

- **Frontend**: [https://ai-emotion-project-llej9t1cm-mustafa-erhans-projects.vercel.app](https://ai-emotion-project-llej9t1cm-mustafa-erhans-projects.vercel.app)
- **Backend API (Swagger)**: [https://aiemotionproject.onrender.com/swagger/index.html](https://aiemotionproject.onrender.com/swagger/index.html)
- **AI Service (Hugging Face)**: [https://huggingface.co/spaces/mustafaep/emotion-analyzer](https://huggingface.co/spaces/mustafaep/emotion-analyzer)

## 📁 Proje Yapısı

```
AiEmotionProject/
├── ai-service/          # Python Gradio AI servisi (Hugging Face)
├── backend/             # .NET 8.0 Web API
├── frontend/            # React + Vite web uygulaması
└── mobile/              # React Native mobil uygulama
```

## 🚀 Hızlı Başlangıç

### AI Service (Python)

```bash
cd ai-service
pip install -r requirements.txt
python app.py
```

Detaylı bilgi için: [ai-service/README.md](ai-service/README.md)

### Backend (.NET)

```bash
cd backend
dotnet restore
dotnet run
```

API `http://localhost:5000` adresinde çalışacaktır. Swagger UI: `http://localhost:5000/swagger`

Detaylı bilgi için: [backend/README.md](backend/README.md)

### Frontend (React)

```bash
cd frontend
npm install
npm run dev
```

Uygulama `http://localhost:5173` adresinde çalışacaktır.

Detaylı bilgi için: [frontend/README.md](frontend/README.md)

### Mobile (React Native)

```bash
cd mobile/mobile
npm install
npm start
```

Detaylı bilgi için: [mobile/mobile/README.md](mobile/mobile/README.md)

## 🛠️ Teknolojiler

- **AI Service**: Python, Gradio, Transformers, Hugging Face
- **Backend**: .NET 8.0, Entity Framework Core, SQLite
- **Frontend**: React, Vite
- **Mobile**: React Native

## 📝 Özellikler

- Metin duygu analizi (pozitif/negatif/nötr)
- Kullanıcı bazlı analiz geçmişi
- RESTful API
- Swagger dokümantasyonu
- Modern ve responsive web arayüzü

## 📄 Lisans

Bu proje eğitim amaçlıdır.
