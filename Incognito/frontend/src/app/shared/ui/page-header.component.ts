import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { NgIf } from '@angular/common';

@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [NgIf],
  template: `
    <header class="page-header">
      <div class="page-header__copy">
        <h1>{{ title }}</h1>
        <p *ngIf="subtitle" class="page-header__subtitle">{{ subtitle }}</p>
      </div>
      <div class="page-header__actions">
        <ng-content select="[actions]"></ng-content>
      </div>
    </header>
  `,
  styleUrl: './page-header.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PageHeaderComponent {
  @Input({ required: true }) title = '';
  @Input() subtitle = '';
}
