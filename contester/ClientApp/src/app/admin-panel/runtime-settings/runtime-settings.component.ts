import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MonacoEditorModule } from 'ngx-monaco-editor-v2';
import { forkJoin } from 'rxjs';
import { AllRuntimeSettings, ApplicationSettingsService } from '../../../generated/client';
import { Constants } from '../../../constants';
import { ToastsService } from '../../toasts/toasts.service';

type SettingKey = keyof AllRuntimeSettings;

@Component({
  selector: 'app-runtime-settings',
  standalone: true,
  imports: [FormsModule, TranslateModule, MonacoEditorModule],
  templateUrl: './runtime-settings.component.html',
  styleUrl: './runtime-settings.component.css'
})
export class RuntimeSettingsComponent implements OnInit {
  settings: AllRuntimeSettings | null = null;
  settingKeys: SettingKey[] = [];
  saving = false;

  readonly editorOptions = Constants.monacoDefaultOptions;
  private readonly markdownKeys = new Set<SettingKey>(['privacyPolicy']);

  constructor(
    private applicationSettingsService: ApplicationSettingsService,
    private toastsService: ToastsService,
    private translate: TranslateService,
  ) { }

  ngOnInit(): void {
    this.applicationSettingsService.apiApplicationSettingsRuntimeSettingsGet().subscribe(settings => {
      this.settings = settings;
      this.settingKeys = Object.keys(settings) as SettingKey[];
    });
  }

  isNumber(key: SettingKey): boolean {
    return typeof this.settings?.[key] === 'number';
  }

  isBoolean(key: SettingKey): boolean {
    return typeof this.settings?.[key] === 'boolean';
  }

  isMarkdown(key: SettingKey): boolean {
    return this.markdownKeys.has(key);
  }

  labelKey(key: SettingKey): string {
    return `adminPanel.runtimeSettings.fields.${String(key)}`;
  }

  save(): void {
    if (!this.settings || this.saving) return;

    this.saving = true;
    const requests = this.settingKeys.map(key =>
      this.applicationSettingsService.apiApplicationSettingsSettingSettingNamePatch(
        String(key),
        // HttpClient leaves string bodies unquoted; [FromBody] string needs a JSON string.
        JSON.stringify(String(this.settings![key] ?? ''))
      )
    );

    forkJoin(requests).subscribe({
      next: () => {
        this.saving = false;
        this.toastsService.show({
          header: this.translate.instant('adminPanel.runtimeSettings.savedHeader'),
          body: this.translate.instant('adminPanel.runtimeSettings.savedBody'),
          type: 'success'
        });
      },
      error: () => {
        this.saving = false;
      }
    });
  }
}
