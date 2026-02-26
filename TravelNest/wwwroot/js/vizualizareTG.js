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
    window.incarcaBileteExistente();
    hartaVizualizareTG();
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
            console.error(err);
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
async function hartaVizualizareTG() {
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
//functii ptr incarcare documente
function IconitaFisier(numeFisier) {
    const ext = numeFisier.split('.').pop().toLowerCase();
    if (['jpg', 'jpeg', 'png'].includes(ext)) 
        return 'fa-solid fa-file-image';
    if (ext === 'pdf') 
        return 'fa-solid fa-file-pdf';
    if (['doc', 'docx'].includes(ext)) 
        return 'fa-solid fa-file-word';
    if (['xls', 'xlsx'].includes(ext)) 
        return 'fa-solid fa-file-excel';
    return 'fa-solid fa-file';
}

async function IncarcaDoc(input) {
    const fisier = input.files[0];
    if (!fisier) 
        return;
    const banner = document.getElementById('bannerVizualizare'); 
    const idGrup = banner.getAttribute('data-idGrup'); 
    const dateForm = new FormData();
    dateForm.append("groupId", idGrup);
    dateForm.append("fisier", fisier);
    const raspuns = await fetch('/TravelGroup/IncarcaDocument', { method: 'POST', body: dateForm });
    const rezultat = await raspuns.json();
    if (rezultat.success) {
        const lista = document.getElementById('ListaDoc');
        const nouRand = `
            <div id="RandDoc-${rezultat.id}" style="display: flex; justify-content: space-between; align-items: center; background: #f8f9fa; padding: 12px 15px; border-radius: 12px; margin-bottom: 10px;">
                <div id="InfoDoc-${rezultat.id}" style="display: flex; align-items: center; gap: 12px;">
                    <div id="IconDoc-${rezultat.id}"><i class="${IconitaFisier(rezultat.nume)}"></i></div>
                    <p id="NumeDoc-${rezultat.id}" style="margin: 0; font-weight: 500; color: #1e293b; font-size: 0.95rem;">${rezultat.nume}</p>
                </div>
                <i id="StergeDoc-${rezultat.id}" class="fa-solid fa-xmark" style="cursor: pointer; color: #cbd5e1;" onclick="EliminaDoc(${rezultat.id})"></i>
            </div>`;
        lista.insertAdjacentHTML('beforeend', nouRand);
    }
}

async function EliminaDoc(id) {
    const raspuns = await fetch(`/TravelGroup/StergeDocument?id=${id}`, { method: 'POST' });
    const rezultat = await raspuns.json();
    if (rezultat.success) {
        const element = document.getElementById(`RandDoc-${id}`);
        if (element) 
            element.remove();
    }
}
//functii sterge membru tg
let dateEliminareCurenta = null;
window.deschidePopUpStergeMembru = function(groupId, profilId, nume, esteAutoEliminare) {
    const popup = document.getElementById('stergeMembru');
    if (!popup) {
        return;
    }
    const titlu = document.getElementById('popUpTitlu');
    const mesaj = document.getElementById('popUpMesaj');
    const btnConfirmare = document.getElementById('btnStergeMembruPopUp');
    dateEliminareCurenta = { groupId, profilId, esteAutoEliminare };

    if (esteAutoEliminare) {
        titlu.innerText = "Leave Group?";
        mesaj.innerText = "Are you sure you want to leave this group?";
        btnConfirmare.innerText = "Leave";
    } else {
        titlu.innerText = "Remove Member?";
        mesaj.innerText = `Are you sure you want to remove ${nume}?`;
        btnConfirmare.innerText = "Remove";
    }

    btnConfirmare.onclick = window.executaEliminareMembru;
    popup.style.display = 'flex';
    popup.style.zIndex = '9999'; 
};
window.inchidePopUpEliminare = function() {
    const popup = document.getElementById('stergeMembru');
    if(popup) popup.style.display = 'none';
    dateEliminareCurenta = null;
};

window.executaEliminareMembru = async function() {
    if (!dateEliminareCurenta) return;
    
    const { groupId, profilId } = dateEliminareCurenta;
    console.log("Se execută eliminarea pentru profilul:", profilId);

    const formData = new FormData();
    formData.append("groupId", groupId);
    formData.append("profilIdDeEliminat", profilId);

    try {
        const response = await fetch('/TravelGroup/StergeMembru', {
            method: 'POST',
            body: formData
        });

        const data = await response.json();
        if (data.success) {
            window.location.href = data.url || window.location.href;
            if (!data.redirected) location.reload();
        } else {
            alert(data.message || "Error processing request.");
        }
    } catch (error) {
        console.error("Error:", error);
    } finally {
        window.inchidePopUpEliminare();
    }
};
window.rezultateZboruriGlobale = [];
window.selectiiItinerariu = {};

window.genereazaSegmente = async function() {
    const orasPlecare = document.getElementById('orasPlecare').value;
    const containerSegmente = document.getElementById('segmenteZbor');
    const butonCauta = document.getElementById('butonCauta');

    if (!orasPlecare.trim()) {
        containerSegmente.style.display = 'none';
        butonCauta.style.display = 'none';
        return;
    }

    const idGrup = document.getElementById('idGrup').value;
    const raspunsServer = await fetch(`/TravelGroup/GetLocatiiTG?idGrup=${idGrup}`);
    const oraseGrup = await raspunsServer.json();

    if (oraseGrup.length === 0) {
        containerSegmente.innerHTML = "<p>Adaugă mai întâi locații în grup pentru a genera rutele.</p>";
        containerSegmente.style.display = 'block';
        butonCauta.style.display = 'none';
        return;
    }

    let rute = [];
    rute.push({ deLa: orasPlecare, la: oraseGrup[0] }); 
    for (let i = 0; i < oraseGrup.length - 1; i++) {
        rute.push({ deLa: oraseGrup[i], la: oraseGrup[i + 1] }); 
    }
    rute.push({ deLa: oraseGrup[oraseGrup.length - 1], la: orasPlecare }); 
    const azi = new Date().toISOString().split('T')[0];
    let htmlSegmente = "";
    rute.forEach((ruta, index) => {
        htmlSegmente += `
            <div id="rutaZbor${index}" style="margin-bottom: 10px;">
                <label id="etichetaRuta${index}" for="dataZbor${index}" style="display: block; font-weight: bold;">
                    ${ruta.deLa} - ${ruta.la}:
                </label>
                <input type="date" id="dataZbor${index}" min="${azi}" onchange="window.actualizeazaDataZbor(${index})">
            </div>
        `;
    });

    containerSegmente.innerHTML = htmlSegmente;
    containerSegmente.style.display = 'block';
    butonCauta.style.display = 'block';
};
window.actualizeazaDataZbor = function(index) {
    const dataCurentaInput = document.getElementById(`dataZbor${index}`);
    const dataUrmatoareInput = document.getElementById(`dataZbor${index + 1}`); 
    if (dataUrmatoareInput && dataCurentaInput.value) {
        dataUrmatoareInput.min = dataCurentaInput.value;
        if (dataUrmatoareInput.value && dataUrmatoareInput.value < dataCurentaInput.value) {
            dataUrmatoareInput.value = "";
        }
    }
};
window.cautaZboruri = async function() {
    const butonCauta = document.getElementById('butonCauta');
    
    // UI Initial: Dezactivăm butonul și informăm utilizatorul
    butonCauta.innerText = "Searching Cities...";
    butonCauta.disabled = true;

    try {
        // 1. Descarcă baza de date de aeroporturi (Algolia Dataset) pentru mapare locală
        const raspunsAirports = await fetch('https://raw.githubusercontent.com/algolia/datasets/master/airports/airports.json');
        if (!raspunsAirports.ok) throw new Error("Nu s-a putut încărca baza de date de aeroporturi.");
        const listaAeroporturi = await raspunsAirports.json();

        let dateCerere = [];
        const divuriRute = document.querySelectorAll('[id^="rutaZbor"]');

        // 2. Procesăm fiecare rând de zbor din interfață
        divuriRute.forEach((div, index) => {
            const etichetaElement = document.getElementById(`etichetaRuta${index}`);
            const dataInputElement = document.getElementById(`dataZbor${index}`);
            
            if (!etichetaElement || !dataInputElement) return;

            const textEticheta = etichetaElement.innerText.replace(':', '').trim();
            // Split după separatorii posibili (linie sau iconiță avion)
            const parti = textEticheta.split(/-|✈️|✈/).map(p => p.trim());
            
            if (parti.length < 2) return;

            const orasPlecare = parti[0].split('(')[0].trim();
            const orasSosire = parti[1].split('(')[0].trim();
            const dataSelectata = dataInputElement.value;

            // Funcție helper pentru căutarea codului IATA în JSON-ul de pe GitHub
            const gasesteIataInFisier = (numeOras) => {
                const aero = listaAeroporturi.find(a => 
                    a.city && a.city.toLowerCase() === numeOras.toLowerCase() && a.iata_code
                );
                return aero ? aero.iata_code : "";
            };

            // Extragere IATA: încercăm întâi regex din paranteze, apoi căutăm în fișier
            const regexIata = /\(([^)]+)\)/;
            let iataDeLa = parti[0].match(regexIata)?.[1] || gasesteIataInFisier(orasPlecare);
            let iataLa = parti[1].match(regexIata)?.[1] || gasesteIataInFisier(orasSosire);

            console.log(`🔎 Rută ${index}: ${orasPlecare}(${iataDeLa}) -> ${orasSosire}(${iataLa})`);

            if (dataSelectata) {
                dateCerere.push({
                    DeLa: orasPlecare,
                    La: orasSosire,
                    IataDeLa: iataDeLa,
                    IataLa: iataLa,
                    DataZbor: dataSelectata
                });
            }
        });

        if (dateCerere.length === 0) {
            alert("Vă rugăm să selectați cel puțin o dată de plecare.");
            resetButonCauta();
            return;
        }

        // 3. Trimiterea datelor către Server (C#)
        console.log("Date trimise la server:", JSON.stringify(dateCerere, null, 2));
        butonCauta.innerText = "Searching Flights...";

        const raspuns = await fetch('/TravelGroup/CautaZboruri', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(dateCerere)
        });

        const textRaspuns = await raspuns.text();

        // Gestionare erori HTTP (ex: 400 Bad Request)
        if (!raspuns.ok) {
            console.error("Eroare Server:", textRaspuns);
            alert("Eroare de la server:\n" + textRaspuns);
            resetButonCauta();
            return;
        }

        // 4. Procesarea Rezultatelor
        let rezultateReale;
        try {
            rezultateReale = JSON.parse(textRaspuns);
        } catch (e) {
            throw new Error("Răspunsul serverului nu a putut fi procesat (JSON invalid).");
        }

        if (rezultateReale.eroare) {
            alert("Eroare: " + rezultateReale.detalii);
            resetButonCauta();
            return;
        }

        // Afișare Pop-up Rezultate
        window.rezultateZboruriGlobale = rezultateReale;
        document.getElementById('popUpRezultateZboruri').style.display = 'flex';
        
        if (typeof window.aplicaSortare === "function") {
            window.aplicaSortare();
        }

    } catch (error) {
        console.error("Eroare critică JS:", error);
        alert("A apărut o problemă la căutare. Verifică consola pentru detalii.");
    } finally {
        resetButonCauta();
    }

    // Funcție internă pentru a activa butonul la loc
    function resetButonCauta() {
        butonCauta.innerText = "Search Flights";
        butonCauta.disabled = false;
    }
};
function parseDurataAmadeus(pt) {
    let ore = 0, minute = 0;
    let match = pt.match(/PT(?:(\d+)H)?(?:(\d+)M)?/);
    if (match) {
        ore = match[1] ? parseInt(match[1]) : 0;
        minute = match[2] ? parseInt(match[2]) : 0;
    }
    return (ore * 60) + minute;
}

