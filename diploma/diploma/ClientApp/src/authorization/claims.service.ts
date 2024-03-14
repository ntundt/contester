import { Injectable } from '@angular/core';
import {UserService} from "../generated/client";
import {AuthorizationService} from "./authorization.service";
import {map, switchMap} from "rxjs/operators";
import {Observable} from "rxjs";

@Injectable({
  providedIn: 'root'
})
export class ClaimsService {
  private claims: Array<string> = [];

  constructor(
    private userService: UserService,
    private authorizationService: AuthorizationService,
  ) {
    authorizationService.getAccessToken().subscribe(token => {
      if (token) {
        userService.apiUsersMyClaimsGet().subscribe(res => {
          this.claims = res.claims ?? [];
        });
      } else {
        this.claims = [];
      }
    });
  }

  public hasClaimObservable(claim: string): Observable<boolean> {
    if (!this.authorizationService.isAuthenticated()) return new Observable(subscriber => subscriber.next(false));
    return this.userService.apiUsersMyClaimsGet().pipe(
      map(res => res.claims?.includes(claim) ?? false),
    );
  }

  public hasClaim(claim: string): boolean {
    return this.claims.includes(claim);
  }

  public getClaims(): Array<string> {
    return this.claims;
  }

  public canAdjustContestGrade(contestId: string): Observable<boolean> {
    return this.userService.apiUsersCanManageGradeAdjustmentsGet(undefined, contestId);
  }
}
