/* Small interop surface. Kept deliberately thin: anything that can be done in C# is. */
window.vms = {
    /**
     * Hands the browser a CSV to save. Blazor Server cannot write a file directly, so the
     * bytes cross the circuit and a blob URL is created here.
     */
    downloadCsv: function (fileName, content) {
        const blob = new Blob([content], { type: 'text/csv;charset=utf-8' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
    }
};

/* ------------------------------------------------------- card reader (bridge) */

/* The bridge listens on the RECEPTION MACHINE's loopback. Blazor Server runs on the
   server, so it cannot reach it - but the browser can, because the browser is on the
   reception machine. These calls therefore run in the browser and the result crosses
   the circuit. */
window.vms.cardReader = {
    BASE: 'http://127.0.0.1:9100',

    status: async function () {
        try {
            const controller = new AbortController();
            const timer = setTimeout(() => controller.abort(), 3000);
            const res = await fetch(this.BASE + '/reader/status', { signal: controller.signal });
            clearTimeout(timer);
            if (!res.ok) return { available: false, detail: 'The bridge returned ' + res.status + '.' };
            return await res.json();
        } catch {
            return {
                available: false,
                detail: 'The card bridge is not running on this machine. Start DI.Vms.CardBridge.exe, then reload.'
            };
        }
    },

    read: async function () {
        const controller = new AbortController();
        // A chip read plus two gateway calls is not instant.
        const timer = setTimeout(() => controller.abort(), 25000);
        try {
            const res = await fetch(this.BASE + '/reader/read', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ photo: true }),
                signal: controller.signal
            });
            const body = await res.json().catch(() => ({}));
            if (!res.ok) throw new Error(body.error || ('The reader returned ' + res.status + '.'));
            return body;
        } finally {
            clearTimeout(timer);
        }
    }
};

/* ---------------------------------------------------------------- signature pad */

window.vms.signature = {
    init: function (canvas) {
        if (!canvas || canvas.dataset.wired) return;
        canvas.dataset.wired = '1';

        const ratio = window.devicePixelRatio || 1;
        const rect = canvas.getBoundingClientRect();
        canvas.width = rect.width * ratio;
        canvas.height = rect.height * ratio;

        const ctx = canvas.getContext('2d');
        ctx.scale(ratio, ratio);
        ctx.lineWidth = 2;
        ctx.lineCap = 'round';
        ctx.lineJoin = 'round';
        ctx.strokeStyle = getComputedStyle(document.body).color || '#0b0b0b';

        let drawing = false;
        const at = (e) => {
            const r = canvas.getBoundingClientRect();
            return { x: e.clientX - r.left, y: e.clientY - r.top };
        };

        canvas.addEventListener('pointerdown', (e) => {
            canvas.setPointerCapture(e.pointerId);
            drawing = true;
            canvas.dataset.ink = '1';
            const p = at(e);
            ctx.beginPath();
            ctx.moveTo(p.x, p.y);
        });

        canvas.addEventListener('pointermove', (e) => {
            if (!drawing) return;
            const p = at(e);
            ctx.lineTo(p.x, p.y);
            ctx.stroke();
        });

        const stop = () => { drawing = false; };
        canvas.addEventListener('pointerup', stop);
        canvas.addEventListener('pointerleave', stop);
    },

    /** Returns base64 PNG, or null if nothing was drawn - check-in requires a signature. */
    get: function (canvas) {
        if (!canvas || !canvas.dataset.ink) return null;
        return canvas.toDataURL('image/png').split(',')[1];
    },

    clear: function (canvas) {
        if (!canvas) return;
        const ctx = canvas.getContext('2d');
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        delete canvas.dataset.ink;
    }
};
