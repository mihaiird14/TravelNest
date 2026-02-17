let selectedCities = [];
let harta, marker;
let listaIdPrieteni = new Set();
document.addEventListener('DOMContentLoaded', () => {
    afisareLocatii();
});

function initializareHarta() {
    if (harta) return;
    harta = L.map('mapContainer').setView([45.9432, 24.9668], 6);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(harta);
}

function afisareLocatii() {
    const container = document.getElementById('locatieButoane');
    if (!container) 
        return;

    if (selectedCities.length === 0) {
        container.innerHTML = `
            <button type="button" id="locatieManual"><i class="fa-solid fa-location-dot"></i> Enter Manually</button>
            <button type="button" id="locatieAI"><i class="fa-solid fa-wand-magic-sparkles"></i> Ask the AI</button>
        `;
        
        document.getElementById('locatieManual').onclick = (e) => {
            e.preventDefault();
            e.stopPropagation();
            document.getElementById('popUpDestinatieManuala').style.display = 'flex';
            if (!harta) initializareHarta();
            else setTimeout(() => { harta.invalidateSize(); }, 100);
            afisareOraseSelectate();
        };
    } else {
        container.innerHTML = '';
        selectedCities.forEach(city => {
            const l = document.createElement('div');
            l.className = 'chip';
            l.style.cssText = "background: #EE5607; color: white; padding: 10px 20px; border-radius: 10px; font-weight: 600; margin-right: 5px; display: inline-flex;";
            l.innerText = city;
            container.appendChild(l);
        });

        const changeBtn = document.createElement('button');
        changeBtn.type = "button";
        changeBtn.className = "modificaLocatii";
        changeBtn.innerHTML = '<i class="fa-solid fa-rotate-right"></i> Change';
        changeBtn.onclick = (e) => {
            e.preventDefault();
            e.stopPropagation();
            document.getElementById('popUpDestinatieManuala').style.display = 'flex';
            setTimeout(() => { harta.invalidateSize(); }, 100);
            afisareOraseSelectate();
        };
        container.appendChild(changeBtn);
    }
}

document.getElementById('inchidePopUp').onclick = (e) => {
    e.preventDefault();
    document.getElementById('popUpDestinatieManuala').style.display = 'none';
};

document.getElementById('btnCautaOras').onclick = (e) => {
    e.preventDefault();
    cautaOras();
};

