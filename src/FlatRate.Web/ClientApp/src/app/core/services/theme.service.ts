import { Injectable, signal, effect, PLATFORM_ID, inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class ThemeService {
  private readonly platformId = inject(PLATFORM_ID);

  readonly isDark = signal<boolean>(false);

  constructor() {
    if (isPlatformBrowser(this.platformId)) {
      const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)');

      // Set initial value from system preference
      this.isDark.set(mediaQuery.matches);

      // Listen for system theme changes
      mediaQuery.addEventListener('change', (e) => {
        this.isDark.set(e.matches);
      });

      // Apply theme whenever isDark changes
      effect(() => {
        this.applyTheme(this.isDark());
      });
    }
  }

  private applyTheme(isDark: boolean): void {
    if (isDark) {
      document.documentElement.classList.add('dark');
    } else {
      document.documentElement.classList.remove('dark');
    }

    // Update meta theme-color dynamically
    const metaThemeColor = document.querySelector('meta[name="theme-color"]');
    if (metaThemeColor) {
      metaThemeColor.setAttribute('content', isDark ? '#242933' : '#eceff4');
    }
  }
}
