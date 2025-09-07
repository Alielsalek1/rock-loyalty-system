import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, OnInit } from '@angular/core';
import {
  BehaviorSubject,
  catchError,
  exhaustMap,
  Observable,
  tap,
  throwError,
  finalize,
} from 'rxjs';
import { User } from '../shared/modules/user.module';
import { ActivatedRoute, Router } from '@angular/router';
import { environment } from '../../env';
import { UserInterface } from '../shared/responseInterface/user.get.response.interface';


@Injectable({
  providedIn: 'root',
})
export class AuthService {
  currentUser: User;
  restaurantId: string;

  private authenticationHandler(
    email: string,
    name: string,
    phoneNumber: string,
    userId: number,
    token: string
  ) {
    const user: User = new User(userId, email, phoneNumber, name, token);
    this.user.next(user);
    this.currentUser = user;

    localStorage.setItem('userInfo' + this.restaurantId, JSON.stringify(user));

    this.setAccessTokenExpiry();
    this.startRefreshTimer();
  }

  user: BehaviorSubject<User> = new BehaviorSubject<User>(null);
  constructor(private http: HttpClient, private router: Router) {
    this.user.subscribe((user) => {
      this.currentUser = user;
    });
    const pathSegments = window.location.pathname.split('/');
    this.restaurantId = pathSegments[1] || '1234';
  }

  isAuth() {
    if (this.currentUser && this.currentUser?.token) return true;
    else return false;
  }

  LogOut() {
    if (this.refreshTimer) {
      clearInterval(this.refreshTimer);
      this.refreshTimer = null;
    }

    if (this.tokenTimeoutId) {
      clearTimeout(this.tokenTimeoutId);
      this.tokenTimeoutId = null;
    }

    this.refreshInProgress = false;

    this.user.next(null);
    this.currentUser = null;

    localStorage.removeItem('userInfo' + this.restaurantId);
    this.clearSpecificCookies(['refreshToken']);

    this.router.navigate([this.restaurantId, 'auth', 'login']);
  }

  SignUp(name: string, email: string, password: string, phoneNumber: string) {
    console.log(this.restaurantId);
    return this.http.post(`${environment.apiUrl}/api/users`, {
      email: email,
      password: password,
      restaurantId: this.restaurantId,
      name: name,
      phoneNumber: phoneNumber,
    });
  }

  logIn(email: string, phoneNumber: string, password: string) {
    return this.http
      .post<UserInterface>(`${environment.apiUrl}/api/auth/login`, {
        email: email,
        phoneNumber: phoneNumber,
        password: password,
        restaurantId: this.restaurantId,
      }, { withCredentials: true })
      .pipe(
        catchError((errorResponse: HttpErrorResponse) => {
          console.log(errorResponse);

          return throwError(() => {
            return new Error(errorResponse.error.message);
          });
        }),
        tap((userInfo) => {
          const email = userInfo.data.user.email;
          const name = userInfo.data.user.name;
          const phoneNumber = userInfo.data.user.phoneNumber;
          const id = userInfo.data.user.id;
          this.authenticationHandler(
            email,
            name,
            phoneNumber,
            id,
            userInfo.data.accessToken
          );
        })
      );
  }

  updateUserInfo(newUser: User) {
    console.log('Updating user with data:', {
      phoneNumber: newUser.phonenumber,
      email: newUser.email,
      name: newUser.name,
    });
    console.log('Current user token:', newUser.token ? 'Token exists' : 'No token');
    console.log('Making request to:', `${environment.apiUrl}/api/users`);

    return this.http
      .put<UserInterface>(`${environment.apiUrl}/api/users`, {
        phoneNumber: newUser.phonenumber,
        email: newUser.email,
        name: newUser.name,
      })
      .pipe(
        tap((userInfo) => {
          console.log(userInfo);
          const email = userInfo.data.user.email;
          const userId = userInfo.data.user.id;
          const phoneNumber = userInfo.data.user.phoneNumber;
          const name = userInfo.data.user.name;
          this.restaurantId = userInfo.data.user.restaurantId;
          this.authenticationHandler(email, name, phoneNumber, userId, this.currentUser.token);
        }),
        catchError((errorResponse) => {
          console.log(errorResponse);

          return throwError(() => {
            return new Error("unkown error");
          });
        })
      );
  }

