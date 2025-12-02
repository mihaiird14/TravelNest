let currentIndex = 0;
let totalSlides = 0;
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
    const carouselWrapper = document.getElementById('carouselWrapper');
    carouselWrapper.innerHTML = "";
    document.getElementById('addMedia').value = '';
    currentIndex = 0;
    totalSlides = 0;
    document.getElementById('caruselSelectie').style.display = 'none';
    document.getElementById("addMedia").style.display = "none"
    document.getElementById("labelAddPoze").style.display = "flex"
})

document.getElementById("addMedia").addEventListener("change", (event) => {
    const fisiere = Array.from(event.target.files)
    if (fisiere.length == 0) {
        return;
    }

    const caruselPoze = document.getElementById('carouselWrapper');
    const carouselContainer = document.getElementById('caruselSelectie');
    caruselPoze.innerHTML = ""
    index = 0;
    totalSlides = fisiere.length
    carouselContainer.style.display = "flex"
    document.getElementById("addMedia").style.display = "none"
    document.getElementById("labelAddPoze").style.display = "none"
    fisiere.forEach((file, index) => {
        const fileURL = URL.createObjectURL(file);
        let element;
        if (file.type.startsWith('image/')) {
            element = document.createElement('img');
            element.src = fileURL;
        } else if (file.type.startsWith('video/')) {
            element = document.createElement('video');
            element.src = fileURL;
            element.controls = true;
        }

        if (element) {
            element.classList.add('carousel-item');
            if (index === 0) element.classList.add('active');
            carouselWrapper.appendChild(element);
        }

    });
    UpdateNrPoze();
})
function showSlide(index) {
    const items = document.querySelectorAll('.carousel-item');
    items.forEach(item => item.classList.remove('active'));
    if (index >= totalSlides) currentIndex = 0;
    else if (index < 0) currentIndex = totalSlides - 1;
    else currentIndex = index;
    items[currentIndex].classList.add('active');
    items.forEach((item, i) => {
        if (i !== currentIndex && item.tagName === 'VIDEO') {
            item.pause();
        }
    });
    UpdateNrPoze();
}
function UpdateNrPoze() {
    document.getElementById('counter').textContent = `${currentIndex + 1} / ${totalSlides}`;
}
document.getElementById("nextBtn").addEventListener('click', (event) => {
    event.preventDefault(); 
    event.stopPropagation(); 
    showSlide(currentIndex + 1);
});

document.getElementById("prevBtn").addEventListener('click', (event) => {
    event.preventDefault();
    event.stopPropagation();
    showSlide(currentIndex - 1);
});
document.getElementById("xBtnFormPostCarusel").addEventListener("click", function () {
    const carouselWrapper = document.getElementById('carouselWrapper');
    carouselWrapper.innerHTML = "";
    document.getElementById('addMedia').value = '';
    currentIndex = 0;
    totalSlides = 0;
    document.getElementById('caruselSelectie').style.display = 'none';
    document.getElementById("addMedia").style.display = "none"
    document.getElementById("labelAddPoze").style.display = "flex"
})