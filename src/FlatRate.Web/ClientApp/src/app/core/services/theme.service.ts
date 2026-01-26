import { Injectable, signal, effect, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export type Theme = 'light' | 'dark' | 'system';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly platformId = inject(PLATFORM_ID);
  private mediaQuery: MediaQueryList | null = null;

  readonly theme = signal<Theme>('system');
  readonly isDark = signal<boolean>(false);

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      this.mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

      // Load saved theme preference
      const savedTheme = localStorage.getItem('theme') as Theme | null;
      if (savedTheme && ['light', 'dark', 'system'].includes(savedTheme)) {
        this.theme.set(savedTheme);
      }

      // Listen for system theme changes
      this.mediaQuery.addEventListener('change', (e) => {
        if (this.theme() === 'system') {
          this.applyTheme(e.matches);
        }
      });

      // Apply initial theme
      effect(() => {
        const currentTheme = this.theme();
        this.updateTheme(currentTheme);
      });
    }
  }

  setTheme(theme: Theme): void {
    this.theme.set(theme);
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem('theme', theme);
    }
  }

  toggleTheme(): void {
    const current = this.theme();
    if (current === 'system') {
      this.setTheme(this.isDark() ? 'light' : 'dark');
    } else if (current === 'dark') {
      this.setTheme('light');
    } else {
      this.setTheme('dark');
    }
  }

  private updateTheme(theme: Theme): void {
    if (!isPlatformBrowser(this.platformId)) return;

    let shouldBeDark: boolean;

    if (theme === 'system') {
      shouldBeDark = this.mediaQuery?.matches ?? false;
    } else {
      shouldBeDark = theme === 'dark';
    }

    this.applyTheme(shouldBeDark);
  }

  private applyTheme(isDark: boolean): void {
    this.isDark.set(isDark);

    if (isDark) {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }
  }
}
