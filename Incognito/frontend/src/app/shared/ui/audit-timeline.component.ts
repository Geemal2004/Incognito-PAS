import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { DatePipe, NgFor, NgIf } from '@angular/common';
import { AuditLogDto } from '../../core/models/api.models';

@Component({
  selector: 'app-audit-timeline',
  standalone: true,
  imports: [NgIf, NgFor, DatePipe],
  template: `
    <div class="audit-timeline" *ngIf="logs.length > 0; else noLogs">
      <article *ngFor="let log of logs" class="audit-item">
        <h4>{{ log.action }}</h4>
        <p>{{ log.entityType }} #{{ log.entityId }}</p>
        <small>{{ log.timestamp | date:'medium' }}<span *ngIf="log.actorUserId"> • {{ log.actorUserId }}</span></small>
      </article>
    </div>
    <ng-template #noLogs>
      <p class="no-logs">No audit entries available.</p>
    </ng-template>
  `,
  styles: [
    '.audit-timeline{display:grid;gap:.6rem}',
    '.audit-item{border-left:3px solid var(--accent-blue);padding:.6rem .8rem;background:#fff;border-radius:8px}',
    '.audit-item h4{margin:0 0 .2rem;font-size:1rem}',
    '.audit-item p{margin:0;color:var(--text-muted)}',
    '.audit-item small{color:var(--text-muted)}',
    '.no-logs{color:var(--text-muted)}'
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AuditTimelineComponent {
  @Input() logs: AuditLogDto[] = [];
}
