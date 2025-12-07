# Frontend - AI Emotion Project

Analyze the emotional tone of your texts with a modern React web application.

## 🌐 Live Link

**Live Application**: [https://ai-emotion-project-llej9t1cm-mustafa-erhans-projects.vercel.app](https://ai-emotion-project-llej9t1cm-mustafa-erhans-projects.vercel.app)

You can use it directly by opening the link above in your browser.

## 🚀 Local Setup

### Requirements

- Node.js 16+
- npm or yarn

### Installation

```bash
npm install
```

### Development

```bash
npm run dev
```

The application will run at `http://localhost:5173`.

### Production Build

```bash
npm run build
```

Build output is created in the `dist/` folder.

### Preview Production Build

```bash
npm run preview
```

## 📦 Technologies

- React 18
- Vite
- Modern CSS

## ✨ Features

- ✨ Modern and user-friendly interface
- 🎨 Gradient design and animations
- 📊 Emotion analysis results (positive/negative/neutral)
- 📈 Score display
- 📜 Analysis history viewing
- 🔍 Filtering and pagination
- ⚡ Fast and responsive design

## 🎯 Usage

1. Enter your username
2. Write the text you want to analyze
3. Click the "Analyze" button
4. View the results
5. Click the "📜 History" button to view previous analyses

## 🔧 Configuration

### Environment Variables

The application uses the `VITE_API_BASE_URL` environment variable:

```bash
# Create .env file
VITE_API_BASE_URL=https://aiemotionproject.onrender.com
```

Default value: `https://aiemotionproject.onrender.com`

## 📡 API

The application uses the following endpoints:

- `POST {VITE_API_BASE_URL}/api/SyncAnalyze` - Perform emotion analysis and save
- `GET {VITE_API_BASE_URL}/api/EmotionRecords` - Get analysis history

## 🔒 Security Features

- ✅ Error Boundary added
- ✅ Environment variable support
- ✅ Error handling improved

## 📝 Recent Changes

- Environment variable support added (VITE_API_BASE_URL)
- Error Boundary added
- Error handling improved
