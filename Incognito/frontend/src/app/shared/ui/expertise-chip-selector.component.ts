import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { NgFor } from '@angular/common';
import { MatChipsModule } from '@angular/material/chips';
import { ResearchAreaDto } from '../../core/models/api.models';

@Component({
  selector: 'app-expertise-chip-selector',
  standalone: true,
  imports: [NgFor, MatChipsModule],
  template: `
    <mat-chip-set class="expertise-chip-set" aria-label="Research area expertise selector">
      <mat-chip *ngFor="let area of areas"
        class="expertise-chip"
        [disableRipple]="true"
        [class.selected]="selectedIds.includes(area.id)"
        (click)="toggle(area.id)"
        (keydown.enter)="toggle(area.id)"
        (keydown.space)="toggle(area.id); $event.preventDefault()"
        tabindex="0">
        {{ area.name }}
      </mat-chip>
    </mat-chip-set>
  `,
  styleUrls: ['./expertise-chip-selector.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ExpertiseChipSelectorComponent {
  @Input() areas: ResearchAreaDto[] = [];
  @Input() selectedIds: number[] = [];
  @Output() selectedIdsChange = new EventEmitter<number[]>();

  toggle(id: number): void {
    const next = this.selectedIds.includes(id)
      ? this.selectedIds.filter((value) => value !== id)
      : [...this.selectedIds, id];

    this.selectedIdsChange.emit(next);
  }
}
