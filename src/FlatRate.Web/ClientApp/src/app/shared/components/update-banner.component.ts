import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { UpdateService } from '../../core/services/update.service';

@Component({
  selector: 'app-update-banner',
  standalone: true,
  template: `
    @if (updateService.updateAvailable()) {
      <div
        class="fixed bottom-0 left-0 right-0 z-50 border-t flex items-center justify-between px-4 sm:px-6 py-3"
        style="background: var(--color-info-bg); border-color: var(--color-info);"
        role="status"
      >
        <div class="flex items-center gap-3">
          <i class="pi pi-refresh" style="color: var(--color-info);"></i>
          <span class="text-sm" style="color: var(--color-text-primary);">A new version is available.</span>
        </div>
        <div class="flex items-center gap-2">
          <button
            (click)="updateService.activateAndReload()"
            class="px-3 py-1.5 rounded-md text-sm font-medium text-white"
            style="background: var(--color-info);"
            aria-label="Reload to update"
          >
            Reload
          </button>
          <button
            (click)="updateService.dismiss()"
            class="p-1.5 rounded-md transition-colors"
            style="color: var(--color-text-muted);"
            aria-label="Dismiss update notification"
          >
            <i class="pi pi-times"></i>
          </button>
        </div>
      </div>
    }
  `,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class UpdateBannerComponent {
  readonly updateService = inject(UpdateService);
}
