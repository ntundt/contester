import { Injectable, inject } from '@angular/core';
import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Observable, catchError } from "rxjs";
import { AuthorizationService } from "./authorization.service";
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthorizationInterceptor implements HttpInterceptor {
  constructor(private authorizationService: AuthorizationService, private router: Router) { }

  public intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (this.authorizationService.isAuthenticated()) {
      req = req.clone({
        setHeaders: {
          Authorization: `Bearer ${this.authorizationService.getAccessToken().getValue()}`,
        }
      });
    }
    return next.handle(req).pipe(catchError(err => {
      if (err.status === 401) {
        this.authorizationService.signOut();
        this.router.navigate(['/login']);
      }
      throw err;
    }));
  }
}
