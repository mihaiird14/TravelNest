let geoJsonLayer = null;
let tariVizitate = null;

document.addEventListener("DOMContentLoaded", async function () {
    // Deduplicate countries - ensure all uppercase
    tariVizitate = new Set((window.tariVizitate || []).map(c => (c || "").toUpperCase().trim()).filter(c => c));

    const totalCountries = 195;
    const visitedCount = tariVizitate.size;
    const percentage = Math.round((visitedCount / totalCountries) * 100);

    document.getElementById('totalCount').innerText = visitedCount;
    document.getElementById('progressPercentage').innerText = percentage + "%";

    const map = L.map('globalMap', {
        scrollWheelZoom: true,
        zoomControl: true
    }).setView([20, 0], 2);

    L.tileLayer('https://server.arcgisonline.com/ArcGIS/rest/services/World_Physical_Map/MapServer/tile/{z}/{y}/{x}', {
        attribution: 'Esri'
    }).addTo(map);

    const res = await fetch('https://raw.githubusercontent.com/nvkelso/natural-earth-vector/master/geojson/ne_110m_admin_0_countries.geojson');
    const data = await res.json();
 
    // NOW create geoJsonLayer after tariVizitate is defined
    geoJsonLayer = L.geoJson(data, {
        style: function (feature) {
            const props = feature.properties;
            
            let cod = "";
            
            if (props.ISO_A2 && props.ISO_A2 !== "-99") {
                cod = props.ISO_A2.toUpperCase().trim();
            }
            else if (props.WB_A2) {
                cod = props.WB_A2.toUpperCase().trim();
            }
            else if (props.ISO_A3 && props.ISO_A3 !== "-99") {
                cod = props.ISO_A3.toUpperCase().trim();
            }
            else if (props.FIPS) {
                cod = props.FIPS.toUpperCase().trim();
            }

            const esteVizitata = cod && tariVizitate.has(cod);
            
            return {
                color: "#555",
                weight: 1,
                opacity: 0.6,
                fillColor: esteVizitata ? "#EE5607" : "#d1d5db",
                fillOpacity: esteVizitata ? 0.6 : 0.15
            };
        },
        onEachFeature: function (feature, layer) {
            const props = feature.properties;
            const admin = props.ADMIN || props.NAME || "";
            
            let cod = "";
            
            if (props.ISO_A2 && props.ISO_A2 !== "-99") {
                cod = props.ISO_A2.toUpperCase().trim();
            }
            else if (props.WB_A2) {
                cod = props.WB_A2.toUpperCase().trim();
            }
            else if (props.ISO_A3 && props.ISO_A3 !== "-99") {
                cod = props.ISO_A3.toUpperCase().trim();
            }
            else if (props.FIPS) {
                cod = props.FIPS.toUpperCase().trim();
            }

            layer.on('click', async function() {
                if (!cod) return;

                const isVisited = tariVizitate.has(cod);
                
                if (isVisited) {
                    // Try to remove
                    const response = await fetch('/Map/ToggleCountry', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ countryCode: cod, isAdding: false })
                    });
                    const result = await response.json();
                    
                    if (result.success) {
                        tariVizitate.delete(cod);
                        updateMapView();
                        showNotification('Country removed', 'success');
                    } else {
                        showNotification(result.message, 'error');
                    }
                } else {
                    // Add country
                    const response = await fetch('/Map/ToggleCountry', {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ countryCode: cod, isAdding: true })
                    });
                    const result = await response.json();
                    
                    if (result.success) {
                        tariVizitate.add(cod);
                        updateMapView();
                        showNotification('Country added', 'success');
                    } else {
                        showNotification(result.message, 'error');
                    }
                }
            });

            layer.on('mouseover', function() {
                layer.setStyle({ fillOpacity: 0.8 });
            });

            layer.on('mouseout', function() {
                const esteVizitata = tariVizitate.has(cod);
                layer.setStyle({ fillOpacity: esteVizitata ? 0.6 : 0.15 });
            });

            if (cod && tariVizitate.has(cod)) {
                layer.bindTooltip(`<b>${admin}</b> ✓ (Click to remove)`, { sticky: true });
            } else {
                layer.bindTooltip(`<b>${admin}</b> (Click to add)`, { sticky: true });
            }
        }
    }).addTo(map);
});

function updateMapView() {
    const totalCountries = 195;
    const updatedPercentage = Math.round((tariVizitate.size / totalCountries) * 100);
    document.getElementById('totalCount').innerText = tariVizitate.size;
    document.getElementById('progressPercentage').innerText = updatedPercentage + "%";

    // Refresh layer styles
    geoJsonLayer.eachLayer(layer => {
        const feature = layer.feature;
        const props = feature.properties;
        
        let cod = "";
        if (props.ISO_A2 && props.ISO_A2 !== "-99") {
            cod = props.ISO_A2.toUpperCase().trim();
        } else if (props.WB_A2) {
            cod = props.WB_A2.toUpperCase().trim();
        } else if (props.ISO_A3 && props.ISO_A3 !== "-99") {
            cod = props.ISO_A3.toUpperCase().trim();
        }

        const esteVizitata = cod && tariVizitate.has(cod);
        layer.setStyle({
            fillColor: esteVizitata ? "#EE5607" : "#d1d5db",
            fillOpacity: esteVizitata ? 0.6 : 0.15
        });

        const admin = props.ADMIN || props.NAME || "";
        if (esteVizitata) {
            layer.bindTooltip(`<b>${admin}</b> ✓ (Click to remove)`, { sticky: true });
        } else {
            layer.bindTooltip(`<b>${admin}</b> (Click to add)`, { sticky: true });
        }
    });
}

function showNotification(message, type) {
    const notification = document.createElement('div');
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 12px 20px;
        background: ${type === 'success' ? '#10b981' : '#ef4444'};
        color: white;
        border-radius: 8px;
        font-weight: 600;
        z-index: 10000;
        animation: slideIn 0.3s ease;
    `;
    notification.textContent = message;
    document.body.appendChild(notification);

    setTimeout(() => notification.remove(), 3000);
}