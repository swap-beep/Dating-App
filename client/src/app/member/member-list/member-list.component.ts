import { Component, OnInit } from '@angular/core';
import { MembersService } from '../../_service/members.service';
import { inject } from '@angular/core';
import { Member } from '../../_models/Member';
import { MemberCardComponent } from "../member-card/member-card.component";

@Component({
  selector: 'app-member-list',
  imports: [MemberCardComponent],
  templateUrl: './member-list.component.html',
  styleUrl: './member-list.component.css'
})
export class MemberListComponent implements OnInit{

 memberService = inject(MembersService)
 

ngOnInit(): void {
  if(this.memberService.members().length===0)
this.loadMembers();
}

loadMembers(){
  this.memberService.getMembers();
  
}
}
