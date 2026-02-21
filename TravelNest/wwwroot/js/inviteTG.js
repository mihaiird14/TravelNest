function meniuInvitatieTG(afiseaza) {
    const popUp = document.getElementById('popUpInvitatieTG');
    if (popUp) {
        popUp.style.display = afiseaza ? 'flex' : 'none';
    }
}

async function cautaPrieteni() {
    const x = document.getElementById('cautaPrietenInput').value;
    const lista = document.getElementById('listaRezultatePrieteni');
    const banner = document.getElementById('bannerVizualizare');
    const groupId = banner.getAttribute('data-idGrup');

    if (x.length < 2)
        return;

    try {
        const raspuns = await fetch(`/TravelGroup/SearchFriends?username=${x}&groupId=${groupId}`);
        const prieteni = await raspuns.json();
        lista.innerHTML = "";
        prieteni.forEach(p => {
            const html = `
                <div class="randPrieten">
                    <span class="numeUtilizator">${p.userName}</span>
                    <button class="butonInvita" onclick="trimiteInvitatie(${p.id})">Invită</button>
                </div>`;
            lista.insertAdjacentHTML('beforeend', html);
        });
    } catch (eroare) {
        console.error("Eroare la căutare:", eroare);
    }
}

async function trimiteInvitatie(idDestinatar) {
    const idGrup = document.getElementById('bannerVizualizare').getAttribute('data-idGrup');
    const formData = new FormData();
    formData.append("groupId", idGrup);
    formData.append("idDestinatar", idDestinatar);

    try {
        const raspuns = await fetch('/TravelGroup/TrimiteRequestParticipareTG', {
            method: 'POST',
            body: formData
        });

        const rezultat = await raspuns.json();
        if (rezultat.success) {
            meniuInvitatieTG(false);
            location.reload();
        }
    } catch (eroare) {
        console.error("Eroare invitatie:", eroare);
    }
}

document.getElementById('inviteBtn')?.addEventListener('click', function() {
    meniuInvitatieTG(true);
});