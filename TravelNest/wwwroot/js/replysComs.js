function ListaRaspunsuri(idCom) {
    const container = document.getElementById(`container-raspunsuri-${idCom}`);
    if (container) {
        if (container.style.display === 'none') {
            container.style.display = 'block'; 
        } else {
            container.style.display = 'none'; 
        }
    }
}
function adaugaReply(idCom) {
    const box = document.getElementById(`idReply-${idCom}`);
    
    if (box) {
        if (box.style.display === 'none') {
            box.style.display = 'flex'; 
            box.classList.add('activ'); 
            setTimeout(() => {
                const input = box.querySelector('input');
                if (input) input.focus();
            }, 100);
        } 
        else {
            box.style.display = 'none';
            box.classList.remove('activ');
            const input = box.querySelector('input');
            if (input) input.value = '';
        }
    } else {
        console.error(`Eroare: Nu am găsit div-ul pentru reply cu id="idReply-${idCom}"`);
    }
}
async function trimiteReply(event, formElement, idComentariu) {
    event.preventDefault(); 
    const input = formElement.querySelector('input[name="text"]');
    const mesajVal = input.value;

    if (!mesajVal.trim()){
        return;
    }
    const dateFormular = new URLSearchParams();
    dateFormular.append('comentariuId', idComentariu);
    dateFormular.append('mesaj', mesajVal);

    try {
        const response = await fetch('/Profil/AddComReply', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            body: dateFormular
        });

        const result = await response.json();

        if (result.success) {
            input.value = '';
            adaugaReply(idComentariu);
            generareReply(idComentariu, result);
            
        } else {
            alert(result.message || "Eroare la trimitere.");
        }
    } catch (err) {
        console.error("Eroare rețea:", err);
    }
}
function generareReply(idParinte, data) {
    const container = document.getElementById(`container-raspunsuri-${idParinte}`);
    
    if (container) {
        const htmlReply = `
            <div class="rand-descriere" style="margin-bottom: 10px; padding: 10px; background: transparent; border: none; box-shadow: none;">
                <div class="comentariu-top">
                    <img src="${data.userImage}" class="poza-rotunda mica" style="width: 25px; height: 25px;">
                    <div class="continut-text">
                        <span class="nume-bold" style="font-size: 13px;">${data.username}</span>
                        <span class="text-normal" style="font-size: 13px;">${data.mesaj}</span>
                    </div>
                </div>
                <div class="zona-actiuni-com" style="margin-left: 38px; margin-top: 0;">
                    <span class="data-text" style="font-size: 10px;">${data.data}</span>
                </div>
            </div>
        `;
        container.insertAdjacentHTML('beforeend', htmlReply);
    }
}
function AfisOptiuniComs(id) {
    document.querySelectorAll('.MeniuOptiuniComs').forEach(m => {
        if(m.id !== `IdMeniuCom-${id}`) 
           m.classList.remove('activ');
    });
    const meniu = document.getElementById(`IdMeniuCom-${id}`);
    if (meniu) {
        meniu.classList.toggle('activ');
    }
}
let idComentariuSelectat = null;
function stergeComentariu(id) {
    idComentariuSelectat = id;
   
    const meniuOptiuni = document.getElementById(`IdMeniuCom-${id}`);
    if(meniuOptiuni) meniuOptiuni.classList.remove('activ');
    document.getElementById('popupStergeCom').style.display = 'flex';
}

function inchidePopUp() {
    document.getElementById('popupStergeCom').style.display = 'none';
    idComentariuSelectat = null;
}

async function StergeCom() {
    if (!idComentariuSelectat) return;
    const id = idComentariuSelectat;

    try {
        const raspuns = await fetch('/Profil/StergeComentariu', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `id=${id}`
        });

        const rezultat = await raspuns.json();

        if (rezultat.success) {
            const elementComentariu = document.getElementById(`comm-${id}`);
            if (elementComentariu) {
                elementComentariu.style.opacity = "0";
                setTimeout(() => {
                    elementComentariu.remove();
                }, 300);
            }
        } else {
            alert(rezultat.message || "Error!");
        }
    } catch (eroare) {
        console.error(eroare);
    } finally {
        inchidePopUp();
    }
}
document.addEventListener('click', function(e) {
    if (!e.target.classList.contains('optiuni-comentariu')) {
        document.querySelectorAll('.MeniuOptiuniComs').forEach(m => m.classList.remove('activ'));
    }
});
function EditCommentOn(id) {
    const meniu = document.getElementById(`IdMeniuCom-${id}`);
    if(meniu) 
        meniu.classList.remove('activ');
    const textVechi = document.getElementById(`textCom-${id}`).innerText;
    document.getElementById(`inputEditare-${id}`).value = textVechi;
    document.getElementById(`zonaTextCom-${id}`).style.display = 'none';
    document.getElementById(`zonaEditareCom-${id}`).style.display = 'block';
}
function anuleazaEditareComentariu(id) {
    document.getElementById(`zonaEditareCom-${id}`).style.display = 'none';
    document.getElementById(`zonaTextCom-${id}`).style.display = 'block';
}
async function saveEdit(id) {
    const input = document.getElementById(`inputEditare-${id}`);
    const mesajUpdated = input.value;

    if (!mesajUpdated.trim()) {
        alert("Comentariul nu poate fi gol!");
        return;
    }

    try {
        const raspuns = await fetch('/Profil/EditComment', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `id=${id}&continutUpdated=${encodeURIComponent(mesajUpdated)}`
        });
        if (!raspuns.ok) {
            console.error("Server Error:", raspuns);
            return;
        }
        const rezultat = await raspuns.json();
        if (rezultat.success) {
            document.getElementById(`textCom-${id}`).innerText = rezultat.newContent;
            const label = document.getElementById(`labelEditat-${id}`);
            if (label && label.innerText === '') {
                label.innerHTML = '<span class="eticheta-editat" style="color:#94a3b8; font-size:11px; font-style:italic; margin-left:5px;">(edited)</span>';
            }
            anuleazaEditareComentariu(id);
        } else {
            alert(rezultat.message || "Eroare la salvare.");
        }
    } catch (err) {
        console.error(err);
        alert("Eroare tehnică (JS). Verifică consola (F12).");
    }
}