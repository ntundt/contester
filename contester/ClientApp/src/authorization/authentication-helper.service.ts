import {
  AuthenticationService, Configuration,
  SignInResult,
} from "../generated/client";
import {BehaviorSubject} from "rxjs";
import {Injectable} from "@angular/core";
import {map, tap} from "rxjs/operators";
import {HttpBackend, HttpClient} from "@angular/common/http";
import {environment} from "../environments/environment";

export interface Credentials {
  accessToken?: string;
  accessTokenExpiry?: string;
  refreshToken?: string;
  refreshTokenExpiry?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthenticationHelperService {
  private authenticationService: AuthenticationService;

  private credentials: BehaviorSubject<Credentials | null> = new BehaviorSubject<Credentials | null>(
    !!localStorage.getItem('credentials') ? JSON.parse(localStorage.getItem('credentials')!) : null
  );

  private isRenewingCredentials: BehaviorSubject<boolean> = new BehaviorSubject<boolean>(false);

  public constructor(
    private httpBackend: HttpBackend
  ) {
    const rawHttpClient = new HttpClient(this.httpBackend);
    const config = new Configuration({});
    this.authenticationService = new AuthenticationService(rawHttpClient, environment.basePath, config);

    this.credentials.subscribe(credentials => {
      if (!!credentials) localStorage.setItem('credentials', JSON.stringify(credentials));
      else localStorage.removeItem('credentials');
    });
  }

  public setCredentials(signInResult: SignInResult | null) {
    if (signInResult == null) {
      this.credentials.next(null);
      return null;
    }

    const credentials: Credentials = {
      accessToken: signInResult.accessToken,
      refreshToken: signInResult.refreshToken,
      accessTokenExpiry: new Date(Date.now() + signInResult.accessTokenTtlSeconds! * 1000).toUTCString(),
      refreshTokenExpiry: new Date(Date.now() + signInResult.refreshTokenTtlSeconds! * 1000).toUTCString(),
    };

    this.credentials.next(credentials);

    return credentials;
  }

  public isAuthenticated(): boolean {
    return !!this.credentials.getValue();
  }

  public getCredentials(): BehaviorSubject<Credentials | null> {
    return this.credentials;
  }

  public getIsRenewingCredentials(): BehaviorSubject<boolean> {
    return this.isRenewingCredentials;
  }

  public renewCredentials() {
    if (!this.isAuthenticated()) throw new Error('Not authenticated, nothing to renew');

    this.isRenewingCredentials.next(true);

    return this.authenticationService.apiAuthRenewCredentialsGet(this.credentials.getValue()?.refreshToken).pipe(map(
      (credentials) => this.setCredentials(credentials),
    ), tap(
      () => this.isRenewingCredentials.next(false)
    ));
  }

  public signOut(): void {
    this.setCredentials(null);
  }
}
