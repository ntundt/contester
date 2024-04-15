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
    //'by'
  ];

  constructor(
    public translate: TranslateService,
  ) { }

  ngOnInit(): void {
  }

  get currentLanguage() {
    return localStorage.getItem('language') ?? 'ru';
  }

  changeLanguage(eventTarget: EventTarget | null) {
    const targetLanguage = (eventTarget as HTMLSelectElement)?.value;
    if (!targetLanguage) return;
    localStorage.setItem('language', targetLanguage);
    this.translate.use(targetLanguage);
  }
}
