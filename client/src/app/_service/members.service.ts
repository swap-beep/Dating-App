
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { Member } from '../_models/Member';
import { of, tap } from 'rxjs';
import { Photo } from '../_models/Photo';


@Injectable({
  providedIn: 'root'
})
export class MembersService {

  private http = inject(HttpClient);

  baseUrl = environment.apiUrl;
  members = signal<Member[]>([]);
  likedMembers = signal<Member[]>([]);
  getMembers() {
    return this.http.get<Member[]>(this.baseUrl + 'user').subscribe({
      next: members => this.members.set(members)
    })
  }
  getMember(username: string) {
    const member = this.members().find(x => x.userName === username);
    if (member !== undefined) return of(member);
    return this.http.get<Member>((this.baseUrl + 'user/' + username));
  }

  getLikedMembers() {
    return this.http.get<Member[]>(this.baseUrl + 'user/likes').pipe(
      tap(members => this.likedMembers.set(members.map(member => ({ ...member, isLiked: true }))))
    );
  }

  likeMember(member: Member) {
    return this.http.post(this.baseUrl + 'user/' + member.userName + '/like', {}).pipe(
      tap(() => {
        this.members.update(members => members.map(item =>
          item.userName === member.userName ? { ...item, isLiked: true } : item));
      })
    );
  }

  unlikeMember(member: Member) {
    return this.http.delete(this.baseUrl + 'user/' + member.userName + '/like').pipe(
      tap(() => {
        this.members.update(members => members.map(item =>
          item.userName === member.userName ? { ...item, isLiked: false } : item));
        this.likedMembers.update(members => members.filter(item => item.userName !== member.userName));
      })
    );
  }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'user', member).pipe(
      tap(() => {
        this.members.update(members => members.map(m => m.userName === member.userName ? member : m))
      })
    )
  }

  setMainPhoto(photo: Photo) {
    return this.http.put(this.baseUrl + 'user/set-main-photo/' + photo.id, {}).pipe(
      tap(() => {
        this.members.update(members => members.map(member => {
          if (!member.photos.some(existingPhoto => existingPhoto.id === photo.id)) return member;
          return {
            ...member,
            photoUrl: photo.url,
            photos: member.photos.map(existingPhoto => ({
              ...existingPhoto,
              isMain: existingPhoto.id === photo.id
            }))
          };
        }))
      })
    )
  }

  deletePhoto(photo: Photo) {
    return this.http.delete(this.baseUrl + 'user/delete-photo/' + photo.id).pipe(
      tap(() => {
        this.members.update(members => members.map(member => ({
          ...member,
          photos: member.photos.filter(existingPhoto => existingPhoto.id !== photo.id),
          photoUrl: member.photoUrl === photo.url ? undefined : member.photoUrl
        })))


      })

    )
  }


}
