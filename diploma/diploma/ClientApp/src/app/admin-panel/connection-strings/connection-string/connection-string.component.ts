import {Component, Input, OnInit} from '@angular/core';
import {ApplicationSettingsService, ConnectionString} from "../../../../generated/client";
import {FaIconComponent} from "@fortawesome/angular-fontawesome";
import {faCheck, faXmark} from "@fortawesome/free-solid-svg-icons";
import {NgIf} from "@angular/common";
import {NgbModal, NgbPopover} from "@ng-bootstrap/ng-bootstrap";
import {
  DeleteConfirmationModalComponent
} from "../../../shared/delete-confirmation-modal/delete-confirmation-modal.component";

enum HealthCheckStatus {
  Pending,
  Healthy,
  Unhealthy,
}

@Component({
  selector: '[app-connection-string]',
  standalone: true,
  imports: [
    FaIconComponent,
    NgIf,
    NgbPopover
  ],
  templateUrl: './connection-string.component.html',
  styleUrl: './connection-string.component.css'
})
export class ConnectionStringComponent implements OnInit {
  @Input({required: true})
  public connectionString: ConnectionString = {};

  public healthCheckStatus: HealthCheckStatus = HealthCheckStatus.Pending;
  public elapsedMs: number = -1;
  public errorMessage: string = '';

  @Input({required:true})
  public onDeleteCallback: () => void = () => {};

  public constructor(
    private applicationSettingsService: ApplicationSettingsService,
    private modalService: NgbModal,
  ) { }

  remove() {
    const modalRef = this.modalService.open(DeleteConfirmationModalComponent);
    modalRef.result.then((result: string | undefined) => {
      if (!result) return;
      this.applicationSettingsService.apiApplicationSettingsConnectionStringConnectionStringIdDelete(this.connectionString.id!).subscribe(() => {
        this.onDeleteCallback();
      });
    });
  }

  ngOnInit() {
    this.applicationSettingsService.apiApplicationSettingsConnectionStringConnectionStringIdHealthCheckGet(this.connectionString.id!)
      .subscribe(result => {
        this.healthCheckStatus = result.success ? HealthCheckStatus.Healthy : HealthCheckStatus.Unhealthy;
        this.elapsedMs = result.elapsedMilliseconds!;
        this.errorMessage = result.message!;
      });
  }

  protected readonly HealthCheckStatus = HealthCheckStatus;
  protected readonly faCheck = faCheck;
  protected readonly faXmark = faXmark;
}
