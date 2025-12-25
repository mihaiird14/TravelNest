document.addEventListener("DOMContentLoaded", function () {
    let currentIndex = 0;
    let totalSlides = 0;

    const xBtn = document.getElementById("xBtn");
    const menuBtn = document.getElementById("menuBtn");
    const sideMenus = document.querySelectorAll(".sideMenu");
    const addPostBtn = document.getElementById("addPost");
    const addPostBtn2 = document.getElementById("addPost2");
    const newPostForm = document.getElementById("newPostForm");
    const xBtnForm = document.getElementById("xBtnFormPost");
    const carouselWrapper = document.getElementById('carouselWrapper');
    const addMediaInput = document.getElementById('addMedia');
    const labelAddPoze = document.getElementById("labelAddPoze");
    const caruselSelectie = document.getElementById('caruselSelectie');
    const counter = document.getElementById('counter');
    const nextBtn = document.getElementById("nextBtn");
    const prevBtn = document.getElementById("prevBtn");
    const xBtnCarusel = document.getElementById("xBtnFormPostCarusel");

    if (xBtn) {
        xBtn.addEventListener("click", function () {
            sideMenus.forEach(x => x.style.display = "none");
            menuBtn.style.display = "inline";
        });
    }

    if (menuBtn) {
        menuBtn.addEventListener("click", function () {
            this.style.display = "none";
            sideMenus.forEach(x => x.style.display = "flex");
        });
    }

    if (addPostBtn) {
        addPostBtn.addEventListener("click", function (event) {
            event.stopPropagation();
            newPostForm.style.display = "flex";
            if (window.resetTagSystem) window.resetTagSystem();
        });
    }
    if (addPostBtn2) {
        addPostBtn2.addEventListener("click", function (event) {
            event.stopPropagation();
            newPostForm.style.display = "flex";
            if (window.resetTagSystem) window.resetTagSystem();
        });
    }

    if (xBtnForm) {
        xBtnForm.addEventListener("click", () => resetFormularFull());
    }

    document.addEventListener('click', (event) => {
        if (newPostForm && newPostForm.style.display === "flex" && !newPostForm.contains(event.target) && event.target !== addPostBtn) {
            newPostForm.style.display = "none";
        }
    });

    if (addMediaInput) {
        addMediaInput.addEventListener("change", (event) => {
            const fisiere = Array.from(event.target.files);
            if (fisiere.length === 0) return;

            carouselWrapper.innerHTML = "";
            caruselSelectie.style.display = "flex";
            addMediaInput.style.display = "none";
            labelAddPoze.style.display = "none";

            currentIndex = 0;
            totalSlides = fisiere.length;

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
            updateCounter();
        });
    }

    function updateCounter() {
        if (counter) counter.textContent = `${currentIndex + 1} / ${totalSlides}`;
    }

    function showSlide(index) {
        const items = document.querySelectorAll('.carousel-item');
        if (items.length === 0) return;

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
        updateCounter();
    }

    if (nextBtn) nextBtn.addEventListener('click', (e) => { 
        e.preventDefault(); 
        e.stopPropagation();
        showSlide(currentIndex + 1); 
    });

    if (prevBtn) prevBtn.addEventListener('click', (e) => { 
        e.preventDefault(); 
        e.stopPropagation();
        showSlide(currentIndex - 1); 
    });


    if (xBtnCarusel) {
        xBtnCarusel.addEventListener("click", (e) => {
            if(e) e.stopPropagation(); 
            resetMediaOnly();
        });
    }

    function resetMediaOnly() {
        if (carouselWrapper) 
            carouselWrapper.innerHTML = "";
        if (addMediaInput) 
            addMediaInput.value = '';
        if (caruselSelectie) 
            caruselSelectie.style.display = 'none';
        if (addMediaInput) 
            addMediaInput.style.display = "none"; 
        if (labelAddPoze) labelAddPoze.style.display = "flex";

        if (window.resetTagSystem) 
            window.resetTagSystem();
    }

    function resetFormularFull() {
        if (newPostForm) newPostForm.style.display = "none";
        resetMediaOnly();
    }
});