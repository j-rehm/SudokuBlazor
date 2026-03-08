window.themeManager = {
  THEME_KEY: 'theme',
  _systemListeners: new WeakMap(),
  _transitionTimer: null,
  _transitionDuration: 1000,

  _normalizeTag(tag) {
    if (tag === null || tag === undefined) return null;
    try {
      const s = String(tag).trim();
      if (s.length === 0) return null;
      if (s.toLowerCase() === 'system') return null;
      return s;
    } catch {
      return null;
    }
  },

  getStoredTheme() {
    try {
      const raw = localStorage.getItem(this.THEME_KEY);
      return this._normalizeTag(raw);
    } catch {
      return null;
    }
  },

  _applyTransition() {
    try {
      if (this._transitionTimer) {
        clearTimeout(this._transitionTimer);
        this._transitionTimer = null;
      }
      document.documentElement.classList.add('theme-transition');
      this._transitionTimer = setTimeout(() => {
        try { document.documentElement.classList.remove('theme-transition'); } catch {}
        this._transitionTimer = null;
      }, this._transitionDuration);
    } catch {}
  },

  setTheme(tag) {
    const normalized = this._normalizeTag ? this._normalizeTag(tag) : (tag === 'system' ? null : tag);
    if (normalized === null) {
      try { localStorage.removeItem(this.THEME_KEY); } catch { }
      const sys = this.detectSystemTheme();
      try { document.documentElement.setAttribute('data-theme', sys); } catch { }
      this._updateMetaThemeColor();
      this._applyTransition();
      return;
    }

    try { document.documentElement.setAttribute('data-theme', normalized); } catch { }
    try { localStorage.setItem(this.THEME_KEY, normalized); } catch { }
    this._updateMetaThemeColor();
    this._applyTransition();
  },

  detectSystemTheme() {
    return (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) ? 'dark' : 'light';
  },

  getEffectiveTheme() {
    return this.getStoredTheme() || this.detectSystemTheme();
  },

  onSystemThemeChange(dotNetRef, methodName) {
    if (!window.matchMedia) return;
    const mq = window.matchMedia('(prefers-color-scheme: dark)');

    const handler = (e) => {
      if (this.getStoredTheme && this.getStoredTheme() !== null) return;
      const theme = e.matches ? 'dark' : 'light';

      try {
        document.documentElement.setAttribute('data-theme', theme);
        this._updateMetaThemeColor();
      } catch {}

      // animate the change
      this._applyTransition();

      try {
        if (dotNetRef && typeof dotNetRef.invokeMethodAsync === 'function' && typeof methodName === 'string') {
          dotNetRef.invokeMethodAsync(methodName, theme).catch(() => {});
        }
      } catch {}
    };

    if (mq.addEventListener) mq.addEventListener('change', handler);
    else mq.addListener(handler);

    const unregister = () => {
      try {
        if (mq.removeEventListener) mq.removeEventListener('change', handler);
        else mq.removeListener(handler);
      } catch {}
    };

    this._systemListeners.set(dotNetRef, unregister);
  },

  removeSystemThemeChangeCallback(dotNetRef) {
    const unregister = this._systemListeners.get(dotNetRef);
    if (unregister) {
      try { unregister(); } catch { }
      this._systemListeners.delete(dotNetRef);
    }
  },

  _updateMetaThemeColor() {
    const meta = document.querySelector('meta[name="theme-color"]');
    if (!meta) return;
    try {
      const color = getComputedStyle(document.documentElement).getPropertyValue('--color-bg') || '';
      meta.setAttribute('content', color.trim());
    } catch { }
  }
};