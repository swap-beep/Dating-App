import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AccountService } from './account.service';

function tokenWithExpiry(exp: number): string {
  const payload = btoa(JSON.stringify({ exp }));
  return `header.${payload}.signature`;
}

describe('AccountService', () => {
  let service: AccountService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [AccountService, provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(AccountService);
    http = TestBed.inject(HttpTestingController);
    localStorage.clear();
  });

  afterEach(() => {
    http.verify();
    localStorage.clear();
  });

  it('stores the user returned by login', () => {
    const user = { username: 'alice', token: tokenWithExpiry(Date.now() / 1000 + 3600) };

    service.login({ username: 'alice', password: 'secret' }).subscribe();
    http.expectOne('/account/login').flush(user);

    expect(service.currentUser()).toEqual(user);
    expect(JSON.parse(localStorage.getItem('user')!)).toEqual(user);
  });

  it('registers and stores the returned user', () => {
    const user = { username: 'alice', token: tokenWithExpiry(Date.now() / 1000 + 3600) };

    service.register({ username: 'alice', password: 'secret' }).subscribe();
    http.expectOne('/account/register').flush(user);

    expect(service.currentUser()).toEqual(user);
  });

  it('rejects expired tokens and clears the session', () => {
    service.setCurrentUser({ username: 'alice', token: tokenWithExpiry(Date.now() / 1000 - 1) });

    expect(service.isAuthenticated()).toBeFalse();
    expect(service.currentUser()).toBeNull();
    expect(localStorage.getItem('user')).toBeNull();
  });
});
