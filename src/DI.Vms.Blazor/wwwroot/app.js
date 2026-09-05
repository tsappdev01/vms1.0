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
   an explicit choice wins in both directions. All this does is keep that attribute in
   step with the remembered choice.

   Delegated from the document rather than bound to the buttons: Blazor renders and
   re-renders the layout, so the buttons come and go, and a handler attached to one of
   them would be attached to a button that is no longer there. Which one looks pressed is
   left to CSS, read from the attribute on <html> - one source of truth, and it cannot
   fall out of step with what is applied.

   The head of App.razor applies the saved choice before the first paint; this file loads
   too late for that, and a desk set to dark would flash white on every navigation.

   The choice must then survive everything that follows: routing between the two screens,
   a reload, a circuit dropping and reconnecting, and Blazor's enhanced navigation, which
   replaces the document and can take an attribute set from outside its render tree with
   it. Rather than guess which of those does it, the attribute is watched and put back
   whenever it stops matching what was chosen. Storage is the record; the attribute is
   just how it is applied. */
(function () {
    const KEY = 'vms-theme';
    const root = document.documentElement;

    function fromStorage() {
        try {
            const saved = localStorage.getItem(KEY);
            return (saved === 'light' || saved === 'dark') ? saved : null;
        } catch (e) {
            return null;
        }
    }

    /* The choice lives here, with storage as the durable copy rather than the only copy.
       Reading it back from storage on every check looked tidier and was wrong: where
       storage is unavailable - a locked-down browser, a private window - the write
       silently does nothing, the read returns null, and the switch clears the attribute
       it had just set. The control then appears to do nothing at all. In memory it still
       works for the session, which is the honest degradation. */
    let preference = fromStorage();

    function remember(choice) {
        preference = (choice === 'light' || choice === 'dark') ? choice : null;
        try {
            if (preference) { localStorage.setItem(KEY, preference); }
            else { localStorage.removeItem(KEY); }
        } catch (e) {
            /* It will not outlive the tab. Better than refusing to switch. */
        }
    }

    /** Puts the attribute back in step with the choice. A no-op when it already is, so
        the observer below cannot drive itself in a loop. */
    function enforce() {
        const have = root.getAttribute('data-theme');
        if (preference === have) { return; }

        if (preference) { root.setAttribute('data-theme', preference); }
        else { root.removeAttribute('data-theme'); }
    }

    function pressed() {
        const choice = preference || 'system';
        document.querySelectorAll('[data-theme-choice]').forEach(function (button) {
            button.setAttribute('aria-pressed', button.dataset.themeChoice === choice ? 'true' : 'false');
        });
    }

    document.addEventListener('click', function (event) {
        const button = event.target.closest ? event.target.closest('[data-theme-choice]') : null;
        if (!button) { return; }

        remember(button.dataset.themeChoice);
        enforce();
        pressed();
    });

    /* Whatever removes or changes the attribute - Blazor's enhanced navigation replacing
       the document, a re-render, anything - this puts it back. Filtered to the one
       attribute, so it is not watching the document generally. */
    new MutationObserver(function () { enforce(); }).observe(root, {
        attributes: true, attributeFilter: ['data-theme']
    });

    /* Enhanced navigation swaps the body, so the buttons are new elements and their
       aria-pressed is gone with the old ones. The attribute on <html> is handled by the
       observer; this is the half the observer cannot see. */
    document.addEventListener('enhancedload', function () { enforce(); pressed(); });

    // Back/forward cache: the head script does not run again.
    window.addEventListener('pageshow', function () { enforce(); pressed(); });

    /* CSS shows the state from the moment the markup exists; this is only for the screen
       reader, so it waits for Blazor to render the layout and then stops watching. */
    const waitForButtons = new MutationObserver(function () {
        if (document.querySelector('[data-theme-choice]')) {
            pressed();
            waitForButtons.disconnect();
        }
    });

    enforce();

    if (document.querySelector('[data-theme-choice]')) { pressed(); }
    else { waitForButtons.observe(document.body || root, { childList: true, subtree: true }); }
})();
