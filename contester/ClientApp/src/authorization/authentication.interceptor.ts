import { Injectable, inject } from '@angular/core';
import {
  HttpBackend, HttpClient,
  HttpErrorResponse,
  HttpEvent,
  HttpHandler, HttpHandlerFn,
  HttpInterceptor,
  HttpRequest
} from "@angular/common/http";
import {Observable, catchError, throwError, filter, take, finalize} from "rxjs";
import {AuthenticationHelperService, Credentials} from "./authentication-helper.service";
import { Router } from '@angular/router';
import {map, switchMap, tap} from "rxjs/operators";
import {AuthenticationService, Configuration} from "../generated/client";

@Injectable({
  providedIn: 'root'
})
export class AuthenticationInterceptor implements HttpInterceptor {
  constructor(private authenticationHelperService: AuthenticationHelperService, private router: Router) { }

  private addAuthHeader<TReq>(req: HttpRequest<TReq>, credentials: Credentials): HttpRequest<TReq> {
    return req.clone({
      setHeaders: {
        Authorization: `Bearer ${credentials.accessToken}`,
      }
    });
  }

  isRefreshingCredentials: boolean = false;

  public intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (!this.authenticationHelperService.isAuthenticated()) return next.handle(req);

    const credentials = this.authenticationHelperService.getCredentials();

    const needsRefresh = new Date(credentials.getValue()?.accessTokenExpiry!).getTime()! - 60_000 < Date.now();

    const handleAuthFailure = (err: HttpErrorResponse) => {
      if (err.status == 401) {
        this.authenticationHelperService.signOut();
        this.router.navigate(['/sign-up-or-sign-in']);
      }
      return throwError(() => err);
    };

    if (needsRefresh) {
      if (!this.isRefreshingCredentials) {
        this.isRefreshingCredentials = true;
        return this.authenticationHelperService.renewCredentials().pipe(
          switchMap((x) => next.handle(this.addAuthHeader(req, this.authenticationHelperService.getCredentials().getValue()!))),
          catchError(handleAuthFailure),
          finalize(() => this.isRefreshingCredentials = false),
        );
      } else {
        return this.authenticationHelperService.getIsRenewingCredentials().pipe(
          filter(x => !x),
          take(1),
          switchMap(() => next.handle(this.addAuthHeader(req, this.authenticationHelperService.getCredentials().getValue()!))),
          catchError(handleAuthFailure),
        );
      }
    }

    return next.handle(this.addAuthHeader(req, this.authenticationHelperService.getCredentials().getValue()!))
      .pipe(catchError(handleAuthFailure));
  }
}
