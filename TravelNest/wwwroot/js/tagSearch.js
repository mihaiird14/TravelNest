document.addEventListener("DOMContentLoaded", function() {
    let activeTags = new Set();
    
    var fileInput = document.getElementById('addMedia');
    var hiddenInput = document.getElementById('finalTagList');
    var suggestionsArea = document.getElementById('sugestiiAutomate');
    var loadingMsg = document.getElementById('loadingFaces');
    var inputTagManual = document.getElementById('inputTag');
    var rezultateTagManual = document.getElementById('rezultateTag');
    var selectedTagsContainer = document.getElementById('selectedTagsContainer');

    window.resetTagSystem = function() {
        activeTags.clear();
        if(hiddenInput) hiddenInput.value = '';
        if(inputTagManual) inputTagManual.value = '';
        if(rezultateTagManual) rezultateTagManual.innerHTML = '';
        if(suggestionsArea) suggestionsArea.innerHTML = '';
        if(loadingMsg) loadingMsg.style.display = 'none';
        if(selectedTagsContainer) selectedTagsContainer.innerHTML = '';
    };

    if(fileInput) {
        fileInput.addEventListener("change", function(e) {
            var files = Array.from(e.target.files);
            var imageFiles = files.filter(f => f.type.indexOf('image') !== -1);
            if (imageFiles.length > 0) scanAllPhotos(imageFiles);
        });
    }

    function scanAllPhotos(files) {
        if(loadingMsg) loadingMsg.style.display = 'block';
        if(suggestionsArea) suggestionsArea.innerHTML = '';

        var promises = files.map(file => {
            var formData = new FormData();
            formData.append('file', file);
            return fetch('/Profil/CheckFacesInPhoto', { method: 'POST', body: formData })
                .then(r => r.json())
                .catch(err => ({ success: false, suggestions: [] }));
        });

        Promise.all(promises).then(results => {
            if(loadingMsg) loadingMsg.style.display = 'none';
            var uniqueList = [];
            var uniqueMap = {};

            results.forEach(data => {
                if (data.success && data.suggestions) {
                    data.suggestions.forEach(u => {
                        var name = u.userName || u.UserName || u.name;
                        if (name && !uniqueMap[name]) {
                            uniqueMap[name] = true;
                            uniqueList.push({ userName: name, poza: u.poza || u.Poza });
                        }
                    });
                }
            });

            if (uniqueList.length > 0) showChips(uniqueList, suggestionsArea, "Suggested Users:");
        });
    }
    if(inputTagManual) {
        inputTagManual.addEventListener("keyup", function() {
            var val = this.value;
            
            if (val.length >= 1) {
                fetch('/Profil/CautareTag?val=' + encodeURIComponent(val))
                .then(r => r.json())
                .then(users => {
                    rezultateTagManual.innerHTML = ''; 
                    
                    if(users && users.length > 0) {
                        var normalizedUsers = users.map(u => ({
                            userName: u.userName || u.UserName || u.name,
                            poza: u.poza || u.Poza
                        }));
                       
                        showChips(normalizedUsers, rezultateTagManual, "Search Results:"); 
                    }
                })
                .catch(err => console.error(err));
            } else {
                rezultateTagManual.innerHTML = '';
            }
        });
    }
    function showChips(users, container, titleText) {
        if(titleText) {
            var label = document.createElement('p');
            label.innerText = titleText;
            label.style.cssText = 'width:100%; color:#666; font-size:12px; margin-bottom:5px; font-weight:bold; margin-left: 5px;';
            container.appendChild(label);
        }

        users.forEach(function(user) {
            var div = document.createElement('div');
            div.className = 'user-chip';

            if(activeTags.has(user.userName)) {
                div.classList.add('active');
            }

            var img = document.createElement('img');
            img.src = user.poza || '/images/profilDefault.png';

            var span = document.createElement('span');
            span.innerText = user.userName;

            div.appendChild(img);
            div.appendChild(span);

            div.onclick = function(e) {
                if(e) e.stopPropagation();

                if (div.classList.contains('active')) {
                    div.classList.remove('active');
                    manageTags(user.userName, false);
                } else {
                    div.classList.add('active');
                    manageTags(user.userName, true);
                }
                
                syncVisuals(user.userName, div.classList.contains('active'));
            };
            
            container.appendChild(div);
        });
    }

function syncVisuals(name, isActive) {
    var allChips = document.querySelectorAll('.user-chip');
    allChips.forEach(chip => {
        var span = chip.querySelector('span');
        if(span && span.innerText === name) {
            if(isActive) 
                chip.classList.add('active');
            else 
                chip.classList.remove('active');
        }
    });
}

function manageTags(name, add) {
        if (add) 
            activeTags.add(name);
        else 
            activeTags.delete(name);
        hiddenInput.value = Array.from(activeTags).join(',');
        renderSelectedTags();
    }

function renderSelectedTags() {
        selectedTagsContainer.innerHTML = '';
        activeTags.forEach(function(tagName) {
            var tagEl = document.createElement('div');
            tagEl.style.cssText = 'background:#007bff; color:white; padding:4px 10px; border-radius:15px; font-size:13px; display:flex; align-items:center; gap:6px;';
            
            var txt = document.createElement('span');
            txt.innerText = tagName;
            
            var xIcon = document.createElement('i');
            xIcon.className = 'fa-solid fa-xmark';
            xIcon.style.cursor = 'pointer';
            
            xIcon.onclick = function(e) {
                if(e) e.stopPropagation();
                manageTags(tagName, false);
                syncVisuals(tagName, false);
            };

            tagEl.appendChild(txt);
            tagEl.appendChild(xIcon);
            selectedTagsContainer.appendChild(tagEl);
        });
    }
});
async function scanareAutomataEdit(mediaFiles) {
    const loadingMsg = document.getElementById('edit-loadingFaces');
    const suggestionsArea = document.getElementById('edit-sugestiiAutomate');
    
    if (!loadingMsg || !suggestionsArea) 
        return;
    
    loadingMsg.style.display = 'block';
    suggestionsArea.innerHTML = '';

    const imagini = mediaFiles.filter(m => m.tip === 'Image');
    
    const promises = imagini.map(async (media) => {
        try {
            const resp = await fetch(media.url);
            const blob = await resp.blob();
            
            const formData = new FormData();
            formData.append('file', blob, "image.jpg");

            const r = await fetch('/Profil/CheckFacesInPhoto', { method: 'POST', body: formData });
            return await r.json();
        } catch (err) {
            return { success: false, suggestions: [] };
        }
    });

    const results = await Promise.all(promises);
    loadingMsg.style.display = 'none';

    let uniqueList = [];
    let uniqueMap = {};

    results.forEach(data => {
        if (data.success && data.suggestions) {
            data.suggestions.forEach(u => {
                const name = u.userName || u.UserName;
                if (name && !uniqueMap[name]) {
                    uniqueMap[name] = true;
                    uniqueList.push({ userName: name, poza: u.poza || u.Poza });
                }
            });
        }
    });

    if (uniqueList.length > 0) {
        afiseazaSugestiiAutomateEdit(uniqueList);
    }
}

