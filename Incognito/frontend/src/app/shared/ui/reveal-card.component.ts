import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { DatePipe, NgIf } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { RevealDetailsDto } from '../../core/models/api.models';

@Component({
  selector: 'app-reveal-card',
  standalone: true,
  imports: [NgIf, DatePipe, MatCardModule, MatIconModule],
  template: `
    <mat-card class="reveal-card" *ngIf="details">
      <div class="reveal-card__header">
        <div class="reveal-card__icon">
          <mat-icon>verified</mat-icon>
        </div>
        <div>
          <p class="ui-eyebrow">Reveal Completed</p>
          <h3>Supervisor Match Confirmed</h3>
          <p class="reveal-card__subtitle">Identity is now visible because this blind match was formally confirmed.</p>
        </div>
      </div>

      <dl class="reveal-card__grid">
        <div>
          <dt>Supervisor</dt>
          <dd>{{ details.supervisorName }}</dd>
        </div>
        <div>
          <dt>Email</dt>
          <dd>{{ details.supervisorEmail }}</dd>
        </div>
        <div *ngIf="details.confirmedAt">
          <dt>Confirmed</dt>
          <dd>{{ details.confirmedAt | date:'medium' }}</dd>
        </div>
      </dl>
    </mat-card>
  `,
  styleUrl: './reveal-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class RevealCardComponent {
  @Input() details: RevealDetailsDto | undefined;
}
