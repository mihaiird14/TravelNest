if (typeof window.conexiuneChat === 'undefined') {
    window.conexiuneChat = new signalR.HubConnectionBuilder()
        .withUrl("/ChatHub")
        .build();
}

window.idDestinatarActiv = window.idDestinatarActiv || null;
window.elIdGrup = document.getElementById("idGrup");
window.idGrup = window.elIdGrup ? window.elIdGrup.value : null;
window.elProfil = document.getElementById("esteAdmin") || document.getElementById("mainChatContainer");
window.idProfilCurent = window.elProfil ? window.elProfil.getAttribute("data-current-id") : null;

async function updateNrMesajeNecitite() {
    try {
        const raspuns = await fetch('/Chat/GetTotalNecititePrivat');
        if (!raspuns.ok) return;
        const date = await raspuns.json();
        const bulinaChat = document.querySelector('a[href*="/Chat"] #punctRosu');

        if (bulinaChat) {
            if (date.count > 0) {
                bulinaChat.innerText = date.count;
                bulinaChat.style.display = "inline-block"; 
            } else {
                bulinaChat.style.display = "none";
            }
        }
    } catch (err) { console.error("Eroare update bulina:", err); }
}


window.conexiuneChat.on("PrimesteMesajPrivat", (expeditorId, text, ora, numeExp, imgExp, mesajId, dataFull) => {
    const esteChatDeschis = window.idDestinatarActiv && (Number(expeditorId) === Number(window.idDestinatarActiv) || Number(expeditorId) === Number(window.idProfilCurent));
    
    if (esteChatDeschis) {
        adaugaMesajPrivatInInterfata(expeditorId, text, ora, mesajId, dataFull);
    }
    
    let idContact = (Number(expeditorId) === Number(window.idProfilCurent)) ? window.idDestinatarActiv : expeditorId;
    let nume = (Number(expeditorId) === Number(window.idProfilCurent)) ? document.getElementById("numeDestinatarHeader").innerText : numeExp;
    let imagine = (Number(expeditorId) === Number(window.idProfilCurent)) ? document.getElementById("avatarDestinatarHeader").src : imgExp;

    actualizeazaRandConversatie(idContact, text, ora, nume, imagine);
    
    if (!esteChatDeschis && Number(expeditorId) !== Number(window.idProfilCurent)) {
        updateNrMesajeNecitite();
    }
});

window.conexiuneChat.on("PrimesteMesajGrup", (expeditorId, text, ora, mesajId, userName, avatarUrl, dataFull) => {
    if (typeof adaugaMesajInInterfata === "function") {
        adaugaMesajInInterfata(expeditorId, text, ora, userName, avatarUrl, mesajId, dataFull);
    }
    const overlay = document.getElementById("fundalOverlay");
    const bulina = document.getElementById("notificareMesajNou");
    if (overlay && window.getComputedStyle(overlay).display === "none" && Number(expeditorId) !== Number(window.idProfilCurent)) {
        if (bulina) bulina.style.display = "block";
    }
});

window.conexiuneChat.on("MesajUpdateEditat", (mesajId, textNou) => {
    const elementText = document.getElementById(`text_${mesajId}`);
    if (elementText) {
        elementText.innerText = textNou;
        elementText.classList.add("editat-flash");
    }
});

window.conexiuneChat.on("MesajEliminatSters", (mesajId) => {
    const elementMesaj = document.getElementById(`msg_${mesajId}`);
    if (elementMesaj) {
        elementMesaj.style.opacity = "0";
        setTimeout(() => elementMesaj.remove(), 300);
    }
});


