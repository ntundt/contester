import {Component, Input, OnInit} from '@angular/core';
import {CodeEditorModule, CodeModel} from "@ngstack/code-editor";
import {NgbActiveModal} from "@ng-bootstrap/ng-bootstrap";
import {AttemptService, SingleAttemptDto} from "../../../generated/client";
import {NgIf} from "@angular/common";

@Component({
  selector: 'app-attempt-src-view-modal',
  standalone: true,
  imports: [
    CodeEditorModule,
    NgIf
  ],
  templateUrl: './attempt-src-view-modal.component.html',
  styleUrl: './attempt-src-view-modal.component.css'
})
export class AttemptSrcViewModalComponent implements OnInit {
  public srcCodeModel: CodeModel = { language: 'sql', value: '', uri: '1' };
  public attempt: SingleAttemptDto | undefined;

  @Input() public attemptId: string | undefined;

  public constructor(
    public activeModal: NgbActiveModal,
    public attemptService: AttemptService,
  ) { }

  public confirm() {
    this.activeModal.close();
  }

  public ngOnInit() {
    this.attemptService.apiAttemptsAttemptIdGet(this.attemptId ?? '').subscribe(res => {
      this.srcCodeModel = {
        ...this.srcCodeModel,
        value: res.solution ?? '',
      };
      this.attempt = res;
    });
  }
}
