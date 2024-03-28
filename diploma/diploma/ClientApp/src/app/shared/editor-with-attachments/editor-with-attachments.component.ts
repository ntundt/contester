import { Component, EventEmitter, Input, Output, forwardRef } from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR, NgModel } from '@angular/forms';
import { FontAwesomeModule } from '@fortawesome/angular-fontawesome';
import { faPaperclip } from '@fortawesome/free-solid-svg-icons';
import { MonacoEditorModule } from 'ngx-monaco-editor-v2';
import { Constants } from 'src/constants';
import { AttachedFileService } from 'src/generated/client';

@Component({
  selector: 'app-editor-with-attachments',
  standalone: true,
  imports: [
    FontAwesomeModule,
    MonacoEditorModule,
    FormsModule,
  ],
  templateUrl: './editor-with-attachments.component.html',
  styleUrl: './editor-with-attachments.component.css',
  providers: []
})
export class EditorWithAttachmentsComponent{
  @Input('value') public value: string | undefined = '';
  @Output() public valueChange = new EventEmitter<string>();

  public editorOptions = Constants.monacoDefaultOptions;
  private editorInstance: any;

  public constructor(
    private readonly attachedFileService: AttachedFileService,
  ) { }

  public onAttachFile(): void {
    const reader = new FileReader();
    const fileInput = document.createElement('input');
    reader.onload = () => {
      if (!reader.result) return;
      const file = new Blob([reader.result], { type: 'text/plain',  });
      const originalFileName = fileInput.files?.item(0)?.name;
      this.attachedFileService.apiFilePostForm(file, originalFileName).subscribe(res => {
        const url = res.fileUrl;
        this.editorInstance.trigger('keyboard', 'type', { text: `[${originalFileName}](${url})` });
      });
    }
    fileInput.type = 'file';
    fileInput.onchange = () => {
      const file = fileInput.files?.item(0);
      if (file) {
        reader.readAsText(file);
      }
    }

    fileInput.click();
  }

  public onEditorInit(editor: any): void {
    this.editorInstance = editor;
  }

  protected faPaperclip = faPaperclip;
}
