document.getElementById("inputTag").addEventListener("keyup", function () {
    let valoareSearch = this.value;
    if (valoareSearch.length >= 3) {
        fetch('/Profil/CautareTag?val=' + encodeURIComponent(valoareSearch))
            .then(raspuns => raspuns.json())
            .then(u => {
                let rez = document.getElementById('rezultateTag');
                rez.innerHTML = '';
                u.forEach(user => {
                    rez.innerHTML +=
                        `<div class="tagRezultat" data-id="${user.id}">
                            <img src="${user.poza}" class="tagImagineUser" />
                            <span>${user.name}</span>
                        </div>`; 
                });
            });
    }
    else {
        let rez = document.getElementById('rezultateTag');
        rez.innerHTML = '';
    }
})