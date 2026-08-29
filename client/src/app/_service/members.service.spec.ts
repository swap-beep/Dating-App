import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { MembersService } from './members.service';
import { Member } from '../_models/Member';
import { Photo } from '../_models/Photo';

describe('MembersService', () => {
  let service: MembersService;
  let http: HttpTestingController;
  const firstPhoto: Photo = { id: 1, url: 'first.jpg', isMain: true };
  const secondPhoto: Photo = { id: 2, url: 'second.jpg', isMain: false };
  const member: Member = {
    id: 1,
    userName: 'alice',
    age: 30,
    photoUrl: firstPhoto.url,
    created: new Date(),
    lastActive: new Date(),
    gender: 'female',
    introduction: '',
    interests: '',
    lookingFor: '',
    city: '',
    country: '',
    photos: [firstPhoto, secondPhoto],
    KnownAs: 'Alice'
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [MembersService, provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(MembersService);
    http = TestBed.inject(HttpTestingController);
    service.members.set([member]);
  });

  afterEach(() => http.verify());

  it('updates profile data through the API and local state', () => {
    const updated = { ...member, introduction: 'Updated' };

    service.updateMember(updated).subscribe();
    http.expectOne({ url: '/user', method: 'PUT' }).flush(null);

    expect(service.members()[0].introduction).toBe('Updated');
  });

  it('sets a photo as main without removing it', () => {
    service.setMainPhoto({ ...secondPhoto }).subscribe();
    http.expectOne('/user/set-main-photo/2').flush(null);

    expect(service.members()[0].photos.map(photo => photo.id)).toEqual([1, 2]);
    expect(service.members()[0].photos.find(photo => photo.id === 1)?.isMain).toBeFalse();
    expect(service.members()[0].photos.find(photo => photo.id === 2)?.isMain).toBeTrue();
    expect(service.members()[0].photoUrl).toBe('second.jpg');
  });

  it('deletes a photo by ID even when the object instance differs', () => {
    service.deletePhoto({ ...secondPhoto }).subscribe();
    http.expectOne('/user/delete-photo/2').flush(null);

    expect(service.members()[0].photos.map(photo => photo.id)).toEqual([1]);
  });
});
