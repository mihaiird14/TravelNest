const conexiune = new signalR.HubConnectionBuilder()
    .withUrl("/NotificariHub")
    .build();
conexiune.on("PrimesteNotificare", function (titlu, mesaj, tip, expeditor, id) {
    const punct = document.getElementById("punctRosu");
    if (punct) {
        let count = parseInt(punct.innerText) || 0;
        punct.innerText = count + 1;
        punct.style.display = "block";
    }

    const listaPagina = document.getElementById("listaNotif");
    if (listaPagina) {
        const mesajGol = document.getElementById("mesajGol");
        if (mesajGol) mesajGol.style.display = "none";

        const cardNou = `
            <div class="cardNotif itemNotif notifNoua">
                <div class="infoNotif">
                    <i class="fa-solid ${tip === 'TGInvite' ? 'fa-envelope-open-text' : 'fa-bell'}"></i>
                    <div class="textNotif">
                        <p class="mesajNotif"><strong>${expeditor}</strong>: ${mesaj}</p>
                        <span class="dataNotif">Now</span>
                    </div>
                </div>
                ${tip === 'TGInvite' ? `
                    <div class="actiuniNotif">
                        <button class="btnAccept" onclick="raspundeInvitatie(event, ${id}, true)">Accept</button>
                        <button class="btnRefuz" onclick="raspundeInvitatie(event, ${id}, false)">Reject</button>
                    </div>` : ''}
            </div>`;
        
        listaPagina.insertAdjacentHTML('afterbegin', cardNou);
    }
});
async function raspundeInvitatie(eveniment, notificareId, accepta) {
    if (!eveniment) {
        return;
    }

    const butonApasat = eveniment.currentTarget || eveniment.target;
    console.log("Buton detectat:", butonApasat);

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
    } catch (eroare) {
        console.error(eroare);
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
    } catch (eroare) {
        console.error("Eroare fetch:", eroare);
    }
}

conexiune.start().catch(err => console.error(err.toString()));
document.addEventListener("DOMContentLoaded", verificaNotifiariNecitite);