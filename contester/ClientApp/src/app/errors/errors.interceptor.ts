import {HttpErrorResponse, HttpHandler, HttpInterceptor, HttpRequest} from '@angular/common/http';
import {Injectable} from "@angular/core";
import {ToastsService} from "../toasts/toasts.service";
import {catchError, tap} from "rxjs/operators";

export interface IError {
  message: string;
  err: number;
}

@Injectable({
  providedIn: 'root'
})
export class ErrorsInterceptor implements HttpInterceptor {

  public constructor(private toastsService: ToastsService) { }

  public intercept(req: HttpRequest<any>, next: HttpHandler) {
    return next.handle(req).pipe(catchError(err => {
      if (err.error?.err) {
        this.toastsService.show({
          header: 'Error',
          body: err.error.message,
          type: 'error',
          delay: 10000
        });
      }

      throw err;
    }));
  }
}
