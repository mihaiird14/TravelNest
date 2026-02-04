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
        const textRaspunsuri = com.nrRaspunsuri > 0
            ? `Vezi ${com.nrRaspunsuri} răspunsuri`
            : 'Răspunde';

        return `
            <div class="rand-descriere">
                <i class="fas fa-ellipsis-h optiuni-comentariu" title="Opțiuni"></i>

                <div class="comentariu-top">
                    <img src="${com.poza}" class="poza-rotunda mica">
                    <div class="continut-text">
                        <span class="nume-bold">${com.username}</span>
                        <span class="text-normal">${com.continut}</span>
                    </div>
                </div>

                <div class="zona-actiuni-com">
                    <span class="data-text">${com.data}</span>

                    <div class="btn-like-com">
                        <i class="far fa-heart"></i> <span>Like</span>
                    </div>

                    <div class="btn-raspunsuri">
                        <span>${textRaspunsuri}</span>
                        ${com.nrRaspunsuri > 0 ? '<i class="fas fa-chevron-down"></i>' : ''}
                    </div>
                </div>
            </div>
        `;
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
            if (countSpan) 
                countSpan.innerText = date.nrLikeuri;
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