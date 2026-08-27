import { Component, inject, OnInit } from '@angular/core';
import { MembersService } from '../../_service/members.service';
import { ActivatedRoute } from '@angular/router';
import { Member } from '../../_models/Member';
import { TabsModule } from 'ngx-bootstrap/tabs';
import{GalleryItem, GalleryModule, ImageItem} from 'ng-gallery';

@Component({
  selector: 'app-member-detail',
  imports: [TabsModule,GalleryModule],
  templateUrl: './member-detail.component.html',
  styleUrl: './member-detail.component.css'
})
export class MemberDetailComponent implements OnInit {

  private memberService = inject(MembersService);
  private route = inject(ActivatedRoute);
  member?: Member;
  images: GalleryItem[]=[];

  toggleLike() {
    if (!this.member) return;
    const request = this.member.isLiked
      ? this.memberService.unlikeMember(this.member)
      : this.memberService.likeMember(this.member);
    request.subscribe(() => this.member = { ...this.member!, isLiked: !this.member!.isLiked });
  }

  ngOnInit(): void {
    this.loadMember();
  }

  loadMember (){
    const username = this.route.snapshot.paramMap.get('username');
    if(!username) return;
    this.memberService.getMember(username).subscribe({
      next: member=>{this.member=member;
        member.photos.map(p =>{
          this.images.push(new ImageItem({
            src: p.url, thumb:p.url
          }))
        })
      }
    })
  }
}
