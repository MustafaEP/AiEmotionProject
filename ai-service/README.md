# AI Service - Emotion Analyzer

Hugging Face Spaces üzerinde çalışan duygu analizi AI servisi.

## 🌐 Canlı Link

**Hugging Face Space**: [https://huggingface.co/spaces/mustafaep/emotion-analyzer](https://huggingface.co/spaces/mustafaep/emotion-analyzer)

Tarayıcıdan yukarıdaki linki açarak doğrudan kullanabilirsiniz.

## 🚀 Yerel Çalıştırma

### Gereksinimler

- Python 3.8+
- pip

### Kurulum

```bash
pip install -r requirements.txt
```

### Çalıştırma

```bash
python app.py
```

Uygulama `http://localhost:7860` adresinde çalışacaktır.

## 📦 Bağımlılıklar

- `gradio>=4.44.0` - Web arayüzü
- `transformers>=4.44.0` - AI modeli
- `torch>=2.2.0` - PyTorch
- `huggingface-hub>=0.23.0` - Model yönetimi

## 🔧 Yapılandırma

Model, `MODEL_ID` environment variable ile değiştirilebilir:

```bash
export MODEL_ID="savasy/bert-base-turkish-sentiment-cased"
python app.py
```

Varsayılan model: `savasy/bert-base-turkish-sentiment-cased` (Türkçe duygu analizi)

## 📡 API Kullanımı

Gradio otomatik olarak REST API endpoint'leri oluşturur. Hugging Face Spaces'te `/api/predict` endpoint'i kullanılabilir.

## 🎯 Özellikler

- Türkçe ve İngilizce metin analizi
- Pozitif/Negatif/Nötr sınıflandırma
- Skor gösterimi (0-1 arası)
- Gradio web arayüzü
- REST API desteği

