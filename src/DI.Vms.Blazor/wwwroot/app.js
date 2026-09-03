window.vms = {
    /** Blazor Server cannot write a file directly, so the bytes cross the circuit. */
    downloadCsv: function (fileName, content) {
        const blob = new Blob(['﻿' + content], { type: 'text/csv;charset=utf-8' });
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

/* The header clock, ticked here rather than on the server. Asia/Dubai is Gulf Standard
   Time and has no daylight saving, so the offset never moves - but naming the zone keeps
   it correct on a machine set to anything else, which a reception PC may well be. */
(function () {
    const format = new Intl.DateTimeFormat('en-GB', {
        timeZone: 'Asia/Dubai',
        day: '2-digit', month: 'short',
        hour: '2-digit', minute: '2-digit',
        hour12: false
    });

    function tick() {
        const el = document.getElementById('gst-clock');
        // The element comes and goes with the layout, so this checks rather than caches.
        if (el) { el.textContent = format.format(new Date()) + ' GST'; }
    }

    tick();
    setInterval(tick, 15000);
})();
