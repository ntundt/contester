import {AfterViewInit, Component, ElementRef, Input, OnDestroy, OnInit, ViewChild} from '@angular/core';
import {NgbActiveModal} from "@ng-bootstrap/ng-bootstrap";
import {TranslatePipe} from "@ngx-translate/core";
import {FormsModule} from "@angular/forms";

@Component({
  selector: 'app-rename-modal',
  imports: [
    TranslatePipe,
    FormsModule
  ],
  templateUrl: './rename-modal.html',
  styleUrl: './rename-modal.css',
})
export class RenameModal implements AfterViewInit {
  @Input({ required: true }) name!: string;
  @Input({ required: true }) title!: string;

  @ViewChild('nameInput')
  private nameInput!: ElementRef<HTMLInputElement>;

  public constructor(
    protected activeModalService: NgbActiveModal,
  ) { }

  rename(): void {
    this.activeModalService.close(this.name.trim());
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.nameInput.nativeElement.focus();
      this.nameInput.nativeElement.select();
    });
  }

}
