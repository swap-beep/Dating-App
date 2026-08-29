import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavComponent } from './nav/nav.component';
import { AccountService } from './_service/account.service';
import { HomeComponent } from "./home/home.component";
import { NgxSpinner, NgxSpinnerComponent } from 'ngx-spinner';
import { SignalrNotificationService } from './_service/signalr-notification.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, CommonModule, NavComponent, HomeComponent,NgxSpinnerComponent],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
  standalone: true,
})
export class AppComponent implements OnInit {
 
  accountService = inject(AccountService);
  private signalrNotificationService = inject(SignalrNotificationService);
  
  ngOnInit(): void {
    this.setCurrentUser();
    this.signalrNotificationService.startConnection();
  }

  setCurrentUser() {
    const userString = localStorage.getItem('user');
    if (!userString) return;
    const user = JSON.parse(userString);
    this.accountService.currentUser.set(user);
  }

  
}
