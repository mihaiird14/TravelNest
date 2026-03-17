document.addEventListener("DOMContentLoaded", function() {
    const inputCautare = document.getElementById('inputCautare');
    const listaSugestii = document.getElementById('rezultateCautare');
    inputCautare.addEventListener('input', async function() {
        const text = this.value.trim();

        if (text.length < 3) {
            listaSugestii.style.display = 'none';
            return;
        }

        try {
            const raspuns = await fetch(`/ForYou/CautaUtilizatori?un=${encodeURIComponent(text)}`);
            if (!raspuns.ok) {
                console.error(raspuns.status);
                return;
            }
            const dateVenite = await raspuns.json();
            afisRez(dateVenite);
        } catch (eroare) {
            console.error(eroare);
        }
    });

    function afisRez(us) {
        if (us.length === 0) {
            rezultateCautare.style.display = 'none';
            return;
        }

        rezultateCautare.innerHTML = us.map(user => `
            <a href="/Profil/Index/${user.id}" id="randUser">
                <img src="${user.imagineProfil || '/images/user_placeholder.png'}" id="pozaUser">
                <span id="numeUser">${user.userName}</span>
            </a>
        `).join('');

        rezultateCautare.style.display = 'block';
    }
    document.addEventListener('click', function(e) {
        if (!inputCautare.contains(e.target) && !rezultateCautare.contains(e.target)) {
            rezultateCautare.style.display = 'none';
        }
    });
});