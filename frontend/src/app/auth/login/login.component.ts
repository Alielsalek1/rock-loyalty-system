import { Component, NgZone, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { AuthService } from '../auth.service';
import { Router } from '@angular/router';
import { finalize, Observable } from 'rxjs';
import { UserInterface } from '../../shared/responseInterface/user.get.response.interface';
import { FacebookAuthService } from '../social-login/facebook-auth.service';
import { GoogleAuthService } from '../social-login/google-auth.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent implements OnInit {
  @ViewChild('loginForm') form: NgForm;
  loading: boolean = false;
  loadingMessage: string = '';
  restuarantId: string;
  constructor(
    private authService: AuthService,
    private facebookAuth: FacebookAuthService,
    private googleAuth: GoogleAuthService,
    private router: Router,
    private toastrService: ToastrService
  ) {}
  onSubmit() {
    const phoneEmailField: string = this.form.value.phone;
    const password: string = this.form.value.password;
    let loginObs: Observable<UserInterface>;
    if (phoneEmailField.startsWith('0')) {
      loginObs = this.authService.logIn(null, phoneEmailField, password);
    } else {
      loginObs = this.authService.logIn(phoneEmailField, null, password);
    }
    loginObs.pipe(finalize(() => {})).subscribe({
      next: (response) => {
        this.redirect();
      },
      error: (error) => {
        this.toastrService.error(error.message);
      },
    });
  }

  ngOnInit(): void {
    this.restuarantId = this.authService.restaurantId;
  }

  onGoogleLogin() {
    this.loading = true;
    this.loadingMessage = 'waiting for google signin';
    this.googleAuth
      .login()
      .then((response) => {
        console.log('sucssesfully logged', response);
        const token = response.authentication.accessToken;
        this.loginGoogle(token);
      })
      .catch((error) => {
        this.toastrService.error('login with google failed');
      })
      .finally(() => {
        this.loading = false;
      });
  }

  onFacebokLogin() {
    this.loading = true;
    this.loadingMessage = 'waiting for facebook signin';
    this.facebookAuth
      .login()
      .then((response: any) => {
        const token = response.authResponse.accessToken;
        this.loginFacebook(token);
      })
      .catch((error) => {
        this.toastrService.error('login with facebook failed');
        this.loading = false;
      });
  }

  private loginFacebook(token: string) {
    this.loadingMessage = 'signing in';
    this.authService
      .loginFaceBook(token)
      .pipe()
      .subscribe({
        next: () => {
          this.redirect();
        },
        error: (error: Error) => {
          this.toastrService.error(error.message);
          this.loading = false;
        },
      });
  }

  private loginGoogle(token: string) {
    this.loading = true;
    this.loadingMessage = 'signing in';
    this.authService.loginGoogle(token).subscribe({
      next: () => {
        this.redirect();
      },
      error: (error: Error) => {
        this.loading = false;
        this.toastrService.error(error.message);
      },
    });
  }

  private redirect(seconds: number = 5) {
    this.toastrService.success(`redirecting in ${seconds} seconds`);
    setTimeout(() => {
      this.router.navigate([this.authService.restaurantId, 'main']);
    }, seconds * 1000);
  }
}
