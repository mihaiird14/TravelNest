let idPostareCurenta = 0;
async function deschidePostare(idPostare) {
    idPostareCurenta = idPostare; 
    const fereastra = document.getElementById('fereastra-postare');
    const containerMedia = document.getElementById('continut-media');
    document.getElementById('lista-comentarii').innerHTML = ''; 
    fereastra.style.display = 'flex';
    containerMedia.innerHTML = '<p style="color:white;">Se încarcă...</p>';
    try {
        const raspuns = await fetch(`/Profil/InfoPostari?postId=${idPostare}`);
        if (!raspuns.ok) throw new Error('Eroare rețea');
            
        const date = await raspuns.json();
        populeazaFereastra(date);

    } catch (eroare) {
        console.error(eroare);
    }
}
async function postCom() {
    const input = document.getElementById('input-comentariu-text');
    const text = input.value;
    if (!text.trim()) return; //verific e com gol???
    try {
        const response = await fetch('/Profil/AdaugaComentariu', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ 
                PostareId: idPostareCurenta, 
                Continut: text 
            })
        });

        const result = await response.json();

        if (result.success) {
            afisCom(result);
              
            input.value = '';
        } else {
            alert('Eroare la postare');
        }
    } catch (err) {
        console.error(err);
    }
}
function afisCom(data) {
    const lista = document.getElementById('lista-comentarii');
    data.nrRaspunsuri = 0;
    lista.insertAdjacentHTML('beforeend', genComs(data));
    bodyDetalii.scrollTop = bodyDetalii.scrollHeight;
    const div = document.createElement('div');
    div.className = 'rand-descriere';
        
    div.innerHTML = `
        <img src="${data.poza}" class="poza-rotunda mica">
        <div class="continut-text">
            <span class="nume-bold">${data.username}</span>
            <span class="text-normal">${data.continut}</span>
            <span class="data-text" style="text-align:left; margin-top:5px;">${data.data}</span>
        </div>
    `;
   
    lista.appendChild(div);
    const bodyDetalii = document.querySelector('.detalii-body');
    bodyDetalii.scrollTop = bodyDetalii.scrollHeight;
}