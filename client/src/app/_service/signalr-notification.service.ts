import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { environment } from '../../environments/environment';
import { AccountService } from './account.service';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root'
})
export class SignalrNotificationService {
  private accountService = inject(AccountService);
  private notificationService = inject(NotificationService);
  private toastr = inject(ToastrService);

  private hubConnection?: signalR.HubConnection;

  startConnection() {
    if (!this.accountService.isAuthenticated()) {
      return;
    }

    if (this.hubConnection) {
      return;
    }

    const token = this.accountService.currentUser()?.token;
    const apiUrl = `${environment.apiUrl.replace(/\/$/, '')}/hubs/notifications`;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(apiUrl, {
        accessTokenFactory: () => token ?? ''
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveNotification', (notification: { message: string; type: string }) => {
      this.toastr.info(notification.message, notification.type === 'match' ? 'New Match' : 'New Like');
      this.notificationService.refreshNotifications();
    });

    this.hubConnection.start().catch(error => console.error('SignalR connection error:', error));
  }
}
