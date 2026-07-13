import {inject, Injectable} from '@angular/core';
import {TranslateService} from "@ngx-translate/core";

export type SupportedLanguage = 'ru' | 'by' | 'en';

@Injectable({
  providedIn: 'root',
})
export class LangSettingsService {
  translateService = inject(TranslateService);

  currentLanguage: SupportedLanguage = (localStorage.getItem('language') ?? 'ru') as SupportedLanguage;

  public setLang(lang: SupportedLanguage): void {
    this.currentLanguage = lang;
    localStorage.setItem('language', this.currentLanguage);
    this.translateService.use(this.currentLanguage);
  }

  public getLang(): SupportedLanguage {
    return this.currentLanguage;
  }
}
