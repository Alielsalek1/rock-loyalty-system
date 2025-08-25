import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { AuthService } from '../../auth/auth.service';

// export const refreshTokenInterceptor: HttpInterceptorFn = (req, next) => {
//   const authService = inject(AuthService);
  
//   const credentialsReq = req.clone({
//     withCredentials: true
//   });

//   return next(credentialsReq).pipe(
//     catchError((error) => {
//       if (error.status === 401 && authService.hasRefreshToken()) {
//         return authService.refreshAccessToken().pipe(
//           switchMap(() => next(credentialsReq)),
//           catchError((refreshError) => {
//             authService.LogOut();
//             return throwError(() => refreshError);
//           })
//         );
//       }
//       return throwError(() => error);
//     })
//   );
// };