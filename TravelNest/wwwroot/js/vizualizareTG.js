let imagineOriginala = "";
let thumbnailOriginal = "";
function actualizeazaVizibilitateButonAI() {
    const inputHidden = document.getElementById('inputThumbnail');
    const btnAI = document.getElementById('btnAI');
    const inputFisier = document.getElementById('incarcaImagine');
    if (!inputHidden || !btnAI) return;

    const valoare = inputHidden.value || "";
    const areFisierSelectat = inputFisier && inputFisier.files && inputFisier.files.length > 0;
    if (valoare.includes("https://upload.wikimedia.org") || areFisierSelectat) {
        btnAI.style.display = 'none';
    } else {
        btnAI.style.display = 'inline-flex'; 
    }
}
function afiseazaEdit(stare) {
    const zonaVizualizare = document.querySelector('.hero-content');
    const zonaEditare = document.getElementById('continutEditareBanner');
    const imagineBanner = document.getElementById('imgThumbnail');
    const inputHidden = document.getElementById('inputThumbnail');
    const inputFisier = document.getElementById('incarcaImagine');
    if (stare) {
        if (imagineBanner)
            imagineOriginala = imagineBanner.src;
        if (inputHidden) 
            thumbnailOriginal = inputHidden.value;
        if (zonaVizualizare)
            zonaVizualizare.style.display = 'none';
        if (zonaEditare) 
            zonaEditare.style.display = 'flex';
        actualizeazaVizibilitateButonAI();
        if (zonaVizualizare) 
            zonaVizualizare.style.display = 'none';
        if (zonaEditare) 
            zonaEditare.style.display = 'flex';
        document.getElementById('editNume').focus();
    } else {
        if (imagineBanner) 
            imagineBanner.src = imagineOriginala;
        if (inputHidden) 
            inputHidden.value = thumbnailOriginal;
        const inputFisier = document.getElementById('incarcaImagine');
        if (inputFisier) inputFisier.value = "";
        if (zonaVizualizare) 
            zonaVizualizare.style.display = 'flex';
        if (zonaEditare) 
            zonaEditare.style.display = 'none';
    }
}

function previewImagine(input) {
    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function (e) {
            document.getElementById('imgThumbnail').src = e.target.result;
            actualizeazaVizibilitateButonAI()
        };
        reader.readAsDataURL(input.files[0]);

    }
}
async function salveazaEditareBanner() {
    const banner = document.getElementById('bannerVizualizare');
    const inputHidden = document.getElementById('inputThumbnail');
    const inputFisier = document.getElementById('incarcaImagine');
    if (!banner || !inputHidden || !inputFisier) {
        return;
    }
    const idGrup = banner.getAttribute('data-idGrup');
    const numeNou = document.getElementById('editNume').value;
    const descriereNoua = document.getElementById('editDescriere').value;
    const fisierImagine = inputFisier.files[0]; 
    const linkAI = inputHidden.value;
    const formData = new FormData();
    formData.append("id", idGrup);
    formData.append("nume", numeNou);
    formData.append("descriere", descriereNoua);
    if (fisierImagine) {
        formData.append("imagineFisier", fisierImagine);
    } else if (linkAI) {
        formData.append("thumbnailLink", linkAI);
    }

    try {
        const raspuns = await fetch('/TravelGroup/ActualizareBannerGrup', {
            method: 'POST',
            body: formData
        });

        if (raspuns.ok) {
            const elTitlu = document.getElementById('titliTG');
            const elDescriere = document.getElementById('descriereTG');
            if (elTitlu) 
                elTitlu.innerText = numeNou;
            if (elDescriere) 
                elDescriere.innerText = descriereNoua;
            if (inputHidden && inputHidden.value !== "") {
                const primaImagine = inputHidden.value.split(',')[0];
                if (banner) {
                    banner.src = primaImagine;
                    imagineOriginala = primaImagine; 
                }
            } 
            else {
                if (banner) 
                    imagineOriginala = banner.src;
            }
            afiseazaEdit(false);
        } else {
        }
    } catch (eroare) {
        console.error("Error:", eroare);
    }
}
document.addEventListener('DOMContentLoaded', () => {
    const btnMagic = document.querySelector('.ai-magic');
    const imagineBanner = document.getElementById('imgThumbnail'); 
    const inputHidden = document.getElementById('inputThumbnail'); 

    if (btnMagic) {
        btnMagic.onclick = async function (e) {
            e.preventDefault();
            const originalContent = btnMagic.innerHTML;
            if (typeof selectedCities === 'undefined' || selectedCities.length === 0) {
                btnMagic.innerHTML = '<i class="fa-solid fa-earth-europe"></i> No cities found';
                setTimeout(() => { btnMagic.innerHTML = originalContent; }, 2000);
                return;
            }
            btnMagic.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Finding...';
            const oraseDeAfisat = selectedCities.slice(0, 4);
            try {
                const results = await Promise.all(oraseDeAfisat.map(async (oras) => {
                    const res = await fetch(`https://en.wikipedia.org/api/rest_v1/page/summary/${encodeURIComponent(oras)}`);
                    const data = await res.json();
                    return data.originalimage ? data.originalimage.source : null;
                }));

                const imaginiValide = results.filter(img => img !== null);

                if (imaginiValide.length > 0) {
                    imagineBanner.src = imaginiValide[0];
                    inputHidden.value = imaginiValide.join(',');
                    actualizeazaVizibilitateButonAI()
                    btnMagic.innerHTML = '<i class="fa-solid fa-check"></i> Cover Set!';
                    setTimeout(() => { btnMagic.innerHTML = originalContent; }, 2000);
                    btnMagic.style.display = 'none';
                } else {
                    throw new Error("No images found on Wikipedia");
                }
            } catch (error) {
                console.error(error);
                btnMagic.innerHTML = '<i class="fa-solid fa-circle-xmark"></i> Failed';
                setTimeout(() => { btnMagic.innerHTML = originalContent; }, 2000);
            }
        };
    }
});

