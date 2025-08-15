import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { inject } from '@angular/core';

export const phoneGuardGuard: CanActivateFn = (route, state) => {
  const authService: AuthService = inject(AuthService);
  const router: Router = inject(Router);
  const user = authService.currentUser;
  const restId = window.location.pathname.split('/')[1];
  console.log(restId);
  console.log('phone guard');
  if (user.phonenumber) {
    return true;
  } else {
    router.navigate([restId, 'auth', 'phone']);
    return false;
  }
};
