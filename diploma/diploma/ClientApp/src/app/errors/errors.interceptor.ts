import {HttpErrorResponse, HttpHandler, HttpInterceptor, HttpRequest} from '@angular/common/http';
import {Injectable} from "@angular/core";
import {ToastsService} from "../toasts/toasts.service";
import {tap} from "rxjs/operators";

export interface IError {
  message: string;
  err: number;
}

@Injectable({
  providedIn: 'root'
})
export class ErrorsInterceptor implements HttpInterceptor {

  public constructor(private toastsService: ToastsService) {
  }

  public intercept(req: HttpRequest<any>, next: HttpHandler) {
    const observable = next.handle(req);
    observable.pipe(tap({
      error: (error: HttpErrorResponse) => {
        if (error.error?.err) {
          this.toastsService.show({header: 'Error', body: error.error.message, type: 'error', delay: 10000});
        }
      }
    }));
    return observable;
  }
}
