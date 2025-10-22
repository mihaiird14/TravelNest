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
