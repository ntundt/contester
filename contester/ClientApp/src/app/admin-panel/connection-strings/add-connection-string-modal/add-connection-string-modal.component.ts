import { Component } from '@angular/core';
import {TranslateModule} from "@ngx-translate/core";
import {ApplicationSettingsService} from "../../../../generated/client";
import {NgbActiveModal} from "@ng-bootstrap/ng-bootstrap";
import {FormsModule} from "@angular/forms";

@Component({
  selector: 'app-add-connection-string-modal',
  standalone: true,
  imports: [
    TranslateModule,
    FormsModule,
  ],
  templateUrl: './add-connection-string-modal.component.html',
  styleUrl: './add-connection-string-modal.component.css'
})
export class AddConnectionStringModalComponent {
  private dbms: string[] = [
    'SqlServer',
    'Oracle',
    'Postgres',
  ];

  protected selectedDbms: string = this.dbms[0];
  protected connectionString: string = '';

  public constructor(
    private applicationSettingsService: ApplicationSettingsService,
    private activeModal: NgbActiveModal,
  ) { }

  getAllowedDbms() {
    return this.dbms;
  }

  save() {
    this.applicationSettingsService.apiApplicationSettingsConnectionStringPost(this.connectionString, this.selectedDbms)
      .subscribe(result => {
        if (result.success) {
          this.activeModal.close({
            success: true,
          });
        }
      });
  }

  cancel() {
    this.activeModal.dismiss();
  }
}
