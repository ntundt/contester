import { Injectable } from '@angular/core';
import {UserService} from "../generated/client";
import {AuthorizationService} from "./authorization.service";
import {map, switchMap} from "rxjs/operators";
import {Observable} from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class PermissionsService {
  private permissions: Array<string> = [];

  constructor(
    private userService: UserService,
    private authorizationService: AuthorizationService,
  ) {
    authorizationService.getAccessToken().subscribe(token => {
      if (token) {
        userService.apiUsersMyPermissionsGet().subscribe(res => {
          this.permissions = res.permissions ?? [];
        });
      } else {
        this.permissions = [];
      }
    });
  }

  public hasPermissionObservable(permission: string): Observable<boolean> {
    if (!this.authorizationService.isAuthenticated()) return new Observable(subscriber => subscriber.next(false));
    return this.userService.apiUsersMyPermissionsGet().pipe(
      map(res => res.permissions?.includes(permission) ?? false),
    );
  }

  public hasPermission(permission: string): boolean {
    return this.permissions.includes(permission);
  }

  public getPermissions(): Array<string> {
    return this.permissions;
  }

  public canAdjustContestGrade(contestId: string): Observable<boolean> {
    return this.userService.apiUsersCanManageGradeAdjustmentsGet(undefined, contestId);
  }
}
