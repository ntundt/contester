import { Component } from '@angular/core';
import { ContestService, ContestSettingsDto } from "../../../generated/client";
import { ActivatedRoute } from "@angular/router";
import { FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators } from "@angular/forms";
import { ToastsService } from "../../toasts/toasts.service";
import { NgbModal, NgbPopover } from '@ng-bootstrap/ng-bootstrap';
import { UserSelectionModalComponent } from './user-selection-modal/user-selection-modal.component';
import { faPlus, faTimes } from '@fortawesome/free-solid-svg-icons';
import { faQuestionCircle } from '@fortawesome/free-regular-svg-icons';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import { NgForOf } from '@angular/common';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    FaIconComponent,
    NgForOf,
    NgbPopover,
    ReactiveFormsModule,
  ],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css'
})
export class SettingsComponent {
  public contest: ContestSettingsDto = {
    id: '',
    name: '',
    description: '',
    startDate: new Date(),
    finishDate: new Date(),
    isPublic: false,
    commissionMembers: [],
  };

  settingsForm = new FormGroup({
    name: new FormControl('', [Validators.required, Validators.maxLength(150)]),
    startDate: new FormControl('', [Validators.required]),
    finishDate: new FormControl('', [Validators.required]),
    isPublic: new FormControl(false),
  });


  public constructor(
    private contestService: ContestService,
    private activatedRoute: ActivatedRoute,
    private toastsService: ToastsService,
    private modalService: NgbModal,
  ) { }

  public ngOnInit(): void {
    this.activatedRoute.parent?.params.subscribe(params => {
      this.contestService.apiContestsContestIdSettingsGet(params['contestId']).subscribe(contest => {
        if (contest) {
          this.contest = contest;

          // idk how but this works
          // TODO: rewrite this properly
          const startDate = new Date(new Date(contest.startDate + 'Z').getTime() - new Date().getTimezoneOffset() * 60000).toISOString().substring(0, 16);
          const finishDate = new Date(new Date(contest.finishDate + 'Z').getTime() - new Date().getTimezoneOffset() * 60000).toISOString().substring(0, 16);
          
          this.settingsForm.setValue({
            name: contest.name ?? null,
            startDate: startDate,
            finishDate: finishDate,
            isPublic: contest.isPublic ?? null,
          });
        }
      });
    });
  }

  public save() {
    this.contestService.apiContestsContestIdPut(this.contest.id ?? '', { 
      name: this.settingsForm.value.name ?? '',
      description: this.contest.description,
      isPublic: this.settingsForm.value.isPublic ?? false,
      startDate: new Date(this.settingsForm.value.startDate ?? new Date()),
      finishDate: new Date(this.settingsForm.value.finishDate ?? new Date()),
      commissionMembers: this.contest.commissionMembers?.map(m => m.id!),
    }).subscribe(() => {
      this.toastsService.show({
        header: 'Success',
        body: 'Contest settings saved',
        delay: 5000,
      })
    });
  }

  public openAddCommissionMemberModal() {
    this.modalService.open(UserSelectionModalComponent)
      .result.then((selectedUser) => {
        if (!selectedUser) return;
        this.contest.commissionMembers = [
          ...(this.contest.commissionMembers ?? []),
          selectedUser
        ];
      }
    );
  }

  public removeCommissionMember(memberId: string) {
    this.contest.commissionMembers = this.contest.commissionMembers?.filter(m => m.id !== memberId);
  }

  protected readonly faPlus = faPlus;
  protected readonly faTimes = faTimes;
  protected readonly faQuestionCircle = faQuestionCircle;
}
