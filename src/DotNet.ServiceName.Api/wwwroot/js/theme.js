/*
 * Theme switch of the home page.
 *
 * The page supports three values: the system theme, light and dark. The choice is kept in
 * localStorage, applied as `data-theme` on the root element and - while the system theme is
 * selected - kept in sync with the operating system when it changes.
 *
 * The script is loaded synchronously in the head of the page, before the first paint, so the page
 * never flashes in the wrong theme. It is a separate file because the security headers policy
 * (default-src 'self') does not allow inline scripts outside of local development.
 */
(function () {
    'use strict';

    var storageKey = 'home-theme';
    var themes = ['system', 'light', 'dark'];
    var root = document.documentElement;
    var query = window.matchMedia('(prefers-color-scheme: dark)');

    function readStoredTheme() {
        try {
            var stored = window.localStorage.getItem(storageKey);
            return themes.indexOf(stored) >= 0 ? stored : null;
        } catch (error) {
            // private browsing modes can deny access to localStorage
            return null;
        }
    }

    function resolveTheme(theme) {
        if (theme === 'light' || theme === 'dark') {
            return theme;
        }

        return query.matches ? 'dark' : 'light';
    }

    function applyTheme(theme) {
        root.setAttribute('data-theme', resolveTheme(theme));

        var inputs = document.querySelectorAll('input[name="home-theme"]');
        for (var index = 0; index < inputs.length; index++) {
            inputs[index].checked = inputs[index].value === theme;
        }
    }

    function storeTheme(theme) {
        try {
            window.localStorage.setItem(storageKey, theme);
        } catch (error) {
            // the theme is still applied for the current page, it just is not remembered
        }
    }

    // the default theme comes from the service configuration, the stored choice wins
    var configured = root.getAttribute('data-theme-default');
    var theme = readStoredTheme() || (themes.indexOf(configured) >= 0 ? configured : 'system');

    applyTheme(theme);

    document.addEventListener('change', function (event) {
        var input = event.target;

        if (input && input.name === 'home-theme' && themes.indexOf(input.value) >= 0) {
            theme = input.value;
            applyTheme(theme);
            storeTheme(theme);
        }
    });

    // follow the operating system while the system theme is selected
    var onSystemThemeChanged = function () {
        if (theme === 'system') {
            applyTheme(theme);
        }
    };

    if (typeof query.addEventListener === 'function') {
        query.addEventListener('change', onSystemThemeChanged);
    } else if (typeof query.addListener === 'function') {
        query.addListener(onSystemThemeChanged);
    }
})();
