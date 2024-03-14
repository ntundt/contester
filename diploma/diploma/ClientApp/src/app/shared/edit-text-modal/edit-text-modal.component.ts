import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { MonacoEditorModule } from 'ngx-monaco-editor-v2';
import { Constants } from 'src/constants';
import { EditorWithAttachmentsComponent } from '../editor-with-attachments/editor-with-attachments.component';

@Component({
  selector: 'app-edit-text-modal',
  standalone: true,
  imports: [
    MonacoEditorModule,
    FormsModule,
    EditorWithAttachmentsComponent,
  ],
  templateUrl: './edit-text-modal.component.html',
  styleUrl: './edit-text-modal.component.css'
})
export class EditTextModalComponent {
  public editorOptions = Constants.monacoDefaultOptions;

  @Input() public text: string = '';
  
  public constructor(private activeModal: NgbActiveModal) { }

  public save() {
    this.activeModal.close(this.text);
  }

  public close() {
    this.activeModal.dismiss();
  }
}
