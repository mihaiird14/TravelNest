    document.addEventListener("DOMContentLoaded", function() {
        const dataStartRaw = '@Model.DataPlecare?.ToString("yyyy-MM-dd")';
        const dataSfarsitRaw = '@Model.DataIntoarcere?.ToString("yyyy-MM-dd")';
        if (!dataStartRaw || !dataSfarsitRaw) {
            console.error("Datele de vacanță lipsesc sau sunt invalide.");
            return;
        }

        const inceputVacanta = new Date(dataStartRaw);
        const sfarsitVacanta = new Date(dataSfarsitRaw);
        const container = document.getElementById('containerZile');
        if (isNaN(inceputVacanta.getTime())) return;

        const numeLuni = ["Ianuarie", "Februarie", "Martie", "Aprilie", "Mai", "Iunie", "Iulie", "August", "Septembrie", "Octombrie", "Noiembrie", "Decembrie"];

   
        let html = `<div id="lunaText">${numeLuni[inceputVacanta.getMonth()]} ${inceputVacanta.getFullYear()}</div>`;

        html += `<div id="gridCalendar">
                    <div class="ziNume">Du</div><div class="ziNume">Lu</div><div class="ziNume">Ma</div>
                    <div class="ziNume">Mi</div><div class="ziNume">Jo</div><div class="ziNume">Vi</div><div class="ziNume">Sâ</div>`;
        const primaZi = new Date(inceputVacanta.getFullYear(), inceputVacanta.getMonth(), 1).getDay();
        const zileInLuna = new Date(inceputVacanta.getFullYear(), inceputVacanta.getMonth() + 1, 0).getDate();
        for (let i = 0; i < primaZi; i++) html += `<div></div>`;
        for (let zi = 1; zi <= zileInLuna; zi++) {
            const dataCurenta = new Date(inceputVacanta.getFullYear(), inceputVacanta.getMonth(), zi);

            const esteInVacanta = dataCurenta >= inceputVacanta && dataCurenta <= sfarsitVacanta;
            const clasa = esteInVacanta ? 'ziVerde' : 'ziNormala'; 

            html += `<div class="ziCelula ${clasa}">${zi}</div>`;
        }

        html += `</div>`;
        container.innerHTML = html;
    });