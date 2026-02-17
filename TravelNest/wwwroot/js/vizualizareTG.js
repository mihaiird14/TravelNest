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