import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize, forkJoin } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatRadioModule } from '@angular/material/radio';
import { MatSelectModule } from '@angular/material/select';
import {
  CreateProjectGroupRequest,
  ProposalUpsertRequest,
  StudentApiService
} from '../../core/services/student-api.service';
import { ResearchAreaDto, StudentPeerDto } from '../../core/models/api.models';
import { getProposalDocumentViewUrl } from '../../core/utils/document-url.util';
import { ToastService } from '../../core/services/toast.service';
import { LoadingSkeletonComponent } from '../../shared/ui/loading-skeleton.component';
import { PageHeaderComponent } from '../../shared/ui/page-header.component';

interface CloudinaryUploadResponse {
  secure_url: string;
}

const CLOUDINARY_CLOUD_NAME = 'dy3jmad0j';
const CLOUDINARY_UPLOAD_PRESET = 'profile_pics_preset';
const MAX_PROPOSAL_DOCUMENT_SIZE_BYTES = 10 * 1024 * 1024;

@Component({
  selector: 'app-proposal-form',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatRadioModule,
    MatSelectModule,
    PageHeaderComponent,
    LoadingSkeletonComponent
  ],
  templateUrl: './proposal-form.component.html',
  styleUrl: './proposal-form.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProposalFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly http = inject(HttpClient);
  private readonly api = inject(StudentApiService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  readonly areas = signal<ResearchAreaDto[]>([]);
  readonly peers = signal<StudentPeerDto[]>([]);
  readonly loading = signal(false);
  readonly editing = signal(false);
  readonly creatingGroup = signal(false);
  readonly uploadingDocument = signal(false);

  private proposalId: number | null = null;

  readonly form = this.fb.group({
    title: this.fb.nonNullable.control('', [Validators.required, Validators.minLength(10), Validators.maxLength(150)]),
    abstract: this.fb.nonNullable.control('', [Validators.required, Validators.minLength(100), Validators.maxLength(1000)]),
    technicalStack: this.fb.nonNullable.control('', [Validators.maxLength(200)]),
    proposalDocumentUrl: this.fb.control<string | null>(null, [Validators.maxLength(500)]),
    researchAreaId: this.fb.nonNullable.control(0, [Validators.required, Validators.min(1)]),
    projectType: this.fb.nonNullable.control<'individual' | 'group'>('individual'),
    projectGroupId: this.fb.control<number | null>(null)
  });

  readonly groupForm = this.fb.group({
    name: this.fb.nonNullable.control('', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]),
    memberStudentIds: this.fb.nonNullable.control<string[]>([], [Validators.required, Validators.minLength(1)])
  });

  readonly memberSearchControl = this.fb.nonNullable.control('');

  constructor() {
    this.syncProjectTypeValidation(this.form.controls.projectType.value);
    this.form.controls.projectType.valueChanges.subscribe((mode) => {
      this.syncProjectTypeValidation(mode ?? 'individual');
    });

    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam !== null) {
      const parsedId = Number(idParam);
      if (!Number.isFinite(parsedId) || parsedId <= 0) {
        this.toast.error('Invalid proposal id.');
        void this.router.navigate(['/student/proposals']);
        return;
      }

      this.editing.set(true);
      this.proposalId = parsedId;
    }

    this.loading.set(true);
    forkJoin({
      areas: this.api.getResearchAreas(),
      peers: this.api.getStudentPeers()
    }).subscribe({
      next: ({ areas, peers }) => {
        this.areas.set(areas);
        this.peers.set(peers);

        if (this.proposalId !== null) {
          this.loadProposalForEdit(this.proposalId);
          return;
        }

        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.toast.error(error?.error?.message ?? 'Unable to load proposal form data.');
      }
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const isGroupProject = raw.projectType === 'group';
    const technicalStack = raw.technicalStack.trim();
    const proposalDocumentUrl = raw.proposalDocumentUrl?.trim() ?? '';
    const basePayload: Omit<ProposalUpsertRequest, 'projectGroupId'> = {
      title: raw.title,
      abstract: raw.abstract,
      technicalStack: technicalStack ? technicalStack : undefined,
      proposalDocumentUrl: proposalDocumentUrl ? proposalDocumentUrl : undefined,
      researchAreaId: raw.researchAreaId
    };

    if (this.proposalId !== null) {
      if (isGroupProject && (!raw.projectGroupId || raw.projectGroupId < 1)) {
        this.toast.error('Select a valid group to update this group proposal.');
        return;
      }

      const payload: ProposalUpsertRequest = {
        ...basePayload,
        projectGroupId: isGroupProject ? raw.projectGroupId ?? undefined : undefined
      };

      this.api.updateProposal(this.proposalId, payload).subscribe({
        next: () => {
          this.toast.success('Proposal updated successfully.');
          void this.router.navigate(['/student/proposals', this.proposalId]);
        },
        error: (error) => this.toast.error(error?.error?.message ?? 'Unable to update proposal.')
      });
      return;
    }

    if (isGroupProject) {
      if (raw.projectGroupId && raw.projectGroupId > 0) {
        const payload: ProposalUpsertRequest = {
          ...basePayload,
          projectGroupId: raw.projectGroupId
        };

        this.api.createProposal(payload).subscribe({
          next: () => {
            this.toast.success('Proposal created successfully.');
            void this.router.navigate(['/student/proposals']);
          },
          error: (error) => this.toast.error(error?.error?.message ?? 'Unable to create proposal.')
        });
        return;
      }

      if (this.groupForm.invalid) {
        this.groupForm.markAllAsTouched();
        this.toast.error('Add team details and teammates before submitting.');
        return;
      }

      this.createGroupAndSubmitProposal(basePayload);
      return;
    }

    const payload: ProposalUpsertRequest = {
      ...basePayload,
      projectGroupId: isGroupProject ? raw.projectGroupId ?? undefined : undefined
    };

    this.api.createProposal(payload).subscribe({
      next: () => {
        this.toast.success('Proposal created successfully.');
        void this.router.navigate(['/student/proposals']);
      },
      error: (error) => this.toast.error(error?.error?.message ?? 'Unable to create proposal.')
    });
  }

  private loadProposalForEdit(id: number): void {
    this.api.getProposal(id).subscribe({
      next: (proposal) => {
        // DECISION: backend details currently return research area name, so edit prefill resolves id by name.
        const researchAreaId = this.resolveResearchAreaId(proposal.researchAreaName);
        const projectType = proposal.isGroupProject ? 'group' : 'individual';

        this.form.patchValue({
          title: proposal.title,
          abstract: proposal.abstract,
          technicalStack: proposal.technicalStack ?? '',
          proposalDocumentUrl: proposal.proposalDocumentUrl?.trim() ? proposal.proposalDocumentUrl : null,
          researchAreaId,
          projectType,
          projectGroupId: proposal.projectGroupId ?? null
        });

        this.syncProjectTypeValidation(projectType);

        this.loading.set(false);
      },
      error: (error) => {
        this.loading.set(false);
        this.toast.error(error?.error?.message ?? 'Unable to load proposal for editing.');
      }
    });
  }

  private resolveResearchAreaId(researchAreaName: string): number {
    const area = this.areas().find((item) => item.name.toLowerCase() === researchAreaName.toLowerCase());
    return area?.id ?? 0;
  }

  submitDisabled(): boolean {
    if (this.uploadingDocument() || this.creatingGroup()) {
      return true;
    }

    if (this.form.invalid) {
      return true;
    }

    const raw = this.form.getRawValue();

    if (raw.projectType !== 'group') {
      return false;
    }

    if (this.editing()) {
      return !raw.projectGroupId || raw.projectGroupId < 1;
    }

    if (raw.projectGroupId && raw.projectGroupId > 0) {
      return false;
    }

    return this.groupForm.invalid;
  }

  filteredPeers(): StudentPeerDto[] {
    const search = this.memberSearchControl.value.trim().toLowerCase();
    const selectedIds = new Set(this.groupForm.controls.memberStudentIds.value);

    return this.peers().filter((peer) => {
      if (selectedIds.has(peer.id)) {
        return false;
      }

      if (!search) {
        return true;
      }

      return peer.fullName.toLowerCase().includes(search) || peer.email.toLowerCase().includes(search);
    });
  }

  selectedPeers(): StudentPeerDto[] {
    const peerById = new Map(this.peers().map((peer) => [peer.id, peer]));
    return this.groupForm.controls.memberStudentIds.value
      .map((id) => peerById.get(id))
      .filter((peer): peer is StudentPeerDto => peer !== undefined);
  }

  addMember(peerId: string): void {
    const control = this.groupForm.controls.memberStudentIds;
    const current = control.value;
    if (current.includes(peerId)) {
      return;
    }

    control.setValue([...current, peerId]);
    control.markAsTouched();
    control.updateValueAndValidity();
    this.memberSearchControl.setValue('');
  }

  removeMember(peerId: string): void {
    const control = this.groupForm.controls.memberStudentIds;
    control.setValue(control.value.filter((id) => id !== peerId));
    control.markAsTouched();
    control.updateValueAndValidity();
  }

  onDocumentSelected(event: Event): void {
    const input = event.target as HTMLInputElement | null;
    const selectedFile = input?.files?.[0];

    if (!selectedFile) {
      return;
    }

    const fileName = selectedFile.name.toLowerCase();
    const isPdfMime = selectedFile.type === 'application/pdf';
    const isPdfExtension = fileName.endsWith('.pdf');

    if (!isPdfMime && !isPdfExtension) {
      this.toast.error('Please choose a PDF file.');
      this.resetFileInput(input);
      return;
    }

    if (selectedFile.size > MAX_PROPOSAL_DOCUMENT_SIZE_BYTES) {
      this.toast.error('Proposal document must be 10MB or smaller.');
      this.resetFileInput(input);
      return;
    }

    const uploadForm = new FormData();
    uploadForm.append('file', selectedFile);
    uploadForm.append('upload_preset', CLOUDINARY_UPLOAD_PRESET);

    this.uploadingDocument.set(true);
    this.http
      .post<CloudinaryUploadResponse>(`https://api.cloudinary.com/v1_1/${CLOUDINARY_CLOUD_NAME}/auto/upload`, uploadForm)
      .pipe(
        finalize(() => {
          this.uploadingDocument.set(false);
          this.resetFileInput(input);
        })
      )
      .subscribe({
        next: (uploadResponse) => {
          const secureUrl = uploadResponse?.secure_url?.trim();

          if (!secureUrl || !this.isPdfUrl(secureUrl)) {
            this.toast.error('Cloudinary did not return a valid PDF URL.');
            return;
          }

          this.form.patchValue({ proposalDocumentUrl: secureUrl });
          this.form.controls.proposalDocumentUrl.markAsDirty();
          this.toast.success('Proposal PDF uploaded successfully.');
        },
        error: () => {
          this.toast.error('Could not upload proposal PDF. Please try again.');
        }
      });
  }

  removeDocument(): void {
    this.form.patchValue({ proposalDocumentUrl: null });
    this.form.controls.proposalDocumentUrl.markAsDirty();
  }

  proposalDocumentFileName(): string {
    const url = this.form.controls.proposalDocumentUrl.value?.trim();
    if (!url) {
      return '';
    }

    try {
      const path = new URL(url).pathname;
      const fileName = path.split('/').pop() ?? 'proposal-document.pdf';
      return decodeURIComponent(fileName);
    } catch {
      return 'proposal-document.pdf';
    }
  }

  proposalDocumentViewUrl(): string | null {
    return getProposalDocumentViewUrl(this.form.controls.proposalDocumentUrl.value);
  }

  private isPdfUrl(url: string): boolean {
    try {
      return new URL(url).pathname.toLowerCase().endsWith('.pdf');
    } catch {
      return false;
    }
  }

  private resetFileInput(input: HTMLInputElement | null): void {
    if (input) {
      input.value = '';
    }
  }

  private createGroupAndSubmitProposal(basePayload: Omit<ProposalUpsertRequest, 'projectGroupId'>): void {
    const raw = this.groupForm.getRawValue();
    const payload: CreateProjectGroupRequest = {
      name: raw.name,
      memberStudentIds: raw.memberStudentIds
    };

    this.creatingGroup.set(true);
    this.api.createProjectGroup(payload).subscribe({
      next: (group) => {
        this.form.controls.projectType.setValue('group');
        this.form.controls.projectGroupId.setValue(group.id);
        this.form.controls.projectGroupId.markAsTouched();
        this.form.controls.projectGroupId.updateValueAndValidity();

        const proposalPayload: ProposalUpsertRequest = {
          ...basePayload,
          projectGroupId: group.id
        };

        this.api.createProposal(proposalPayload).subscribe({
          next: () => {
            this.groupForm.reset({ name: '', memberStudentIds: [] });
            this.memberSearchControl.setValue('');
            this.toast.success('Group created and proposal submitted successfully.');
            this.creatingGroup.set(false);
            void this.router.navigate(['/student/proposals']);
          },
          error: (error) => {
            this.creatingGroup.set(false);
            this.toast.error(error?.error?.message ?? 'Group created, but proposal submission failed. Please try again.');
          }
        });
      },
      error: (error) => {
        this.creatingGroup.set(false);
        this.toast.error(error?.error?.message ?? 'Unable to create group.');
      }
    });
  }

  private syncProjectTypeValidation(mode: 'individual' | 'group'): void {
    const groupControl = this.form.controls.projectGroupId;

    if (mode === 'group') {
      if (this.editing()) {
        groupControl.setValidators([Validators.required, Validators.min(1)]);
      } else {
        groupControl.clearValidators();
        groupControl.setErrors(null);
      }
    } else {
      groupControl.setValue(null, { emitEvent: false });
      groupControl.clearValidators();
      groupControl.setErrors(null);
    }

    groupControl.updateValueAndValidity({ emitEvent: false });
  }
}
