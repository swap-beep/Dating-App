import { CommonModule } from '@angular/common';
import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Member } from '../_models/Member';
import { MessagesService } from '../_service/messages.service';

@Component({
  selector: 'app-messages',
  imports: [CommonModule, FormsModule],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.css'
})
export class MessagesComponent implements OnInit {
  private messageService = inject(MessagesService);
  private route = inject(ActivatedRoute);

  matches = this.messageService.matches;
  messages = this.messageService.messages;
  selectedUsername: string | null = null;
  newMessage = '';

  ngOnInit(): void {
    this.messageService.getMatches().subscribe({
      next: matches => {
        if (matches.length > 0) {
          const requestedUser = this.route.snapshot.queryParamMap.get('username');
          const firstMatch = requestedUser
            ? matches.find(member => member.userName.toLowerCase() === requestedUser.toLowerCase()) ?? matches[0]
            : matches[0];

          if (firstMatch) {
            this.selectMatch(firstMatch);
          }
        }
      }
    });
  }

  selectMatch(member: Member): void {
    this.selectedUsername = member.userName;
    this.messageService.getMessages(member.userName).subscribe();
  }

  sendMessage(): void {
    const username = this.selectedUsername;
    const content = this.newMessage.trim();

    if (!username || !content) {
      return;
    }

    this.messageService.sendMessage(username, content).subscribe({
      next: message => {
        this.messages.update(current => [...current, message]);
        this.newMessage = '';
      }
    });
  }
}
