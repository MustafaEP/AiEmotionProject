import gradio as gr
from transformers import pipeline
import os

# 1) Model seçimi (ENV ile değiştirilebilir)
# Varsayılan: Türkçe duygu analizi
MODEL_ID = os.getenv("MODEL_ID", "savasy/bert-base-turkish-sentiment-cased")

clf = pipeline("sentiment-analysis", model=MODEL_ID)

# 2) Çıkışı normalize eden yardımcı (emoji yok)
def normalize(label: str):
    l = (label or "").upper()
    if "NEG" in l:        # NEG, NEGATIVE, olumsuz
        return "negative"
    if "NEU" in l:        # NEU, NEUTRAL, nötr
        return "neutral"
    if "POS" in l:        # POS, POSITIVE, olumlu
        return "positive"
    # bilinmez durum
    return "neutral"

# 3) Gradio arayüzünde gösterilecek fonksiyon (emoji alanı kaldırıldı)
def analyze(text: str):
    result = clf(text)[0]   # ör: {'label': 'POSITIVE', 'score': 0.99}
    norm_key = normalize(result["label"])
    return {
        "label_raw": result["label"],
        "score": float(result["score"]),
        "label": norm_key      # "positive" | "neutral" | "negative"
    }

# 4) Gradio Blocks: hem UI hem de API
with gr.Blocks() as demo:
    gr.Markdown("# Emotion Analyzer (TR/EN)")
    inp = gr.Textbox(label="Metninizi yazın")
    btn = gr.Button("Analiz Et")
    out = gr.JSON(label="Sonuç")
    btn.click(fn=analyze, inputs=inp, outputs=out)

# Önemli: Gradio, arka planda otomatik REST endpoint de üretir
# /{SPACE_URL}/run/predict (v4'te /api/predict) şeklinde
if __name__ == "__main__":
    demo.launch()