function adaugaMesajPrivatInInterfata(expeditorId, text, ora, mesajId, dataFull) {
    const container = document.getElementById("listaMesajePrivat");
    if (!container) return;

    const esteEu = Number(expeditorId) === Number(window.idProfilCurent);
    const recent = esteMesajRecent(dataFull); 

    const htmlMesaj = `
        <div class="mesajDivPrivat ${esteEu ? 'sent' : 'received'}" id="msg_${mesajId}">
            <div class="bulaPrivat">
                <div class="continutBulaPrivat" style="display: flex; align-items: center; gap: 8px;">
                    <span id="text_${mesajId}">${text}</span>
                    ${(esteEu && recent) ? `
                        <div class="actiuniMesajPrivat">
                            <i class="fa-solid fa-pen" onclick="deschideEditare(${mesajId}, '${text.replace(/'/g, "\\'")}')"></i>
                            <i class="fa-solid fa-trash" onclick="deschideStergere(${mesajId})"></i>
                        </div>` : ''}
                </div>
                <small class="oraChatPrivat">${ora}</small>
            </div>
        </div>`;

    container.insertAdjacentHTML('beforeend', htmlMesaj);
    container.scrollTop = container.scrollHeight;
}
function adaugaMesajInInterfata(expeditorId, text, ora, userName, avatarUrl, mesajId, dataFull) {
    const container = document.getElementById("listaMessages");
    if (!container) return;

    const esteEu = Number(expeditorId) === Number(window.idProfilCurent);
    const recent = esteMesajRecent(dataFull);
    const htmlMesaj = `
        <div class="mesajDiv ${esteEu ? 'sent' : 'received'}" id="msg_${mesajId}">
            <img src="${avatarUrl || '/images/default.png'}" class="imgProf">
            <div class="mesajText">
                ${!esteEu ? `<span class="mesajUser">${userName}</span>` : ''}
                <div class="bulaChat">
                    <span id="text_${mesajId}">${text}</span>
                    ${(esteEu && recent) ? `
                        <div class="actiuniMesaj" style="display:none;"> <i class="fa-solid fa-pen" onclick="deschideEditare(${mesajId}, '${text.replace(/'/g, "\\'")}')"></i>
                            <i class="fa-solid fa-trash" onclick="deschideStergere(${mesajId})"></i>
                        </div>` : ''}
                </div>
                <small class="oraChat">${ora}</small>
            </div>
        </div>`;

    container.insertAdjacentHTML('beforeend', htmlMesaj);
    container.scrollTop = container.scrollHeight;
}
function actualizeazaRandConversatie(idContact, text, ora, nume, imagine) {
    const container = document.getElementById("containerConversatii");
    if (!container) return;
    let rand = document.getElementById(`conv_${idContact}`);
    const esteMesajNouPrimit = (Number(idContact) !== Number(window.idProfilCurent)) && (Number(idContact) !== Number(window.idDestinatarActiv));

    if (rand) {
        rand.querySelector(".snippetUltimulMesaj").innerText = text;
        rand.querySelector(".oraTrimisData").innerText = ora;
        if (esteMesajNouPrimit) rand.classList.add("conversatie-necitita");
        container.prepend(rand);
    } else { 
        const htmlNou = `<div class="randContactConversatie ${esteMesajNouPrimit ? 'conversatie-necitita' : ''}" onclick="deschideChatPrivat(${idContact}, '${nume}', '${imagine}')" id="conv_${idContact}">...</div>`;
        container.insertAdjacentHTML('afterbegin', htmlNou);
    }
}


function deschideStergere(mesajId) {
    const modal = document.getElementById("modalStergereChat");
    const btnConfirma = document.getElementById("butonConfirmaConfirm");
    if (modal && btnConfirma) {
        modal.style.display = "flex";
        btnConfirma.onclick = async () => {
            await window.conexiuneChat.invoke("MesajDelete", Number(mesajId));
            inchideModalStergere();
        };
    }
}

function inchideModalStergere() {
    const modal = document.getElementById("modalStergereChat");
    if (modal) modal.style.display = "none";
}

function deschideEditare(mesajId, textVechi) {
    const modal = document.getElementById("modalEditareChat");
    const textArea = document.getElementById("textEditareNou");
    const btnSalva = document.getElementById("butonConfirmaEdit");
    if (modal && textArea && btnSalva) {
        textArea.value = textVechi;
        modal.style.display = "flex";
        btnSalva.onclick = async () => {
            await window.conexiuneChat.invoke("MesajEdit", Number(mesajId), textArea.value);
            inchideModalEditare();
        };
    }
}

function inchideModalEditare() {
    const modal = document.getElementById("modalEditareChat");
    if (modal) modal.style.display = "none";
}

