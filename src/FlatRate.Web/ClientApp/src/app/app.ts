import { Component, ChangeDetectionStrategy, inject, OnInit } from '@angular/core';
import { LayoutComponent } from './shared/components/layout.component';
import { ThemeService } from './core/services/theme.service';

@Component({
  selector: 'app-root',
  imports: [LayoutComponent],
  template: `<app-layout />`,
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class App implements OnInit {
  private readonly themeService = inject(ThemeService);

  ngOnInit(): void {
    // ThemeService initializes theme on construction
  }
}
