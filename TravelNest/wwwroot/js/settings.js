const imgProfil = document.getElementById('ImagineProfil');
const imgPrev = document.getElementById('previzualizare');
const btnRemove = document.getElementById('removeProfileImg');
const resetInput = document.getElementById('ResetImage');
const defaultImg = '/images/profilDefault.png';
window.onload=function(){
    document.getElementById("ProfilSet").style.borderBottom="3px solid #107373";
    document.getElementById("sectiuneSetariPrivacy").style.display="none"
}
document.getElementById("ProfilSet").addEventListener("click",function(){
        document.getElementById("ProfilSet").style.borderBottom="3px solid #107373";
        document.getElementById("ConfSet").style.borderBottom="none";
        document.getElementById("Arhive").style.borderBottom="none";
        document.getElementById("paginaProfilSet").style.display="flex"
        document.getElementById("sectiuneSetariPrivacy").style.display="none"
})
document.getElementById("ConfSet").addEventListener("click",function(){
        document.getElementById("ConfSet").style.borderBottom="3px solid #107373";
        document.getElementById("ProfilSet").style.borderBottom="none";
        document.getElementById("Arhive").style.borderBottom="none";
        document.getElementById("paginaProfilSet").style.display="none"
        document.getElementById("sectiuneSetariPrivacy").style.display="flex"
})
document.getElementById("Arhive").addEventListener("click",function(){
        document.getElementById("Arhive").style.borderBottom="3px solid #107373";
        document.getElementById("ProfilSet").style.borderBottom="none";
        document.getElementById("ConfSet").style.borderBottom="none";
        document.getElementById("paginaProfilSet").style.display="none"
        document.getElementById("sectiuneSetariPrivacy").style.display="none"
})
imgProfil.addEventListener('change', function () {
     const file = this.files[0];
    if (file) {
            const reader = new FileReader();
            reader.onload = () => imgPrev.src = reader.result;
        reader.readAsDataURL(file);
        resetInput.value = 'false';
        }
    });

btnRemove.addEventListener('click', function () {
    imgPrev.src = defaultImg;
    imgProfil.value = ''; 
    resetInput.value = 'true';
    });
document.addEventListener("DOMContentLoaded", function () {
    const profilIdInput = document.getElementById('hiddenProfilId');
    if (!profilIdInput) return;

    const profilId = profilIdInput.value;
    const isPrivateCheckbox = document.querySelector('input[name="isPrivate"]');
    const noTagsCheckbox = document.querySelector('input[name="noTags"]');

    if (isPrivateCheckbox) {
        isPrivateCheckbox.addEventListener('change', () => {
            updateSetting('/Settings/makePrivateProfile', profilId, isPrivateCheckbox.checked);
        });
    }

    if (noTagsCheckbox) {
        noTagsCheckbox.addEventListener('change', () => {
            updateSetting('/Settings/AllowAutoTag', profilId, noTagsCheckbox.checked);
        });
    }
});

function updateSetting(url, id, status) {
    const formData = new URLSearchParams();
    formData.append('profilId', id);
    formData.append('status', status);

    fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: formData
    })
    .then(response => {
        if (!response.ok) {
            // Această linie îți va spune în consolă dacă e 400, 404 sau 500
            console.error(`Serverul a răspuns cu eroarea: ${response.status}`);
            throw new Error('Server error');
        }
        return response.json();
    })
    .then(data => {
        if (data.success) {
            console.log("Salvat cu succes!");
        } else {
            console.error("Eroare logică server:", data.message);
        }
    })
    .catch(err => console.error("Fetch error:", err));
}