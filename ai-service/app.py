import gradio as gr
from transformers import pipeline
import os

MODEL_ID = os.getenv("MODEL_ID", "savasy/bert-base-turkish-sentiment-cased")

clf = pipeline("sentiment-analysis", model=MODEL_ID)

def normalize(label: str):
    l = (label or "").upper()
    if "NEG" in l:
        return "negative"
    if "NEU" in l:
        return "neutral"
    if "POS" in l:
        return "positive"
    return "neutral"

def analyze(text: str):
    result = clf(text)[0]
    norm_key = normalize(result["label"])
    return {
        "label_raw": result["label"],
        "score": float(result["score"]),
        "label": norm_key
    }

with gr.Blocks() as demo:
    gr.Markdown("# Emotion Analyzer (TR/EN)")
    inp = gr.Textbox(label="Metninizi yazın")
    btn = gr.Button("Analiz Et")
    out = gr.JSON(label="Sonuç")
    btn.click(fn=analyze, inputs=inp, outputs=out)

if __name__ == "__main__":
    demo.launch()