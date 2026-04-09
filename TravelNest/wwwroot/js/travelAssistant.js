let oraseSelectate = [];
document.addEventListener("DOMContentLoaded", () => {
    const butonGenereaza = document.getElementById("butonGenereaza");
    const butonRefresh = document.getElementById("refreshOrase");
    const inputVibe = document.getElementById("inputVibe");
    const rezultatGenerate = document.getElementById("rezultatGenerate");
    const listaCities = document.getElementById("listaCities");

    async function genereazaSugestii() {
        const textPrompt = inputVibe.value;
        if (!textPrompt.trim()) return;

        const oraseExistente = Array.from(document.querySelectorAll("#numeOras"))
            .map(el => el.textContent)
            .join(", ");

        butonGenereaza.disabled = true;
        butonRefresh.disabled = true;
        const textOriginal = butonGenereaza.innerHTML;
        butonGenereaza.innerHTML = 'Thinking... <i class="fa-solid fa-circle-notch fa-spin"></i>';

        try {
            const dateTrimise = new URLSearchParams();
            dateTrimise.append("prompt", textPrompt);
            dateTrimise.append("oraseExcluse", oraseExistente);
            //apel ptr destinatii
            const raspuns = await fetch('/TravelAssistant/GenereazaSugestii', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: dateTrimise
            });
            const dateRezultat = await raspuns.json();

            if (dateRezultat.success) {
                listaCities.innerHTML = "";
                dateRezultat.orase.forEach((oras, index) => {
                    const descriere = dateRezultat.descrieri?.[index] ?? "A wonderful destination.";
                    const htmlOras = `
                        <div class="itemOras selectable-city" onclick="toggleCitySelection(this, '${oras}')">
                            <div class="selection-indicator">
                                <i class="fa-regular fa-circle"></i>
                            </div>
                            <div class="city-content">
                                <span id="numeOras">${oras}</span>
                                <p id="detaliiCity">${descriere}</p>
                                <span id="distantaTrip">OPTION 0${index + 1}</span>
                            </div>
                        </div>
                    `;
                    listaCities.insertAdjacentHTML('beforeend', htmlOras);
                });

                //apel pentru postari
                const raspunsPostari = await fetch('/TravelAssistant/GetPostariAssistant', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        orase: dateRezultat.orase,
                        prompt: textPrompt
                    })
                });
                const datePostari = await raspunsPostari.json();
                // ptr debug
                /*
                console.log("Raspuns postari RAW:", datePostari);        
                console.log("Success:", datePostari.success);              
                console.log("Postari count:", datePostari.postari?.length); 
                console.log("Prima postare:", datePostari.postari?.[0]); 
                */
                if (datePostari.success) {
                    afiseazaPostari(datePostari.postari);
                }

                rezultatGenerate.style.display = "block";
                rezultatGenerate.scrollIntoView({ behavior: 'smooth' });
            }
        } catch (error) {
            console.error("Eroare:", error);
        } finally {
            butonGenereaza.disabled = false;
            butonRefresh.disabled = false;
            butonGenereaza.innerHTML = textOriginal;
        }
    }

function afiseazaPostari(postari) {
    const gridImagini = document.getElementById("gridImagini");
    if (!gridImagini) 
        return;
    gridImagini.innerHTML = "";

    postari.forEach((p, index) => {
        const div = document.createElement("div");
        div.id = "ramaPoza";
        if (index === 0) 
            div.style.cssText = "grid-column: span 2; grid-row: span 2;";
        if (index === 3) 
            div.style.cssText = "grid-column: span 2;";

        div.innerHTML = `
            <img id="imaginePostare" 
                 src="${p.imageUrl ?? '/images/placeholder1.jpg'}" 
                 alt="${p.locatie ?? ''}" 
                 loading="lazy" />
            ${p.dinBazaDate ? '' : '<span class="badge-web">📸 via Unsplash</span>'}
        `;
        gridImagini.appendChild(div);
    });
}

    butonGenereaza.addEventListener("click", genereazaSugestii);
    butonRefresh.addEventListener("click", genereazaSugestii);
});
function toggleCitySelection(element, oras) {
    element.classList.toggle('selected');
    const icon = element.querySelector('.selection-indicator i');

    if (element.classList.contains('selected')) {
        oraseSelectate.push(oras);
        icon.className = "fa-solid fa-circle-check";
    } else {
        oraseSelectate = oraseSelectate.filter(o => o !== oras);
        icon.className = "fa-regular fa-circle";
    }
    
    actualizeazaButonPlutitor();
}

function actualizeazaButonPlutitor() {
    const container = document.getElementById('btnAutomatGrup'); 
    const count = document.getElementById('countSelectate');
    
    if (oraseSelectate.length > 0) {
        container.style.display = 'flex';
        count.innerText = oraseSelectate.length;
    } else {
        container.style.display = 'none';
    }
}

document.getElementById('btnCreeazaGrup')?.addEventListener('click', async () => {
    if (oraseSelectate.length === 0) 
        return;

    const btn = document.getElementById('btnCreeazaGrup');
    btn.innerHTML = 'Creating... <i class="fa-solid fa-spinner fa-spin"></i>';

    try {
        const response = await fetch('/TravelGroup/CreareAutomata', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(oraseSelectate)
        });

        const result = await response.json();
        if (result.success) {
            window.location.href = `/TravelGroup/Vizualizare/${result.groupId}`;
        }
    } catch (err) {
        console.error(err);
        btn.innerHTML = 'Create Travel Group <i class="fa-solid fa-arrow-right"></i>';
    }
});