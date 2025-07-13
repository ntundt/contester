import {Component, OnInit} from '@angular/core';
import {UpdateUserInfoCommand, UserDto, UserService} from "../../generated/client";
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {ToastsService} from "../toasts/toasts.service";
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TranslateModule,
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnInit {
  public constructor(
    private usersService: UserService,
    private toastsService: ToastsService
  ) { }

  profileForm = new FormGroup({
    firstName: new FormControl('', [Validators.required, Validators.maxLength(50)]),
    lastName: new FormControl('', [Validators.required, Validators.maxLength(50)]),
    patronymic: new FormControl('', [Validators.maxLength(50)]),
    additionalInfo: new FormControl('', [Validators.required, Validators.maxLength(150)]),
  });

  public ngOnInit(): void {
    this.usersService.apiUsersGet().subscribe((res) => {
      this.profileForm.setValue({
        firstName: res.firstName ?? null,
        lastName: res.lastName ?? null,
        patronymic: res.patronymic ?? null,
        additionalInfo: res.additionalInfo ?? null,
      });
    });
  }

  public submit(): void {
    const command = this.profileForm.value as UpdateUserInfoCommand;

    this.usersService.apiUsersMyInfoPut(command)
      .subscribe((res) => {
        this.toastsService.show({
          header: 'Success',
          body: 'Profile updated successfully',
          delay: 5000,
        })
      });
  }
}
