import { Component, inject, input } from '@angular/core';
import { Member } from '../../_models/Member';
import { Router, RouterLink } from '@angular/router';
import { MembersService } from '../../_service/members.service';

@Component({
  selector: 'app-member-card',
  imports: [RouterLink],
  templateUrl: './member-card.component.html',
  styleUrl: './member-card.component.css'
})
export class MemberCardComponent {
  member = input.required<Member>();
  private membersService = inject(MembersService);
  private router = inject(Router);

  toggleLike() {
    const request = this.member().isLiked
      ? this.membersService.unlikeMember(this.member())
      : this.membersService.likeMember(this.member());

    request.subscribe();
  }

  openMessages() {
    if (this.member().isMatch) {
      this.router.navigate(['/messages'], { queryParams: { username: this.member().userName } });
    }
  }
}
