import { Component, inject, OnInit, output } from '@angular/core';
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { AccountService } from '../_service/account.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-register',
  imports: [ReactiveFormsModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.css'
})
export class RegisterComponent implements OnInit {

  private accountService = inject(AccountService);
//userFromHomeComponent = input.required<any>();
// @Output() cancelRegister = new EventEmitter();
private toastr = inject(ToastrService)
cancelRegister = output<boolean>();
model :any = {}
protected registerForm : FormGroup=new FormGroup({});

  ngOnInit(): void {
    this.initializeForm();
  }

  initializeForm(){
    this.registerForm = new FormGroup({

username: new FormControl('' , Validators.required),
password: new FormControl('',[Validators.required , Validators.minLength(5),Validators.maxLength(10)]),
confirmPassword: new FormControl('',[Validators.required ,this.matchValues('password')])
    }) ;
  this.registerForm.controls['password'].valueChanges.subscribe(()=>
  
  {
    this.registerForm.controls['confirmPassword'].updateValueAndValidity();
  })
  
  }


    matchValues(matchTo: string) : ValidatorFn{

return (control : AbstractControl): ValidationErrors|null =>{
  const parent = control.parent;
  if(!parent) return null;

  const matchValue = parent.get(matchTo)?.value;
  return control.value === matchValue ? null : {passwordMismatch: true}
}

    }

register(){
  if (this.registerForm.invalid) return;

  this.accountService.register(this.registerForm.value).subscribe({
    next: () => this.cancelRegister.emit(false),
    error: error => this.toastr.error(error.error)
  });
}

cancel(){
 this.cancelRegister.emit(false);
}
}
