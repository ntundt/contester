import { NgModule, ModuleWithProviders, SkipSelf, Optional } from '@angular/core';
import { Configuration } from './configuration';
import { HttpClient } from '@angular/common/http';


import { ApplicationSettingsService } from './api/applicationSettings.service';
import { AttachedFileService } from './api/attachedFile.service';
import { AttemptService } from './api/attempt.service';
import { AuthenticationService } from './api/authentication.service';
import { ContestService } from './api/contest.service';
import { ContestApplicationsService } from './api/contestApplications.service';
import { GradeAdjustmentsService } from './api/gradeAdjustments.service';
import { ProblemService } from './api/problem.service';
import { SchemaDescriptionService } from './api/schemaDescription.service';
import { ScoreboardService } from './api/scoreboard.service';
import { UserService } from './api/user.service';

@NgModule({
  imports:      [],
  declarations: [],
  exports:      [],
  providers: [
    ApplicationSettingsService,
    AttachedFileService,
    AttemptService,
    AuthenticationService,
    ContestService,
    ContestApplicationsService,
    GradeAdjustmentsService,
    ProblemService,
    SchemaDescriptionService,
    ScoreboardService,
    UserService ]
})
export class ApiModule {
    public static forRoot(configurationFactory: () => Configuration): ModuleWithProviders<ApiModule> {
        return {
            ngModule: ApiModule,
            providers: [ { provide: Configuration, useFactory: configurationFactory } ]
        };
    }

    constructor( @Optional() @SkipSelf() parentModule: ApiModule,
                 @Optional() http: HttpClient) {
        if (parentModule) {
            throw new Error('ApiModule is already loaded. Import in your base AppModule only.');
        }
        if (!http) {
            throw new Error('You need to import the HttpClientModule in your AppModule! \n' +
            'See also https://github.com/angular/angular/issues/20575');
        }
    }
}
