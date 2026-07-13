import {AfterViewInit, Component, ElementRef, Input, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {PrincipalDto, UserGroupService} from "../../../generated/client";
import {debounceTime, distinctUntilChanged, switchMap, tap} from "rxjs/operators";
import {filter, Subject, Subscription} from "rxjs";
import {NgbActiveModal} from "@ng-bootstrap/ng-bootstrap";
import {TranslatePipe} from "@ngx-translate/core";
import {FormsModule} from "@angular/forms";
import {PrincipalCard} from "../principal-card/principal-card";
import {FaIconComponent} from "@fortawesome/angular-fontawesome";
import {faSearch} from "@fortawesome/free-solid-svg-icons";

@Component({
  selector: 'app-principal-selection-modal',
  imports: [
    TranslatePipe,
    FormsModule,
    PrincipalCard,
    FaIconComponent
  ],
  templateUrl: './principal-selection-modal.html',
  styleUrl: './principal-selection-modal.css',
})
export class PrincipalSelectionModal implements OnInit, AfterViewInit, OnDestroy {
  @Input() public excludedPrincipalIds: string[] = [];

  @ViewChild('searchInput') public searchInput!: ElementRef<HTMLInputElement>;

  public query: string = '';
  public principals: Array<PrincipalDto> = [];
  public loading: boolean = false;

  private searchSubject = new Subject<string>();
  private searchSubscription!: Subscription;

  public constructor(
    private activeModalService: NgbActiveModal,
    private userGroupService: UserGroupService,
  ) { }

  public ngOnInit(): void {
    this.searchSubscription = this.searchSubject.pipe(
      filter(search => search.length > 0),
      debounceTime(300),
      distinctUntilChanged(),
      tap(() => this.loading = true),
      switchMap(searchQuery => this.userGroupService.apiUserGroupSearchPrincipalGet(searchQuery))
    ).subscribe({
      next: (res) => {
        this.principals = res.filter(p => !this.excludedPrincipalIds.includes(p.id!));
        this.loading = false;
      },
      error: (err) => {
        console.error('Error fetching principals:', err);
        this.loading = false;
      }
    });
  }

  public searchChanged(): void {
    this.searchSubject.next(this.query);
  }

  public selectPrincipal(principal: PrincipalDto): void {
    this.activeModalService.close(principal);
  }

  public closeModal(): void {
    this.activeModalService.dismiss();
  }

  public ngAfterViewInit(): void {
    setTimeout(() => {
      this.searchInput.nativeElement.focus();
    }, 0);
  }

  public ngOnDestroy(): void {
    if (this.searchSubscription) {
      this.searchSubscription.unsubscribe();
    }
  }

  protected readonly faSearch = faSearch;
}
