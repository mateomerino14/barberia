const modeKey = 'barberia.theme';
const paletteKey = 'barberia.palette';
const darkQuery = '(prefers-color-scheme: dark)';

/** Cómo elige la persona el modo de color. */
export type ThemeMode = 'auto' | 'light' | 'dark';

/** Paleta de la barbería tal como la manda la API. */
export type Palette = 'classic' | 'bottleGreen' | 'wine' | 'mustard' | 'graphite';

/** Paletas en el orden en que se muestran al dueño. */
export const palettes: readonly Palette[] = ['classic', 'bottleGreen', 'wine', 'mustard', 'graphite'];

/** Lee un valor guardado en el navegador, o null si no se puede. */
function readStorage(key: string): string | null {
  try {
    return localStorage.getItem(key);
  }
  catch {
    return null;
  }
}

/** Guarda un valor en el navegador si se puede. */
function writeStorage(key: string, value: string): void {
  try {
    localStorage.setItem(key, value);
  }
  catch {
    // Sin almacenamiento el modo dura solo mientras la página esté abierta.
  }
}

/** Modo de color y paleta de la página, guardados en el navegador de cada persona. */
class Theme {
  mode = $state<ThemeMode>('auto');
  systemDark = $state(false);

  /** Indica si la página se ve en oscuro ahora. */
  get isDark(): boolean {
    if (this.mode === 'auto') {
      return this.systemDark;
    }
    return this.mode === 'dark';
  }

  /** Lee lo guardado, sigue los cambios del sistema y aplica el modo. */
  init(): void {
    const saved = readStorage(modeKey);
    if (saved === 'light' || saved === 'dark') {
      this.mode = saved;
    }
    const query = matchMedia(darkQuery);
    this.systemDark = query.matches;
    query.addEventListener('change', (event) => {
      this.systemDark = event.matches;
      this.apply();
    });
    this.apply();
  }

  /** Pasa de claro a oscuro o al revés, y lo recuerda. */
  toggle(): void {
    if (this.isDark) {
      this.mode = 'light';
    }
    else {
      this.mode = 'dark';
    }
    writeStorage(modeKey, this.mode);
    this.apply();
  }

  /** Aplica la paleta de la barbería y la recuerda para la próxima visita. */
  setPalette(palette: Palette): void {
    document.documentElement.dataset.palette = palette;
    writeStorage(paletteKey, palette);
  }

  /** Marca el modo en la página para que los tokens cambien. */
  private apply(): void {
    if (this.isDark) {
      document.documentElement.dataset.theme = 'dark';
    }
    else {
      document.documentElement.dataset.theme = 'light';
    }
  }
}

export const theme = new Theme();
