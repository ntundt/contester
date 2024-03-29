import { Component, Input, OnDestroy, OnInit, Output } from '@angular/core';
import * as moment from 'moment';

@Component({
  selector: 'app-timer',
  standalone: true,
  imports: [],
  templateUrl: './timer.component.html',
  styleUrl: './timer.component.css'
})
export class TimerComponent implements OnInit, OnDestroy {
  @Input() until: Date = new Date();
  @Input() onEnd: () => void = () => {};

  timeLeftString: string = '';

  public constructor() { }

  private endedTimeout: ReturnType<typeof setTimeout> | undefined;
  private refreshTimerInterval: ReturnType<typeof setInterval> | undefined;

  @Output() refreshTimer() {
    const totalRemainingSeconds = Math.floor((new Date(this.until).getTime() - Date.now()) / 1000);
    const hms = moment().startOf('day').seconds(totalRemainingSeconds).format('HH:mm:ss')
    if (totalRemainingSeconds < 24 * 60 * 60) {
      this.timeLeftString = hms;
    } else {
      const days = Math.floor(totalRemainingSeconds / (24 * 60 * 60));
      this.timeLeftString = `${days}d ${hms}`;
    }
  }

  ngOnInit(): void {
    this.endedTimeout = setTimeout(() => {
      this.onEnd();
      clearInterval(this.refreshTimerInterval);
    }, new Date(this.until).getTime() - Date.now());

    this.refreshTimerInterval = setInterval(() => {
      if (new Date(this.until).getTime() > Date.now()) {
        this.refreshTimer();
      } else {
        clearInterval(this.refreshTimerInterval);
      }
    }, 1000);

    this.refreshTimer();
  }

  ngOnDestroy(): void {
    clearTimeout(this.endedTimeout);
    clearInterval(this.refreshTimerInterval);
  }
}
