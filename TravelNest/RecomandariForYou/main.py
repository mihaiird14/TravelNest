import io
import torch
import uvicorn
from PIL import Image
from fastapi import FastAPI, UploadFile, File, Form, HTTPException
from transformers import CLIPProcessor, CLIPModel
from typing import Optional

app = FastAPI()

class AnalizatorPostari:
    def __init__(self):
        self.modelName = "openai/clip-vit-base-patch32"
        self.device = "cuda" if torch.cuda.is_available() else "cpu"
        self.model = CLIPModel.from_pretrained(self.modelName).to(self.device)
        self.processor = CLIPProcessor.from_pretrained(self.modelName)
        
        # Etichete
        self.etichete = [
            "nature", "architecture", "food and drinks", "beach and sea", "mountain",
            "nightlife", "art", "sport", "animals", "people", "transport"
        ]

    def extrageEtichete(self, imagine, prag=0.12):
        inputs = self.processor(
            text=self.etichete,
            images=imagine,
            return_tensors="pt",
            padding=True
        ).to(self.device)

        with torch.no_grad():
            outputs = self.model(**inputs)
        
        # Calcul scoruri
        probs = outputs.logits_per_image.softmax(dim=1)[0].cpu().numpy()
        
        # Log debug
        print("\nScoruri:")
        for i, scor in enumerate(probs):
            print(f"  {self.etichete[i]}: {scor:.4f}")
        
        return [self.etichete[i] for i, v in enumerate(probs) if v >= prag]

analizator = AnalizatorPostari()

@app.post("/analizeazaPostare")
async def analizaPostare(
        descriere: Optional[str] = Form(None),
        fisier: UploadFile = File(...)
):
    # Validare
    if fisier.content_type and not fisier.content_type.startswith("image/"):
        raise HTTPException(status_code=400, detail="Invalid type")

    try:
        # Citire imagine
        data = await fisier.read()
        image = Image.open(io.BytesIO(data)).convert("RGB")
        
        # Redimensionare
        if image.width > 5000 or image.height > 5000:
            image.thumbnail((1024, 1024))
        
        # Analiza
        tags = analizator.extrageEtichete(image)
        rezultat = ", ".join(tags)
        
        return {
            "cod": 200,
            "metadate": rezultat,
            "info": f"Found {len(tags)} tags"
        }
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

if __name__ == "__main__":
    # Start server
    uvicorn.run(app, host="127.0.0.1", port=8000)