//editare locatii + harta
let selectedCities = window.selectedCities || [];
let harta, marker;
let listaIdPrieteni = new Set();
document.addEventListener('DOMContentLoaded', () => {
    afisareLocatii();
});
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
            if (!harta) 
                initializareHarta();
            else 
                setTimeout(() => { harta.invalidateSize(); }, 100);
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
function permietConfirmareLocatii() {
    const btnConfirm = document.getElementById('btnConfirmaDestinatii');
    if (!btnConfirm) 
        return;
    if (selectedCities.length === 0) {
        btnConfirm.disabled = true;
        btnConfirm.style.opacity = "0.5";
        btnConfirm.style.cursor = "not-allowed";
    } else {
        btnConfirm.disabled = false;
        btnConfirm.style.opacity = "1";
        btnConfirm.style.cursor = "pointer";
    }
}
function initializareHarta() {
    if (harta) 
        return;
    harta = L.map('mapContainer').setView([45.9432, 24.9668], 6);
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(harta);
}
document.addEventListener('DOMContentLoaded', () => {
    afisareLocatii();
    const btnInchide = document.getElementById('inchidePopUp');
    if (btnInchide) {
        btnInchide.onclick = (e) => {
            e.preventDefault();
            const popUp = document.getElementById('popUpDestinatieManuala');
            if (popUp) 
                popUp.style.display = 'none';
        };
    }
    const btnCauta = document.getElementById('btnCautaOras');
    if (btnCauta) {
        btnCauta.onclick = (e) => {
            e.preventDefault();
            cautaOras();
        };
    }
    const btnEdit = document.getElementById('btnEditDestinatie');
    if (btnEdit) {
        btnEdit.onclick = (e) => {
            e.preventDefault();
            e.stopPropagation();
            const popUp = document.getElementById('popUpDestinatieManuala');
            if (popUp) {
                popUp.style.display = 'flex';
                if (!harta)
                    initializareHarta();
            
                setTimeout(() => { 
                    harta.invalidateSize(); 
                    if (selectedCities.length > 0) {
                        const ultimulOras = selectedCities[selectedCities.length - 1];
                        centrareHartaPeOras(ultimulOras);
                    }
                }, 200);

                afisareOraseSelectate();
                permietConfirmareLocatii();
            }
        };
    }
});
async function centrareHartaPeOras(numeOras) {
    if (!numeOras)
        return;
    
    try {
        const response = await fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(numeOras)}`);
        const results = await response.json();
        
        if (results.length > 0) {
            const city = results[0];
            const lat = city.lat;
            const lon = city.lon;

            if (harta) {
                harta.setView([lat, lon], 12);
                if (marker) 
                    harta.removeLayer(marker);
                marker = L.marker([lat, lon]).addTo(harta);
            }
        }
    } catch (err) {
        console.error("Error!")
   }
}
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
    permietConfirmareLocatii();
}

const btnConfirma = document.getElementById('btnConfirmaDestinatii');
if (btnConfirma) {
    btnConfirma.onclick = async (e) => {
        e.preventDefault();
        const banner = document.getElementById('bannerVizualizare');
        if (banner) {
            const idGrup = banner.getAttribute('data-idGrup'); 
            const originalText = btnConfirma.innerText;
            btnConfirma.innerHTML = '<i class="fa-solid fa-spinner fa-spin"></i> Saving...';
            btnConfirma.disabled = true;
            await salveazaLocatiiNoi(idGrup, selectedCities);
        } 
        else {
            document.getElementById('popUpDestinatieManuala').style.display = 'none';
            afisareLocatii();
        }
    };
}

async function salveazaLocatiiNoi(id, orase) {
    let thumbnailLink = "";
    if (orase.length > 0) {
        try {
            const oras = orase[0];
            const resWiki = await fetch(`https://en.wikipedia.org/api/rest_v1/page/summary/${encodeURIComponent(oras)}`);
            const dataWiki = await resWiki.json();
            
            if (dataWiki.originalimage && dataWiki.originalimage.source) {
                thumbnailLink = dataWiki.originalimage.source;
            }
        } catch (err) {
            console.error("Wikipedia fetch error:", err);
        }
    }

    const formData = new FormData();
    formData.append("id", id);
    orase.forEach(oras => formData.append("oraseSelectate", oras));
    if (thumbnailLink !== "") {
        formData.append("thumbnailLink", thumbnailLink);
    }

    try {
        const res = await fetch('/TravelGroup/ModificareDestinatieTG', {
            method: 'POST',
            body: formData
        });

        if (res.ok) {
            location.reload(); 
        }
    } catch (err) {
        console.error("Error saving:", err);
    }
}

