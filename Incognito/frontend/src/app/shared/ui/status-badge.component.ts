import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-status-badge',
  standalone: true,
  imports: [NgClass],
  template: `<span class="badge" [ngClass]="statusClass">{{ status }}</span>`,
  styleUrl: './status-badge.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class StatusBadgeComponent {
  @Input({ required: true }) status = '';

  get statusClass(): string {
    const value = this.status.toLowerCase();
    if (value === 'pending') return 'pending';
    if (value === 'underreview' || value === 'under review') return 'review';
    if (value === 'matched') return 'matched';
    if (value === 'withdrawn') return 'withdrawn';
    if (value === 'revealed') return 'revealed';
    return 'default';
  }
}
