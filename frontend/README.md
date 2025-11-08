# Frontend - AI Emotion Project

Modern bir React web uygulaması ile metinlerinizin duygusal tonunu analiz edin.

## 🌐 Canlı Link

**Canlı Uygulama**: [https://ai-emotion-project-llej9t1cm-mustafa-erhans-projects.vercel.app](https://ai-emotion-project-llej9t1cm-mustafa-erhans-projects.vercel.app)

Tarayıcıdan yukarıdaki linki açarak doğrudan kullanabilirsiniz.

## 🚀 Yerel Çalıştırma

### Gereksinimler

- Node.js 16+
- npm veya yarn

### Kurulum

```bash
npm install
```

### Geliştirme

```bash
npm run dev
```

Uygulama `http://localhost:5173` adresinde çalışacaktır.

### Production Build

```bash
npm run build
```

Build çıktısı `dist/` klasöründe oluşturulur.

### Preview Production Build

```bash
npm run preview
```

## 📦 Teknolojiler

- React 18
- Vite
- Modern CSS

## ✨ Özellikler

- ✨ Modern ve kullanıcı dostu arayüz
- 🎨 Gradient tasarım ve animasyonlar
- 📊 Duygu analizi sonuçları (pozitif/negatif/nötr)
- 📈 Skor gösterimi
- 📜 Analiz geçmişi görüntüleme
- 🔍 Filtreleme ve sayfalama
- ⚡ Hızlı ve responsive tasarım

## 🎯 Kullanım

1. Kullanıcı adınızı girin
2. Analiz etmek istediğiniz metni yazın
3. "Analiz Et" butonuna tıklayın
4. Sonuçları görüntüleyin
5. "📜 Geçmiş" butonuna tıklayarak önceki analizleri görüntüleyin

## 📡 API

Uygulama aşağıdaki endpoint'leri kullanmaktadır:

- `POST https://aiemotionproject.onrender.com/api/SyncAnalyze` - Duygu analizi yap ve kaydet
- `GET https://aiemotionproject.onrender.com/api/EmotionRecords` - Analiz geçmişini getir

