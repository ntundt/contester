import { Component } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  title = 'app';

  getSelectedLanguage() {
    const lang = localStorage.getItem('language');
    return lang ?? 'ru';
  }

  setLanguage(lang: string) {
    localStorage.setItem('language', lang);
  }
  
  constructor(
    private translate: TranslateService,
  ) {
    this.translate.setDefaultLang(this.getSelectedLanguage());
  }
}
