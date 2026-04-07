const conexiuneChat = new signalR.HubConnectionBuilder()
    .withUrl("/ChatHub")
    .build();

const idGrup = document.getElementById("idGrup").value;
const idProfilCurent = document.getElementById("esteAdmin").getAttribute("data-current-id"); 
conexiuneChat.on("PrimesteMesajGrup", (expeditorId, text, ora, mesajId, userName, avatarUrl, dataFull) => {
    adaugaMesajInInterfata(expeditorId, text, ora, userName, avatarUrl, mesajId, dataFull);
    
    const overlay = document.getElementById("fundalOverlay");
    const bulina = document.getElementById("notificareMesajNou");
    if (window.getComputedStyle(overlay).display === "none" && Number(expeditorId) !== Number(idProfilCurent)) {
        bulina.style.display = "block";
    }
});
conexiuneChat.on("MesajUpdateEditat", (mesajId, textNou) => {
    const elementText = document.getElementById(`text_${mesajId}`);
    if (elementText) {
        elementText.innerText = textNou;
        elementText.classList.add("editat-flash");
    }
});

conexiuneChat.on("MesajEliminatSters", (mesajId) => {
    const elementMesaj = document.getElementById(`msg_${mesajId}`);
    if (elementMesaj) {
        elementMesaj.style.opacity = "0";
        setTimeout(() => elementMesaj.remove(), 300);
    }
});

function confirmaStergere(mesajId) {
    if (confirm("Sigur vrei să ștergi acest mesaj? (Limita 10 min)")) {
        conexiuneChat.invoke("MesajDelete", mesajId)
            .catch(err => alert(err.message));
    }
}

function pregatesteEditare(mesajId, textVechi) {
    const nouText = prompt("Editează mesajul:", textVechi);
    if (nouText && nouText !== textVechi) {
        conexiuneChat.invoke("MesajEdit", mesajId, nouText);
    }
}
async function pornesteConexiune() {
    try {
        await conexiuneChat.start();
        await conexiuneChat.invoke("JoinGrupChat", parseInt(idGrup));
    } catch (err) {
        console.error("SignalR Connection Error: ", err);
    }
}

function esteMesajRecent(dataTrimite) {
    const dataMesaj = new Date(dataTrimite);
    const acum = new Date();
    const diferentaMinute = (acum - dataMesaj) / 60000;
    return diferentaMinute < 10;
}

function adaugaMesajInInterfata(expeditorId, text, ora, userName, avatarUrl, mesajId, dataFull) {
    const container = document.getElementById("listaMessages");
    const esteEu = Number(expeditorId) === Number(idProfilCurent); 
    const recent = esteMesajRecent(dataFull);
    const htmlMesaj = `
        <div class="mesajDiv ${esteEu ? 'sent' : 'received'}" id="msg_${mesajId}">
            <img src="${avatarUrl}" class="imgProf">
            <div class="mesajText">
                <div class="headerMesaj">
                    <span class="mesajUser">${esteEu ? 'You' : userName}</span>
                    ${(esteEu && recent) ? `
                        <div class="actiuniMesaj">
                            <i class="fa-solid fa-pen" onclick="deschideEditare(${mesajId}, '${text}')"></i>
                            <i class="fa-solid fa-trash" onclick="deschideStergere(${mesajId})"></i>
                        </div>` : ''}
                </div>
                <div class="bulaChat">
                    <span id="text_${mesajId}">${text}</span>
                    <span class="oraChat">${ora}</span>
                </div>
            </div>
        </div>`;

    container.insertAdjacentHTML('beforeend', htmlMesaj);
}
function deschideStergere(mesajId) {
    const modal = document.getElementById("modalCustomChat");
    const btnConfirma = document.getElementById("butonConfirma");
    
    document.getElementById("titluModalTitle").innerText = "Delete Message";
    document.getElementById("mesajModalMessage").innerText = "This action is permanent. Do you want to proceed?";
    
    modal.style.display = "flex";
    
    btnConfirma.onclick = async () => {
        await conexiuneChat.invoke("MesajDelete", Number(mesajId));
        inchideModalCustom();
    };
}

function inchideModalCustom() {
    document.getElementById("modalCustomChat").style.display = "none";
}
document.getElementById("butonSend").addEventListener("click", () => {
    const input = document.getElementById("inputMesaj");
    const text = input.value;
    const expeditorId = parseInt(document.getElementById("esteAdmin").getAttribute("data-current-id"));
    const idGrupActual = parseInt(document.getElementById("idGrup").value); // [cite: 77]

    if (isNaN(expeditorId) || isNaN(idGrupActual)) {
        return;
    }

    if (text.trim() !== "") {
        conexiuneChat.invoke("TrimiteMesajGrup", expeditorId, idGrupActual, text)
            .catch(err => console.error("Eroare la invoke:", err));
            
        input.value = "";
    }
});
function afiseazaChat(stare) {
    const overlay = document.getElementById("fundalOverlay");
    const bulina = document.getElementById("notificareMesajNou");

    if (stare) {
        overlay.style.display = "flex";
        bulina.style.display = "none"; 
       
        fetch(`/Chat/MarcheazaCititeGrup?idGrup=${idGrup}`, { method: 'POST' });
        
        incarcaIstoricChat();
    } else {
        overlay.style.display = "none";
    }
}
async function incarcaIstoricChat() {
    const raspuns = await fetch(`/Chat/GetIstoricGrup?idGrup=${idGrup}`);
    const mesaje = await raspuns.json();
    const container = document.getElementById("listaMessages");
    container.innerHTML = "";
    
    mesaje.forEach(m => {
        adaugaMesajInInterfata(m.expeditorId, m.text, m.ora, m.userName, m.avatarUrl, m.id, m.dataFull);
    });
}
let idMesajInEditare = null;

function deschideEditare(mesajId, textVechi) {
    idMesajInEditare = mesajId;
    const modal = document.getElementById("modalEditareChat");
    const textArea = document.getElementById("textEditareNou");
    const btnSalva = document.getElementById("butonConfirmaEdit");

    textArea.value = textVechi;
    modal.style.display = "flex";

    btnSalva.onclick = async () => {
        const textNou = textArea.value;
        if (textNou && textNou !== textVechi) {
            await conexiuneChat.invoke("MesajEdit", Number(idMesajInEditare), textNou);
        }
        inchideModalEditare();
    };
}

function inchideModalEditare() {
    document.getElementById("modalEditareChat").style.display = "none";
}
async function verificaNotificareInitiala() {
    const raspuns = await fetch(`/Chat/GetNumarNecitite?idGrup=${idGrup}`);
    const date = await raspuns.json();
    const bulina = document.getElementById("notificareMesajNou");

    if (date.count > 0) {
        bulina.style.display = "block";
    }
}
pornesteConexiune().then(() => verificaNotificareInitiala());