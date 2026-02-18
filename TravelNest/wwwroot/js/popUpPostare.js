let listaMediaCurenta = [];
let indexSlideCurent = 0;
let postare=0
window.deschidePostare = async function(idPostare) {
    postare=idPostare
    const fereastra = document.getElementById('fereastra-postare');
    const containerMedia = document.getElementById('continut-media');
    fereastra.style.display = 'flex';
    containerMedia.innerHTML = '<p style="color:white;">Se încarcă...</p>';
    const btnLike=document.getElementById("btn-like-modal")
    if (btnLike) {
        btnLike.onclick =function(){
            apreciazaPostare(idPostare);
        }
    }
    try {
        const raspuns = await fetch(`/Profil/InfoPostari?postId=${idPostare}`);

        if (!raspuns.ok) throw new Error('Nu am putut încărca postarea.');

        const date = await raspuns.json();
        populeazaFereastra(date);

    } catch (eroare) {
        console.error(eroare);
        containerMedia.innerHTML = '<p style="color:red;">Eroare!</p>';
    }
}
async function postCom() {
    const input = document.getElementById('input-comentariu-text');
    const text = input.value;
    if (!text.trim()) 
        return; //verific e com gol???
    try {
        const response = await fetch('/Profil/AdaugaComentariu', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ 
                PostareId: postare, 
                Continut: text 
            })
        });

        const rz = await response.json();

        if (rz.success) {
            afisCom(rz);
            updateNrComentarii(1);
            input.value = '';
        } else {
            alert('Eroare la postare');
        }
    } catch (err) {
        console.error(err);
    }
}
function afisCom(data) {
    const bodyDetalii = document.querySelector('.detalii-body'); 
    const lista = document.getElementById('lista-comentarii');
    if (typeof genComs === "function") {
        data.nrRaspunsuri = 0;
        lista.insertAdjacentHTML('beforeend', genComs(data));
    } else {
        const div = document.createElement('div');
        div.className = 'rand-descriere';
        div.innerHTML = `
            <div class="comentariu-top">
                <img src="${data.poza}" class="poza-rotunda mica">
                <div class="continut-text">
                    <span class="nume-bold">${data.username}</span>
                    <span class="text-normal">${data.continut}</span>
                </div>
            </div>
            <div class="zona-actiuni-com">
                 <span class="data-text">${data.data}</span>
            </div>
        `;
        lista.appendChild(div);
    }
    if (bodyDetalii) {
        bodyDetalii.scrollTop = bodyDetalii.scrollHeight;
    }
}
function genComs(com) {
    const comId = com.id;
    //apare edited daca este editat
    const etichetaEditat = com.esteEditat 
        ? `<span class="eticheta-editat" style="color:#94a3b8; font-size:11px; font-style:italic;">(edited)</span>` 
        : '';
    const butonEditare = (com.AutorComentariu === true || com.AutorComentariu === undefined) 
        ? `<div class="ElementeDinMeniu btn-editare-custom" onclick="EditCommentOn(${comId})" style="color: #1e293b;">
               <i class="fas fa-pencil-alt"></i> Edit
           </div>` 
        : '';
    //apreciere buton like uri
    const clasaInima = com.esteApreciat ? 'fas fa-heart text-danger' : 'fa-regular fa-heart';
    const comentariiNrLikes = (com.nrLikeuriComentariu > 0) ? com.nrLikeuriComentariu : '0';
    let htmlRaspunsuri = "";
    if (com.raspunsuri && com.raspunsuri.length > 0) {
        com.raspunsuri.forEach(rep => {
            const etichetaEditatRep = rep.esteEditat 
                    ? `<span class="eticheta-editat" style="color:#94a3b8; font-size:11px; font-style:italic;">(edited)</span>` 
                    : '';
        
            // Calculăm starea inimii pentru reply
            const clasaInimaRep = rep.euAmDatLikeReply ? 'fa-solid fa-heart text-danger' : 'fa-regular fa-heart';
            const textNrLikeuriRep = (rep.nrLikeuriReply > 0) ? rep.nrLikeuriReply : '0';
        
            const meniuReply = rep.autorReply ? `
                    <i class="fas fa-ellipsis-h optiuni-comentariu" style="font-size: 10px; cursor:pointer; position:absolute; right:10px; top:10px;" onclick="vizualizareMeniureplys(${rep.id})"></i>
                    <div id="IdMeniuReply-${rep.id}" class="MeniuOptiuniComs" style="font-size: 11px;">
                        <div class="ElementeDinMeniu" onclick="EditareReply(${rep.id})" style="color: #1e293b;">
                            <i class="fas fa-pencil-alt"></i> Edit
                        </div>
                        <div class="ElementeDinMeniu" onclick="stergeReply(${rep.id})">
                            <i class="fas fa-trash-alt"></i> Delete
                        </div>
                    </div>` : '';

            htmlRaspunsuri += `
                <div class="rand-descriere" id="reply-container-${rep.id}" style="position: relative; margin-bottom: 10px; padding: 10px; background: transparent; border: none; box-shadow: none;">
                    ${meniuReply}
                    <div class="comentariu-top">
                        <img src="${rep.userImage}" class="poza-rotunda mica" style="width: 25px; height: 25px;">
                        <div class="continut-text" style="display:flex; flex-direction:column; width:100%;">
                            <div style="display: flex; align-items: baseline; gap: 5px;">
                                <span class="nume-bold" style="font-size: 13px;">${rep.username}</span>
                                <span id="labelEditatReply-${rep.id}">${etichetaEditatRep}</span>
                            </div>
                        
                            <div id="zonaTextReply-${rep.id}">
                                <span class="text-normal text-reply-wrap" id="textReply-${rep.id}" style="font-size: 13px;">${rep.mesaj}</span>
                            </div>

                            <div id="zonaEditareReply-${rep.id}" style="display:none; margin-top: 5px;">
                                <textarea id="inputEditareReply-${rep.id}" class="input-editare-custom" rows="2" style="width:100%; border:1px solid #ccc; border-radius:5px; padding:5px; font-size:12px;">${rep.mesaj}</textarea>
                                <div style="display:flex; gap:5px; margin-top:5px;">
                                    <button onclick="saveEditReply(${rep.id})" style="background:#0095f6; color:white; border:none; padding:3px 10px; border-radius:4px; cursor:pointer; font-size:11px;">Save</button>
                                    <button onclick="anuleazaEditareReply(${rep.id})" style="background:transparent; border:1px solid #ccc; padding:3px 10px; border-radius:4px; cursor:pointer; font-size:11px;">Cancel</button>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="zona-actiuni-com" style="margin-left: 38px; margin-top: 5px; display: flex; align-items: center; gap: 15px;">
                        <span class="data-text" style="font-size: 10px;">${rep.data}</span>
                    
                        <div class="btn-like-reply" onclick="apreciazaReply(${rep.id})" style="cursor: pointer; display: flex; align-items: center; gap: 4px;">
                            <i class="${clasaInimaRep}" id="icon-like-reply-${rep.id}" style="font-size: 11px;"></i> 
                            <span id="count-like-reply-${rep.id}" style="font-size: 11px; font-weight: 600;">${textNrLikeuriRep}</span>
                        </div>
                    </div>
                </div>`;
        });
    }
    //afis nr + raspunsuri
    const butonVeziRaspunsuri = `
        <div id="btn-raspunsuri-container-${comId}" class="btn-vezi-raspunsuri" onclick="ListaRaspunsuri(${comId})" 
             style="margin-left: 50px; margin-top: 5px; cursor: pointer; font-size: 12px; color: #8e8e8e; font-weight: 600; 
                    display: ${com.nrRaspunsuri > 0 ? 'flex' : 'none'}; align-items: center; gap: 5px;">
             <span style="height: 1px; width: 20px; background-color: #8e8e8e; display: inline-block;"></span> 
             See <span id="numar-raspunsuri-${comId}">${com.nrRaspunsuri}</span> replys
        </div>`;

    return `
    <div class="rand-descriere" id="comm-${comId}" style="position: relative;"> 
        
        <i class="fas fa-ellipsis-h optiuni-comentariu" title="Options" onclick="AfisOptiuniComs(${comId})"></i>
        <div id="IdMeniuCom-${comId}" class="MeniuOptiuniComs">
            ${butonEditare}
            <div class="ElementeDinMeniu" onclick="stergeComentariu(${comId})">
                <i class="fas fa-trash-alt"></i> Delete
            </div>
        </div>

        <div class="comentariu-top">
            <img src="${com.poza}" class="poza-rotunda mica">
            
            <div class="continut-text" style="width: 100%; display: flex; flex-direction: column;">
                <div style="display: flex; align-items: baseline; gap: 8px;">
                    <span class="nume-bold">${com.username}</span>
                    <span id="labelEditat-${comId}">${etichetaEditat}</span>
                </div>
                
                <div id="zonaTextCom-${comId}" style="display: block;">
                    <span class="text-normal text-reply-wrap" id="textCom-${comId}">${com.continut}</span>
                </div>

                <div id="zonaEditareCom-${comId}" style="display:none; margin-top: 5px;">
                    <textarea id="inputEditare-${comId}" class="input-editare-custom" rows="2" style="width:100%; border:1px solid #ccc; border-radius:5px; padding:5px;">${com.continut}</textarea>
                    <div style="display:flex; gap:5px; margin-top:5px;">
                        <button onclick="saveEdit(${comId})" class="btn-salvare-mic" style="background:#0095f6; color:white; border:none; padding:5px 10px; border-radius:4px; cursor:pointer;">Save</button>
                        <button onclick="anuleazaEditareComentariu(${comId})" class="btn-anulare-mic" style="background:transparent; border:1px solid #ccc; padding:5px 10px; border-radius:4px; cursor:pointer;">Cancel</button>
                    </div>
                </div>
            </div>
        </div>

        <div class="zona-actiuni-com">
            <span class="data-text">${com.data}</span>
           <div class="btn-like-com" onclick="apreciazaComentariu(${comId})" style="cursor: pointer; display: flex; align-items: center; gap: 4px;">
                <i class="${clasaInima}" id="icon-like-com-${comId}"></i> 
                <span id="count-like-com-${comId}" style="font-size: 12px; font-weight: 600;">${comentariiNrLikes}</span>
           </div>
            <div class="btn-raspunsuri" onclick="adaugaReply(${comId})" style="margin-left: 15px; cursor: pointer;">
                <span>Reply</span>
            </div>
        </div>

        <div id="idReply-${comId}" class="RaspunsuriComentarii" style="display:none; margin-left: 50px; margin-top: 10px;">
            <form onsubmit="trimiteReply(event, this, ${comId})" style="display:flex; width:100%; gap:5px;">
                <input type="text" name="text" class="RaspunsInput" placeholder="Reply to ${com.username}" autocomplete="off" maxlength="100" style="border: none; border-bottom: 1px solid #ccc; background: transparent; width: 100%; outline: none; font-size: 13px;">
                <button type="submit" id="BtnRaspunsuri" style="background: none; border: none; color: #0095f6; font-weight: 600; cursor: pointer;">Post</button>
            </form>
        </div>

        ${butonVeziRaspunsuri}

        <div id="container-raspunsuri-${comId}" class="sectiuneReplys" style="display:none; margin-left: 50px;">
            ${htmlRaspunsuri}
        </div>
    </div>`;
}
function populeazaFereastra(date) {
    document.getElementById('header-poza-profil').src = date.userImage;
    document.getElementById('header-username').textContent = date.username;
    document.getElementById('header-locatie').textContent = date.locatie || '';
    document.getElementById('body-poza-profil').src = date.userImage;
    document.getElementById('body-username').textContent = date.username;
    document.getElementById('descriere-text').textContent = date.descriere;
    document.getElementById('data-postarii').textContent = date.data;
    window.datePostareCurenta = date;
    listaMediaCurenta = date.media;
    indexSlideCurent = 0;
    //vizualizare iconita ptr meniu optiuni
    const boxOptiuni = document.getElementById('boxOptiuniPostari');
    const meniuDropdown = document.getElementById('MeniuOptiuniPostare');
    if (boxOptiuni) {
        boxOptiuni.style.display = 'none'; 
    }
    if (meniuDropdown) {
        meniuDropdown.style.display = 'none'; 
        meniuDropdown.classList.remove('activ');
    }
    if (date.esteAutorPostare === true && boxOptiuni) {
        boxOptiuni.style.display = 'block'; 
    }
    const countSpan = document.getElementById('text-like-count-modal');
    const iconLike = document.getElementById('iconInima');
    //nr like uri si nr comentarii + raspunsuri
    if (countSpan) 
        countSpan.innerText = date.nrLikeuri;
    const commSpan = document.getElementById('text-comm-count-modal');
    if (commSpan) 
        commSpan.innerText = date.totalComentarii;
    if (iconLike) {
        if (date.esteApreciata) {
            iconLike.className = 'fa-solid fa-heart text-danger'; 
        } else {
            iconLike.className = 'fa-regular fa-heart'; 
        }
    }
    //afisare lista de taguri la postare
    const iconita = document.getElementById('userTagsIcon');
    const containerLista = document.getElementById('listaTags');
    if (containerLista) {
        containerLista.style.display = 'none';
        containerLista.innerHTML = ''; 
    }
    if (iconita) {
        iconita.style.display = 'none';
        iconita.style.backgroundColor = 'rgba(0, 0, 0, 0.7)';
        iconita.style.color = 'white';
    }
    if (date.taguri && date.taguri.length > 0) {
        if (iconita)
            iconita.style.display = 'flex';
        if (containerLista) {
            date.taguri.forEach(tag => {
                const rand = document.createElement('a');
                rand.className = 'tag-row';
                rand.href = `/Profil/Index?user=${tag.username}`; 
                const pozaUrl = tag.userImage || '/images/profilDefault.png';
                rand.innerHTML = `
                    <img src="${pozaUrl}" alt="user">
                    <span>@${tag.username}</span>
                `;
                containerLista.appendChild(rand);
            });
        }
    }
    afiseazaSlide(0);
    const listaCom = document.getElementById('lista-comentarii');
    listaCom.innerHTML = '';
    if (date.comentarii && date.comentarii.length > 0) {
    date.comentarii.forEach(com => {
        const div = document.createElement('div');
        div.className = 'rand-descriere';
        div.innerHTML = `
            <img src="${com.poza}" class="poza-rotunda mica">
            <div class="continut-text">
                <span class="nume-bold">${com.username}</span>
                <span class="text-normal">${com.continut}</span>
                <span class="data-text" style="text-align:left; margin-top:5px;">${com.data}</span>
            </div>
        `;
        listaCom.appendChild(div);
    });
    }
    const iconCom = document.querySelector('.detalii-footer .fa-comment');
    if(iconCom) {
        iconCom.setAttribute('title', `${date.totalComentarii} comentarii`);
    }

    listaCom.innerHTML = '';

    if (date.comentarii && date.comentarii.length > 0) {
        date.comentarii.forEach(com => {
            listaCom.innerHTML += genComs(com);
        });
    }
}
function afiseazaSlide(index) {
    const container = document.getElementById('continut-media');
    const btnInapoi = document.getElementById('btn-inapoi');
    const btnInainte = document.getElementById('btn-inainte');
    if (!listaMediaCurenta || listaMediaCurenta.length === 0) {
        container.innerHTML = '<p style="color:white">Fără media</p>';
        btnInapoi.style.display = 'none';
        btnInainte.style.display = 'none';
        return;
    }
    if (listaMediaCurenta.length === 1) {
        btnInapoi.style.display = 'none';
        btnInainte.style.display = 'none';
    } else {
        btnInapoi.style.display = 'flex';
        btnInainte.style.display = 'flex';
    }
    if (index < 0) index = 0;
    if (index >= listaMediaCurenta.length) index = listaMediaCurenta.length - 1;

    indexSlideCurent = index;
    container.innerHTML = '';
    const element = listaMediaCurenta[index];
    let tagMedia;

    if (element.tip === 'Video') {
        tagMedia = document.createElement('video');
        tagMedia.src = element.url;
        tagMedia.controls = true;
        tagMedia.autoplay = true;
        tagMedia.muted = true;
    } else {
        tagMedia = document.createElement('img');
        tagMedia.src = element.url;
    }

    tagMedia.className = 'element-media-vizual';
    container.appendChild(tagMedia);
    btnInapoi.classList.remove('disabled');
    btnInainte.classList.remove('disabled');
    if (index === 0) {
        btnInapoi.classList.add('disabled');
    }
    if (index === listaMediaCurenta.length - 1) {
        btnInainte.classList.add('disabled');
    }
}
function schimbaSlide(directie) {
    afiseazaSlide(indexSlideCurent + directie);
}
function inchideFereastra() {
    document.getElementById('fereastra-postare').style.display = 'none';
    document.getElementById('continut-media').innerHTML = '';
}
document.getElementById('fereastra-postare').addEventListener('click', function(e) {
    if (e.target === this) {
        inchideFereastra();
    }
});
function apreciazaPostare(idPostare) {
    const icon = document.getElementById('iconInima');
    const countSpan = document.getElementById('text-like-count-modal');
    const esteRosieAcum = icon.classList.contains('text-danger');
    let numarCurent = parseInt(countSpan.innerText) || 0;
    if (esteRosieAcum) {
        icon.className = 'fa-regular fa-heart';
        countSpan.innerText = Math.max(0, numarCurent - 1);
    } else {
        icon.className = 'fa-solid fa-heart text-danger';
        countSpan.innerText = numarCurent + 1;
    }
    fetch('/Profil/LikePostare', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `postId=${idPostare}`
    })
    .then(r => r.json())
    .then(data => {
        if (data.success && data.nrLikeuri !== undefined) {
                countSpan.innerText = data.nrLikeuri;
        }
    })
    .catch(err => {
        console.error("Eroare rețea:", err);
        if (esteRosieAcum) {
            icon.className = 'fa-solid fa-heart text-danger';
            countSpan.innerText = numarCurent;
        } else {
            icon.className = 'fa-regular fa-heart';
            countSpan.innerText = numarCurent;
        }
    });
}
//functie ptr deschidere + inchidere meniu optiuni postari
function afisMeniuOptiuniPostari() {
    const meniu = document.getElementById('MeniuOptiuniPostare');
    if (meniu) {
        meniu.classList.toggle('activ');
    }
}
document.addEventListener('click', function (e) {
    const box = document.getElementById('boxOptiuniPostari');
    const meniu = document.getElementById('MeniuOptiuniPostare');
    if (meniu && meniu.classList.contains('activ')) {
        if (box && !box.contains(e.target)) {
            meniu.classList.remove('activ');
        }
    }
});
//popup ptr stergere postare
function StergerePostare() {
    const meniu = document.getElementById('MeniuOptiuniPostare');
    if(meniu) 
        meniu.classList.remove('activ');
    document.getElementById('popupStergePostare').style.display = 'flex';
}
function inchidePopUpPostare() {
    document.getElementById('popupStergePostare').style.display = 'none';
}
async function ConfirmaStergerePostare() {
    if (!postare){
        return; 
    }
    try {
        const raspuns = await fetch('/Profil/StergePostare', {
            method: 'POST',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
            body: `postareId=${postare}`
        });

        const r = await raspuns.json();

        if (r.success) {
            inchidePopUpPostare(); 
            inchideFereastra();
            window.location.reload(); 
        }
    } catch (err) {
        console.error("Error!", err);;
    }
}
//functie arhivare postare
async function ArhiveazaPostare() {
    if (!postare) 
        return;

    try {
        const formData = new FormData();
        formData.append('postareId', postare);

        const raspuns = await fetch('/Profil/ArhivarePostare', {
            method: 'POST',
            body: formData
        });

        const r = await raspuns.json();

        if (r.success) {
            inchideFereastra();       
            window.location.reload();
        }
    } catch (err) {
        console.error("Error: ", err);
    }
}
async function DezarhivarePostare() {
    if (!postare) 
        return;

    try {
        const formData = new FormData();
        formData.append('postareId', postare);

        const raspuns = await fetch('/Profil/DezarhivarePostare', {
            method: 'POST',
            body: formData
        });

        const r = await raspuns.json();

        if (r.success) {
            inchideFereastra();       
            window.location.reload();
        }
    } catch (err) {
        console.error("Error: ", err);
    }
}
//functii editare postare
let indexSlideEdit = 0;
let editTagsList = [];
function EditarePostare() {
    inchideFereastra();
    const meniu = document.getElementById('MeniuOptiuniPostare');
    if (meniu) 
        meniu.classList.remove('activ');
    const locElem = document.getElementById('header-locatie');
    const descElem = document.getElementById('descriere-text');
    
    document.getElementById('edit-inputLocatie').value = locElem ? locElem.innerText : '';
    document.getElementById('edit-inputDescriere').value = descElem ? descElem.innerText : '';
    document.getElementById('idEditPost').value = postare; 
    indexSlideEdit = 0;
    editTagsList = [];
    document.getElementById('edit-inputTag').value = '';
    document.getElementById('edit-rezultateTag').style.display = 'none';
    if (window.datePostareCurenta && window.datePostareCurenta.taguri) {
            editTagsList = window.datePostareCurenta.taguri.map(t => ({
            username: t.username || t.userName, 
            userImage: t.userImage || t.poza
        }));
    }
    vizualizareTags();
    afiseazaSlideEdit(0);
    if (listaMediaCurenta && listaMediaCurenta.length > 0) {
        scanareAutomataEdit(listaMediaCurenta);
    }
    const modalEdit = document.getElementById('formularEditarePostare');
    if (modalEdit) {
        modalEdit.style.display = 'flex';
    }
}

