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

/* Appearance: light, dark, or whatever the device says.

   The stylesheet already carries a dark palette twice over - once behind
   prefers-color-scheme for the device default, once behind :root[data-theme="dark"] so
   an explicit choice wins in both directions. All this does is set or clear that
   attribute and remember it.

   Delegated from the document rather than bound to the buttons: Blazor renders and
   re-renders the layout, so the buttons come and go, and a handler attached to one of
   them would be attached to a button that is no longer there. Which one looks pressed is
   left to CSS, read from the attribute on <html> - one source of truth, and it cannot
   fall out of step with what is applied.

   The head of App.razor applies the saved choice before the first paint; this file loads
   too late for that, and a desk set to dark would flash white on every navigation. */
(function () {
    const KEY = 'vms-theme';

    function remember(choice) {
        try {
            if (choice === 'system') { localStorage.removeItem(KEY); }
            else { localStorage.setItem(KEY, choice); }
        } catch (e) {
            /* A browser with storage blocked still gets the change, just not the memory
               of it. Better than refusing to switch. */
        }
    }

    function pressed(choice) {
        document.querySelectorAll('[data-theme-choice]').forEach(function (button) {
            button.setAttribute('aria-pressed', button.dataset.themeChoice === choice ? 'true' : 'false');
        });
    }

    function current() {
        return document.documentElement.getAttribute('data-theme') || 'system';
    }

    document.addEventListener('click', function (event) {
        const button = event.target.closest ? event.target.closest('[data-theme-choice]') : null;
        if (!button) { return; }

        const choice = button.dataset.themeChoice;

        if (choice === 'system') { document.documentElement.removeAttribute('data-theme'); }
        else { document.documentElement.setAttribute('data-theme', choice); }

        remember(choice);
        pressed(choice);
    });

    /* CSS shows the state from the moment the markup exists; this is only for the
       screen reader, so it waits for Blazor to render the layout and then stops
       watching. */
    const watch = new MutationObserver(function () {
        if (document.querySelector('[data-theme-choice]')) {
            pressed(current());
            watch.disconnect();
        }
    });

    if (document.querySelector('[data-theme-choice]')) { pressed(current()); }
    else { watch.observe(document.body || document.documentElement, { childList: true, subtree: true }); }
})();
