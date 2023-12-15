import { Injectable } from '@angular/core';
import {UserService} from "../generated/client";
import {AuthorizationService} from "./authorization.service";

@Injectable({
  providedIn: 'root'
})
export class ClaimsService {
  private claims: Array<string> = [];

  constructor(private userService: UserService, private authorizationService: AuthorizationService) {
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

  public hasClaim(claim: string): boolean {
    return this.claims.includes(claim);
  }

  public getClaims(): Array<string> {
    return this.claims;
  }
}
