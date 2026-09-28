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
  const storageKey = 'home-theme'
  const themes = ['system', 'light', 'dark']
  const root = document.documentElement
  const systemTheme = window.matchMedia('(prefers-color-scheme: dark)')
  // keep in sync with the page background colors of home.css
  const themeColors = { light: '#ffffff', dark: '#141d2f' }
  const themeColorMeta = document.querySelector('meta[name="theme-color"]')

  function readStoredTheme () {
    try {
      const stored = window.localStorage.getItem(storageKey)
      return themes.indexOf(stored) >= 0 ? stored : null
    } catch (error) {
      // private browsing modes can deny access to localStorage
      return null
    }
  }

  function resolveTheme (theme) {
    if (theme === 'light' || theme === 'dark') {
      return theme
    }

    return systemTheme.matches ? 'dark' : 'light'
  }

  function applyTheme (theme) {
    const resolved = resolveTheme(theme)
    root.setAttribute('data-theme', resolved)

    if (themeColorMeta) {
      themeColorMeta.setAttribute('content', themeColors[resolved])
    }

    const inputs = document.querySelectorAll('input[name="home-theme"]')
    for (let index = 0; index < inputs.length; index++) {
      inputs[index].checked = inputs[index].value === theme
    }
  }

  function storeTheme (theme) {
    try {
      window.localStorage.setItem(storageKey, theme)
    } catch (error) {
      // the theme is still applied for the current page, it just is not remembered
    }
  }

  // the default theme comes from the service configuration, the stored choice wins
  const configured = root.getAttribute('data-theme-default')
  let theme = readStoredTheme() || (themes.indexOf(configured) >= 0 ? configured : 'system')

  applyTheme(theme)

  document.addEventListener('change', function (event) {
    const input = event.target

    if (input && input.name === 'home-theme' && themes.indexOf(input.value) >= 0) {
      theme = input.value
      applyTheme(theme)
      storeTheme(theme)
    }
  })

  // follow the operating system while the system theme is selected
  function onSystemThemeChanged () {
    if (theme === 'system') {
      applyTheme(theme)
    }
  }

  if (typeof systemTheme.addEventListener === 'function') {
    systemTheme.addEventListener('change', onSystemThemeChanged)
  } else if (typeof systemTheme.addListener === 'function') {
    systemTheme.addListener(onSystemThemeChanged)
  }
})()
