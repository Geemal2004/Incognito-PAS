import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';

@Component({
  standalone: true,
  selector: 'app-not-found',
  imports: [RouterLink, MatButtonModule],
  template: `
    <section class="center-panel">
      <h1>Page not found</h1>
      <p>The page you requested does not exist.</p>
      <a mat-flat-button color="primary" routerLink="/">Back home</a>
    </section>
  `,
  styles: [
    `.center-panel{min-height:80vh;display:grid;place-items:center;text-align:center}`
  ]
})
export class NotFoundComponent {}
