import { inject, Inject } from '@angular/core';
import { ActivatedRoute, CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../auth/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const router: Router = inject(Router);
  const authService: AuthService = inject(AuthService);
  const restId = window.location.pathname.split('/')[1];
  if (authService.isAuth()) return true;
  else {
    router.navigate([restId, 'auth', 'login']);
    return false;
  }
};
