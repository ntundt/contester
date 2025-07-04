import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import {ContestService} from "../../generated/client";
import {map, scan, tap} from "rxjs/operators";
import {PermissionsService} from "../../authorization/permissions.service";
import {merge, forkJoin} from "rxjs";

export const contestStartedGuard: CanActivateFn = (route, state) => {
  const contestService = inject(ContestService);
  const permissionsService = inject(PermissionsService);

  const hasPermission = permissionsService.hasPermissionObservable('ManageContests');

  const contestId = route.params.contestId;
  const contestGoingOn = contestService.apiContestsGet().pipe(
    map(contests => contests.contests?.find(contest => contest.id === contestId)),
    // @ts-ignore
    map(contest => (new Date(contest?.startDate)?.getTime() ?? 0) < Date.now()
      // @ts-ignore
      && (new Date(contest?.finishDate)?.getTime() ?? 0) > Date.now()),
  );

  return forkJoin([hasPermission, contestGoingOn]).pipe(
    map(([hasPermission, contestGoingOn]) => hasPermission || contestGoingOn),
  );
};
