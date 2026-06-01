const NOMINATIM = 'https://nominatim.openstreetmap.org/search';
const GEOJSON_URL = 'https://raw.githubusercontent.com/nvkelso/natural-earth-vector/master/geojson/ne_110m_admin_0_countries.geojson';

let countryCentroids = {};
let map = null;
let geoJsonLayer = null;
const flightPolylines = [];
const profileMarkers = [];

// Normalizeaza un obiect — accepta atat PascalCase cat si camelCase
function n(obj, ...keys) {
    for (const k of keys) {
        if (obj[k] !== undefined && obj[k] !== null)
            return obj[k];
        const lower = k.charAt(0).toLowerCase() + k.slice(1);
        if (obj[lower] !== undefined && obj[lower] !== null)
            return obj[lower];
    }
    return undefined;
}

document.addEventListener('DOMContentLoaded', async function () {
    const mapDiv = document.createElement('div');
    mapDiv.id = 'friendsMap';
    mapDiv.style.cssText = 'position:fixed;top:0;left:0;width:100%;height:100%;z-index:0;';
    document.body.insertBefore(mapDiv, document.body.firstChild);

    map = L.map('friendsMap', { scrollWheelZoom: true, zoomControl: true }).setView([20, 0], 2);

    L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Physical_Map/MapServer/tile/{z}/{y}/{x}', {
        attribution: 'Esri'
    }).addTo(map);

    const res = await fetch(GEOJSON_URL);
    const data = await res.json();

    data.features.forEach(f => {
        const props = f.properties;
        const cod = getIso(props);
        if (!cod) return;
        try {
            const bounds = L.geoJson(f).getBounds();
            const center = bounds.getCenter();
            countryCentroids[cod] = [center.lat, center.lng];
        } catch (_) { }
    });

    geoJsonLayer = L.geoJson(data, {
        style: { color: '#555', weight: 1, opacity: 0.5, fillColor: '#d1d5db', fillOpacity: 0.12 }
    }).addTo(map);

    const friends = window.friendsMapData || [];
    for (const friend of friends) {
        const grupuri = n(friend, 'Grupuri', 'grupuri') || [];
        for (const grup of grupuri) {
            await renderFriendGroup(friend, grup);
        }
    }

    updateLegend(friends);
});

function getIso(props) {
    if (props.ISO_A2 && props.ISO_A2 !== '-99')
        return props.ISO_A2.toUpperCase().trim();
    if (props.WB_A2)
        return props.WB_A2.toUpperCase().trim();
    if (props.ISO_A3 && props.ISO_A3 !== '-99')
        return props.ISO_A3.toUpperCase().trim();
    return '';
}

async function geocodeAirport(aeroport, oras) {
    const q = encodeURIComponent(`${aeroport} airport ${oras}`);
    try {
        const r = await fetch(`${NOMINATIM}?format=json&q=${q}&limit=1`);
        const d = await r.json();
        if (d && d[0])
            return [parseFloat(d[0].lat), parseFloat(d[0].lon)];
    } catch (_) { }
    return null;
}

function getCentroid(codTara) {
    if (!codTara)
        return null;
    return countryCentroids[codTara.toUpperCase()] || null;
}

function createProfileIcon(imgUrl, label, status) {
    const badge = status === 'upcoming'
        ? `<div class="fm-badge fm-soon">SOON</div>`
        : status === 'flying'
            ? `<div class="fm-badge fm-flying">✈ FLYING</div>`
            : '';

    return L.divIcon({
        className: '',
        html: `
            <div class="fm-marker-wrap">
                ${badge}
                <div class="fm-avatar">
                    <img src="${imgUrl}" onerror="this.src='/images/profilDefault.png'" />
                </div>
                <div class="fm-pin-tip"></div>
                <div class="fm-name">${label}</div>
            </div>`,
        iconSize: [56, 76],
        iconAnchor: [28, 70],
        popupAnchor: [0, -72]
    });
}

