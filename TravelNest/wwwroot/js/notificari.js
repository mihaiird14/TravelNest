const conexiune = new signalR.HubConnectionBuilder()
    .withUrl("/NotificariHub")
    .build();
conexiune.on("PrimesteNotificare", function (titlu, mesaj, tip, expeditor, id, expeditorId, pozaExpeditor) {
    const punct = document.getElementById("punctRosu");
    if (punct) {
        let count = parseInt(punct.innerText) || 0;
        punct.innerText = count + 1;
        punct.style.display = "flex";
    }

    const listaPagina = document.getElementById("listaNotificari"); 
    if (listaPagina) {
        const mesajGol = document.getElementById("mesajGrup");
        if (mesajGol) 
            mesajGol.style.display = "none";
        const imagine = pozaExpeditor ? pozaExpeditor : "/images/profilDefault.png";
        let butoaneExtra = "";
        if (tip === 'FollowRequest' || tip === 'Follow') {
            butoaneExtra += `<a href="/Profil/Index/${expeditorId}" class="btn-view-notif">View</a>`;
        }
        if (tip === 'FollowRequest') {
            butoaneExtra += `
                <button id="butonAccept" onclick="raspundeCerere(${id}, 'Accept')">Accept</button>
                <button id="butonRefuz" onclick="raspundeCerere(${id}, 'Decline')">Decline</button>`;
        } else if (tip === 'TGInvite') {
            butoaneExtra += `
                <button id="butonAccept" onclick="raspundeInvitatie(event, ${id}, true)">Accept</button>
                <button id="butonRefuz" onclick="raspundeInvitatie(event, ${id}, false)">Reject</button>`;
        }

        const cardNou = `
            <div id="cardNotif" class="notificareNoua" id="notif-${id}">
                <div id="infoNotif">
                    <img src="${imagine}" style="width: 40px; height: 40px; border-radius: 50%; object-fit: cover; margin-right: 10px;" />
                    <div id="textNotif">
                        <p id="mesajNotif"><strong>${expeditor}</strong> ${mesaj}</p>
                        <span id="dataNotif">Now</span>
                    </div>
                </div>
                <div id="actiuniNotif" style="display: flex; gap: 5px; margin-top: 10px; justify-content: flex-end;">
                    ${butoaneExtra}
                </div>
            </div>`;
        
        listaPagina.insertAdjacentHTML('afterbegin', cardNou);
    }
});
async function deschideMeniuNotificari() {
    await fetch('/Profil/MarcheazaCitite', { method: 'POST' });
    const punct = document.getElementById("punctRosu");
    if (punct) {
        punct.innerText = "0";
        punct.style.display = "none";
    }
}
async function raspundeInvitatie(eveniment, notificareId, accepta) {
    if (!eveniment) {
        return;
    }

    const butonApasat = eveniment.currentTarget || eveniment.target;
    const dateForm = new FormData();
    dateForm.append("notificareId", notificareId);
    dateForm.append("accepta", accepta);

    try {
        const raspuns = await fetch('/Notifications/RaspundeInviteTG', {
            method: 'POST',
            body: dateForm
        });
        const rezultat = await raspuns.json();
        if (rezultat.success) {
            location.reload();
        }
    }
    catch (err) {
        console.error(err);
    }
}
async function verificaNotifiariNecitite() {
    try {
        const raspuns = await fetch('/Notifications/NrNotificariNecitite');
        const date = await raspuns.json();
        const punct = document.getElementById("punctRosu");
        const numarEfectiv = date.count !== undefined ? date.count : date.Count;

        if (punct && numarEfectiv > 0) {
            punct.innerText = numarEfectiv;
            punct.style.display = "block";
        } else if (punct) {
            punct.style.display = "none";
        }
    }
    catch (err) {
        console.error(err);
    }
}
async function raspundeCerere(id, actiune) {
    const url = actiune === 'Accept' ? '/Profil/AcceptFollow' : '/Profil/RefuzaFollow';

    try {
        const raspuns = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `notificareId=${id}`
        });

        const data = await raspuns.json();

        if (data.success) {
            location.reload();
        } else {
            alert(data.message || "A apărut o eroare la procesarea cererii.");
        }
    } catch (err) {
        console.error("Eroare la procesare cerere:", err);
    }
}
conexiune.start().catch(err => console.error(err.toString()));
document.addEventListener("DOMContentLoaded", verificaNotifiariNecitite);