  confirmEmail(token: string) {
    return this.http
      .put(`${environment.apiUrl}/api/auth/confirm-email/${token}`, {})
      .pipe(
        catchError((errorResponse: HttpErrorResponse) => {
          console.log(errorResponse);
          let error: string = 'unkown error';
          switch (errorResponse.status) {
            case 400:
              error = 'email already confirmed';
              break;
            case 401:
              error = 'no token provided';
              break;
            case 500:
              error = 'internal server error';
          }
          return throwError(() => {
            return new Error(error);
          });
        })
      );
  }

  UpdatePassword(token: string, password: string) {
    return this.http
      .put(`${environment.apiUrl}/api/password/${token}`, {
        password: password,
      })
      .pipe(
        catchError((errorResponse: HttpErrorResponse) => {
          let error: string = 'unkown error';
          console.log(errorResponse);
          console.log('caught error');
          switch (errorResponse.status) {
            case 500:
              error = 'internal server error';
          }
          return throwError(() => {
            return new Error(error);
          });
        })
      );
  }

  forgotPassword(email: string): Observable<any> {
    return this.http
      .post(`${environment.apiUrl}/api/password/forgot-password-email`, {
        email: email,
        restaurantId: this.restaurantId,
      })
      .pipe(
        catchError((response: HttpErrorResponse) => {
          let errorMsg = 'unkown error';
          console.log(response);
          switch (response.status) {
            case 404:
              errorMsg = 'this email is not linked to any account';
              break;
            case 500:
              errorMsg = 'internal server error';
              break;
          }
          return throwError(() => {
            return new Error(errorMsg);
          });
        })
      );
  }

  loginGoogle(token: string): Observable<UserInterface> {
    return this.http
      .post<UserInterface>(`${environment.apiUrl}/api/oauth2/signin-google`, {
        accessToken: token,
        restaurantId: this.restaurantId,
      })
      .pipe(
        tap((userInfo) => {
          const email = userInfo.data.user.email;
          const name = userInfo.data.user.name;
          const id = userInfo.data.user.id;
          const token = userInfo.data.accessToken;
          const phonenumber = userInfo.data.user.phoneNumber;
          this.authenticationHandler(email, name, phonenumber, id, token);
        })
      );
  }

  // -------------------- admin services --------------------- (should be removed)

  adminLogin(email: string, password: string) {
    return this.http
      .post<UserInterface>(`${environment.apiUrl}/api/admin/login`, {
        email: email,
        password: password,
        restaurantId: this.restaurantId,
      })
      .pipe(
        catchError((errorResponse: HttpErrorResponse) => {
          console.log(errorResponse);
          return throwError(() => {
            return new Error(errorResponse.error.message);
          });
        }),
        tap((userInfo) => {
          const email = userInfo.data.user.email;
          const name = userInfo.data.user.name;
          const phoneNumber = userInfo.data.user.phoneNumber;
          const id = userInfo.data.user.id;
          this.authenticationHandler(
            email,
            name,
            phoneNumber,
            id,
            userInfo.data.accessToken
          );
        })
      );
  }

  onAdminLogOut() {
    this.user.next(null);
    localStorage.removeItem('userInfo' + this.restaurantId);
    this.router.navigate([this.restaurantId, 'admin', 'login']);
  }

  // -------------------- access token & refresh token handlers ---------------------

  private refreshTimer: ReturnType<typeof setInterval>;
  private tokenTimeoutId: any;
  private refreshInProgress: boolean = false;

  startRefreshTimer(): void {
    if (this.refreshTimer) {
      clearInterval(this.refreshTimer);
    }

    // Check every minute if refresh is needed
    this.refreshTimer = setInterval(() => {
      this.checkAndRefreshToken(5); // Refresh 5 minutes before expiry
    }, 60 * 1000);
  }