async function cautaOras() {
    const cautare = document.getElementById('mapSearchInput').value;
    if (cautare.length < 3) return;
    try {
        const response = await fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(cautare)}`);
        const results = await response.json();
        if (results.length > 0) {
            const city = results[0];
            harta.setView([city.lat, city.lon], 12);
            if (marker) harta.removeLayer(marker);
            marker = L.marker([city.lat, city.lon]).addTo(harta);
            const cityName = city.display_name.split(',')[0];
            if (!selectedCities.includes(cityName)) {
                selectedCities.push(cityName);
                afisareOraseSelectate();
            }
        }
    } catch (err) { console.error(err); }
}

window.removeCity = function(cityName) {
    selectedCities = selectedCities.filter(c => c !== cityName);
    afisareOraseSelectate();
};

function afisareOraseSelectate() {
    const container = document.getElementById('listaOraseSelectate');
    if (!container) return;
    container.innerHTML = '';
    selectedCities.forEach(city => {
        const oras = document.createElement('div');
        oras.className = 'chip';
        oras.style.cssText = "background: #FEEDE5; color: #EE5607; padding: 6px 12px; border-radius: 50px; font-weight: 600; display: flex; align-items: center; gap: 8px;";
        oras.innerHTML = `${city} <i class="fa-solid fa-xmark" style="cursor:pointer" onclick="removeCity('${city}')"></i>`;
        container.appendChild(oras);
    });
}

document.getElementById('btnConfirmaDestinatii').onclick = (e) => {
    e.preventDefault();
    document.getElementById('popUpDestinatieManuala').style.display = 'none';
    afisareLocatii();
};
const inputStart = document.getElementById('startDate');
const inputEnd = document.getElementById('endDate');

function validareDate() {
    const startVal = inputStart.value;
    const endVal = inputEnd.value;

    if (startVal) {
        inputEnd.min = startVal;
    } else {
        inputEnd.removeAttribute('min');
    }

    if (startVal && endVal) {
        const dStart = new Date(startVal);
        const dEnd = new Date(endVal);

        if (dStart > dEnd) {
            inputEnd.value = "";
            inputEnd.classList.add('date-error');
        } else {
            inputEnd.classList.remove('date-error');
        }
    }
}

inputStart.addEventListener('change', validareDate);
inputEnd.addEventListener('change', validareDate);

//cod imagine
const btnUploadManual = document.querySelector('#butoaneImaginiTG button:first-child');
const inputFisier = document.getElementById('uploadImagineInput');
const zonaPreview = document.getElementById('previzualizareImagineTG');
btnUploadManual.addEventListener('click', (e) => {
    e.preventDefault();
    inputFisier.click();
});
inputFisier.addEventListener('change', function() {
    const fisier = this.files[0];
    if (fisier) {
        const reader = new FileReader(); 
        reader.onload = function(e) {
            zonaPreview.innerHTML = `
                <img src="${e.target.result}" 
                     style="width: 100%; height: 100%; object-fit: cover; border-radius: 12px;">
            `;
        }
        reader.readAsDataURL(fisier);
    }
});
document.addEventListener('DOMContentLoaded', () => {
    const btnAI = document.getElementById('btnAI');
    const previewContainer = document.getElementById('previzualizareImagineTG');
    if (btnAI && previewContainer) {
        const originalContent = btnAI.innerHTML;
        previewContainer.style.height = "250px";
        previewContainer.style.width = "100%";
        previewContainer.style.overflow = "hidden";
        previewContainer.style.position = "relative";
        previewContainer.style.display = "flex";
        previewContainer.style.justifyContent = "center";
        previewContainer.style.alignItems = "center";
        btnAI.onclick = async function (e) {
            e.preventDefault();
            e.stopPropagation();

            if (selectedCities.length === 0) {
                btnAI.innerHTML = '<i class="fa-solid fa-triangle-exclamation"></i> Select a city';
                setTimeout(() => { btnAI.innerHTML = originalContent; }, 2000);
                return;
            }
            previewContainer.style.display = "flex"; 
            previewContainer.innerHTML = `
                <div id="aiLoading" style="color:#EE5607; text-align:center;">
                    <i class="fa-solid fa-clone fa-bounce" style="font-size:2rem;"></i><br>
                    <span style="font-weight:600;">Building Collage...</span>
                </div>
            `;

            const oraseDeAfisat = selectedCities.slice(0, 4);
            try {
                const results = await Promise.all(oraseDeAfisat.map(async (oras) => {
                    const res = await fetch(`https://en.wikipedia.org/api/rest_v1/page/summary/${encodeURIComponent(oras)}`);
                    const data = await res.json();
                    return data.originalimage ? data.originalimage.source : null;
                }));
                const imaginiValide = results.filter(img => img !== null);
                if (imaginiValide.length > 0) {
                    previewContainer.innerHTML = "";
                    previewContainer.style.display = "grid"; 
                    previewContainer.style.gap = "4px";
                    previewContainer.style.padding = "4px";
                    if (imaginiValide.length === 1) {
                        previewContainer.style.gridTemplateColumns = "1fr";
                        previewContainer.style.gridTemplateRows = "1fr";
                    } else if (imaginiValide.length === 2) {
                        previewContainer.style.gridTemplateColumns = "1fr 1fr";
                        previewContainer.style.gridTemplateRows = "1fr";
                    } else {
                        previewContainer.style.gridTemplateColumns = "1fr 1fr";
                        previewContainer.style.gridTemplateRows = "1fr 1fr";
                    }

                    imaginiValide.forEach((url, index) => {
                        const img = document.createElement('img');
                        img.src = url;
                        img.style.cssText = "width: 100%; height: 100%; object-fit: cover; border-radius: 4px; min-height: 0;";
                        if (imaginiValide.length === 3 && index === 0) {
                            img.style.gridColumn = "span 2";
                        }
                        previewContainer.appendChild(img);
                    });
                } else {
                    throw new Error("No images");
                }
            } catch (error) {
                previewContainer.style.display = "flex";
                previewContainer.innerHTML = "<span>Images not found.</span>";
            }
        };
    }
});

