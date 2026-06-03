from fastapi import FastAPI
from pydantic import BaseModel
from sentence_transformers import SentenceTransformer, util
from deep_translator import GoogleTranslator
import torch

app = FastAPI()

model = SentenceTransformer("all-MiniLM-L6-v2")
translator = GoogleTranslator(source="auto", target="en")

CATEGORIES = [
    "airplane flight ticket airport travel",
    "hotel hostel accommodation lodging room booking check-in stay overnight",
    "restaurant food meal lunch dinner breakfast cafe coffee drink",
    "museum entry ticket castle fortress citadel monument sightseeing tour attraction visit cultural heritage",
    "bar nightclub concert show event party entertainment",
    "taxi uber bus metro subway train local transport",
    "shopping souvenir gift clothes store market",
    "other miscellaneous expense",
]

CATEGORY_LABELS = [
    "Flights",
    "Accommodation",
    "Food",
    "Tourism",
    "Entertainment",
    "Transport",
    "Shopping",
    "Other",
]

category_embeddings = model.encode(CATEGORIES, convert_to_tensor=True)


def translate(text: str) -> str:
    try:
        return translator.translate(text)
    except Exception:
        return text


class Expense(BaseModel):
    title: str
    amount: float


class ClassifyRequest(BaseModel):
    expenses: list[Expense]


@app.post("/classify")
def classify(request: ClassifyRequest):
    if not request.expenses:
        return []

    titles = [translate(e.title) for e in request.expenses]
    title_embeddings = model.encode(titles, convert_to_tensor=True)

    scores = util.cos_sim(title_embeddings, category_embeddings)

    totals: dict[str, float] = {}
    for i, expense in enumerate(request.expenses):
        best_idx = int(torch.argmax(scores[i]))
        label = CATEGORY_LABELS[best_idx]
        totals[label] = totals.get(label, 0.0) + expense.amount

    return [{"category": cat, "amount": round(amt, 2)} for cat, amt in totals.items()]


@app.get("/health")
def health():
    return {"status": "ok"}

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=8001)