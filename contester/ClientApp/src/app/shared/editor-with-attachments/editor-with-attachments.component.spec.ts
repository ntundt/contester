import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditorWithAttachmentsComponent } from './editor-with-attachments.component';

describe('EditorWithAttachmentsComponent', () => {
  let component: EditorWithAttachmentsComponent;
  let fixture: ComponentFixture<EditorWithAttachmentsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditorWithAttachmentsComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(EditorWithAttachmentsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
