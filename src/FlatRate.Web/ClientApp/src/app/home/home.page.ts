import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [ButtonModule, RouterLink],
  template: `
    <div class="flex flex-col items-center justify-center min-h-screen p-8">
      <h1 class="text-4xl font-bold text-gray-900 mb-4">FlatRate</h1>
      <p class="text-lg text-gray-600 mb-8">Monthly utility billing for rental properties</p>
      <div class="flex flex-wrap justify-center gap-4">
        <p-button
          label="Manage Properties"
          icon="pi pi-building"
          routerLink="/properties"
        />
        <p-button
          label="View Bills"
          icon="pi pi-file"
          severity="secondary"
          routerLink="/bills"
        />
        <p-button
          label="Create Bill"
          icon="pi pi-plus"
          severity="success"
          routerLink="/bills/create"
        />
      </div>
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomePage {}