function afiseazaSlideEdit(index) {
    const wrapper = document.getElementById('editCarouselWrapper');
    const counter = document.getElementById('editCounter');
    const btnPrev = document.querySelector('#formularEditarePostare .prev');
    const btnNext = document.querySelector('#formularEditarePostare .next');

    if (!listaMediaCurenta || listaMediaCurenta.length === 0)
        return;
    if (index < 0) 
        index = 0;
    if (index >= listaMediaCurenta.length) 
        index = listaMediaCurenta.length - 1;
    indexSlideEdit = index;
    if (counter) 
        counter.innerText = (indexSlideEdit + 1) + " / " + listaMediaCurenta.length;
    const showBtns = listaMediaCurenta.length > 1 ? 'flex' : 'none';
    if (btnPrev) {
        btnPrev.style.display = showBtns;
        btnPrev.style.opacity = indexSlideEdit === 0 ? '0.3' : '1';
        btnPrev.style.pointerEvents = indexSlideEdit === 0 ? 'none' : 'auto';
    }
    if (btnNext) {
        btnNext.style.display = showBtns;
        btnNext.style.opacity = indexSlideEdit === listaMediaCurenta.length - 1 ? '0.3' : '1';
        btnNext.style.pointerEvents = indexSlideEdit === listaMediaCurenta.length - 1 ? 'none' : 'auto';
    }
    if (wrapper && wrapper.parentElement) {
        wrapper.parentElement.style.backgroundColor = '#eef7ff';
        wrapper.parentElement.style.display = 'flex';
        wrapper.parentElement.style.alignItems = 'center';
        wrapper.parentElement.style.justifyContent = 'center';
    }

    wrapper.innerHTML = '';
    const media = listaMediaCurenta[indexSlideEdit];
    let element;
    if (media.tip === 'Video' || (media.url && media.url.endsWith('.mp4'))) {
        element = document.createElement('video');
        element.src = media.url;
        element.controls = true;
        element.muted = true; 
    } else {
        element = document.createElement('img');
        element.src = media.url;
    }
    element.style.width = '200%'; 
    element.style.maxHeight = '150%';
    element.style.width = 'auto';
    element.style.height = 'auto';
    element.style.objectFit = 'contain'; 
    element.style.borderRadius = '8px'; 
    element.style.boxShadow = '0 4px 15px rgba(0,0,0,0.15)'; 
    element.style.display = 'block';
    element.style.marginRight="20px";
    wrapper.appendChild(element);
}

