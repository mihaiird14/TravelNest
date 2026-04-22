
function deschideItinerariu() {
    document.getElementById("popUpItinerariu").style.display = "flex";
     
}

function inchideItinerariu() {
    document.getElementById("popUpItinerariu").style.display = "none";
    document.body.style.overflow = "auto";
}
document.addEventListener("DOMContentLoaded", () => {
    if (localStorage.getItem("itinerariuDeschis") === "true") {
        deschideItinerariu();
        localStorage.removeItem("itinerariuDeschis");
    }
});

function reincarcaSiPastreazaDeschis() {
    localStorage.setItem("itinerariuDeschis", "true");
    location.reload();
}
function deschideManual() {
    document.getElementById("popUpManual").style.display = "flex";
}

function inchideManual() {
    document.getElementById("popUpManual").style.display = "none";
}

function salveazaManual() {
    const titlu = document.getElementById("inputTitlu").value;
    const zi = document.getElementById("inputZi").value;
    const ora = document.getElementById("inputOra").value;
    const desc = document.getElementById("inputDesc").value;

    if (!titlu || !ora) {
        return;
    }

    console.log("Salvare activitate manuală:", { zi, ora, titlu, desc });
    inchideManual();
}

async function genereazaAIItinerariu() {
    const prompt = document.getElementById("inputItinerariu").value;
    if (!prompt) return;

    console.log("Se generează itinerariu pentru:", prompt);
}

//add itinerariu 
window.salveazaManual = async function() {
    const model = {
        TravelGroupId: parseInt(document.getElementById("idGrup").value),
        Zi: parseInt(document.getElementById("inputZi").value),
        Ora: document.getElementById("inputOra").value,
        Titlu: document.getElementById("inputTitlu").value,
        Descriere: document.getElementById("inputDesc").value
    };

    const resp = await fetch('/TravelGroup/AdaugaActivitateManual', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(model)
    });

    if ((await resp.json()).success) 
        reincarcaSiPastreazaDeschis();
};
window.eliminaActivitate = async function(id) {
    try {
        const response = await fetch(`/TravelGroup/StergeActivitate?id=${id}`, {
            method: 'POST',
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });

        if (response.ok) {
            const date = await response.json();
            if (date.success) {
                const element = document.getElementById(`activitate-${id}`);
                if (element) {
                    element.style.transition = "all 0.3s ease";
                    element.style.opacity = "0";
                    element.style.transform = "scale(0.8)";
                    
                    setTimeout(() => {
                        element.remove();
                        redistribuieZigZag();
                        
                        if (document.querySelectorAll('.zi-container').length === 0) {
                            location.reload();
                        }
                    }, 300);
                }
            }
        }
    } catch (err) {
        console.error("Eroare server:", err);
    }
};

function redistribuieZigZag() {
    const items = document.querySelectorAll('.zi-container');
    items.forEach((item, index) => {
        item.classList.remove('stanga', 'dreapta');
        
        //recalculam
        if (index % 2 === 0) {
            item.classList.add('stanga');
        } else {
            item.classList.add('dreapta');
        }
    });
}
window.deschideEditare = function(id, zi, ora, titlu, desc) {
    document.getElementById("inputTitlu").value = titlu;
    document.getElementById("inputZi").value = zi;
    document.getElementById("inputOra").value = ora;
    document.getElementById("inputDesc").value = desc;
    document.querySelector("#boxManual h4").innerText = "Edit Activity";
    const btnSave = document.getElementById("btnSalveazaManual");
    btnSave.innerText = "Update Activity";
    btnSave.onclick = () => finalizeazaEditare(id);
    deschideManual();
};

async function finalizeazaEditare(id) {
    const model = {
        Id: id,
        Zi: parseInt(document.getElementById("inputZi").value),
        Ora: document.getElementById("inputOra").value,
        Titlu: document.getElementById("inputTitlu").value,
        Descriere: document.getElementById("inputDesc").value
    };

    const response = await fetch('/TravelGroup/EditeazaActivitate', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(model)
    });

    const result = await response.json();
    if (result.success) {
        location.reload();
    }
}

window.inchideManual = function() {
    document.getElementById("popUpManual").style.display = "none";
    document.querySelector("#boxManual h4").innerText = "Add Activity";
    const btnSave = document.getElementById("btnSalveazaManual");
    btnSave.innerText = "Save";
    btnSave.onclick = salveazaManual; 
    document.getElementById("formManual").querySelectorAll("input, textarea, select").forEach(el => el.value = "");
};
window.genereazaAIItinerariu = async function() {
    const prompt = document.getElementById("inputItinerariu").value;
    const idGrup = document.getElementById("idGrup").value;
    const btn = document.getElementById("btnAIItinerariu");

    if (!prompt) return alert("Please enter some preferences first!");

    btn.disabled = true;
    btn.innerHTML = '<i class="fa-solid fa-circle-notch fa-spin"></i> Generating...';

    const formData = new FormData();
    formData.append("idGrup", idGrup);
    formData.append("promptUtilizator", prompt);

    try {
        const response = await fetch('/TravelGroup/ItinerariuAI', {
            method: 'POST',
            body: formData
        });

        const data = await response.json();
        if (data.success) {
            localStorage.setItem("itinerariuDeschis", "true");
            location.reload();
        } else {
            btn.disabled = false;
        }
    } catch (e) {
        console.error(e);
        btn.disabled = false;
    }
};