window.aplicaSortare = function() {
    const criteriuElement = document.getElementById('sortareZbor');
    if (!criteriuElement) return;
    
    const criteriu = criteriuElement.value;
    const containerOptiuni = document.getElementById('containerOptiuni');
    let htmlOptiuni = "";

    if (Array.isArray(window.rezultateZboruriGlobale)) {
        window.rezultateZboruriGlobale.forEach((segment, indexSegment) => {
            htmlOptiuni += `<div id="containerRuta${indexSegment}" style="margin-bottom: 30px;">`;
            htmlOptiuni += `<h5 style="color: #EE5607; border-bottom: 2px solid #EE5607; padding-bottom: 5px; margin-top: 20px;">${segment.titluRuta}</h5>`;
            
            // Verificăm dacă există zboruri
            if (segment.zboruri && Array.isArray(segment.zboruri) && segment.zboruri.length > 0) {
                let zboruriSortate = [...segment.zboruri];
                
                // SORTARE SIGURĂ (Optional Chaining ?. previne crash-ul)
                zboruriSortate.sort((a, b) => {
                    try {
                        if (criteriu === 'pretAsc') return parseFloat(a.price?.total || 0) - parseFloat(b.price?.total || 0);
                        if (criteriu === 'pretDesc') return parseFloat(b.price?.total || 0) - parseFloat(a.price?.total || 0);
                        
                        const escaleA = a.itineraries?.[0]?.segments?.length || 1;
                        const escaleB = b.itineraries?.[0]?.segments?.length || 1;
                        if (criteriu === 'escaleAsc') return (escaleA - 1) - (escaleB - 1);
                        
                        // Folosim o funcție de siguranță pentru durată
                        if (criteriu === 'durataAsc') {
                            const durA = typeof parseDurataAmadeus === 'function' ? parseDurataAmadeus(a.itineraries?.[0]?.duration) : 0;
                            const durB = typeof parseDurataAmadeus === 'function' ? parseDurataAmadeus(b.itineraries?.[0]?.duration) : 0;
                            return durA - durB;
                        }
                    } catch (e) { return 0; }
                    return 0;
                });

                zboruriSortate.forEach((zbor, indexZbor) => {
                    // Accesăm datele folosind ?. pentru a evita "undefined"
                    const itinerariu = zbor.itineraries?.[0];
                    if (!itinerariu) return;

                    const segmentZbor = itinerariu.segments?.[0];
                    if (!segmentZbor) return;

                    const codCompanie = segmentZbor.carrierCode || "??";
                    
                    // FALLBACK pentru numele companiei (dacă dictionare lipsește)
                    const numeCompanieFull = (segment.dictionare?.carriers && segment.dictionare.carriers[codCompanie]) 
                                            ? segment.dictionare.carriers[codCompanie] 
                                            : `Airline ${codCompanie}`;

                    const urlLogo = `https://images.kiwi.com/airlines/64/${codCompanie}.png`;
                    const numarZbor = segmentZbor.number || "";
                    const plecare = (segmentZbor.departure?.at || "").replace('T', ' ').substring(0, 16);
                    const sosire = (segmentZbor.arrival?.at || "").replace('T', ' ').substring(0, 16);
                    const pret = (zbor.price?.total || "0") + " " + (zbor.price?.currency || "EUR");
                    const escale = (itinerariu.segments?.length || 1) - 1;
                    const textDurata = (itinerariu.duration || "").replace('PT', '').toLowerCase();

                    const idCard = `cardZbor${indexSegment}_${indexZbor}`;
                    const idBtn = `btnAlege${indexSegment}_${indexZbor}`;

                    htmlOptiuni += `
                        <div id="${idCard}" class="card-zbor" style="border: 1px solid #e9ecef; padding: 15px; margin-bottom: 10px; border-radius: 8px; display: flex; justify-content: space-between; align-items: center; background: white;">
                            <div class="detalii-zbor" style="display: flex; align-items: center; gap: 15px;">
                                <img src="${urlLogo}" onerror="this.src='https://via.placeholder.com/40?text=✈️'" alt="${numeCompanieFull}" style="width: 40px; height: 40px; object-fit: contain;">
                                <div>
                                    <p style="margin:0;"><strong>${numeCompanieFull}</strong> <span style="color: #666; font-size: 12px;">(${codCompanie} ${numarZbor})</span></p>
                                    <p style="margin:0; font-size: 13px;">${plecare} ➡️ ${sosire}</p>
                                    <p style="margin:0; font-size: 12px; color: #666;">Stops: ${escale} | Duration: ${textDurata}</p>
                                </div>
                            </div>
                            <div style="text-align: right;">
                                <div class="pret-zbor" style="font-size: 18px; font-weight: bold; color: #2c3e50; margin-bottom: 5px;">${pret}</div>
                                <button id="${idBtn}" class="btn-alege" 
                                    style="background: #EE5607; color: white; border: none; padding: 8px 15px; border-radius: 5px; cursor: pointer;"
                                    onclick="window.selecteazaZborPentruRuta(${indexSegment}, '${numeCompanieFull.replace(/'/g, "\\'")}', '${numarZbor}', '${plecare}', '${sosire}', '${idCard}', '${idBtn}', '${urlLogo}')">
                                    Select
                                </button>
                            </div>
                        </div>
                    `;
                });
            } else {
                htmlOptiuni += `
                    <div style="padding: 20px; text-align: center; background: #fff5f5; border-radius: 8px; border: 1px solid #feb2b2;">
                        <p style="color: #c53030; margin: 0; font-weight: bold;">No flights found for this segment.</p>
                        <p style="color: #c53030; margin: 0; font-size: 12px;">Try another date or airport.</p>
                    </div>`;
            }
            htmlOptiuni += `</div>`;
        });
    }

    containerOptiuni.innerHTML = htmlOptiuni;
};
window.selecteazaZborPentruRuta = function(indexSegment, companieNumeComplet, model, oraPlecare, oraSosire, idCard, idBtn, urlLogo) {
    window.selectiiItinerariu[indexSegment] = { companie: companieNumeComplet, model, oraPlecare, oraSosire, urlLogo };
    const containerRuta = document.getElementById(`containerRuta${indexSegment}`);
    const toateButoanele = containerRuta.querySelectorAll('.btn-alege');
    const toateCardurile = containerRuta.querySelectorAll('.card-zbor');

    toateButoanele.forEach(btn => {
        btn.innerText = "Select";
        btn.style.backgroundColor = "#EE5607";
    });
    toateCardurile.forEach(card => {
        card.style.borderColor = "#e9ecef";
        card.style.backgroundColor = "#f8f9fa";
    });

    document.getElementById(idCard).style.borderColor = "#28a745";
    document.getElementById(idCard).style.backgroundColor = "#eafbf0";
    const btnAles = document.getElementById(idBtn);
    btnAles.innerText = "Selected ✓";
    btnAles.style.backgroundColor = "#28a745";

    if (Object.keys(window.selectiiItinerariu).length === window.rezultateZboruriGlobale.length) {
        document.getElementById('butonConfirmaItinerariu').style.display = 'block';
    }
};

