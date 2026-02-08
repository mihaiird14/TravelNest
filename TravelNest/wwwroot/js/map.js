document.addEventListener("DOMContentLoaded", function () {
    const inputLocatie = document.getElementById('inputLocatie');
    const listaLocatii = document.getElementById('listaLocatii');
    const miniHarta = document.getElementById('miniHarta');
    const editInput = document.getElementById('edit-inputLocatie');
    function aplicaStiluriDropdown(lista) {
        if (!lista) return;
        lista.style.display = 'none';
        lista.style.position = 'absolute';
        lista.style.top = '100%';
        lista.style.left = '0';
        lista.style.width = '100%';
        lista.style.background = 'white';
        lista.style.border = '1px solid #ddd';
        lista.style.borderTop = 'none';
        lista.style.zIndex = '1000';
        lista.style.maxHeight = '200px';
        lista.style.overflowY = 'auto';
        lista.style.borderRadius = '0 0 10px 10px';
        lista.style.boxShadow = '0 4px 12px rgba(0,0,0,0.1)';
        
        if (lista.parentElement) {
            lista.parentElement.style.position = 'relative';
        }
    }
    aplicaStiluriDropdown(listaLocatii);
    let timeoutSearch;      
    let harta = null;  
    let markerCurent = null;

    if (inputLocatie) {
        inputLocatie.addEventListener('keyup', function () {
            const valoare = this.value;
            clearTimeout(timeoutSearch);
            if (valoare.length < 3) {
                listaLocatii.style.display = 'none';
                return;
            }
            timeoutSearch = setTimeout(() => { cautaLocatie(valoare, inputLocatie, listaLocatii, afiseazaHarta); }, 500);
        });
    }
    let timeoutSearchEdit;
    let hartaEdit = null;
    let markerEdit = null;
    let editLista = null;
    let editHartaDiv = null;

    if (editInput) {
        const parent = editInput.parentElement;
        parent.style.position = 'relative';

        editLista = document.createElement('div');
        editLista.id = 'edit-listaLocatii';
        aplicaStiluriDropdown(editLista);
        parent.appendChild(editLista);

        editHartaDiv = document.createElement('div');
        editHartaDiv.id = 'edit-miniHarta';
        editHartaDiv.style.cssText = 'height:180px; width:100%; display:none; margin-top:10px; border-radius:8px; z-index:1;';
        parent.appendChild(editHartaDiv);

        editInput.addEventListener('keyup', function () {
            const valoare = this.value;
            clearTimeout(timeoutSearchEdit);
            if (valoare.length < 3) {
                editLista.style.display = 'none';
                return;
            }
            timeoutSearchEdit = setTimeout(() => { cautaLocatie(valoare, editInput, editLista, afiseazaHartaEdit); }, 500);
        });
    }

    document.addEventListener('click', function (e) {
        if (listaLocatii) listaLocatii.style.display = 'none';
        if (editLista) editLista.style.display = 'none';
    });
    function cautaLocatie(text, inputField, listContainer, callbackHarta) {
        const apiUrl = `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(text)}&addressdetails=1&limit=5`;

        fetch(apiUrl)
            .then(res => res.json())
            .then(rezultate => {
                listContainer.innerHTML = '';
                if (!rezultate || rezultate.length === 0) {
                    listContainer.style.display = 'none';
                    return;
                }

                listContainer.style.display = 'block';

                rezultate.forEach(loc => {
                    let label = loc.display_name;
                    if (loc.address) {
                        const oras = loc.address.city || loc.address.town || loc.address.village || loc.address.county;
                        const tara = loc.address.country;
                        if (oras && tara) label = oras + ', ' + tara;
                    }

                    const item = document.createElement('div');
                    item.className = 'item-locatie';
                    item.style.padding = '12px 15px';
                    item.style.cursor = 'pointer';
                    item.style.borderBottom = '1px solid #f0f2f5';
                    item.style.display = 'flex';
                    item.style.alignItems = 'center';
                    item.style.gap = '10px';
                    item.style.fontSize = '14px';
                    item.style.color = '#1c1e21';
                    item.innerHTML = `<i class="fa-solid fa-location-dot" style="color:#5596f5"></i> <span>${label}</span>`;

                    item.onmouseover = () => item.style.backgroundColor = '#f0f8ff';
                    item.onmouseout = () => item.style.backgroundColor = 'white';

                    item.onclick = function () {
                        inputField.value = label;
                        listContainer.style.display = 'none';
                        callbackHarta(loc.lat, loc.lon);
                    };
                    listContainer.appendChild(item);
                });
            });
    }

    function afiseazaHarta(lat, lon) {
        miniHarta.style.display = 'block';
        if (!harta) {
            harta = L.map('miniHarta').setView([lat, lon], 12);
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(harta);
        } else {
            harta.setView([lat, lon], 12);
            harta.invalidateSize();
        }
        if (markerCurent) markerCurent.setLatLng([lat, lon]);
        else markerCurent = L.marker([lat, lon]).addTo(harta);
    }

    function afiseazaHartaEdit(lat, lon) {
        editHartaDiv.style.display = 'block';
        if (!hartaEdit) {
            hartaEdit = L.map('edit-miniHarta').setView([lat, lon], 12);
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png').addTo(hartaEdit);
        } else {
            hartaEdit.setView([lat, lon], 12);
            setTimeout(() => { hartaEdit.invalidateSize(); }, 100);
        }
        if (markerEdit) markerEdit.setLatLng([lat, lon]);
        else markerEdit = L.marker([lat, lon]).addTo(hartaEdit);
    }
});