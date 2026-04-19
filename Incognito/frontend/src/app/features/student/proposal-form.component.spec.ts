import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { ProposalFormComponent } from './proposal-form.component';
import { StudentApiService } from '../../core/services/student-api.service';
import { ToastService } from '../../core/services/toast.service';

class StudentApiServiceStub {
  getResearchAreas() {
    return of([{ id: 1, name: 'Artificial Intelligence' }]);
  }
  createProposal() {
    return of({});
  }
}

class ToastServiceStub {
  success(): void {}
  error(): void {}
}

describe('ProposalFormComponent', () => {
  let component: ProposalFormComponent;
  let fixture: ComponentFixture<ProposalFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProposalFormComponent],
      providers: [
        provideNoopAnimations(),
        provideRouter([]),
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap({})
            }
          }
        },
        { provide: StudentApiService, useClass: StudentApiServiceStub },
        { provide: ToastService, useClass: ToastServiceStub }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProposalFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('is created', () => {
    expect(component).toBeTruthy();
  });

  it('requires title and abstract constraints', () => {
    component.form.patchValue({ title: 'short', abstract: 'tiny', researchAreaId: 0 });
    expect(component.form.valid).toBeFalse();
  });
});
