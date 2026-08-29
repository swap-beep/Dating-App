import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { errorInterceptor } from './error.interceptor';
import { AccountService } from '../_service/account.service';

describe('errorInterceptor', () => {
  let httpClient: HttpClient;
  let http: HttpTestingController;
  let accountService: AccountService;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    router = jasmine.createSpyObj<Router>('Router', ['navigateByUrl']);
    TestBed.configureTestingModule({
      providers: [
        AccountService,
        { provide: Router, useValue: router },
        { provide: ToastrService, useValue: jasmine.createSpyObj('ToastrService', ['error']) },
        provideHttpClient(withInterceptors([errorInterceptor])),
        provideHttpClientTesting()
      ]
    });
    httpClient = TestBed.inject(HttpClient);
    http = TestBed.inject(HttpTestingController);
    accountService = TestBed.inject(AccountService);
    accountService.setCurrentUser({ username: 'alice', token: 'token' });
  });

  afterEach(() => {
    http.verify();
    localStorage.clear();
  });

  it('clears the session and returns home after a 401', () => {
    httpClient.get('/protected').subscribe({ error: () => undefined });
    http.expectOne('/protected').flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });

    expect(accountService.currentUser()).toBeNull();
    expect(router.navigateByUrl).toHaveBeenCalledWith('/');
  });
});
