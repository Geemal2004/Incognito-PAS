import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { StatusBadgeComponent } from './status-badge.component';

@Component({
  selector: 'app-proposal-card',
  standalone: true,
  imports: [NgFor, NgIf, MatCardModule, MatButtonModule, MatIconModule, StatusBadgeComponent],
  template: `
    <mat-card class="proposal-card">
      <div class="proposal-card__header">
        <div class="proposal-card__title-wrap">
          <h3>{{ title }}</h3>
          <p *ngIf="meta" class="proposal-card__meta">{{ meta }}</p>
        </div>
        <app-status-badge [status]="status"></app-status-badge>
      </div>

      <p class="proposal-card__abstract" [class.proposal-card__abstract--expanded]="showFullDetails">{{ abstract }}</p>

      <div class="proposal-card__details">
        <span class="proposal-card__detail">
          <mat-icon>category</mat-icon>
          {{ researchArea }}
        </span>
      </div>

      <div class="proposal-card__chips" *ngIf="techChips.length > 0">
        <span class="proposal-card__chip" *ngFor="let chip of techChips">{{ chip }}</span>
      </div>

      <div class="proposal-card__actions">
        <ng-content select="[actions]"></ng-content>
        <button *ngIf="showPrimary" mat-flat-button class="btn-primary" (click)="primaryClick.emit()">{{ primaryLabel }}</button>
      </div>
    </mat-card>
  `,
  styleUrl: './proposal-card.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProposalCardComponent {
  @Input({ required: true }) title = '';
  @Input({ required: true }) abstract = '';
  @Input({ required: true }) researchArea = '';
  @Input() techStack = '';
  @Input() meta = '';
  @Input() status = '';
  @Input() primaryLabel = 'Open';
  @Input() showPrimary = false;
  @Input() showFullDetails = false;
  @Output() primaryClick = new EventEmitter<void>();

  get techChips(): string[] {
    if (!this.techStack.trim()) {
      return [];
    }

    return this.techStack
      .split(',')
      .map((value) => value.trim())
      .filter((value) => value.length > 0)
      .slice(0, this.showFullDetails ? Number.MAX_SAFE_INTEGER : 6);
  }
}
