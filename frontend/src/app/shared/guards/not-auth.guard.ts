import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { inject } from '@angular/core';

export const notAuthGuard: CanActivateFn = (route, state) => {
  const router: Router = inject(Router);
  const authService: AuthService = inject(AuthService);
  const restId = window.location.pathname.split('/')[1];
  if (!authService.isAuth()) {
    return true;
  } else {
    router.navigate([restId, 'main']);
    return false;
  }
};
