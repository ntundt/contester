import { BrowserModule } from '@angular/platform-browser';
import {ChangeDetectorRef, NgModule} from '@angular/core';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS, HttpClient } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { LoginScreenComponent } from './login-screen/login-screen.component';
import {
  ApplicationSettingsService,
  AttachedFileService,
  AttemptService,
  AuthenticationService,
  BASE_PATH, ContestApplicationsService, ContestService,
  GradeAdjustmentsService,
  ProblemService,
  SchemaDescriptionService, ScoreboardService, UserService
} from "../generated/client";
import {environment} from "../environments/environment";
import { ContestComponent } from './main-area/contest.component';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import {ProblemsComponent} from "./main-area/problems/problems.component";
import {AuthorizationInterceptor} from "../authorization/authorization.interceptor";
import {AuthorizationService} from "../authorization/authorization.service";
import {ProblemComponent} from "./main-area/problem/problem.component";
import {provideMarkdown} from "ngx-markdown";
import {CodeEditorModule} from "@ngstack/code-editor";
import {ScoreboardComponent} from "./main-area/scoreboard/scoreboard.component";
import {SchemasComponent} from "./main-area/schemas/schemas.component";
import {ParticipantsComponent} from "./main-area/participants/participants.component";
import {SettingsComponent} from "./main-area/settings/settings.component";
import {AddFileModalComponent} from "./main-area/schemas/add-file-modal/add-file-modal.component";
import {NgbActiveModal, NgbDropdown} from "@ng-bootstrap/ng-bootstrap";
import {AttemptsComponent} from "./main-area/attempts/attempts.component";
import {ConfirmSignUpComponent} from "./confirm-sign-up/confirm-sign-up.component";
import {ContestsComponent} from "./contests/contests.component";
import {EditProblemComponent} from "./main-area/edit-problem/edit-problem.component";
import {ToastsComponent} from "./toasts/toasts.component";
import {AccountControlComponent} from "./nav-menu/account-control/account-control.component";
import {PermissionsService} from "../authorization/permissions.service";
import {PasswordResetComponent} from "./password-reset/password-reset.component";
import {ErrorsInterceptor} from "./errors/errors.interceptor";
import {ProfileComponent} from "./profile/profile.component";
import {contestStartedGuard} from "./guards/contest-started.guard";
import { FinalScoreboardComponent } from './final-scoreboard/final-scoreboard.component';
import { MonacoEditorModule } from 'ngx-monaco-editor-v2';
import { ContestApplicationComponent } from './contest-application/contest-application.component';
import { TimerComponent } from './shared/timer/timer.component';
import { ProblemAttemptsComponent } from './main-area/problem/problem-attempts/problem-attempts.component';
import { SignUpScreenComponent } from './sign-up-screen/sign-up-screen.component';
import { ApplicationSettingsComponent } from './application-settings/application-settings.component';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { AdminPanelComponent } from './admin-panel/admin-panel.component';
import { EnterEmailConfirmationCodeComponent } from './enter-email-confirmation-code/enter-email-confirmation-code.component';
import { FooterComponent } from "./footer/footer.component";
import { ResultSetViewerComponent } from "./result-set-viewer/result-set-viewer.component";
import {APP_BASE_HREF} from "@angular/common";
import {UsersControlComponent} from "./admin-panel/users-control/users-control.component";
import {ConnectionStringsComponent} from "./admin-panel/connection-strings/connection-strings.component";

function HttpLoaderFactory(http: HttpClient) {
  return new TranslateHttpLoader(http,
    './assets/i18n/',
    '.json');
}

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    LoginScreenComponent,
    ContestComponent,
    ContestApplicationComponent,
  ],
  imports: [
    BrowserModule.withServerTransition({appId: 'ng-cli-universal'}),
    HttpClientModule,
    TranslateModule.forRoot({
      defaultLanguage: 'en',
      loader: {
        provide: TranslateLoader,
        useFactory: HttpLoaderFactory,
        deps: [HttpClient],
      },
    }),
    FormsModule,
    RouterModule.forRoot([
      {path: 'login', component: LoginScreenComponent},
      {path: 'sign-up', component: SignUpScreenComponent},
      {path: 'confirm-sign-up', component: ConfirmSignUpComponent},
      {
        path: 'admin-panel', component: AdminPanelComponent, children: [
          {path:'users-control', component: UsersControlComponent},
          {path:'connection-strings', component: ConnectionStringsComponent},
        ]
      },
      {path: '', component: ContestsComponent, pathMatch: 'full'},
      {path: 'reset-password', component: PasswordResetComponent},
      {path: 'profile', component: ProfileComponent},
      {path: 'scoreboard/:contestId', component: FinalScoreboardComponent},
      {path: 'enter-email-confirmation-code', component: EnterEmailConfirmationCodeComponent},
      {
        path: 'contest/:contestId', component: ContestComponent, children: [
          {path: 'schemas', component: SchemasComponent},
          {path: 'problems', component: ProblemsComponent},
          {path: 'attempts', component: AttemptsComponent},
          {path: 'participants', component: ParticipantsComponent},
          {path: 'scoreboard', component: ScoreboardComponent},
          {path: 'settings', component: SettingsComponent},
          {path: 'problems/:problemId', component: ProblemComponent},
          {path: 'problems/:problemId/edit', component: EditProblemComponent}
        ]
      },
      {path: 'attempts/:attemptId/result-set', component: ResultSetViewerComponent},
      {path: 'contest-application/:contestId', component: ContestApplicationComponent},
      {path: 'settings', component: ApplicationSettingsComponent},
    ], {paramsInheritanceStrategy: 'always'}),
    ReactiveFormsModule,
    FontAwesomeModule,
    CodeEditorModule.forRoot(),
    MonacoEditorModule.forRoot(),
    AddFileModalComponent,
    ToastsComponent,
    AccountControlComponent,
    TimerComponent,
    ProblemAttemptsComponent,
    ApplicationSettingsComponent,
    FooterComponent,
  ],
  providers: [
    { provide: BASE_PATH, useValue: environment.basePath },
    { provide: APP_BASE_HREF, useValue: environment.appBaseHref },
    { provide: HTTP_INTERCEPTORS, useClass: AuthorizationInterceptor, multi: true },
    { provide: HTTP_INTERCEPTORS, useClass: ErrorsInterceptor, multi: true },
    AuthenticationService,
    AuthorizationService,
    ProblemService,
    SchemaDescriptionService,
    AttemptService,
    ScoreboardService,
    ContestService,
    NgbActiveModal,
    NgbDropdown,
    UserService,
    PermissionsService,
    GradeAdjustmentsService,
    AttachedFileService,
    ContestApplicationsService,
    ApplicationSettingsService,
    provideMarkdown(),
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
