class CalendarZile {
    constructor() {
        this.container = document.getElementById('containerZile');
        if (!this.container)
            return;

        this.inceputPerioda = this.container.dataset.startDate || '';
        this.sfarsitPerioda = this.container.dataset.endDate || '';
        this.lunaCurenta = new Date(this.inceputPerioda) || new Date();
        this.zileSel = [];
        this.esteAdmin = document.getElementById('esteAdmin')?.value === 'true';
        this.btnConfirm = document.getElementById('confirmareData');

        this.init();
    }

    init() {
        this.butoaneNavigare();
        this.afisare();
    }

    butoaneNavigare() {
        const prev = document.getElementById('lunaPrecedenta');
        const next = document.getElementById('lunaUrmatoare');

        if (prev) 
            prev.onclick = () => this.schimbaLunaCalendar(-1);
        if (next) 
            next.onclick = () => this.schimbaLunaCalendar(1);
        if (this.btnConfirm) 
            this.btnConfirm.onclick = () => this.submitDates();
    }

    schimbaLunaCalendar(offset) {
        this.lunaCurenta.setMonth(this.lunaCurenta.getMonth() + offset);
        this.afisare();
    }

    afisare() {
        this.container.innerHTML = '';
        this.zileleSaptamanii();
        this.zileLibere();
        this.zipeLuni();
        this.actualizareLuna();
        this.attachDayListeners();
    }

    zileleSaptamanii() {
        const days = ['Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa', 'Su'];
        days.forEach(d => {
            const cell = document.createElement('div');
            cell.className = 'nume-zi-header';
            cell.textContent = d;
            this.container.appendChild(cell);
        });
    }

    zileLibere() {
        const year = this.lunaCurenta.getFullYear();
        const month = this.lunaCurenta.getMonth();
        const startDay = new Date(year, month, 1).getDay();
        const slots = startDay === 0 ? 6 : startDay - 1;

        for (let i = 0; i < slots; i++) {
            this.container.appendChild(document.createElement('div'));
        }
    }

    zipeLuni() {
        const year = this.lunaCurenta.getFullYear();
        const month = this.lunaCurenta.getMonth();
        const daysCount = new Date(year, month + 1, 0).getDate();

        for (let day = 1; day <= daysCount; day++) {
            const dateKey = this.formatDateKey(year, month, day);
            const cell = document.createElement('div');
            
            cell.className = 'zi-calendar';
            cell.setAttribute('data-date', dateKey);
            cell.textContent = day;
            if (this.isOfficialRange(dateKey)) {
                cell.classList.add('perioada-oficiala');
            }
            if (this.zileSel.indexOf(dateKey) > -1) {
                cell.classList.add('zi-selectata');
            }

            this.container.appendChild(cell);
        }
    }

    formatDateKey(y, m, d) {
        const month = String(m + 1).padStart(2, '0');
        const day = String(d).padStart(2, '0');
        return `${y}-${month}-${day}`;
    }

    isOfficialRange(dateKey) {
        if (!this.inceputPerioda || !this.sfarsitPerioda) return false;
        return dateKey >= this.inceputPerioda && dateKey <= this.sfarsitPerioda;
    }

    actualizareLuna() {
        const label = document.getElementById('numeLunaAn');
        if (!label) return;

        const monthStr = this.lunaCurenta.toLocaleString('default', { month: 'long' });
        const yearStr = this.lunaCurenta.getFullYear();
        label.textContent = `${monthStr} ${yearStr}`;
    }

    attachDayListeners() {
        const cells = this.container.querySelectorAll('[data-date]');
        
        cells.forEach(cell => {
            if (!this.esteAdmin) return;

            cell.classList.add('clickable');
            cell.addEventListener('click', (evt) => {
                evt.preventDefault();
                this.toggleDate(cell.getAttribute('data-date'));
            });
        });
    }

    toggleDate(dateKey) {
        const idx = this.zileSel.indexOf(dateKey);
        
        if (idx !== -1) {
            // remove if already selected
            this.zileSel.splice(idx, 1);
        } else {
            // reset if we already have 2 selected
            if (this.zileSel.length === 2) {
                this.zileSel.length = 0;
            }
            this.zileSel.push(dateKey);
        }

        this.zileSel.sort();
        this.afisare();
        this.updateConfirmBtn();
    }

    updateConfirmBtn() {
        if (!this.btnConfirm) return;

        const shouldShow = this.zileSel.length === 2;
        this.btnConfirm.classList.toggle('visible', shouldShow);
    }

    submitDates() {
        if (this.zileSel.length < 2) return;

        const banner = document.getElementById('bannerVizualizare');
        if (!banner) 
            return;

        const groupId = banner.getAttribute('data-idGrup');
        if (!groupId) 
            return;

        // prepare the request
        const formularCalendar = new FormData();
        formularCalendar.append('id', groupId);
        formularCalendar.append('dataPlecare', this.zileSel[0]);
        formularCalendar.append('dataIntoarcere', this.zileSel[1]);

        fetch('/TravelGroup/ActualizarePerioada', {
            method: 'POST',
            body: formularCalendar
        })
        .then(res => {
            if (res.ok) {
                sessionStorage.setItem('reminderZboruri', 'true');
                window.location.reload();
            }
        })
        .catch(err => {
            console.log('Failed to update period:', err);
        });
    }
}

document.addEventListener('DOMContentLoaded', function() {
    window.calendarZile = new CalendarZile();
});