//cauta prieteni
document.addEventListener('DOMContentLoaded', () => {
    const friendInput = document.getElementById('friendSearch');
    const resultsContainer = document.getElementById('friendResults');
    const friendsList = document.getElementById('prieteniAdaugati');
    let listaIdPrieteni = new Set();
    if (friendInput && resultsContainer) {
        friendInput.addEventListener('input', async () => {
            const username = friendInput.value.trim();
            if (username.length < 2) {
                resultsContainer.style.display = 'none';
                return;
            }
            try {
                const response = await fetch(`/TravelGroup/CautaPrieteniTravelGroup?username=${encodeURIComponent(username)}`);
                const prieteniGasiti = await response.json();
                const prieteniNoi = prieteniGasiti.filter(p => !listaIdPrieteni.has(p.id));
                if (prieteniNoi.length > 0) {
                    afiseazaRezultate(prieteniNoi);
                } else {
                    resultsContainer.style.display = 'none';
                }
            } catch (error) {
                console.error("Eroare la căutare:", error);
            }
        });
        function afiseazaRezultate(lista) {
            resultsContainer.innerHTML = lista.map(p => `
                <div class="result-item" data-id="${p.id}" data-name="${p.userName}">
                    <img src="${p.pozaProfil}" class="user-dropdown-photo">
                    <span>${p.userName}</span>
                </div>
            `).join('');
            resultsContainer.style.display = 'block';
        }
        resultsContainer.addEventListener('click', (e) => {
            const item = e.target.closest('.result-item');
            if (!item) 
                return;
            const id = item.dataset.id;
            const name = item.dataset.name;
            const poza = item.querySelector('img').src; 
            adaugaChipPrieten(id, name, poza);
            friendInput.value = "";
            resultsContainer.style.display = 'none';
        });
        function adaugaChipPrieten(id, name) {
            listaIdPrieteni.add(id);
            const prietenAdaugat = document.createElement('div');
            prietenAdaugat.className = 'chip';  
            prietenAdaugat.style.cssText = "background: #f1f5f9; color: #1e293b; padding: 5px 12px; border-radius: 50px; display: inline-flex; align-items: center; gap: 8px; margin: 5px; font-weight: 600; border: 1px solid #e2e8f0;";
            prietenAdaugat.innerHTML = `
                <span>${name}</span>
                <i class="fa-solid fa-circle-xmark" style="cursor:pointer; color: #64748b;" data-id="${id}"></i>
            `;
            prietenAdaugat.querySelector('i').onclick = () => {
                listaIdPrieteni.delete(id);
                prietenAdaugat.remove();
            };

            friendsList.appendChild(prietenAdaugat);
        }
        document.addEventListener('click', (e) => {
            if (!friendInput.contains(e.target) && !resultsContainer.contains(e.target)) {
                resultsContainer.style.display = 'none';
            }
        });
    }
});
const btnCreate = document.getElementById('createGroupTG');
if (btnCreate) {
    btnCreate.addEventListener('mouseenter', () => {
        if (typeof selectedCities === 'undefined' || selectedCities.length === 0) {
            btnCreate.disabled = true;
            btnCreate.classList.add('btn-blocked');
            btnCreate.title = "Please add at least one destination to enable this button.";
        } else {
            btnCreate.disabled = false;
            btnCreate.classList.remove('btn-blocked');
            btnCreate.title = "Ready to go!";
        }
    });
    document.getElementById('btnConfirmaDestinatii').addEventListener('click', () => {
        if (selectedCities.length > 0) {
            btnCreate.disabled = false;
            btnCreate.classList.remove('btn-blocked');
        }
    });
}
document.getElementById('formCreateGroup').onsubmit = async function(e) {
    e.preventDefault(); 
    const btnAI = document.getElementById('btnAI');
    let imaginiExistente = this.querySelectorAll('#previzualizareImagineTG img');
    if (imaginiExistente.length === 0 && btnAI) {
        await btnAI.onclick({ preventDefault: () => {}, stopPropagation: () => {} });
    }
    const container = document.getElementById('hiddenInputsContainer');
    container.innerHTML = ''; 
    const toateImaginile = Array.from(this.querySelectorAll('#previzualizareImagineTG img'))
                                .map(img => img.src);
    
    const thumbnailInput = document.getElementById('inputThumbnail');
    if (thumbnailInput && toateImaginile.length > 0) {
        thumbnailInput.value = toateImaginile.join(','); 
    }
    selectedCities.forEach(oras => {
        container.innerHTML += `<input type="hidden" name="oraseSelectate" value="${oras}">`;
    });
    listaIdPrieteni.forEach(id => {
        container.innerHTML += `<input type="hidden" name="idPrieteni" value="${id}">`;
    });
    this.submit();
};
