import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { AuthService } from '../core/services/auth.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [ButtonModule, RouterLink],
  template: `
    <div class="min-h-[calc(100vh-10rem)]">
      <!-- Hero Section -->
      <section class="relative overflow-hidden py-16 md:py-24">
        <!-- Background gradient -->
        <div
          class="absolute inset-0 opacity-30"
          style="background: radial-gradient(ellipse at top, var(--color-accent-bg) 0%, transparent 50%);"
        ></div>

        <div class="relative max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div class="text-center">
            <!-- Logo Icon -->
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 80 80" class="w-20 h-20 mb-8 inline-block" style="filter: drop-shadow(0 10px 15px rgba(0,0,0,0.1));" role="img" aria-label="FlatRate logo">
              <defs>
                <clipPath id="heroLogo"><rect width="80" height="80" rx="18"/></clipPath>
                <filter id="heroGlow"><feDropShadow dx="0" dy="1.5" stdDeviation="1.5" flood-color="#2e3440" flood-opacity="0.2"/></filter>
              </defs>
              <g clip-path="url(#heroLogo)">
                <rect width="80" height="80" fill="#2e3440"/>
                <path d="M12,40 L22,40 L26,32 L30,48 L34,32 L38,48 L42,40 L52,40" fill="none" stroke="#ebcb8b" stroke-width="7" stroke-linecap="round" stroke-linejoin="round" opacity=".15"/>
                <path d="M12,40 L22,40 L26,32 L30,48 L34,32 L38,48 L42,40 L52,40" fill="none" stroke="#ebcb8b" stroke-width="3" stroke-linecap="round" stroke-linejoin="round" filter="url(#heroGlow)"/>
                <polygon points="52,30 68,40 52,50" fill="#ebcb8b"/>
              </g>
            </svg>

            <h1 class="text-4xl md:text-5xl lg:text-6xl font-bold mb-6" style="color: var(--color-text-primary);">
              Utility Billing
              <span class="block" style="color: var(--color-accent);">Made Simple</span>
            </h1>

            <p class="text-lg md:text-xl max-w-2xl mx-auto mb-10" style="color: var(--color-text-secondary);">
              Generate professional utility bills for your rental properties with tiered water and electricity rates.
            </p>

            <!-- CTA Buttons -->
            <div class="flex flex-col sm:flex-row justify-center gap-4">
              @if (authService.isAuthenticated()) {
                <p-button
                  label="Create Bill"
                  icon="pi pi-plus"
                  size="large"
                  routerLink="/bills/create"
                  styleClass="px-8"
                />
                <p-button
                  label="Manage Properties"
                  icon="pi pi-building"
                  severity="secondary"
                  size="large"
                  [outlined]="true"
                  routerLink="/properties"
                  styleClass="px-8"
                />
              } @else {
                <p-button
                  label="Sign in to Get Started"
                  icon="pi pi-google"
                  size="large"
                  (onClick)="authService.login()"
                  styleClass="px-8"
                />
              }
            </div>
          </div>
        </div>
      </section>

      <!-- Features Section -->
      <section class="py-16" style="background: var(--color-bg-secondary);">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <h2 class="text-2xl md:text-3xl font-bold text-center mb-12" style="color: var(--color-text-primary);">
            Everything You Need
          </h2>

          <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
            <!-- Feature 1 -->
            <div class="glass-card p-6 text-center">
              <div
                class="w-14 h-14 rounded-xl flex items-center justify-center mx-auto mb-4"
                style="background: var(--color-accent-bg);"
              >
                <i class="pi pi-building text-2xl" style="color: var(--color-accent);"></i>
              </div>
              <h3 class="text-lg font-semibold mb-2" style="color: var(--color-text-primary);">
                Property Management
              </h3>
              <p style="color: var(--color-text-secondary);">
                Store property details and default rates for quick bill generation.
              </p>
            </div>

            <!-- Feature 2 -->
            <div class="glass-card p-6 text-center">
              <div
                class="w-14 h-14 rounded-xl flex items-center justify-center mx-auto mb-4"
                style="background: var(--color-accent-bg);"
              >
                <i class="pi pi-chart-bar text-2xl" style="color: var(--color-accent);"></i>
              </div>
              <h3 class="text-lg font-semibold mb-2" style="color: var(--color-text-primary);">
                Tiered Billing
              </h3>
              <p style="color: var(--color-text-secondary);">
                Support for tiered water and sanitation rates following municipal structures.
              </p>
            </div>

            <!-- Feature 3 -->
            <div class="glass-card p-6 text-center">
              <div
                class="w-14 h-14 rounded-xl flex items-center justify-center mx-auto mb-4"
                style="background: var(--color-accent-bg);"
              >
                <i class="pi pi-file-pdf text-2xl" style="color: var(--color-accent);"></i>
              </div>
              <h3 class="text-lg font-semibold mb-2" style="color: var(--color-text-primary);">
                PDF Export
              </h3>
              <p style="color: var(--color-text-secondary);">
                Generate professional PDF invoices ready to share with tenants.
              </p>
            </div>
          </div>
        </div>
      </section>

      <!-- Quick Stats Section -->
      <section class="py-16">
        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
            <div
              class="p-6 rounded-xl border text-center"
              style="background: var(--color-bg-card); border-color: var(--color-border);"
            >
              <div class="text-3xl font-bold mb-1" style="color: var(--color-accent);">3</div>
              <div style="color: var(--color-text-secondary);">Tier Water Rates</div>
            </div>
            <div
              class="p-6 rounded-xl border text-center"
              style="background: var(--color-bg-card); border-color: var(--color-border);"
            >
              <div class="text-3xl font-bold mb-1" style="color: var(--color-accent);">15%</div>
              <div style="color: var(--color-text-secondary);">VAT Calculated</div>
            </div>
            <div
              class="p-6 rounded-xl border text-center"
              style="background: var(--color-bg-card); border-color: var(--color-border);"
            >
              <div class="text-3xl font-bold mb-1" style="color: var(--color-accent);">ZAR</div>
              <div style="color: var(--color-text-secondary);">South African Rand</div>
            </div>
          </div>
        </div>
      </section>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomePage {
  readonly authService = inject(AuthService);
}