async function renderFriendGroup(friend, grup) {
    const status = n(grup, 'Status', 'status');
    const codTara = n(grup, 'CodTara', 'codTara');
    const zborAzi = n(grup, 'ZborAzi', 'zborAzi');
    const numeGrup = n(grup, 'NumeGrup', 'numeGrup');
    const imgUrl = n(friend, 'ImagineProfil', 'imagineProfil') || '/images/profilDefault.png';
    const nume = n(friend, 'Nume', 'nume') || '';

    if (zborAzi) {
        await renderFlightAnimation(friend, grup);
        return;
    }

    const coords = getCentroid(codTara);
    if (!coords)
        return;

    const icon = createProfileIcon(imgUrl, nume, status);
    const marker = L.marker(coords, { icon }).addTo(map);
    profileMarkers.push(marker);
}

async function renderFlightAnimation(friend, grup) {
    const zborAzi = n(grup, 'ZborAzi', 'zborAzi');
    const codTara = n(grup, 'CodTara', 'codTara');
    const numeGrup = n(grup, 'NumeGrup', 'numeGrup');
    const imgUrl = n(friend, 'ImagineProfil', 'imagineProfil') || '/images/profilDefault.png';
    const nume = n(friend, 'Nume', 'nume') || '';

    const aeroportPlecare = n(zborAzi, 'AeroportPlecare', 'aeroportPlecare');
    const aeroportSosire = n(zborAzi, 'AeroportSosire', 'aeroportSosire');
    const orasPlecare = n(zborAzi, 'OrasPlecare', 'orasPlecare');
    const orasSosire = n(zborAzi, 'OrasSosire', 'orasSosire');
    const numarZbor = n(zborAzi, 'NumarZbor', 'numarZbor');
    const dataPlecare = n(zborAzi, 'DataPlecare', 'dataPlecare');
    const dataSosire = n(zborAzi, 'DataSosire', 'dataSosire');

    const coordsFrom = await geocodeAirport(aeroportPlecare, orasPlecare) || getCentroid(codTara) || [0, 0];
    const coordsTo = await geocodeAirport(aeroportSosire, orasSosire) || coordsFrom;

    const [latFrom, lngFrom] = coordsFrom;
    const [latTo, lngTo] = coordsTo;

    const polyline = L.polyline(
        [[latFrom, lngFrom], [latTo, lngTo]],
        { color: '#EE5607', weight: 2, dashArray: '6 8', opacity: 0.75 }
    ).addTo(map);
    flightPolylines.push(polyline);

    const midLat = (latFrom + latTo) / 2;
    const midLng = (lngFrom + lngTo) / 2;

    const icon = createProfileIcon(imgUrl, nume, 'flying');
    const marker = L.marker([midLat, midLng], { icon }).addTo(map);
    profileMarkers.push(marker);

    animateAlongLine(marker, [latFrom, lngFrom], [latTo, lngTo],
        new Date(dataPlecare), new Date(dataSosire));
}

function animateAlongLine(marker, from, to, depDate, arrDate) {
    const totalMs = arrDate - depDate;
    if (totalMs <= 0)
        return;

    function tick() {
        const now = Date.now();
        const elapsed = now - depDate.getTime();
        const progress = Math.min(Math.max(elapsed / totalMs, 0), 1);
        marker.setLatLng([from[0] + (to[0] - from[0]) * progress, from[1] + (to[1] - from[1]) * progress]);
        if (progress < 1)
            requestAnimationFrame(tick);
    }
    requestAnimationFrame(tick);
}

function updateLegend(friends) {
    const legend = document.getElementById('overlapLegend');
    if (!friends.length) {
        legend.style.display = 'none';
        return;
    }

    legend.style.display = 'block';
    legend.innerHTML = '<h4>Friends traveling</h4>';

    friends.forEach(f => {
        const grupuri = n(f, 'Grupuri', 'grupuri') || [];
        const imgUrl = n(f, 'ImagineProfil', 'imagineProfil') || '/images/profilDefault.png';
        const nume = n(f, 'Nume', 'nume') || '';
        const statuses = grupuri.map(g => n(g, 'ZborAzi', 'zborAzi') ? 'flying' : n(g, 'Status', 'status'));
        const label = statuses.includes('flying') ? '✈' : statuses.includes('active') ? '📍' : '🗓';

        const row = document.createElement('div');
        row.className = 'legend-row';
        row.innerHTML = `
            <img src="${imgUrl}" onerror="this.src='/images/profilDefault.png'" class="legend-avatar"/>
            <span>${nume}</span>
            <span class="legend-status">${label}</span>`;
        legend.appendChild(row);
    });
}