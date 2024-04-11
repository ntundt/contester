import { AsyncPipe } from '@angular/common';
import { Component, Input, OnDestroy, OnInit, Output } from '@angular/core';
import * as moment from 'moment';
import { Observable, map, timer } from 'rxjs';

@Component({
  selector: 'app-timer',
  standalone: true,
  imports: [AsyncPipe],
  templateUrl: './timer.component.html',
  styleUrl: './timer.component.css'
})
export class TimerComponent implements OnInit, OnDestroy {
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
    this.endedTimeout = setTimeout(this.onEnd, this.until.getTime() - Date.now());
  }

  ngOnDestroy(): void {
    clearTimeout(this.endedTimeout);
  }
}
