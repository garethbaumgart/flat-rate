import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [ButtonModule],
  template: `
    <div class="flex flex-col items-center justify-center min-h-screen p-8">
      <h1 class="text-4xl font-bold text-gray-900 mb-4">FlatRate</h1>
      <p class="text-lg text-gray-600 mb-8">Monthly utility billing for rental properties</p>
      <p-button label="Get Started" icon="pi pi-arrow-right" iconPos="right" />
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HomePage {}
