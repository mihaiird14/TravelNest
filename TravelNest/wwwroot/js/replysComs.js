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
            generareReply(idComentariu, result);
            const container = document.getElementById(`container-raspunsuri-${idComentariu}`);
            if(container) {
                container.style.display = 'block';
            }
            const spanNumar = document.getElementById(`numar-raspunsuri-${idComentariu}`);
        const containerButon = document.getElementById(`btn-raspunsuri-container-${idComentariu}`);

        if (spanNumar) {
            let nrCurent = parseInt(spanNumar.innerText) || 0;
            spanNumar.innerText = nrCurent + 1;
        }
        if (containerButon && containerButon.style.display === 'none') {
            containerButon.style.display = 'flex';
        }
            
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
            <div class="rand-descriere" id="reply-container-${data.id}" style="position: relative; margin-bottom: 10px; padding: 10px; background: transparent; border: none; box-shadow: none;">
                <i class="fas fa-ellipsis-h optiuni-comentariu" style="font-size: 10px; cursor:pointer; position:absolute; right:10px; top:10px;" onclick="vizualizareMeniureplys(${data.id})"></i>
                <div id="IdMeniuReply-${data.id}" class="MeniuOptiuniComs" style="font-size: 11px;">
                    <div class="ElementeDinMeniu" onclick="EditareReply(${data.id})" style="color: #1e293b;">
                        <i class="fas fa-pencil-alt"></i> Edit
                    </div>
                    <div class="ElementeDinMeniu" onclick="stergeReply(${data.id})">
                        <i class="fas fa-trash-alt"></i> Delete
                    </div>
                </div>

                <div class="comentariu-top">
                    <img src="${data.userImage}" class="poza-rotunda mica" style="width: 25px; height: 25px;">
                    <div class="continut-text" style="display:flex; flex-direction:column; width:100%;">
                        <span class="nume-bold" style="font-size: 13px;">${data.username}</span>
                        <div id="zonaTextReply-${data.id}">
                            <span class="text-normal text-reply-wrap" id="textReply-${data.id}" style="font-size: 13px;">${data.mesaj}</span>
                        </div>
                        <div id="zonaEditareReply-${data.id}" style="display:none; margin-top: 5px;">
                            <textarea id="inputEditareReply-${data.id}" class="input-editare-custom" rows="2" style="width:100%; border:1px solid #ccc; border-radius:5px; padding:5px; font-size:12px;">${data.mesaj}</textarea>
                            <div style="display:flex; gap:5px; margin-top:5px;">
                                <button onclick="saveEditReply(${data.id})" style="background:#0095f6; color:white; border:none; padding:3px 10px; border-radius:4px; cursor:pointer; font-size:11px;">Save</button>
                                <button onclick="anuleazaEditareReply(${data.id})" style="background:transparent; border:1px solid #ccc; padding:3px 10px; border-radius:4px; cursor:pointer; font-size:11px;">Cancel</button>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="zona-actiuni-com" style="margin-left: 38px; margin-top: 5px; display: flex; align-items: center; gap: 15px;">
                    <span class="data-text" style="font-size: 10px;">${data.data}</span>
                    
                    <div class="btn-like-reply" onclick="apreciazaReply(${data.id})" style="cursor: pointer; display: flex; align-items: center; gap: 4px;">
                        <i class="fa-regular fa-heart" id="icon-like-reply-${data.id}" style="font-size: 11px;"></i> 
                        <span id="count-like-reply-${data.id}" style="font-size: 11px; font-weight: 600;">0</span>
                    </div>
                </div>
            </div>`;
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

//functie like comentarii
async function apreciazaComentariu(idComentariu) {
    const icon = document.getElementById(`icon-like-com-${idComentariu}`);
    const countSpan = document.getElementById(`count-like-com-${idComentariu}`);

    if (!icon || !countSpan){
        return;
    }
    const esteApreciatDeja = icon.classList.contains('fa-solid');
    let numarCurent = parseInt(countSpan.innerText) || 0;

    if (esteApreciatDeja) {
        icon.className = 'fa-regular fa-heart'; 
        numarCurent = Math.max(0, numarCurent - 1);
    } else {
        icon.className = 'fa-solid fa-heart text-danger'; 
        numarCurent++;
    }
    countSpan.innerText = numarCurent > 0 ? numarCurent : '0';

    try {
        const raspuns = await fetch('/Profil/AdaugaLikeComentariu', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `comId=${idComentariu}`
        });

        const date = await raspuns.json();

        if (date.success) {
            countSpan.innerText = date.nrLikeuri > 0 ? date.nrLikeuri : '0';
            if (date.liked) {
                icon.className = 'fa-solid fa-heart text-danger';
            } else {
                icon.className = 'fa-regular fa-heart';
            }
        } else {
            console.error("Error:", date.message);
        }
    } catch (eroare) {
        console.error("Error:", eroare);
    }
}
//functi like reply
async function apreciazaReply(replyId) {
    const icon = document.getElementById(`icon-like-reply-${replyId}`);
    const countSpan = document.getElementById(`count-like-reply-${replyId}`);
    if (!icon || !countSpan) 
        return;
    const esteApreciatDeja = icon.classList.contains('fa-solid');
    let numarCurent = parseInt(countSpan.innerText) || 0;

    if (esteApreciatDeja) {
        icon.className = 'fa-regular fa-heart';
        numarCurent = Math.max(0, numarCurent - 1);
    } else {
        icon.className = 'fa-solid fa-heart text-danger';
        numarCurent++;
    }
    countSpan.innerText = numarCurent > 0 ? numarCurent : '0';
    try {
        const raspuns = await fetch('/Profil/AdaugaLikeReplyComentariu', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `replyId=${replyId}`
        });
        const date = await raspuns.json();
        if (date.success) {
            countSpan.innerText = date.nrLikeuri > 0 ? date.nrLikeuri : '0';
            icon.className = date.liked ? 'fa-solid fa-heart text-danger' : 'fa-regular fa-heart';
        }
    } catch (eroare) {
        console.error("Eroare like reply:", eroare);
    }
}
//functii dropdown edit/delete replys
 function vizualizareMeniureplys(id) {
    const meniu = document.getElementById(`IdMeniuReply-${id}`);
    if (meniu)
        meniu.classList.toggle('activ');
}
function EditareReply(id) {
    const meniu = document.getElementById(`IdMeniuReply-${id}`);
    if(meniu) 
        meniu.classList.remove('activ');
    document.getElementById(`zonaTextReply-${id}`).style.display = 'none';
    document.getElementById(`zonaEditareReply-${id}`).style.display = 'block';
}
function anuleazaEditareReply(id) {
    document.getElementById(`zonaEditareReply-${id}`).style.display = 'none';
    document.getElementById(`zonaTextReply-${id}`).style.display = 'block';
}
async function saveEditReply(id) {
    const input = document.getElementById(`inputEditareReply-${id}`);
    const mesajNou = input.value;
    if (!mesajNou.trim()) 
        return;
    try {
        const res = await fetch('/Profil/EditReply', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `ReplyId=${id}&continut=${encodeURIComponent(mesajNou)}`
        });

        const data = await res.json();
        if (data.success) {
            document.getElementById(`textReply-${id}`).innerText = data.mesajNou;
            const label = document.getElementById(`labelEditatReply-${id}`);
            if (label && label.innerHTML === '') {
                label.innerHTML = '<span class="eticheta-editat" style="color:#94a3b8; font-size:11px; font-style:italic;">(edited)</span>';
            }
            anuleazaEditareReply(id);
        }
    } catch (e) {
        console.error("Eroare la editare reply:", e);
    }
}
let idReplyDeSters = null;
function stergeReply(id) {
    const meniu = document.getElementById(`IdMeniuReply-${id}`);
    if(meniu) 
        meniu.classList.remove('activ');
    idReplyDeSters = id;
    document.getElementById('popupStergeReply').style.display = 'flex';
}

function inchidePopUpReply() {
    document.getElementById('popupStergeReply').style.display = 'none';
    idReplyDeSters = null;
}
async function ConfirmaStergereReply() {
    if (!idReplyDeSters) 
        return;
    const idReply = idReplyDeSters;
    const elementReply = document.getElementById(`reply-container-${idReply}`);
    let idComentariuParinte = null;
    if (elementReply) {
        const containerParinte = elementReply.closest('.sectiuneReplys');
        if (containerParinte && containerParinte.id.startsWith('container-raspunsuri-')) {
            idComentariuParinte = containerParinte.id.replace('container-raspunsuri-', '');
        }
    }

    try {
        const res = await fetch('/Profil/StergeReply', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `ReplyId=${idReply}`
        });

        const data = await res.json();
        
        if (data.success) {
            if (elementReply) {
                elementReply.style.opacity = '0';
                setTimeout(() => elementReply.remove(), 300);
            }

            if (idComentariuParinte) {
                const spanNumar = document.getElementById(`numar-raspunsuri-${idComentariuParinte}`);
                const btnContainer = document.getElementById(`btn-raspunsuri-container-${idComentariuParinte}`);

                if (spanNumar) {
                    let nrCurent = parseInt(spanNumar.innerText) || 0;
                    let nrNou = Math.max(0, nrCurent - 1);
                    
                    spanNumar.innerText = nrNou;
                    if (nrNou === 0 && btnContainer) {
                        btnContainer.style.display = 'none';
                        const containerGol = document.getElementById(`container-raspunsuri-${idComentariuParinte}`);
                        if(containerGol) containerGol.style.display = 'none';
                    }
                }
            }

        } else {
            alert("Error deleting reply!");
        }
    } catch (e) {
        console.error("Eroare la ștergere reply:", e);
    } finally {
        inchidePopUpReply();
    }
}