  // Set access token expiry timeout
  setAccessTokenExpiry(): void {
    try {
      if (!this.currentUser || !this.currentUser.expirationDate) {
        console.log('No current user or expiration date available');
        return;
      }

      const expiryTime = this.currentUser.expirationDate.getTime();
      const timeUntilExpiry = expiryTime - new Date().getTime();

      console.log(`Access Token expires at: ${this.currentUser.expirationDate.toTimeString()}`);
      console.log(`Time until expiry: ${Math.floor(timeUntilExpiry / 60000)} minutes` +
        ` and ${Math.floor((timeUntilExpiry % 60000) / 1000)} seconds`);

      if (this.tokenTimeoutId) {
        clearTimeout(this.tokenTimeoutId);
      }

      this.tokenTimeoutId = setTimeout(() => {
        console.log('Access token expired, attempting refresh...');

        this.refreshAccessToken().subscribe({ // try to refresh token on expiry
          next: () => console.log('Token refreshed due to expiry'),
          error: () => {
            console.log('Failed to refresh expired token, logging out');
            this.LogOut();
          }
        });

      }, timeUntilExpiry);

    } catch (error) {
      console.error('Error setting access token expiry:', error);
      this.LogOut();
    }
  }

  // Main checking function for refresh
  private checkAndRefreshToken(minutes: number): void {
    if (this.refreshInProgress) {
      console.log('Refresh already in progress, skipping...');
      return;
    }

    console.log('Checking if token needs refresh...');

    const user = this.user.getValue();
    const minutesBeforeExpiry = new Date(user.expirationDate.getTime() - minutes * 60 * 1000);
    const now = new Date();

    if (now >= minutesBeforeExpiry) {
      console.log('Token expiring soon, proactively refreshing...');
      this.refreshAccessToken().subscribe({
        next: () => console.log('Proactive token refresh successful'),
        error: (error) => {
          console.error('Proactive token refresh failed:', error);
          this.handleRefreshTokenExpiry();
        }
      });
    }
  }

  // HTTP-based token refresh using backend endpoint
  refreshAccessToken(): Observable<any> {
    if (this.refreshInProgress) {
      console.log('Refresh already in progress');
      return throwError(() => new Error('Refresh in progress'));
    }

    this.refreshInProgress = true;

    const refreshAccessToken = this.getRefreshTokenFromCookie();

    return this.http.put(
      `${environment.apiUrl}/api/tokens/refresh-tokens`,
      {},
      {
        withCredentials: true,
        headers: {
          'Authorization': `Bearer ${refreshAccessToken}` // Current access token for auth
        }
      }
    ).pipe(
      tap((response: any) => {

        const user = this.user.getValue();
        if (user && response.data?.accessToken) {
          user.updateToken(response.data.accessToken);

          this.currentUser = user;
          this.user.next(user);
          localStorage.setItem('userInfo' + this.restaurantId, JSON.stringify(user));

          // Reset access token expiry timer
          this.setAccessTokenExpiry();
        }
      }),
      catchError((error: HttpErrorResponse) => {
        console.error('Token refresh failed:', error);

        if (error.status === 401 || error.status === 403) {
          console.log('Refresh token expired or invalid');
          this.handleRefreshTokenExpiry();
        }

        return throwError(() => new Error('Token refresh failed'));
      }),
      finalize(() => {
        this.refreshInProgress = false;
      })
    );
  }

  // Handle refresh token expiry
  private handleRefreshTokenExpiry(): void {
    console.log('Refresh token expired, cleaning up session');

    if (this.refreshTimer) {
      clearInterval(this.refreshTimer);
      this.refreshTimer = null;
    }

    if (this.tokenTimeoutId) {
      clearTimeout(this.tokenTimeoutId);
      this.tokenTimeoutId = null;
    }

    this.refreshInProgress = false;
    this.LogOut();
  }

  // helper gets refresh token
  private getRefreshTokenFromCookie(): string | null {
    try {
      const cookies = document.cookie.split(';');
      const refreshTokenCookie = cookies.find(cookie =>
        cookie.trim().startsWith('refreshToken=')
      );

      if (refreshTokenCookie) {
        return refreshTokenCookie.split('=')[1].trim();
      }

      return null;
    } catch (error) {
      console.error('Error reading refresh token from cookie:', error);
      return null;
    }
  }

  // helper clears specific cookie names
  private clearSpecificCookies(cookieNames: string[]): void {
    cookieNames.forEach(name => {
      document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/`;
      document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/; domain=${window.location.hostname}`;

      // Clear for parent domain if subdomain
      const hostParts = window.location.hostname.split('.');
      if (hostParts.length > 2) {
        const parentDomain = '.' + hostParts.slice(-2).join('.');
        document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/; domain=${parentDomain}`;
      }
    });
  }

}