window.confirmaItinerariul = function() {
    window.inchidePopUp();
    document.getElementById('formularZbor').style.display = 'none';
    let htmlBilete = `
    <div id="detaliiBiletContainer">
        <h5 id="flightTitleMain">Confirmed Flights</h5>
        <div id="listaZboruriCompacte">
    `;
    for (let i = 0; i < window.rezultateZboruriGlobale.length; i++) {
        let bilet = window.selectiiItinerariu[i];
        let plecareSplit = bilet.oraPlecare.split(' ');
        let sosireSplit = bilet.oraSosire.split(' ');
        let dataPlecare = plecareSplit[0];
        let oraPlecare = plecareSplit[1] || plecareSplit[0];
        let dataSosire = sosireSplit[0];
        let oraSosire = sosireSplit[1] || sosireSplit[0];

        htmlBilete += `
        <div id="rutaCompacta${i}">
            <span id="legLabel${i}">Leg ${i + 1}</span>
            
            <div id="rowSus${i}">
                <div id="airlineInfo${i}">
                    <div id="airlineLogo${i}">
                        <img src="${bilet.urlLogo}" alt="${bilet.companie}">
                    </div>
                    <div id="flightCode${i}">
                        <p id="airlineName${i}">${bilet.companie}</p>
                        <p id="planeModel${i}">Flight ${bilet.model}</p>
                    </div>
                </div>

                <div id="flightTimes${i}">
                    <div id="departCol${i}">
                        <span id="departLabel${i}">DEP</span>
                        <p id="departTime${i}">${oraPlecare}</p>
                        <span id="departDate${i}">${dataPlecare}</span>
                    </div>
                    <div id="arriveCol${i}">
                        <span id="arriveLabel${i}">ARR</span>
                        <p id="arriveTime${i}">${oraSosire}</p>
                        <span id="arriveDate${i}">${dataSosire}</span>
                    </div>
                </div>
            </div>
        </div>
        `;
    }
    htmlBilete += `
        </div>
        <button id="ticketBtnMain" onclick="window.genereazaDetaliiBilete()">Generate Tickets Details</button>
    </div>
    `;

    const containerBilete = document.getElementById('containerBilete');
    containerBilete.innerHTML = htmlBilete;
    containerBilete.style.display = 'block';
};
document.getElementById('inchidePopUp').addEventListener('click', function() {
    document.getElementById('popUpRezultateZboruri').style.display = 'none';
});
window.genereazaDetaliiBilete = async function() {
    const btn = document.getElementById('ticketBtnMain');
    btn.innerText = "Saving Tickets...";
    btn.disabled = true;
    const idGrup = parseInt(document.getElementById('idGrup').value);
    let bileteDeSalvat = [];
    for (let i = 0; i < window.rezultateZboruriGlobale.length; i++) {
        let biletSelectat = window.selectiiItinerariu[i];
        let segmentGlobal = window.rezultateZboruriGlobale[i];
        let zborOriginal = segmentGlobal.zboruri.find(z => z.itineraries[0].segments[0].number === biletSelectat.model);
        let segmentZbor = zborOriginal.itineraries[0].segments[0];
        let ruteParts = segmentGlobal.titluRuta.split('-');
        let orasPlecareExtras = ruteParts[0].replace(/\([A-Z]{3}\)/, '').trim();
        let orasSosireExtras = ruteParts[1].replace(/\([A-Z]{3}\)/, '').trim();
        bileteDeSalvat.push({
            grupId: idGrup,
            numeCompanie: biletSelectat.companie,
            numarZbor: biletSelectat.model,
            logo: biletSelectat.urlLogo,
            orasPlecare: orasPlecareExtras,
            orasSosire: orasSosireExtras,
            aeroportPlecare: segmentZbor.departure.iataCode,
            aeroportSosire: segmentZbor.arrival.iataCode,
            dataPlecare: segmentZbor.departure.at, 
            dataSosire: segmentZbor.arrival.at,
            pret: parseFloat(zborOriginal.price.total) 
        });
    }
    try {
        const raspuns = await fetch('/TravelGroup/SalveazaZboruriTG', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(bileteDeSalvat)
        });

        const rezultat = await raspuns.json();

        if (raspuns.ok) {
            btn.innerText = "Saved ✓";
            btn.style.backgroundColor = "#28a745";
            setTimeout(() => {
                    location.reload();
                }, 1000);
            }
        } else {
            btn.innerText = "Generate Tickets Details";
            btn.disabled = false;
        }
    } catch (er) {
        btn.innerText = "Generate Tickets Details";
        btn.disabled = false;
    }
};

