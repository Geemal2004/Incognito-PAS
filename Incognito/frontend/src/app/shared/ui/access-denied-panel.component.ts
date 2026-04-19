import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-access-denied-panel',
  standalone: true,
  imports: [RouterLink, MatButtonModule, MatIconModule],
  template: `
    <section class="access-panel">
      <mat-icon>lock</mat-icon>
      <h2>Access denied</h2>
      <p>You do not have the required role to open this view.</p>
      <a mat-flat-button color="primary" routerLink="/">Go to home</a>
    </section>
  `,
  styles: [
    '.access-panel{min-height:60vh;display:grid;place-items:center;align-content:center;gap:.4rem;text-align:center}',
    'mat-icon{font-size:2rem;height:2rem;width:2rem;color:var(--accent-red)}',
    'p{color:var(--text-muted)}'
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AccessDeniedPanelComponent {}
