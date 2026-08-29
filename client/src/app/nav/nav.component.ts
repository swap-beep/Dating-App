import { CommonModule, TitleCasePipe } from '@angular/common';
import { Component, computed, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../_service/account.service';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { NotificationService } from '../_service/notification.service';
import { SignalrNotificationService } from '../_service/signalr-notification.service';

@Component({
  selector: 'app-nav',
  imports: [FormsModule , BsDropdownModule , RouterLink , RouterLinkActive,CommonModule,TitleCasePipe],
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.css',
  standalone:true
})
export class NavComponent {
  accountService = inject(AccountService);
  private router = inject(Router);
  private toastr = inject(ToastrService);
  private notificationService = inject(NotificationService);
  private signalrNotificationService = inject(SignalrNotificationService);
  model: any = {};

  notifications = this.notificationService.notifications;
  unreadCount = computed(() => this.notifications().filter(n => !n.isRead).length);

  isLoggedIn = computed(() => !!this.accountService.currentUser());

  constructor() {
    if (this.accountService.isAuthenticated()) {
      this.notificationService.getNotifications().subscribe({
        next: notifications => {
          notifications.slice(0, 3).forEach(notification => {
            this.toastr.info(notification.message, notification.type === 'match' ? 'New Match' : 'New Like');
          });
        }
      });
      this.signalrNotificationService.startConnection();
    }
  }

  login() {
   this.accountService.login(this.model).subscribe({
     next: () => {
       this.router.navigateByUrl('/members');
       this.notificationService.getNotifications().subscribe();
       this.signalrNotificationService.startConnection();
     },
     error: error => this.toastr.error(error.error)
   });
 }

 logout() {
   this.accountService.logout();
   this.router.navigateByUrl('/');
 }
}