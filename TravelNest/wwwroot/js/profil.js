document.getElementById("xBtn").addEventListener("click", function () {
    elem = document.querySelectorAll(".sideMenu");
    elem.forEach(function (x) {
        x.style.display = "none";
    })
    document.getElementById("menuBtn").style.display = "inline";
})
document.getElementById("menuBtn").addEventListener("click", function () {
    document.getElementById("menuBtn").style.display = "none";
    document.querySelectorAll(".sideMenu").forEach(function (x) {
        x.style.display = "flex";
    })
})
document.getElementById("addPost").addEventListener("click", function (event) {
    event.stopPropagation();
    document.getElementById("newPostForm").style.display = "flex";
    document.getElementById('inputTag').value = '';
    document.getElementById('rezultateTag').innerHTML = '';
})
document.addEventListener('click', (event) => {
    const button = document.getElementById("addPost");
    if (!document.getElementById("newPostForm").contains(event.target) && event.target !== button) {
        document.getElementById("newPostForm").style.display = "none";
    }
})
document.getElementById("xBtnFormPost").addEventListener("click", function () {
    document.getElementById("newPostForm").style.display = "none";
})