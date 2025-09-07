import { inject, Inject } from '@angular/core';
import { ActivatedRoute, CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../auth/auth.service';
import { ToastrService } from 'ngx-toastr';
import { User } from '../../shared/modules/user.module';

export const authGuard: CanActivateFn = (route, state) => {
  const router: Router = inject(Router);
  const authService: AuthService = inject(AuthService);
  const toastr: ToastrService = inject(ToastrService);
  const restId = window.location.pathname.split('/')[1];

  // First check if user exists in memory
  if (authService.currentUser && authService.currentUser.token) {
    return true;
  }

  // If not we try to restore the user from localStorage
  try {
    const userKey = 'userInfo' + authService.restaurantId;
    const raw = localStorage.getItem(userKey);

    if (!raw) {
      toastr.error('Authentication error');
      router.navigate([restId, 'auth', 'login']);
      return false;
    }

    const parsed = JSON.parse(raw);
    console.log('Parsed user from localStorage:', parsed);


    if (!parsed) {
      toastr.error('User not authenticated');
      router.navigate([restId, 'auth', 'login']);
      return false;
    }

    const user = new User(
      parsed.id,
      parsed.email,
      parsed.phonenumber,
      parsed.name,
      parsed._token,
    );

    authService.user.next(user);
    authService.currentUser = user;

    // Restarting access token expiration and refresh timer
    if (user.expirationDate) {
      authService.setAccessTokenExpiry();
      authService.startRefreshTimer();
    }

    return true;

  } catch (error) {
    console.error('Error restoring user from localStorage:', error);
    toastr.error('User not authenticated');
    router.navigate([restId, 'auth', 'login']);
    return false;
  }
};
