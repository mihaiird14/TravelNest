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
            var imageFiles = files.filter(function(f) {
                return f.type.indexOf('image') !== -1;
            });
            
            if (imageFiles.length > 0) {
                scaneazaPoze(imageFiles);
            }
        });
    }

    function scaneazaPoze(files) {
        if(loadingMsg) loadingMsg.style.display = 'block';
        if(suggestionsArea) suggestionsArea.innerHTML = '';

        var promises = files.map(function(file) {
            var formData = new FormData();
            formData.append('file', file);

            return fetch('/Profil/CheckFacesInPhoto', {
                method: 'POST',
                body: formData
            })
            .then(function(response) {
                return response.json();
            })
            .catch(function(err) {
                return { success: false, suggestions: [] };
            });
        });

        Promise.all(promises).then(function(results) {
            if(loadingMsg) loadingMsg.style.display = 'none';
            
            var uniqueUsersMap = {};
            var uniqueList = [];

            for(var i = 0; i < results.length; i++) {
                var data = results[i];
                if (data.success && data.suggestions && data.suggestions.length > 0) {
                    for(var j = 0; j < data.suggestions.length; j++) {
                        var user = data.suggestions[j];
                        if (!uniqueUsersMap[user.userName]) {
                            uniqueUsersMap[user.userName] = true;
                            uniqueList.push(user);
                        }
                    }
                }
            }

            if (uniqueList.length > 0) {
                afiseazaSugestii(uniqueList);
            }
        });
    }

    function afiseazaSugestii(users) {
        suggestionsArea.innerHTML = '';
        var label = document.createElement('p');
        label.innerText = 'Suggested Users:';
        label.style.width = '100%';
        label.style.color = '#666';
        label.style.fontSize = '12px';
        label.style.marginBottom = '5px';
        label.style.fontWeight = 'bold';
        suggestionsArea.appendChild(label);

        for (var i = 0; i < users.length; i++) {
            createChip(users[i]);
        }
    }

    function createChip(user) {
        var div = document.createElement('div');
        div.className = 'user-chip';

        var img = document.createElement('img');
        if (user.poza) {
            img.src = user.poza;
        } else {
            img.src = '/images/profilDefault.png';
        }

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
        };

        suggestionsArea.appendChild(div);
    }

    if(inputTagManual) {
        inputTagManual.addEventListener("keyup", function() {
            var val = this.value;
            if (val.length >= 1) {
                fetch('/Profil/CautareTag?val=' + encodeURIComponent(val))
                .then(function(res) { return res.json(); })
                .then(function(users) {
                    rezultateTagManual.innerHTML = '';
                    for (var i = 0; i < users.length; i++) {
                        var u = users[i];
                        var resDiv = document.createElement('div');
                        resDiv.style.display = 'flex';
                        resDiv.style.alignItems = 'center';
                        resDiv.style.padding = '10px';
                        resDiv.style.cursor = 'pointer';
                        resDiv.style.borderBottom = '1px solid #eee';
                        resDiv.style.background = 'white';

                        var resImg = document.createElement('img');
                        resImg.src = u.poza;
                        resImg.style.width = '30px';
                        resImg.style.height = '30px';
                        resImg.style.borderRadius = '50%';
                        resImg.style.marginRight = '10px';

                        var resName = document.createElement('span');
                        resName.innerText = u.name;

                        resDiv.appendChild(resImg);
                        resDiv.appendChild(resName);

                        (function(n) {
                            resDiv.onclick = function(e) {
                                if(e) e.stopPropagation();
                                manageTags(n, true);
                                rezultateTagManual.innerHTML = '';
                                inputTagManual.value = '';
                            };
                        })(u.name);

                        rezultateTagManual.appendChild(resDiv);
                    }
                });
            } else {
                rezultateTagManual.innerHTML = '';
            }
        });
    }

    function manageTags(name, add) {
        if (add) {
            activeTags.add(name);
        } else {
            activeTags.delete(name);
        }
        
        var tagsArray = Array.from(activeTags);
        hiddenInput.value = tagsArray.join(',');
        
        selectedTagsContainer.innerHTML = '';
        
        for (var k = 0; k < tagsArray.length; k++) {
            var tagName = tagsArray[k];
            var tagEl = document.createElement('div');
            tagEl.style.background = '#007bff';
            tagEl.style.color = 'white';
            tagEl.style.padding = '4px 8px';
            tagEl.style.borderRadius = '5px';
            tagEl.style.margin = '2px';
            tagEl.style.fontSize = '14px';
            tagEl.style.display = 'flex';
            tagEl.style.alignItems = 'center';

            var txt = document.createElement('span');
            txt.innerText = tagName;
            
            var xIcon = document.createElement('i');
            xIcon.className = 'fa-solid fa-xmark';
            xIcon.style.marginLeft = '5px';
            xIcon.style.cursor = 'pointer';
            
            (function(n) {
                xIcon.onclick = function(e) {
                    if(e) e.stopPropagation();
                    manageTags(n, false);
                    var allChips = document.querySelectorAll('.user-chip');
                    for(var j=0; j<allChips.length; j++) {
                        if(allChips[j].innerText === n) {
                            allChips[j].classList.remove('active');
                        }
                    }
                };
            })(tagName);

            tagEl.appendChild(txt);
            tagEl.appendChild(xIcon);
            selectedTagsContainer.appendChild(tagEl);
        }
    }
});