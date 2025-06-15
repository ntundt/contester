import {Component, OnInit} from '@angular/core';
import {TranslateModule} from "@ngx-translate/core";
import {ApplicationSettingsService, AuthenticationService, ConnectionString} from "../../../generated/client";
import {ConnectionStringComponent} from "./connection-string/connection-string.component";
import {NgForOf} from "@angular/common";
import {faPlus} from "@fortawesome/free-solid-svg-icons";
import {FaIconComponent} from "@fortawesome/angular-fontawesome";
import {NgbModal} from "@ng-bootstrap/ng-bootstrap";
import {AddConnectionStringModalComponent} from "./add-connection-string-modal/add-connection-string-modal.component";

@Component({
  selector: 'app-connection-strings',
  standalone: true,
  imports: [TranslateModule, ConnectionStringComponent, NgForOf, FaIconComponent],
  templateUrl: './connection-strings.component.html',
  styleUrl: './connection-strings.component.css'
})
export class ConnectionStringsComponent implements OnInit {
  public constructor(
    private applicationSettingsService: ApplicationSettingsService,
    private modalService: NgbModal,
  ) { }

  public connectionStrings: ConnectionString[] = [];

  private fetchConnectionStrings() {
    this.applicationSettingsService.apiApplicationSettingsConnectionStringGet().subscribe(connectionStrings => {
      this.connectionStrings = connectionStrings;
    })
  }

  add() {
    const modalRef = this.modalService.open(AddConnectionStringModalComponent);
    modalRef.closed.subscribe((res: any) => {
      if (res?.success) {
        this.fetchConnectionStrings();
      }
    });
  }

  onConnectionStringDeleted() {
    this.fetchConnectionStrings();
  }

  ngOnInit() {
    this.fetchConnectionStrings();
  }

  protected readonly faPlus = faPlus;
}