window.bazaDateAeroporturi = [];
//fisier de pe github cu toate aeroporturile =)))
window.onload = async function() {
    try {
        const raspuns = await fetch('https://raw.githubusercontent.com/algolia/datasets/master/airports/airports.json');
        window.bazaDateAeroporturi = await raspuns.json();
    } catch (e) {
        console.error(e);
    }
};

document.getElementById('orasPlecare').addEventListener('input', function() {
    const textCautat = this.value.toLowerCase().trim();
    const containerSugestii = document.getElementById('listaSugestii');
    
    if (textCautat.length < 2) {
        containerSugestii.style.display = 'none';
        return;
    }

    const rezultate = window.bazaDateAeroporturi.filter(a => 
        (a.city && a.city.toLowerCase().includes(textCautat)) || 
        (a.name && a.name.toLowerCase().includes(textCautat)) ||
        (a.iata_code && a.iata_code.toLowerCase().includes(textCautat))
    ).slice(0, 7);

    if (rezultate.length > 0) {
        let htmlSugestii = "";
        rezultate.forEach(aeroport => {
            if(!aeroport.iata_code) return; 

            const valoareDeSalvat = `${aeroport.city} (${aeroport.iata_code})`;
            
            htmlSugestii += `
                <div class="autocomplete-item" onclick="window.selecteazaAeroport('${valoareDeSalvat}')">
                    <div>
                        <strong>${aeroport.city}</strong>, ${aeroport.country}<br>
                        <span style="font-size: 11px; color: #888;">${aeroport.name}</span>
                    </div>
                    <span class="iata-badge">${aeroport.iata_code}</span>
                </div>
            `;
        });
        containerSugestii.innerHTML = htmlSugestii;
        containerSugestii.style.display = 'block';
    } else {
        containerSugestii.style.display = 'none';
    }
});


