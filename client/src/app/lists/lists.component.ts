import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MembersService } from '../_service/members.service';
import { MemberCardComponent } from '../member/member-card/member-card.component';

@Component({
  selector: 'app-lists',
  imports: [CommonModule, MemberCardComponent],
  templateUrl: './lists.component.html',
  styleUrl: './lists.component.css'
})
export class ListsComponent implements OnInit {
  membersService = inject(MembersService);

  ngOnInit(): void {
    this.membersService.getLikedMembers().subscribe();
  }

}
