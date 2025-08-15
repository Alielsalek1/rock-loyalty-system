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
import { enviroment } from '../../env';
import { UserInterface } from '../shared/responseInterface/user.get.response.interface';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  currentUser: User;
  restaurantId: string;

  user: BehaviorSubject<User> = new BehaviorSubject<User>(null);
  constructor(private http: HttpClient, private router: Router) {
    this.user.subscribe((user) => {
      this.currentUser = user;
    });
    this.restaurantId = "1234" // should be changed
  }

  isAuth() {
    if (this.currentUser && this.currentUser?.token) return true;
    else return false;
  }

  autoLogOut(date: Date) {
    const differenceInMs: number = date.getTime() - new Date().getTime();
    setTimeout(() => {
      this.LogOut();
    }, differenceInMs);
  }

  autoLogin() {
    const userData = JSON.parse(
      localStorage.getItem('userInfo' + this.restaurantId)
    );

    if (!userData) {
      return;
    }
    const token = userData._token;
    const expireDate = new Date(userData._tokenExpirationDate);
    const user: User = new User(token, expireDate);
    user.email = userData.email;
    user.name = userData.name;
    user.phonenumber = userData.phonenumber;
    user.id = userData.id;

    if (user.token) {
      this.user.next(user);
      this.autoLogOut(expireDate);
    }
  }

  LogOut() {
    this.user.next(null);
    localStorage.removeItem('userInfo' + this.restaurantId);
    this.router.navigate([this.restaurantId, 'auth', 'login']);
  }

  SignUp(name: string, email: string, password: string, phoneNumber: string) {
    console.log(this.restaurantId);
    return this.http.post(`${enviroment.apiUrl}/api/users`, {
      email: email,
      password: password,
      restaurantId: this.restaurantId,
      name: name,
      phoneNumber: phoneNumber,
    });
  }

  logIn(email: string, phoneNumber: string, password: string) {
    return this.http
      .post<UserInterface>(`${enviroment.apiUrl}/api/auth/login`, {
        email: email,
        phoneNumber: phoneNumber,
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

  updateUserInfo(newUser: User) {
    return this.http
      .put<UserInterface>(`${enviroment.apiUrl}/api/users`, {
        phoneNumber: newUser.phonenumber,
        email: newUser.email,
        name: newUser.name,
      })
      .pipe(
        tap((userInfo) => {
          console.log(userInfo);
          let user: User = new User(
            this.currentUser.token,
            this.currentUser.expirationDate
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
      .put(`${enviroment.apiUrl}/api/auth/confirm-email/${token}`, {})
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
      .put(`${enviroment.apiUrl}/api/password/${token}`, {
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
      .post<UserInterface>(`${enviroment.apiUrl}/api/oauth2/signin-facebook`, {
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
      .post(`${enviroment.apiUrl}/api/password/forgot-password-email`, {
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
      .post<UserInterface>(`${enviroment.apiUrl}/api/oauth2/signin-google`, {
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
    localStorage.setItem('userInfo' + this.restaurantId, JSON.stringify(user));
  }
}
