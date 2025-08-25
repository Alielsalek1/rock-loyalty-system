import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, OnInit } from '@angular/core';
import {
  BehaviorSubject,
  catchError,
  exhaustMap,
  Observable,
  tap,
  throwError,
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
    const user: User = new User(token);
    user.email = email;
    user.name = name;
    user.phonenumber = phoneNumber;
    user.id = userId;

    this.user.next(user);
    this.currentUser = user;

    localStorage.setItem('userInfo' + this.restaurantId, JSON.stringify(user));

    this.setAccessTokenExpiry();
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
    this.user.next(null);
    localStorage.removeItem('userInfo' + this.restaurantId);
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
          let user: User = new User(
            this.currentUser.token,
          );
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

  loginFaceBook(token: string): Observable<UserInterface> {
    return this.http
      .post<UserInterface>(`${environment.apiUrl}/api/oauth2/signin-facebook`, {
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

  // -------------------- access token handlers ---------------------

  setAccessTokenExpiry(): void {
    try {

      console.log("setting token expiry")

      if (!this.currentUser || !this.currentUser.expirationDate) {
        throw new Error('No current user or expiration date available');
      }

      const expiryTime = this.currentUser.expirationDate.getTime();
      const timeUntilExpiry = expiryTime - new Date().getTime();

      console.log(`Token expires at: ${this.currentUser.expirationDate}`);
      console.log(`Time until expiry: ${Math.floor(timeUntilExpiry / 60000)} minutes`);

      // Clear any existing timeout
      const timeoutID = localStorage.getItem('tokenTimeoutId');

      if (timeoutID != null) {
        clearTimeout(Number(timeoutID));
      }

      const tokenTimeoutId = setTimeout(() => {
        console.log('Access token expired, logging out user');
        this.LogOut();
      }, timeUntilExpiry);

      localStorage.setItem('tokenTimeoutId', tokenTimeoutId.toString());

    } catch (error) {
      console.error('Error setting token expiry:', error);

      // Fallback: 
      const tokenTimeoutId = setTimeout(() => {
        this.LogOut();
      }, 15 * 60 * 1000);

      console.log("fallback token is created: 15 minutes")
      localStorage.setItem('tokenTimeoutId', tokenTimeoutId.toString());
    }
  }

  // ----------------------------------------- refresh token handlers ----------------------------------------

  hasRefreshToken(): boolean {
    return document.cookie.includes('refreshToken=');
  }



}


