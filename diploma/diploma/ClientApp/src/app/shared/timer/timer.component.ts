import { AsyncPipe } from '@angular/common';
import { Component, Input, OnChanges, OnDestroy, OnInit, Output, SimpleChanges } from '@angular/core';
import * as moment from 'moment';
import { Observable, map, timer } from 'rxjs';
import { Constants } from 'src/constants';

@Component({
  selector: 'app-timer',
  standalone: true,
  imports: [AsyncPipe],
  templateUrl: './timer.component.html',
  styleUrl: './timer.component.css'
})
export class TimerComponent implements OnInit, OnDestroy, OnChanges {
  @Input() until: Date = new Date();
  @Input() onEnd: () => void = () => {};

  timeLeftObservable: Observable<string> = timer(0, 1000).pipe(
    map(() => this.getTimeLeftString(Math.floor((new Date(this.until).getTime() - Date.now()) / 1000)))
  );

  public constructor() { }

  private endedTimeout: ReturnType<typeof setTimeout> | undefined;

  private getTimeLeftString(seconds: number): string {
    const hms = moment().startOf('day').seconds(seconds).format('HH:mm:ss')
    if (seconds < 24 * 60 * 60) {
      return hms;
    } else {
      const days = Math.floor(seconds / (24 * 60 * 60));
      return `${days}d ${hms}`;
    }
  }

  ngOnInit(): void {
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.until) {
      clearTimeout(this.endedTimeout);
      // if a value of more than MaxInt32 is passed, setTimeout's callback is invoked immediately
      const timeRemainingMs = new Date(this.until).getTime() - Date.now();
      this.endedTimeout = setTimeout(() => {
        this.onEnd();
      }, timeRemainingMs > Constants.MaxInt32 ? Constants.MaxInt32 : timeRemainingMs);
    }
  }

  ngOnDestroy(): void {
    clearTimeout(this.endedTimeout);
  }
}
