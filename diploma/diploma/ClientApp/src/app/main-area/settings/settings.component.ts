import { Component } from '@angular/core';
import {ContestDto, ContestService} from "../../../generated/client";
import {ActivatedRoute} from "@angular/router";
import {FormsModule} from "@angular/forms";
import {ToastsService} from "../../toasts/toasts.service";

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [
    FormsModule
  ],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.css'
})
export class SettingsComponent {
  public contest: ContestDto = {
    name: '',
    startDate: new Date(),
    finishDate: new Date(),
    isPublic: false,
  };

  public constructor(private contestService: ContestService, private activatedRoute: ActivatedRoute, private toastsService: ToastsService) { }

  public ngOnInit(): void {
    this.activatedRoute.parent?.params.subscribe(params => {
      this.contestService.apiContestsGet(params['contestId']).subscribe(contest => {
        const targetContest = contest.contests?.find(c => c.id === params['contestId']);
        if (targetContest) {
          this.contest = targetContest;
        }
      });
    });
  }

  public save() {
    this.contestService.apiContestsContestIdPut(this.contest.id ?? '', this.contest).subscribe(() => {
      this.toastsService.show({
        header: 'Success',
        body: 'Contest settings saved',
        delay: 5000,
      })
    });
  }
}
