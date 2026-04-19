import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-filter-bar',
  standalone: true,
  template: '<section class="filter-bar ui-card" role="search"><ng-content></ng-content></section>',
  styleUrl: './filter-bar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class FilterBarComponent {}
