import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { ScoreboardEntryDto } from 'src/generated/client';
import { environment } from 'src/environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ScoreboardUpdatesService {
  private hubConnection: signalR.HubConnection | undefined;

  constructor() { }

  public startConnection(contestId: string): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.basePath}/scoreboardUpdatesHub`)
      .build();
    
    this.hubConnection
      .start()
      .then(() => {
        this.hubConnection?.invoke('JoinContestGroup', contestId);
      });
  }

  public stopConnection(): void {
    this.hubConnection
      ?.stop();
  }

  public addListener(listener: (update: ScoreboardEntryDto[]) => void): void {
    this.hubConnection
      ?.on('ReceiveScoreboardUpdate', listener);
  }
}
