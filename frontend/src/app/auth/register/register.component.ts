import { Component, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { AuthService } from '../auth.service';
import { finalize } from 'rxjs';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { FacebookAuthService } from '../social-login/facebook-auth.service';
import { GoogleAuthService } from '../social-login/google-auth.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrl: './register.component.css',
})
export class RegisterComponent implements OnInit {
  @ViewChild('registerForm') form: NgForm;
  isLoading: boolean = false;
  loading: boolean = false;
  loadingMessage: string = '';
  restuarantId: string;
  constructor(
    private authService: AuthService,
    private toastrService: ToastrService,
    private facebookAuth: FacebookAuthService,
    private googleAuth: GoogleAuthService,
    private router: Router
  ) {}
  ngOnInit(): void {
    this.restuarantId = this.authService.restaurantId;
  }

  onSubmit() {
    const values = this.form.value;
    this.loading = true;
    this.loadingMessage = 'Signing in';
    this.authService
    .SignUp(values.name, values.email, values.password, values.phoneNumber)
      .pipe(
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe({
        next: (res) => {
          this.toastrService.success('confirmation email sent');
        },
        error: (err) => {
          this.toastrService.error(err.message);
        },
      });
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
      this.router.navigate(['/main']);
    }, seconds * 1000);
  }
}