async function deschideChatPrivat(idDestinatar, nume, imagine) {
    window.idDestinatarActiv = idDestinatar;
    

    const rand = document.getElementById(`conv_${idDestinatar}`);
    if (rand && rand.classList.contains("conversatie-necitita")) {
        rand.classList.remove("conversatie-necitita");
        const badge = rand.querySelector(".badgeNecititeConversatie");
        if (badge) badge.remove();
    }

    document.getElementById("ecranGolSelecteaza").style.display = "none";
    document.getElementById("fereastraChatPrivat").style.display = "flex";
    document.getElementById("avatarDestinatarHeader").src = imagine;
    document.getElementById("numeDestinatarHeader").innerText = nume;

    await fetch(`/Chat/MarcheazaCititePrivat?idExpeditor=${idDestinatar}`, { method: 'POST' });
    updateNrMesajeNecitite();

    // Istoric
    const response = await fetch(`/Chat/GetIstoricPrivat?destinatarId=${idDestinatar}`);
    const mesaje = await response.json();
    const container = document.getElementById("listaMesajePrivat");
    if (container) {
        container.innerHTML = "";
        mesaje.forEach(m => adaugaMesajPrivatInInterfata(m.expeditorId, m.text, m.ora, m.id, m.dataFull));
        container.scrollTop = container.scrollHeight;
    }
    await window.conexiuneChat.invoke("JoinPrivateChat", Number(window.idProfilCurent), Number(idDestinatar));
}
async function pornesteConexiune() {
    if (window.conexiuneChat.state === signalR.HubConnectionState.Disconnected) {
        try {
            await window.conexiuneChat.start();
            if (window.idGrup) await window.conexiuneChat.invoke("JoinGrupChat", parseInt(window.idGrup));
        } catch (err) { console.error("SignalR Error: ", err); }
    }
}

function esteMesajRecent(dataTrimite) {
    if (!dataTrimite) return false;
    const dataMesaj = new Date(dataTrimite);
    return (new Date() - dataMesaj) / 60000 < 10;
}
async function trimiteMesajPrivat() {
    const input = document.getElementById("msgInputPrivat");
    if (!input)
        return;
    const text = input.value;
    if (window.idDestinatarActiv && text.trim() !== "") {
        try {
            await window.conexiuneChat.invoke("TrimiteMesajPrivat", 
                Number(window.idProfilCurent), 
                Number(window.idDestinatarActiv), 
                text
            );
            
            input.value = ""; 
            input.focus();
        } catch (err) {
            console.error(err);
        }
    }
}

window.trimiteMesajPrivat = trimiteMesajPrivat;
async function verificaNotificareInitiala() {
    if (!window.idGrup) return;
    const raspuns = await fetch(`/Chat/GetNumarNecitite?idGrup=${window.idGrup}`);
    if (raspuns.ok) {
        const date = await raspuns.json();
        const bulina = document.getElementById("notificareMesajNou");
        if (bulina) bulina.style.display = date.count > 0 ? "block" : "none";
    }
}
const btnSendGrup = document.getElementById("butonSend");
if (btnSendGrup) {l
    btnSendGrup.onclick = () => {
        const input = document.getElementById("inputMesaj");
        if (!input || !window.idProfilCurent || !window.idGrup) 
            return;
        
        const text = input.value;
        if (text.trim() !== "") {
            window.conexiuneChat.invoke("TrimiteMesajGrup", 
                parseInt(window.idProfilCurent), 
                parseInt(window.idGrup), 
                text
            ).catch(err => console.error("Eroare trimitere grup:", err));
            input.value = "";
            input.focus();
        }
    };
}
async function incarcaIstoricGrup() {
    if (!window.idGrup) return;

    try {
        const response = await fetch(`/Chat/GetIstoricGrup?idGrup=${window.idGrup}`);
        if (!response.ok) return;
        
        const mesaje = await response.json();
        const container = document.getElementById("listaMessages");
        
        if (container) {
            container.innerHTML = "";
            mesaje.forEach(m => {
                adaugaMesajInInterfata(m.expeditorId, m.text, m.ora, m.userName, m.avatarUrl, m.id, m.dataFull);
            });
            container.scrollTop = container.scrollHeight;
        }

        await fetch(`/Chat/MarcheazaCititeGrup?idGrup=${window.idGrup}`, { method: 'POST' });
    } catch (err) {
        console.error(err);
    }
}
window.pornesteConexiune().then(() => {
    updateNrMesajeNecitite();
    if (window.idGrup) {
        incarcaIstoricGrup(); 
        verificaNotificareInitiala();
    }
});