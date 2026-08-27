import { CommonModule, NgIf, TitleCasePipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import {FormsModule} from '@angular/forms';
import { AccountService } from '../_service/account.service';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { computed } from '@angular/core';

@Component({
  selector: 'app-nav',
  imports: [FormsModule , BsDropdownModule , RouterLink , RouterLinkActive,CommonModule,TitleCasePipe],
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.css',
  standalone:true
})
export class NavComponent {
accountService = inject(AccountService);
private router = inject(Router)
private toastr = inject(ToastrService)
model: any={};

 // Computed signal to check if user is logged in
 isLoggedIn = computed(() => !!this.accountService.currentUser());

 login() {
   this.accountService.login(this.model).subscribe({
     next: () => {
       this.router.navigateByUrl('/members');
     },
     error: error => this.toastr.error(error.error)
   });
 }

 logout() {
   this.accountService.logout();
   this.router.navigateByUrl('/');
 }
}