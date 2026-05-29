import { Injectable } from '@angular/core';
import {UserService} from "../generated/client";
import {AuthenticationHelperService} from "./authentication-helper.service";
import {map, switchMap, tap} from "rxjs/operators";
import {Observable, of} from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class PermissionsService {
  private permissions: Array<string> = [];

  constructor(
    private userService: UserService,
    private authenticationHelperService: AuthenticationHelperService,
  ) {
    authenticationHelperService.getCredentials().pipe(
      switchMap(token => {
        if (!token) return of([]);

        return userService.apiUsersMyPermissionsGet().pipe(
          map(res => res.permissions ?? [])
        );
      }),
      tap(permissions => this.permissions = permissions)
    ).subscribe();
  }

  public hasPermissionObservable(permission: string): Observable<boolean> {
    if (!this.authenticationHelperService.isAuthenticated()) return of(false);
    return this.userService.apiUsersMyPermissionsGet().pipe(
      map(res => res.permissions?.includes(permission) ?? false),
    );
  }

  public hasPermission(permission: string): boolean {
    return this.permissions.includes(permission);
  }

  public canAdjustContestGrade(contestId: string): Observable<boolean> {
    return this.userService.apiUsersCanManageGradeAdjustmentsGet(undefined, contestId);
  }
}
