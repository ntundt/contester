import {Component, OnInit} from '@angular/core';
import {UpdateUserInfoCommand, UserDto, UserService} from "../../generated/client";
import {FormsModule} from "@angular/forms";
import {ToastsService} from "../toasts/toasts.service";

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    FormsModule,
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  public user: UpdateUserInfoCommand = {
    firstName: '',
    lastName: '',
    patronymic: '',
    additionalInfo: '',
  }
  public constructor(
    private usersService: UserService,
    private toastsService: ToastsService
  ) { }

  public ngOnInit(): void {
    this.usersService.apiUsersGet().subscribe((res) => {
      this.user = res;
    });
  }

  public submit(): void {
    this.usersService.apiUsersMyInfoPut({
      firstName: this.user?.firstName ?? '',
      lastName: this.user?.lastName ?? '',
      patronymic: this.user?.patronymic ?? '',
      additionalInfo: this.user?.additionalInfo ?? ''
    }).subscribe((res) => {
      this.toastsService.show({
        header: 'Success',
        body: 'Profile updated successfully',
        delay: 5000,
      })
    });
  }
}