window.selecteazaAeroport = function(valoare) {
    document.getElementById('orasPlecare').value = valoare;
    document.getElementById('listaSugestii').style.display = 'none';
    if (typeof window.genereazaSegmente === 'function') {
        window.genereazaSegmente();
    }
};
document.addEventListener('click', function(e) {
    if (!e.target.closest('.autocomplete-container')) {
        document.getElementById('listaSugestii').style.display = 'none';
    }
});
function inchidePopUpLocatii(){
    document.getElementById('popUpDestinatieManuala').style.display = 'none';
}
window.inchidePopUp = function() {
    document.getElementById('popUpRezultateZboruri').style.display = 'none';
};

window.incarcaBileteExistente = async function() {
    const idGrup = parseInt(document.getElementById('idGrup').value);
    if (!idGrup) 
        return;

    try {
        const raspuns = await fetch(`/TravelGroup/GetZboruriGrup?idGrup=${idGrup}`);
        if (raspuns.ok) {
            const zboruriSalvate = await raspuns.json();
            if (zboruriSalvate && zboruriSalvate.length > 0) {
                document.getElementById('formularZbor').style.display = 'none';
                
                let htmlBilete = `
                <div id="detaliiBiletContainer">
                    <h5 id="flightTitleMain">Confirmed Itinerary</h5>
                    <div id="listaZboruriCompacte">
                `;

                zboruriSalvate.forEach((bilet, i) => {
                    let dataPlecare = bilet.dataPlecare.split('T')[0];
                    let oraPlecare = bilet.dataPlecare.split('T')[1].substring(0, 5);
                    let dataSosire = bilet.dataSosire.split('T')[0];
                    let oraSosire = bilet.dataSosire.split('T')[1].substring(0, 5);

                    htmlBilete += `
                    <div id="rutaCompacta${i}">
                        <span id="legLabel${i}">Leg ${i + 1}</span>
                        
                        <div id="rowSus${i}">
                            <div id="airlineInfo${i}">
                                <div id="airlineLogo${i}">
                                    <img src="${bilet.logo}" alt="${bilet.numeCompanie}">
                                </div>
                                <div id="flightCode${i}">
                                    <p id="airlineName${i}">${bilet.numeCompanie}</p>
                                    <p id="planeModel${i}">Flight ${bilet.numarZbor}</p>
                                </div>
                            </div>

                            <div id="flightTimes${i}">
                                <div id="departCol${i}">
                                    <span id="departLabel${i}">DEP (${bilet.aeroportPlecare})</span>
                                    <p id="departTime${i}">${oraPlecare}</p>
                                    <span id="departDate${i}">${dataPlecare}</span>
                                </div>
                                <div id="arriveCol${i}">
                                    <span id="arriveLabel${i}">ARR (${bilet.aeroportSosire})</span>
                                    <p id="arriveTime${i}">${oraSosire}</p>
                                    <span id="arriveDate${i}">${dataSosire}</span>
                                </div>
                            </div>
                        </div>
                    </div>
                    `;
                });
                htmlBilete += `
                    </div>
                    <button id="butonEditareZboruri" style="width: 100%; background-color: #f8f9fa; color: #34495e; border: 1px solid #ced4da; padding: 14px; border-radius: 8px; font-weight: bold; font-size: 15px; cursor: pointer; margin-top: 20px;" onclick="window.editeazaItinerariul()">Edit Flights</button>
                </div>
                `;
                const containerBilete = document.getElementById('containerBilete');
                containerBilete.innerHTML = htmlBilete;
                containerBilete.style.display = 'block';
            }
        }
    } catch (eroare) {
        console.error(eroare);
    }
};
window.editeazaItinerariul = function() {

};