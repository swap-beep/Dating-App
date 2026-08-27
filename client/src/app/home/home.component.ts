import { Component, inject } from '@angular/core';
import { RegisterComponent } from "../register/register.component";
import { HttpClient } from '@angular/common/http';
import { CommonModule, NgIf } from '@angular/common';

@Component({
  selector: 'app-home',
  imports: [RegisterComponent,CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent  {
registerMode = false;

 http = inject(HttpClient);


registerToggle(){
  this.registerMode = !this.registerMode;
}
cancelRegisterMode(event: boolean){
this.registerMode = event;
}

}
