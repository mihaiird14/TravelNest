
document.addEventListener("DOMContentLoaded", function () {

    const inputLocatie = document.getElementById('inputLocatie');
    const listaLocatii = document.getElementById('listaLocatii');
    const miniHarta = document.getElementById('miniHarta');

    let timeoutSearch;      
    let harta = null;  
    let markerCurent = null;
    if (!inputLocatie) {
        return;
    }
    inputLocatie.addEventListener('keyup', function () {
        const valoare = this.value;
        clearTimeout(timeoutSearch);

        if (valoare.length < 3) {
            listaLocatii.style.display = 'none';
            return;
        }
        timeoutSearch = setTimeout(function () {
            cautaLocatie(valoare);
        }, 500);
    });
    document.addEventListener('click', function (event) {
        if (event.target !== inputLocatie && event.target !== listaLocatii) {
            listaLocatii.style.display = 'none';
        }
    });

    function cautaLocatie(text) {
        const apiUrl =
            'https://nominatim.openstreetmap.org/search' +
            '?format=json' +
            '&q=' + encodeURIComponent(text) +
            '&addressdetails=1' +
            '&limit=5';

        fetch(apiUrl)
            .then(function (response) {
                return response.json();
            })
            .then(function (rezultate) {

                listaLocatii.innerHTML = '';

                if (!rezultate || rezultate.length === 0) {
                    listaLocatii.style.display = 'none';
                    return;
                }

                listaLocatii.style.display = 'block';

                rezultate.forEach(function (loc) {
                    let label = loc.display_name; 
                    if (loc.address) {
                        const oras =
                            loc.address.city ||
                            loc.address.town ||
                            loc.address.village ||
                            loc.address.county;

                        const tara = loc.address.country;

                        if (oras && tara) {
                            label = oras + ', ' + tara;
                        }
                    }

                    const item = document.createElement('div');
                    item.className = 'item-locatie';
                    item.innerHTML =
                        '<i class="fa-solid fa-location-dot"></i> ' +
                        '<span>' + label + '</span>';

                    item.onclick = function () {
                        inputLocatie.value = label;
                        listaLocatii.style.display = 'none';
                        afiseazaHarta(loc.lat, loc.lon);
                    };

                    listaLocatii.appendChild(item);
                });
            })
            .catch(function (err) {
                console.error('Eroare la căutare locație:', err);
            });
    }

    function afiseazaHarta(lat, lon) {
        miniHarta.style.display = 'block';

        if (!harta) {
            harta = L.map('miniHarta').setView([lat, lon], 10);

            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '&copy; OpenStreetMap contributors'
            }).addTo(harta);
        } else {
            harta.setView([lat, lon], 10);
            harta.invalidateSize(); 
        }

        if (markerCurent) {
            markerCurent.setLatLng([lat, lon]);
        } else {
            markerCurent = L.marker([lat, lon]).addTo(harta);
        }
    }
});
