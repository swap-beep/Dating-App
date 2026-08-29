import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AppNotification } from '../_models/notification';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  notifications = signal<AppNotification[]>([]);

  getNotifications() {
    return this.http.get<AppNotification[]>(this.baseUrl + 'user/notifications').pipe(
      tap(notifications => this.notifications.set(notifications))
    );
  }

  addNotification(notification: AppNotification) {
    this.notifications.update(list => [notification, ...list]);
  }

  refreshNotifications() {
    return this.getNotifications().subscribe();
  }
}
