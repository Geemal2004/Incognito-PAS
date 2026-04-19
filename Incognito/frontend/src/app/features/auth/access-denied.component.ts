import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';

@Component({
  standalone: true,
  selector: 'app-access-denied',
  imports: [RouterLink, MatButtonModule],
  template: `
    <section class="center-panel">
      <h1>Access denied</h1>
      <p>You do not have permission to view this area.</p>
      <a mat-flat-button color="primary" routerLink="/">Go to home</a>
    </section>
  `,
  styles: [
    `.center-panel{min-height:80vh;display:grid;place-items:center;text-align:center}`
  ]
})
export class AccessDeniedComponent {}
