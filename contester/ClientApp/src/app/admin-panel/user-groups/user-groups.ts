import {Component, OnDestroy, OnInit} from '@angular/core';
import {TranslatePipe} from "@ngx-translate/core";
import {PrincipalDto, UserGroupService} from "../../../generated/client";
import {debounceTime, distinctUntilChanged, switchMap, tap} from "rxjs/operators";
import {NgbModal} from "@ng-bootstrap/ng-bootstrap";
import {FaIconComponent} from "@fortawesome/angular-fontawesome";
import {faSearch} from "@fortawesome/free-solid-svg-icons";
import {ActivatedRoute, Router, RouterLink} from "@angular/router";
import {DatePipe, UpperCasePipe} from "@angular/common";
import {InitialsPipe} from "../../pipes/initials-pipe";
import {InputObjectNameModalComponent} from "../../shared/input-object-name-modal/input-object-name-modal.component";
import {Subject, Subscription} from "rxjs";
import {UuidToColorMapper} from "../../shared/uuid-to-color-mapper";

@Component({
  selector: 'app-user-groups',
  imports: [
    TranslatePipe,
    InitialsPipe,
    FaIconComponent,
    RouterLink,
    UpperCasePipe,
    DatePipe
  ],
  templateUrl: './user-groups.html',
  styleUrl: './user-groups.css',
})
export class UserGroups implements OnInit, OnDestroy {
  public constructor(
    public userGroupService: UserGroupService,
    public modalService: NgbModal,
    public router: Router,
    private route: ActivatedRoute,
  ) { }

  pageSize = 10;
  currentPage = 1;
  searchTerm = '';

  userGroups: Array<PrincipalDto>;
  totalFoundCount: number = 0;
  loadingUserGroups: boolean = true;

  searchSubject = new Subject<string>();
  private routeSubscription: Subscription;

  ngOnInit() {
    this.routeSubscription = this.route.queryParams.pipe(
      tap(params => {
        this.loadingUserGroups = true;
        this.currentPage = params['page'] ? Number(params['page']) : 1;
        this.searchTerm = params['search'] || '';
      }),
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(searchQuery => {
        const filterQuery = this.searchTerm ? `Name@=*${this.searchTerm}` : undefined;
        return this.userGroupService.apiUserGroupSearchGet(
          filterQuery,
          undefined,
          this.currentPage,
          this.pageSize
        );
      }),
    ).subscribe({
      next: (res) => {
        this.userGroups = res.data || [];
        this.totalFoundCount = res.totalCount || 0;
        this.loadingUserGroups = false;
      },
      error: (err) => {
        console.error('Error fetching groups:', err);
        this.loadingUserGroups = false;
      }
    });

  }

  ngOnDestroy() {
    if (this.routeSubscription) {
      this.routeSubscription.unsubscribe();
    }
  }

  createNewGroup() {
    const modalRef = this.modalService.open(InputObjectNameModalComponent);
    modalRef.componentInstance.title = 'Create new group';
    modalRef.result.then((result: string | undefined) => {
      if (!result) return;
      this.userGroupService
        .apiUserGroupPost({
          name: result,
        })
        .subscribe({
          next: (group) => this.router.navigateByUrl(`/admin-panel/user-groups/${group.id}`),
          error: console.error,
        });
    });
  }

  onSearchChanged(value: string) {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { search: value || null, page: 1 }, // Reset to page 1 on a new search
      queryParamsHandling: 'merge'
    });
  }
  goToPage(page: number) {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { page: page },
      queryParamsHandling: 'merge'
    });
  }
  protected readonly faSearch = faSearch;
  protected readonly Math = Math;
  protected readonly Array = Array;
  protected readonly UuidToColorMapper = UuidToColorMapper;
}
