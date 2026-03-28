window.themeInterop = {
    getThemePreference: () => localStorage.getItem('theme'),
    getSystemDarkMode: () => window.matchMedia('(prefers-color-scheme: dark)').matches,
    setThemePreference: (isDark) => localStorage.setItem('theme', isDark ? 'dark' : 'light')
};