//harta si pin uri 
document.addEventListener('DOMContentLoaded', () => {
    initTripMap();
});

async function initTripMap() {
    const mapDiv = document.getElementById('tripMainMap');
    if (!mapDiv || !window.selectedCities || window.selectedCities.length === 0)
        return;
    const tripMap = L.map('tripMainMap', { 
        scrollWheelZoom: false,
        zoomControl: true 
    }).setView([20, 0], 2); 

    L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Physical_Map/MapServer/tile/{z}/{y}/{x}', {
        attribution: 'Esri'
    }).addTo(tripMap);
    fetch('https://raw.githubusercontent.com/datasets/geo-boundaries-world-110m/master/countries.geojson')
        .then(res => res.json())
        .then(data => {
            L.geoJson(data, {
                style: {
                    color: "#000",      
                    weight: 1.5,
                    opacity: 0.6,
                    fillOpacity: 0       
                }
            }).addTo(tripMap);
        });

    const markers = [];
    for (const oras of window.selectedCities) {
        try {
            const res = await fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(oras)}`);
            const results = await res.json(); 
            if (results && results.length > 0) {
                const marker = L.marker([results[0].lat, results[0].lon]).addTo(tripMap);
                marker.bindPopup(`<b>${oras}</b>`);
                markers.push(marker);
            }
        } catch (e) { 
            
        }
    }
    if (markers.length > 0) {
        const group = new L.featureGroup(markers); 
        if (markers.length === 1) {
            tripMap.setView(markers[0].getLatLng(), 4); 
        } else {
            tripMap.fitBounds(group.getBounds(), { 
                padding: [50, 50], 
                maxZoom: 5 
            });
        }
    }
}