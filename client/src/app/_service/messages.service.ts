import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { map, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { Member } from '../_models/Member';
import { Message } from '../_models/message';

@Injectable({
  providedIn: 'root'
})
export class MessagesService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  matches = signal<Member[]>([]);
  messages = signal<Message[]>([]);

  getMatches() {
    return this.http.get<Member[]>(this.baseUrl + 'user/matches').pipe(
      tap(members => this.matches.set(members))
    );
  }

  getMessages(username: string) {
    return this.http.get<Message[]>(this.baseUrl + 'user/' + username + '/messages').pipe(
      tap(messages => this.messages.set(messages))
    );
  }

  sendMessage(username: string, content: string) {
    return this.http.post<Message>(this.baseUrl + 'user/' + username + '/messages', { content }).pipe(
      map(message => ({
        ...message,
        createdAt: message.createdAt ?? new Date().toISOString()
      }))
    );
  }
}
