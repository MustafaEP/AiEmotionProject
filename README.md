# AI Emotion Project

AI-based emotion analysis project. This project is a full-stack application that analyzes the emotional tone of texts.

## 🌐 Live Links

- **Frontend**: [https://ai-emotion-project-llej9t1cm-mustafa-erhans-projects.vercel.app](https://ai-emotion-project-llej9t1cm-mustafa-erhans-projects.vercel.app)
- **Backend API (Swagger)**: [https://aiemotionproject.onrender.com/swagger/index.html](https://aiemotionproject.onrender.com/swagger/index.html)
- **AI Service (Hugging Face)**: [https://huggingface.co/spaces/mustafaep/emotion-analyzer](https://huggingface.co/spaces/mustafaep/emotion-analyzer)

## 📁 Project Structure

```
AiEmotionProject/
├── ai-service/          # Python Gradio AI service (Hugging Face)
├── backend/             # .NET 8.0 Web API
├── frontend/            # React + Vite web application
└── mobile/              # React Native mobile application
```

## 🚀 Quick Start

### AI Service (Python)

```bash
cd ai-service
pip install -r requirements.txt
python app.py
```

For detailed information: [ai-service/README.md](ai-service/README.md)

### Backend (.NET)

```bash
cd backend
dotnet restore
dotnet run
```

The API will run at `http://localhost:5000`. Swagger UI: `http://localhost:5000/swagger`

For detailed information: [backend/README.md](backend/README.md)

### Frontend (React)

```bash
cd frontend
npm install
npm run dev
```

The application will run at `http://localhost:5173`.

For detailed information: [frontend/README.md](frontend/README.md)

### Mobile (React Native)

```bash
cd mobile/mobile
npm install
npm start
```

For detailed information: [mobile/mobile/README.md](mobile/mobile/README.md)

## 🛠️ Technologies

- **AI Service**: Python, Gradio, Transformers, Hugging Face
- **Backend**: .NET 8.0, Entity Framework Core, SQLite
- **Frontend**: React, Vite
- **Mobile**: React Native

## 📝 Features

- Text emotion analysis (positive/negative/neutral)
- User-based analysis history
- RESTful API
- Swagger documentation
- Modern and responsive web interface

## 📄 License

This project is for educational purposes.
