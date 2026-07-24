import { Injectable } from '@angular/core';
import {Subject} from "rxjs";

@Injectable({ providedIn: 'root' })
export class ContestEventsService {
  private problemsChangedSubject = new Subject<void>();

  problemsChanged$ = this.problemsChangedSubject.asObservable();

  problemsChanged() {
    this.problemsChangedSubject.next();
  }
}
