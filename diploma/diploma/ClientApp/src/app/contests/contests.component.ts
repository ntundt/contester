import { Component, OnInit } from '@angular/core';
import {ContestDto, ContestParticipationDto, ContestService} from "../../generated/client";
import {FaIconComponent} from "@fortawesome/angular-fontawesome";
import {faChartSimple, faClock, faPlus} from "@fortawesome/free-solid-svg-icons";
import {RouterLink} from "@angular/router";
import {AuthorizationService} from "../../authorization/authorization.service";
import {DatePipe, NgFor, NgIf} from "@angular/common";
import {NgbModal} from "@ng-bootstrap/ng-bootstrap";
import {InputObjectNameModalComponent} from "../shared/input-object-name-modal/input-object-name-modal.component";
import {PermissionsService} from "../../authorization/permissions.service";

@Component({
  selector: 'app-contests',
  standalone: true,
  imports: [
    FaIconComponent,
    RouterLink,
    NgIf,
    NgFor,
    DatePipe,
  ],
  templateUrl: './contests.component.html',
  styleUrl: './contests.component.css'
})
export class ContestsComponent implements OnInit {
  public contests: Array<ContestParticipationDto> = [];

  public constructor(
    private contestService: ContestService,
    public authorizationService: AuthorizationService,
    private modalService: NgbModal,
    public permissionsService: PermissionsService
  ) { }

  public ngOnInit() {
    this.fetchContests();
  }

  private fetchContests() {
    this.contestService.apiContestsGet(undefined, undefined, '-StartDate').subscribe(res => {
      this.contests = res.contests ?? [];
    });
  }

  public createContest() {
    const modalRef = this.modalService.open(InputObjectNameModalComponent);
    modalRef.componentInstance.title = 'Create contest';
    modalRef.componentInstance.placeholder = 'Contest name';
    modalRef.componentInstance.confirmButtonText = 'Create';

    modalRef.result.then((result) => {
      this.contestService.apiContestsPost({
        name: result,
        description: '',
        participants: [],
        startDate: new Date(Date.now() + 24 * 60 * 60 * 1000),
      }).subscribe(res => {
        this.fetchContests();
      });
    });
  }

  public getCurrentContests() {
    return this.contests.filter(contest => new Date(contest.finishDate!).getTime()! > new Date().getTime());
  }

  public getFinishedContests() {
    return this.contests.filter(contest => new Date(contest.finishDate!).getTime()! < new Date().getTime());
  }

  public shouldOpenContestApplication(contest: ContestParticipationDto) {
    return !this.permissionsService.hasPermission('ManageContests')
      && !contest.isPublic! && new Date(contest.startDate!).getTime()! > new Date().getTime()!;
  }

  public tooLateToApply(contest: ContestParticipationDto) {
    return new Date(contest.startDate!).getTime()! < new Date().getTime() && !contest.isPublic! && !contest.userParticipates!
      && !this.permissionsService.hasPermission('ManageContests');
  }

  public shouldOpenContest(contest: ContestParticipationDto) {
    return this.permissionsService.hasPermission('ManageContests')
      || ((contest.isPublic || contest.userParticipates) && new Date(contest.startDate!).getTime()! < new Date().getTime()!);
  }

  protected readonly faClock = faClock;
  protected readonly faChartSimple = faChartSimple;
  protected readonly faPlus = faPlus;
}
