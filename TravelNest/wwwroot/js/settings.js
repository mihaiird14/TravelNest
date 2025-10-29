const imgProfil = document.getElementById('ImagineProfil');
const imgPrev = document.getElementById('previzualizare');
const btnRemove = document.getElementById('removeProfileImg');
const resetInput = document.getElementById('ResetImage');
const defaultImg = '/images/profilDefault.png';

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