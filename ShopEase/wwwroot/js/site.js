(() => {
    'use strict';
    const root = document.documentElement;
    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    const toggle = document.getElementById('motion-toggle');
    let paused = false;
    try { paused = localStorage.getItem('void-motion') === 'off'; } catch { /* storage can be unavailable */ }
    function applyMotion() {
        const off = paused || reducedMotion.matches;
        root.dataset.motion = off ? 'off' : 'on';
        if (toggle) {
            toggle.setAttribute('aria-pressed', String(off));
            toggle.disabled = reducedMotion.matches;
            const label = reducedMotion.matches ? 'Анимации отключены настройками системы' : (off ? 'Включить анимации' : 'Приостановить анимации');
            toggle.setAttribute('aria-label', label);
            toggle.title = label;
        }
    }
    toggle?.addEventListener('click', () => {
        paused = !paused;
        try { localStorage.setItem('void-motion', paused ? 'off' : 'on'); } catch { }
        applyMotion();
    });
    reducedMotion.addEventListener('change', applyMotion);
    applyMotion();
    // Native cross-document transitions progressively enhance ordinary MVC navigation.
    const nativeTransitions = 'onpageswap' in window && window.CSS?.supports('view-transition-name', 'root');
    if (!nativeTransitions && root.dataset.motion !== 'off') document.body.classList.add('page-entering');
    window.addEventListener('pageshow', () => document.body.classList.remove('page-leaving'));
    if (!nativeTransitions) {
        let navigating = false;
        document.addEventListener('click', event => {
            if (event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey || root.dataset.motion === 'off') return;
            const link = event.target instanceof Element ? event.target.closest('a[href]') : null;
            if (!link || link.hasAttribute('download') || link.target || link.dataset.bsToggle) return;
            const url = new URL(link.href, location.href);
            if (url.origin !== location.origin || !/^https?:$/.test(url.protocol) || url.hash || url.href === location.href) return;
            if (navigating) { event.preventDefault(); return; }
            event.preventDefault();
            navigating = true;
            document.body.classList.remove('page-entering');
            document.body.classList.add('page-leaving');
            setTimeout(() => location.assign(url.href), 150);
        });
    }
})();
