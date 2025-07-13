import { Injectable } from '@angular/core';

export interface ToastInfo {
  header: string;
  body: string;
  delay?: number;
  type?: string;
}

@Injectable({
  providedIn: 'root'
})
export class ToastsService {
  public toasts: Array<ToastInfo> = [];

  constructor() { }

  public show(toast: ToastInfo) {
    this.toasts.push(toast);
  }

  public remove(toast: ToastInfo) {
    this.toasts = this.toasts.filter(t => t != toast);
  }

  public getToasts(): Array<ToastInfo> {
    return this.toasts;
  }
}
