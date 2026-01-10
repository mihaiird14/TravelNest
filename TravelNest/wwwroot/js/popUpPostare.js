    let listaMediaCurenta = [];
    let indexSlideCurent = 0;
    async function deschidePostare(idPostare) {
        const fereastra = document.getElementById('fereastra-postare');
        const containerMedia = document.getElementById('continut-media');
        fereastra.style.display = 'flex';
        containerMedia.innerHTML = '<p style="color:white;">Se încarcã...</p>';

        try {
            const raspuns = await fetch(`/Profil/InfoPostari?postId=${idPostare}`);

            if (!raspuns.ok) throw new Error('Nu am putut încãrca postarea.');

            const date = await raspuns.json();
            populeazaFereastra(date);

        } catch (eroare) {
            console.error(eroare);
            containerMedia.innerHTML = '<p style="color:red;">Eroare!</p>';
        }
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

        afiseazaSlide(0);
    }
       function afiseazaSlide(index) {
        const container = document.getElementById('continut-media');
        const btnInapoi = document.getElementById('btn-inapoi');
        const btnInainte = document.getElementById('btn-inainte');
        if (!listaMediaCurenta || listaMediaCurenta.length === 0) {
            container.innerHTML = '<p style="color:white">Fãrã media</p>';
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