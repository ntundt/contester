import { NgFor } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-application-settings',
  standalone: true,
  imports: [
    TranslateModule,
    NgFor,
    FormsModule,
  ],
  templateUrl: './application-settings.component.html',
  styleUrl: './application-settings.component.css'
})
export class ApplicationSettingsComponent implements OnInit {
  languages = [
    'en',
    'ru',
    'by'
  ];

  constructor(
    public translate: TranslateService,
  ) { }

  ngOnInit(): void {
  }

  currentLanguage = localStorage.getItem('language') ?? 'ru';

  saveChanges() {
    localStorage.setItem('language', this.currentLanguage);
    this.translate.use(this.currentLanguage);
  }
}
