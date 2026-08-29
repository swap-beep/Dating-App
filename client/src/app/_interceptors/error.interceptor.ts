import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { catchError } from 'rxjs/operators';
import { AccountService } from '../_service/account.service';


export const errorInterceptor: HttpInterceptorFn = (req, next) => {

  const router = inject(Router);
  const toastr = inject(ToastrService);
  const accountService = inject(AccountService);

  return next(req).pipe(
    catchError(error => {
      if (error) {
        switch (error.status) {
          case 400:
            if (error.error.errors) {
              const modalStateError = [];
              for (const key in error.error.errors) {
                if (error.error.errors[key]) {
                  modalStateError.push(error.error.errors[key]);
                }
              }
              throw modalStateError.flat();
            } else {
              toastr.error(error.error, error.status);
            }
            break;
          case 401:
            accountService.logout();
            router.navigateByUrl('/');
            toastr.error('Unauthorised', error.status);
            break;
          case 404:
            router.navigateByUrl('/not-found');
            break;
          case 500:
            const navigationExtras: NavigationExtras = { state: { error: error.error } };
            router.navigateByUrl('/server-error', navigationExtras);
            break;
          default:
            toastr.error('Something Went Wrong');
            break;
        }
      }
      throw error;
    })
  );
};