function afiseazaSugestiiAutomateEdit(users) {
    const container = document.getElementById('edit-sugestiiAutomate');
    container.innerHTML = '<p style="width:100%; color:#666; font-size:12px; margin-bottom:5px; font-weight:bold; margin-left: 5px;">Suggested Users:</p>';

    users.forEach(user => {
        const div = document.createElement('div');
        div.className = 'user-chip';
      
        if (editTagsList.some(u => u.username === user.userName)) {
            div.classList.add('active');
        }

        div.innerHTML = `
            <img src="${user.poza || '/images/profilDefault.png'}">
            <span>@${user.userName}</span>
        `;

        div.onclick = (e) => {
            if(e) e.stopPropagation();
            if (div.classList.contains('active')) {
                div.classList.remove('active');
                stergeTagEdit(user.userName);
            } else {
                div.classList.add('active');
                adaugaTagEdit({ username: user.userName, userImage: user.poza });
            }
        };
        container.appendChild(div);
    });
}
function sincronizeazaToateListeleEdit(username, esteSelectat) {
    const containere = [
        document.getElementById('edit-rezultateTag'),    
        document.getElementById('edit-sugestiiAutomate') 
    ];

    containere.forEach(container => {
        if (!container) 
            return;
       
        const chips = container.querySelectorAll('.user-chip');
        
        chips.forEach(chip => {
            const span = chip.querySelector('span');
            if (span && (span.innerText === username || span.innerText === '@' + username)) {
                if (esteSelectat) {
                    chip.classList.add('active'); 
                } else {
                    chip.classList.remove('active'); 
                }
            }
        });
    });
}
function viewTags() {
    const lista = document.getElementById('listaTags');
    const icon = document.getElementById('userTagsIcon'); 
    if (!lista) 
        return;
    if (lista.style.display === 'none' || lista.style.display === '') {
        lista.style.display = 'flex'; 
        if(icon) 
            icon.style.backgroundColor = 'white';
        if(icon) 
            icon.style.color = 'black';
    } else {
        lista.style.display = 'none'; 
        if(icon)
            icon.style.backgroundColor = 'rgba(0, 0, 0, 0.7)';
        if(icon) 
            icon.style.color = 'white';
    }
}