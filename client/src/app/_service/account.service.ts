import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { User } from '../_models/user';
import { map } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
private http = inject (HttpClient);

baseurl = 'https://localhost:5001/';
currentUser = signal<User | null>(null);

login(model:any){
  return this.http.post<User>(this.baseurl + 'account/login' , model).pipe(
    map( user => {
      if (user){
        localStorage.setItem('user' , JSON.stringify(user));
        this.currentUser.set(user);
      }
    })
  )
}
register(model:any){
  return this.http.post<User>(this.baseurl + 'account/register' , model).pipe(
    map( user => {
      if (user){
        localStorage.setItem('user' , JSON.stringify(user));
        this.currentUser.set(user);
      }
      return user;
    })
  )
}
 
logout(){
  localStorage.removeItem('user');
    this.currentUser.set(null);
  
}
}
