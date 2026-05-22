document.addEventListener("DOMContentLoaded", async function () {
    const tariVizitate = new Set(window.tariVizitate || []);

    document.getElementById('totalCount').innerText = tariVizitate.size;

    const map = L.map('globalMap', {
        scrollWheelZoom: true,
        zoomControl: true
    }).setView([20, 0], 2);

    L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Physical_Map/MapServer/tile/{z}/{y}/{x}', {
        attribution: 'Esri'
    }).addTo(map);

    const res = await fetch('https://raw.githubusercontent.com/nvkelso/natural-earth-vector/master/geojson/ne_110m_admin_0_countries.geojson');
    const data = await res.json();

    L.geoJson(data, {
        style: function (feature) {
            const cod = (feature.properties.ISO_A2 || "").toUpperCase();
            const esteVizitata = tariVizitate.has(cod);
            return {
                color: "#555",
                weight: 1,
                opacity: 0.6,
                fillColor: esteVizitata ? "#EE5607" : "#d1d5db",
                fillOpacity: esteVizitata ? 0.6 : 0.15
            };
        },
        onEachFeature: function (feature, layer) {
            const cod = (feature.properties.ISO_A2 || "").toUpperCase();
            const nume = feature.properties.ADMIN || feature.properties.NAME || "";
            if (tariVizitate.has(cod)) {
                layer.bindTooltip(`<b>${nume}</b> ✓`, { sticky: true });
            }
        }
    }).addTo(map);
});