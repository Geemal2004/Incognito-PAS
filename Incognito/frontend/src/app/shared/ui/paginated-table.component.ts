import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { NgFor } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';

export interface TableColumn {
  key: string;
  label: string;
}

@Component({
  selector: 'app-paginated-table',
  standalone: true,
  imports: [NgFor, MatButtonModule],
  template: `
    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            <th *ngFor="let column of columns">{{ column.label }}</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let row of rows">
            <td *ngFor="let column of columns">{{ row[column.key] }}</td>
          </tr>
        </tbody>
      </table>
      <footer *ngIf="totalPages > 1">
        <button mat-stroked-button (click)="goToPage(page - 1)" [disabled]="page <= 1">Previous</button>
        <span>Page {{ page }} of {{ totalPages }}</span>
        <button mat-stroked-button (click)="goToPage(page + 1)" [disabled]="page >= totalPages">Next</button>
      </footer>
    </div>
  `,
  styles: [
    '.table-wrap{background:#fff;border:1px solid var(--surface-border);border-radius:12px;overflow:hidden}',
    'table{width:100%;border-collapse:collapse}',
    'th,td{padding:.65rem;border-bottom:1px solid #eef2f8;text-align:left}',
    'thead{background:#f7faff}',
    'footer{display:flex;justify-content:flex-end;gap:.8rem;align-items:center;padding:.8rem}'
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PaginatedTableComponent {
  @Input() columns: TableColumn[] = [];
  @Input() rows: Record<string, unknown>[] = [];
  @Input() page = 1;
  @Input() totalPages = 1;
  @Output() pageChange = new EventEmitter<number>();

  goToPage(next: number): void {
    if (next < 1 || next > this.totalPages) {
      return;
    }

    this.pageChange.emit(next);
  }
}
