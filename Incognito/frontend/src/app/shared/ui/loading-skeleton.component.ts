import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { NgFor } from '@angular/common';

@Component({
  selector: 'app-loading-skeleton',
  standalone: true,
  imports: [NgFor],
  template: `
    <div class="skeleton-wrap">
      <div class="skeleton" *ngFor="let row of rowsArray"></div>
    </div>
  `,
  styleUrl: './loading-skeleton.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoadingSkeletonComponent {
  @Input() rows = 4;

  get rowsArray(): number[] {
    return Array.from({ length: this.rows }, (_, i) => i);
  }
}
