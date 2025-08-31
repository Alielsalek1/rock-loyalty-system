import {
  HttpHandlerFn,
  HttpInterceptorFn,
  HttpRequest,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../../auth/auth.service';
import { exhaustMap, retry, take } from 'rxjs';
import { User } from '../modules/user.module';

export const authInterceptor: HttpInterceptorFn = (
  req: HttpRequest<any>,
  next: HttpHandlerFn
) => {

  const authService: AuthService = inject(AuthService);
  const user: User = authService.user.getValue();
  let modifiedReq: HttpRequest<any> = req;

  if (user) {
    modifiedReq = req.clone({
      headers: req.headers.set('Authorization', `Bearer ${user.token}`),
    });
  }
  return next(modifiedReq);
};
