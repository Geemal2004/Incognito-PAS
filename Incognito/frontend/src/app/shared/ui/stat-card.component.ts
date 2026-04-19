import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component, Input } from '@angular/core';

@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <article class="stat-card" [class.tone-pending]="tone === 'pending'" [class.tone-review]="tone === 'review'" [class.tone-matched]="tone === 'matched'" [class.tone-revealed]="tone === 'revealed'" [class.tone-danger]="tone === 'danger'">
      <p class="label">{{ label }}</p>
      <h3>{{ value }}</h3>
      <small *ngIf="hint">{{ hint }}</small>
    </article>
  `,
  styleUrl: './stat-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StatCardComponent {
  @Input({ required: true }) label = '';
  @Input({ required: true }) value: string | number = 0;
  @Input() hint = '';
  @Input() tone: 'default' | 'pending' | 'review' | 'matched' | 'revealed' | 'danger' = 'default';
}
