const addPostBtn2 = document.getElementsByClassName("AddPost2")[0];
if (addPostBtn2) {
    addPostBtn2.addEventListener("click", function (event) {
        event.preventDefault();
        window.location.href = '/Profil?openPost=true';
    });
}