function schimbaSlideEdit(directie) {
    afiseazaSlideEdit(indexSlideEdit + directie);
}

function inchideEditare() {
    document.getElementById('formularEditarePostare').style.display = 'none';
}

document.addEventListener("DOMContentLoaded", function () {
    const editInput = document.getElementById('edit-inputLocatie');
    if (!editInput)
        return;
    const parentGroup = editInput.parentElement;
    parentGroup.style.position = 'relative'; 

    let editLista = document.getElementById('edit-listaLocatii');
    if (!editLista) {
        editLista = document.createElement('div');
        editLista.id = 'edit-listaLocatii';
        Object.assign(editLista.style, {
            display: 'none',
            position: 'absolute',
            top: '100%',
            left: '0',
            width: '100%',
            background: 'white',
            border: '1px solid #e2e8f0',
            borderTop: 'none',
            zIndex: '10000', 
            maxHeight: '200px',
            overflowY: 'auto',
            borderRadius: '0 0 10px 10px',
            boxShadow: '0 10px 15px -3px rgba(0, 0, 0, 0.1)'
        });
        parentGroup.appendChild(editLista);
    }

    let editHartaDiv = document.getElementById('edit-miniHarta');
    if (!editHartaDiv) {
        editHartaDiv = document.createElement('div');
        editHartaDiv.id = 'edit-miniHarta';
        Object.assign(editHartaDiv.style, {
            height: '180px',
            width: '100%',
            display: 'none',
            marginTop: '10px',
            borderRadius: '8px',
            zIndex: '1',
            border: '1px solid #cbd5e1'
        });
        parentGroup.appendChild(editHartaDiv);
    }

    let timeoutSearchEdit;
    let hartaEdit = null;
    let markerEdit = null;
    editInput.addEventListener('keyup', function () {
        const valoare = this.value;
        clearTimeout(timeoutSearchEdit);

        if (valoare.length < 3) {
            editLista.style.display = 'none';
            return;
        }

        timeoutSearchEdit = setTimeout(() => {
            cautaLocatieEdit(valoare);
        }, 500);
    });

    document.addEventListener('click', (e) => {
        if (e.target !== editInput && e.target !== editLista) {
            editLista.style.display = 'none';
        }
    });

    function cautaLocatieEdit(text) {
        const apiUrl = `https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(text)}&addressdetails=1&limit=5`;

        fetch(apiUrl)
            .then(res => res.json())
            .then(rezultate => {
                editLista.innerHTML = '';
                if (!rezultate || rezultate.length === 0) {
                    editLista.style.display = 'none';
                    return;
                }

                editLista.style.display = 'block';

                rezultate.forEach(loc => {
                    let label = loc.display_name;
                    if (loc.address) {
                        const oras = loc.address.city || loc.address.town || loc.address.village || loc.address.county;
                        const tara = loc.address.country;
                        if (oras && tara) label = `${oras}, ${tara}`;
                    }

                    const item = document.createElement('div');
                    item.className = 'item-locatie';
                    Object.assign(item.style, {
                        padding: '12px 15px',
                        cursor: 'pointer',
                        borderBottom: '1px solid #f1f5f9',
                        display: 'flex',
                        alignItems: 'center',
                        gap: '10px',
                        fontSize: '14px',
                        color: '#334155',
                        backgroundColor: 'white',
                        transition: 'background-color 0.2s'
                    });

                    item.innerHTML = `<i class="fa-solid fa-location-dot" style="color:#3b82f6;"></i> <span>${label}</span>`;

                    item.onmouseover = () => { item.style.backgroundColor = '#eff6ff'; item.style.color = '#1e40af'; };
                    item.onmouseout = () => { item.style.backgroundColor = 'white'; item.style.color = '#334155'; };

                    item.onclick = function () {
                        editInput.value = label;
                        editLista.style.display = 'none';
                        afiseazaHartaEdit(loc.lat, loc.lon);
                    };

                    editLista.appendChild(item);
                });
            });
    }

    function afiseazaHartaEdit(lat, lon) {
        editHartaDiv.style.display = 'block';

        if (!hartaEdit) {
            hartaEdit = L.map('edit-miniHarta').setView([lat, lon], 12);
            L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                attribution: '&copy; OpenStreetMap'
            }).addTo(hartaEdit);
        } else {
            hartaEdit.setView([lat, lon], 12);
        }
        setTimeout(() => { hartaEdit.invalidateSize(); }, 200);

        if (markerEdit) {
            markerEdit.setLatLng([lat, lon]);
        } else {
            markerEdit = L.marker([lat, lon]).addTo(hartaEdit);
        }
    }
});
//editare tags editare
document.addEventListener("DOMContentLoaded", function () {
    const editInputTag = document.getElementById('edit-inputTag');
    const editResults = document.getElementById('edit-rezultateTag');
    let editSearchTimeout;
    if (editInputTag) {
        editInputTag.addEventListener('keyup', function () {
            const term = this.value.trim();
            clearTimeout(editSearchTimeout);

            if (term.length < 1) {
                editResults.style.display = 'none';
                return;
            }

            editSearchTimeout = setTimeout(() => {
                fetch(`/Profil/CautareTag?val=${encodeURIComponent(term)}`)
                .then(r => r.json())
                .then(users => {
                    editResults.innerHTML = ''; 
                    if (!users || users.length === 0) {
                        editResults.style.display = 'none';
                        return;
                    }
                    editResults.style.display = 'flex'; 
                    const label = document.createElement('p');
                    label.innerText = "Search Results:";
                    editResults.appendChild(label);
                    users.forEach(user => {
                        const div = document.createElement('div');
                        div.className = 'user-chip'; 
                        const esteDejaSelectat = editTagsList.some(u => u.username === user.userName);
                        if (esteDejaSelectat) {
                            div.classList.add('active');
                        }
                        const img = document.createElement('img');
                        img.src = user.poza || '/images/profilDefault.png';
                        const span = document.createElement('span');
                        span.innerText = user.userName.startsWith('@') ? user.userName : '@' + user.userName;
                        div.appendChild(img);
                        div.appendChild(span);
                        div.onclick = (e) => {
                            if(e) 
                                e.stopPropagation();
                            if (div.classList.contains('active')) {
                                div.classList.remove('active');
                                stergeTagEdit(user.userName);
                            } else {
                                div.classList.add('active');
                                adaugaTagEdit({
                                    username: user.userName,
                                    userImage: user.poza
                                });
                            }
                        };
                        editResults.appendChild(div);
                    });
                });
            }, 300);
        });
    }
});
function adaugaTagEdit(user) {
    if (editTagsList.some(u => u.username === user.username))
        return;
    editTagsList.push({
        username: user.username,
        userImage: user.userImage
    });
    vizualizareTags(); 
    sincronizeazaToateListeleEdit(user.username, true);
}
function vizualizareTags() {
    const container = document.getElementById('edit-selectedTagsContainer');
    const hiddenInput = document.getElementById('edit-finalTagList');
    
    if (!container || !hiddenInput) 
        return;
    container.innerHTML = '';

    hiddenInput.value = JSON.stringify(editTagsList.map(u => u.username));
    editTagsList.forEach(user => {
        const chip = document.createElement('div');
        chip.style.cssText = "display:flex; align-items:center; gap:8px; background:#0095f6; color:white; padding:6px 12px; border-radius:20px; font-size:13px; font-weight:600;";
        
        chip.innerHTML = `
            <span>@${user.username}</span>
            <i class="fa-solid fa-times" onclick="stergeTagEdit('${user.username}')" style="cursor: pointer; font-size: 12px; margin-left: 5px;"></i>
        `;
        container.appendChild(chip);
    });
}
function eliminaVizualizareTag(username, esteActiv) {
    const toateChips = document.querySelectorAll('#edit-rezultateTag .user-chip');
    toateChips.forEach(chip => {
        const span = chip.querySelector('span');
        if (span && (span.innerText === username || span.innerText === '@' + username)) {
            if (esteActiv) chip.classList.add('active');
            else chip.classList.remove('active');
        }
    });
}
function stergeTagEdit(username) {
    editTagsList = editTagsList.filter(u => u.username !== username);
    vizualizareTags();
    eliminaVizualizareTag(username,false);
    sincronizeazaToateListeleEdit(username, false);
}