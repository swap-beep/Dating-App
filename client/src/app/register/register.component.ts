import { Component, EventEmitter, inject, input, Input, output, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../_service/account.service';

@Component({
  selector: 'app-register',
  imports: [FormsModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent {
  private accountService = inject(AccountService);
//userFromHomeComponent = input.required<any>();
// @Output() cancelRegister = new EventEmitter();

cancelRegister = output<boolean>();
model :any = {}

register(){
  this.accountService.register(this.model).subscribe({
    next: response => {
      console.log(response)
    },
    error : error=> console.log(error)
  })
}

cancel(){
 this.cancelRegister.emit(false);
}
}
