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
    listaMediaCurenta = date.media;
    indexSlideCurent = 0;
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
            iconLike.className = 'fa-solid fa-heart text-danger'; // Roșie
        } else {
            iconLike.className = 'fa-regular fa-heart'; // Goală
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