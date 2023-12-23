import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import {AuthorizationService} from "../../authorization/authorization.service";

export const authGuard: CanActivateFn = (route, state) => {
  const authorizationService = inject(AuthorizationService);
  return authorizationService.isAuthenticated();
};
