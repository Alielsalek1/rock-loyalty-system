import { Component, NgZone, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { AuthService } from '../auth.service';
import { Router } from '@angular/router';
import { finalize, Observable } from 'rxjs';
import { UserInterface } from '../../shared/responseInterface/user.get.response.interface';
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

  phoneEmailPattern: RegExp = /^01\d{8,9}$|^[^\s@]+@[^\s@]+\.[^\s@]+$/;

  constructor(
    private authService: AuthService,
    private googleAuth: GoogleAuthService,
    private router: Router,
    private toastrService: ToastrService,
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
    this.loading = true;
    this.loadingMessage = 'Logging in...';
    loginObs.pipe(finalize(() => { })).subscribe({
      next: (response) => {
        this.loading = false;
        this.redirect();
      },
      error: (error) => {
        this.loading = false;
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
  
    this.googleAuth.login().subscribe({
      next: (response) => {
        this.toastrService.success('Google Login Successful: redirecting...');
        this.loginGoogle(response.credential);
      },
      error: (error) => {
        this.toastrService.error('Google login failed');
        this.toastrService.error(error.message);
        this.loading = false;
      }
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

  private redirect() {
    this.toastrService.success(`Login Successful`);
    this.router.navigate([this.authService.restaurantId, 'main']);
  }
}
