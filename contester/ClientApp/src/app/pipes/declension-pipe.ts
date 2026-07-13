import {inject, Pipe, PipeTransform} from '@angular/core';
import {LangSettingsService, SupportedLanguage} from "../services/lang-settings-service";

type Case = 'nom-singular' | 'gen-singular' | 'dat-singular' | 'acc-singular' | 'ins-singular' | 'pre-singular' | 'voc-singular'
  | 'nom-plural' | 'gen-plural' | 'dat-plural' | 'acc-plural' | 'ins-plural' | 'pre-plural' | 'voc-plural';

@Pipe({
  name: 'declension',
})
export class DeclensionPipe implements PipeTransform {
  private langSettings = inject(LangSettingsService);

  declineEn(numeral: number): Case {
    if (!Number.isInteger(numeral)) return 'nom-plural';
    if (numeral === 1) return 'nom-singular';
    return 'nom-plural';
  }

  declineEastSlavic(numeral: number, lang: 'ru' | 'by'): Case {
    if (!Number.isInteger(numeral)) return 'gen-plural';

    const n = Math.abs(numeral);
    const lastTwo = n % 100;
    const last = n % 10;

    if (lastTwo >= 11 && lastTwo <= 14) {
      return 'gen-plural';
    }

    if (last === 1) {
      return 'nom-singular';
    }

    if (last >= 2 && last <= 4) {
      return ({
        'ru': 'gen-singular',
        'by': 'nom-plural',
      })[lang] as Case;
    }

    return 'gen-plural';
  }

  decline(numeral: number, lang: SupportedLanguage): Case {
    if (lang === 'en') return this.declineEn(numeral);
    return this.declineEastSlavic(numeral, lang);
  }

  transform(value: string, ...args: unknown[]): string {
    const lang = this.langSettings.getLang();
    const numeral = args[0] as number;
    return `${value}.${this.decline(numeral, lang)}`;
  }

}
