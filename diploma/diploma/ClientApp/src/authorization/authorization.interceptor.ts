import { Injectable } from '@angular/core';
import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { Observable } from "rxjs";
import { AuthorizationService } from "./authorization.service";

@Injectable({
  providedIn: 'root'
})
export class AuthorizationInterceptor implements HttpInterceptor {
  constructor(private authorizationService: AuthorizationService) { }

  public intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    if (this.authorizationService.isAuthenticated()) {
      req = req.clone({
        setHeaders: {
          Authorization: `Bearer ${this.authorizationService.getAccessToken().getValue()}`,
        }
      });
    }
    return next.handle(req);
  }
}
