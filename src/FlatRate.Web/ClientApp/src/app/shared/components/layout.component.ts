import { Component, ChangeDetectionStrategy, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { ThemeService } from '../../core/services/theme.service';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, ButtonModule],
  template: `
    <div class="min-h-screen flex flex-col" style="background-color: var(--color-bg-primary);">
      <!-- Navigation Header -->
      <header class="sticky top-0 z-50 border-b" style="background: var(--glass-bg); backdrop-filter: blur(10px); border-color: var(--color-border);">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div class="flex justify-between items-center h-16">
            <!-- Logo -->
            <a routerLink="/" class="flex items-center gap-3 no-underline">
              <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 80 80" class="w-10 h-10" role="img" aria-label="FlatRate logo">
                <defs>
                  <clipPath id="navLogo"><rect width="80" height="80" rx="18"/></clipPath>
                  <filter id="navGlow"><feDropShadow dx="0" dy="1.5" stdDeviation="1.5" flood-color="#2e3440" flood-opacity="0.2"/></filter>
                </defs>
                <g clip-path="url(#navLogo)">
                  <rect width="80" height="80" fill="#2e3440"/>
                  <path d="M12,40 L22,40 L26,32 L30,48 L34,32 L38,48 L42,40 L52,40" fill="none" stroke="#ebcb8b" stroke-width="7" stroke-linecap="round" stroke-linejoin="round" opacity=".15"/>
                  <path d="M12,40 L22,40 L26,32 L30,48 L34,32 L38,48 L42,40 L52,40" fill="none" stroke="#ebcb8b" stroke-width="3" stroke-linecap="round" stroke-linejoin="round" filter="url(#navGlow)"/>
                  <polygon points="52,30 68,40 52,50" fill="#ebcb8b"/>
                </g>
              </svg>
              <span class="text-xl font-bold" style="color: var(--color-text-primary);">FlatRate</span>
            </a>

            <!-- Navigation Links -->
            <nav class="desktop-nav">
              <a
                routerLink="/"
                routerLinkActive="active-nav-link"
                [routerLinkActiveOptions]="{ exact: true }"
                class="nav-link"
              >
                <i class="pi pi-home mr-2"></i>
                Home
              </a>
              @if (authService.isAuthenticated()) {
                <a
                  routerLink="/properties"
                  routerLinkActive="active-nav-link"
                  class="nav-link"
                >
                  <i class="pi pi-building mr-2"></i>
                  Properties
                </a>
                <a
                  routerLink="/bills"
                  routerLinkActive="active-nav-link"
                  [routerLinkActiveOptions]="{ exact: true }"
                  class="nav-link"
                >
                  <i class="pi pi-file mr-2"></i>
                  Bills
                </a>
                <a
                  routerLink="/bills/create"
                  routerLinkActive="active-nav-link"
                  class="nav-link"
                >
                  <i class="pi pi-plus mr-2"></i>
                  Create Bill
                </a>
              }
            </nav>

            <!-- User Menu, Theme Toggle & Mobile Menu -->
            <div class="flex items-center gap-2">
              <!-- Auth Section -->
              @if (authService.loading()) {
                <div class="w-8 h-8 rounded-full animate-pulse" style="background: var(--color-bg-tertiary);"></div>
              } @else if (authService.isAuthenticated()) {
                <div class="flex items-center gap-3">
                  <div class="hidden sm:flex items-center gap-2">
                    <div class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-semibold text-white gradient-accent">
                      {{ getInitials(authService.user()?.name) }}
                    </div>
                    <span class="text-sm font-medium" style="color: var(--color-text-primary);">
                      {{ authService.user()?.name }}
                    </span>
                  </div>
                  <button
                    (click)="authService.logout()"
                    class="p-2 rounded-lg transition-colors hover:bg-[var(--color-error-bg)]"
                    style="color: var(--color-text-secondary);"
                    title="Sign out"
                    aria-label="Sign out"
                  >
                    <i class="pi pi-sign-out text-lg"></i>
                  </button>
                </div>
              } @else {
                <button
                  (click)="authService.login()"
                  class="flex items-center gap-2 px-4 py-2 rounded-lg font-medium text-sm text-white gradient-accent hover:opacity-90 transition-opacity"
                >
                  <i class="pi pi-google"></i>
                  <span class="hidden sm:inline">Sign in</span>
                </button>
              }

              <button
                (click)="themeService.toggleTheme()"
                class="p-2 rounded-lg transition-colors"
                style="color: var(--color-text-secondary);"
                [attr.aria-label]="themeService.isDark() ? 'Switch to light mode' : 'Switch to dark mode'"
              >
                @if (themeService.isDark()) {
                  <i class="pi pi-sun text-lg"></i>
                } @else {
                  <i class="pi pi-moon text-lg"></i>
                }
              </button>

              <!-- Mobile menu button -->
              <button
                class="mobile-menu-btn p-2 rounded-lg"
                style="color: var(--color-text-secondary);"
                (click)="toggleMobileMenu()"
                aria-label="Toggle mobile menu"
              >
                <i class="pi" [class.pi-bars]="!mobileMenuOpen()" [class.pi-times]="mobileMenuOpen()"></i>
              </button>
            </div>
          </div>
        </div>

        <!-- Mobile Navigation -->
        @if (mobileMenuOpen()) {
          <div class="mobile-nav-container border-t" style="border-color: var(--color-border); background: var(--glass-bg);">
            <nav class="px-4 py-3 flex flex-col gap-1">
              @if (authService.isAuthenticated()) {
                <!-- User info in mobile -->
                <div class="flex items-center gap-3 px-3 py-3 mb-2 rounded-lg" style="background: var(--color-accent-bg);">
                  <div class="w-10 h-10 rounded-full flex items-center justify-center text-sm font-semibold text-white gradient-accent">
                    {{ getInitials(authService.user()?.name) }}
                  </div>
                  <div>
                    <div class="font-medium" style="color: var(--color-text-primary);">{{ authService.user()?.name }}</div>
                    <div class="text-xs" style="color: var(--color-text-muted);">{{ authService.user()?.email }}</div>
                  </div>
                </div>
              }

              <a
                routerLink="/"
                routerLinkActive="active-nav-link-mobile"
                [routerLinkActiveOptions]="{ exact: true }"
                class="mobile-nav-link"
                (click)="mobileMenuOpen.set(false)"
              >
                <i class="pi pi-home mr-3"></i>
                Home
              </a>

              @if (authService.isAuthenticated()) {
                <a
                  routerLink="/properties"
                  routerLinkActive="active-nav-link-mobile"
                  class="mobile-nav-link"
                  (click)="mobileMenuOpen.set(false)"
                >
                  <i class="pi pi-building mr-3"></i>
                  Properties
                </a>
                <a
                  routerLink="/bills"
                  routerLinkActive="active-nav-link-mobile"
                  [routerLinkActiveOptions]="{ exact: true }"
                  class="mobile-nav-link"
                  (click)="mobileMenuOpen.set(false)"
                >
                  <i class="pi pi-file mr-3"></i>
                  Bills
                </a>
                <a
                  routerLink="/bills/create"
                  routerLinkActive="active-nav-link-mobile"
                  class="mobile-nav-link"
                  (click)="mobileMenuOpen.set(false)"
                >
                  <i class="pi pi-plus mr-3"></i>
                  Create Bill
                </a>

                <div class="border-t my-2" style="border-color: var(--color-border);"></div>

                <button
                  (click)="authService.logout(); mobileMenuOpen.set(false)"
                  class="mobile-nav-link text-left w-full"
                  style="color: var(--color-error);"
                >
                  <i class="pi pi-sign-out mr-3"></i>
                  Sign out
                </button>
              } @else {
                <button
                  (click)="authService.login(); mobileMenuOpen.set(false)"
                  class="mobile-nav-link text-left w-full"
                  style="color: var(--color-accent);"
                >
                  <i class="pi pi-google mr-3"></i>
                  Sign in with Google
                </button>
              }
            </nav>
          </div>
        }
      </header>

      <!-- Main Content -->
      <main class="flex-1">
        <router-outlet />
      </main>

      <!-- Footer -->
      <footer class="border-t py-6" style="border-color: var(--color-border); background: var(--color-bg-secondary);">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div class="flex flex-col md:flex-row justify-between items-center gap-4">
            <p class="text-sm" style="color: var(--color-text-muted);">
              FlatRate - Utility billing for rental properties
            </p>
            <p class="text-sm" style="color: var(--color-text-muted);">
              Built with Angular & .NET
            </p>
          </div>
        </div>
      </footer>
    </div>
  `,
  styles: [`
    :host {
      display: block;
    }

    .desktop-nav {
      display: none;
    }

    @media (min-width: 768px) {
      .desktop-nav {
        display: flex;
        align-items: center;
        gap: 0.25rem;
      }
    }

    .mobile-menu-btn {
      display: block;
    }

    @media (min-width: 768px) {
      .mobile-menu-btn {
        display: none;
      }
    }

    .mobile-nav-container {
      display: block;
    }

    @media (min-width: 768px) {
      .mobile-nav-container {
        display: none;
      }
    }

    .nav-link {
      display: flex;
      align-items: center;
      padding: 0.5rem 1rem;
      border-radius: 0.5rem;
      font-weight: 500;
      font-size: 0.875rem;
      color: var(--color-text-secondary);
      text-decoration: none;
      transition: all 0.2s ease;
    }

    .nav-link:hover {
      background: var(--color-accent-bg);
      color: var(--color-accent);
    }

    .active-nav-link {
      background: var(--color-accent-bg) !important;
      color: var(--color-accent) !important;
    }

    .mobile-nav-link {
      display: flex;
      align-items: center;
      padding: 0.75rem 1rem;
      border-radius: 0.5rem;
      font-weight: 500;
      color: var(--color-text-secondary);
      text-decoration: none;
      transition: all 0.2s ease;
      background: transparent;
      border: none;
      cursor: pointer;
    }

    .mobile-nav-link:hover {
      background: var(--color-accent-bg);
      color: var(--color-accent);
    }

    .active-nav-link-mobile {
      background: var(--color-accent-bg) !important;
      color: var(--color-accent) !important;
    }

    @keyframes pulse {
      0%, 100% { opacity: 1; }
      50% { opacity: 0.5; }
    }

    .animate-pulse {
      animation: pulse 2s cubic-bezier(0.4, 0, 0.6, 1) infinite;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LayoutComponent {
  readonly themeService = inject(ThemeService);
  readonly authService = inject(AuthService);
  readonly mobileMenuOpen = signal(false);

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update(v => !v);
  }

  getInitials(name: string | undefined): string {
    if (!name) return '?';
    return name
      .split(' ')
      .map(part => part[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  }
}
