import { NgForOf, NgIf } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { UserDto, UserService } from 'src/generated/client';

@Component({
  selector: 'app-user-selection-modal',
  standalone: true,
  imports: [NgForOf, FormsModule, NgIf],
  templateUrl: './user-selection-modal.component.html',
  styleUrl: './user-selection-modal.component.css'
})
export class UserSelectionModalComponent implements OnInit {
  @Input() public excludedUserIds: string[] | undefined;
  public query: string = '';
  public users: UserDto[] = [];

  public constructor(
    private activeModalService: NgbActiveModal,
    private userService: UserService,
  ) { }

  public selectUser(user: UserDto): void {
    this.activeModalService.close(user);
  }

  private updateSearch(): void {
    this.userService.apiUsersSearchGet(this.query).subscribe(users => {
      this.users = users.filter(u => !this.excludedUserIds?.includes(u.id!));
    });
  }

  public searchChanged(): void {
    this.updateSearch();
  }

  public closeModal(): void {
    this.activeModalService.close();
  }

  public ngOnInit(): void {
    this.updateSearch();
  }
}
