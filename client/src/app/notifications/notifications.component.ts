import { CommonModule, DatePipe } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { NotificationService } from '../_service/notification.service';

@Component({
  selector: 'app-notifications',
  imports: [CommonModule, DatePipe],
  templateUrl: './notifications.component.html',
  styleUrl: './notifications.component.css'
})
export class NotificationsComponent implements OnInit {
  private notificationService = inject(NotificationService);

  notifications = this.notificationService.notifications;

  ngOnInit(): void {
    this.notificationService.getNotifications().subscribe();
  }
}
