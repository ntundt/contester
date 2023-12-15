import { Component } from '@angular/core';
import { NgIf } from "@angular/common";
import {
  NgbActiveModal,
  NgbNav,
  NgbNavContent,
  NgbNavItem,
  NgbNavItemRole,
  NgbNavLink, NgbNavOutlet
} from "@ng-bootstrap/ng-bootstrap";
import {FormsModule} from "@angular/forms";

export interface AddFileModalResult {
  dbms: string;
  description?: string;
  sourceDbms?: string;
}

@Component({
  selector: 'app-add-file-modal',
  standalone: true,
  imports: [NgIf, NgbNav, NgbNavContent, NgbNavItemRole, NgbNavLink, NgbNavItem, NgbNavOutlet, FormsModule],
  templateUrl: './add-file-modal.component.html',
  styleUrl: './add-file-modal.component.css'
})
export class AddFileModalComponent {
  public targetDbms: string = 'SqlServer';
  public description: string | undefined;
  public sourceDbms?: string;

  public constructor(public activeModal: NgbActiveModal) { }

  public closeModal() {
    this.activeModal.close();
  }

  public fileChanged(event: Event) {
    const target = event.target as HTMLInputElement;
    const file = target.files?.item(0);
    if (file) {
      const reader = new FileReader();
      reader.onload = () => {
        this.description = reader.result as string;
      }
      reader.readAsText(file);
    }
  }

  public save() {
    this.activeModal.close({
      dbms: this.targetDbms,
      description: this.description,
      sourceDbms: this.sourceDbms,
    } as AddFileModalResult);
  }
}
