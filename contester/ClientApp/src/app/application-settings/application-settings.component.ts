import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import {LangSettingsService, SupportedLanguage} from "../services/lang-settings-service";

@Component({
  selector: 'app-application-settings',
  standalone: true,
  imports: [
    TranslateModule,
    FormsModule,
  ],
  templateUrl: './application-settings.component.html',
  styleUrl: './application-settings.component.css'
})
export class ApplicationSettingsComponent implements OnInit {
  languages: SupportedLanguage[] = [
    'en',
    'ru',
    'by'
  ];

  constructor(
    public langSettings: LangSettingsService,
  ) { }

  currentLanguage: SupportedLanguage = 'ru';

  ngOnInit(): void {
    this.currentLanguage = this.langSettings.getLang();
  }

  saveChanges() {
    this.langSettings.setLang(this.currentLanguage);
  }
}
