import {AuthenticationService, BeginSignUpCommand, ConfirmSignUpCommand, UserService} from "../generated/client";
import {map, tap} from "rxjs/operators";
import {BehaviorSubject, from, Observable, Subject} from "rxjs";
import {Injectable} from "@angular/core";

@Injectable({
  providedIn: 'root'
})
export class AuthorizationService {
  private accessToken: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(localStorage.getItem('accessToken'));

  public constructor(private authenticationService: AuthenticationService, private userService: UserService) {
    this.accessToken.subscribe(token => {
      if (token) localStorage.setItem('accessToken', token);
      else localStorage.removeItem('accessToken');
    });
  }

  private setAccessToken(accessToken: string | null) {
    this.accessToken.next(accessToken);
  }

  public isAuthenticated(): boolean {
    return !!this.accessToken.getValue();
  }

  public getAccessToken(): BehaviorSubject<string | null> {
    return this.accessToken;
  }

  public signIn(email: string, password: string): Observable<string | undefined> {
    return this.authenticationService.apiAuthSignInGet(email, password)
      .pipe(tap(res => {
        return this.setAccessToken(res.token ?? null);
      })).pipe(map(res => res.token));
  }

  public signOut(): void {
    this.setAccessToken(null);
  }

  public beginSignUp(command: BeginSignUpCommand): Observable<void> {
    return this.authenticationService.apiAuthBeginSignUpPost(command);
  }

  public confirmSignUp(command: ConfirmSignUpCommand): Observable<void> {
    return this.authenticationService.apiAuthConfirmSignUpPost(command);
  }
}
