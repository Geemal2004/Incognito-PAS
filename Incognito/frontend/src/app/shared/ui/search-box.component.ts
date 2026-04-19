import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';

@Component({
  selector: 'app-search-box',
  standalone: true,
  imports: [FormsModule, MatFormFieldModule, MatInputModule, MatIconModule],
  template: `
    <mat-form-field appearance="outline" class="search-box">
      <mat-label>{{ label }}</mat-label>
      <mat-icon matPrefix>search</mat-icon>
      <input matInput [placeholder]="placeholder" [ngModel]="value" (ngModelChange)="valueChange.emit($event)" autocomplete="off" />
    </mat-form-field>
  `,
  styleUrl: './search-box.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class SearchBoxComponent {
  @Input() label = 'Search';
  @Input() placeholder = 'Search';
  @Input() value = '';
  @Output() valueChange = new EventEmitter<string>();
}
