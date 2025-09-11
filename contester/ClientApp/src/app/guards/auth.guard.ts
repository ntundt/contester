import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import {AuthenticationHelperService} from "../../authorization/authentication-helper.service";

export const authGuard: CanActivateFn = (route, state) => {
  const authorizationService = inject(AuthenticationHelperService);
  return authorizationService.isAuthenticated();
};
