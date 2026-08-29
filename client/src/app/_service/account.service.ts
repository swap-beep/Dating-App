import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { User } from '../_models/user';
import { map } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AccountService {
private http = inject (HttpClient);

baseurl = environment.apiUrl;
currentUser = signal<User | null>(null);

login(model:any){
  return this.http.post<User>(this.baseurl + 'account/login' , model).pipe(
    map( user => {
      if (user){
       this.setCurrentUser(user);
      }
    })
  )
}
constructor() {
  const userJson = localStorage.getItem('user');
  if (userJson) {
    this.currentUser.set(JSON.parse(userJson));
  }
}

register(model:any){
  return this.http.post<User>(this.baseurl + 'account/register' , model);
}

isAuthenticated(): boolean {
  const user = this.currentUser();
  if (!user || this.isTokenExpired(user.token)) {
    if (user) this.logout();
    return false;
  }
  return true;
}

private isTokenExpired(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return typeof payload.exp !== 'number' || payload.exp * 1000 <= Date.now();
  } catch {
    return true;
  }
}

setCurrentUser(user:User){

  localStorage.setItem('user',JSON.stringify(user));
  this.currentUser.set(user);
}
 
logout(){
  localStorage.removeItem('user');
    this.currentUser.set(null);
  
}
}
