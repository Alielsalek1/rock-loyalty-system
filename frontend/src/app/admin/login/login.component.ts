import { Component, ViewChild, NgModule } from '@angular/core';
import { NgForm, FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-admin-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class AdminLoginComponent {
  @ViewChild('loginForm') form: NgForm;
  loading: boolean = false;
  loadingMessage: string = '';
  
  constructor(
    private router: Router,
    private toastrService: ToastrService,
    private authService: AuthService
  ) {}

  onSubmit() {
    const email: string = this.form.value.email;
    const password: string = this.form.value.password;
    
    this.loading = true;
    this.loadingMessage = 'Logging in...';
    
    this.authService.adminLogin(email, password)
      .pipe(finalize(() => {
        this.loading = false;

        // forced routing for now
        this.router.navigate(['/', this.authService.restaurantId, 'admin', 'main', 'restaurant']);
      }))
      .subscribe({
        next: (response) => {
          this.toastrService.success('Login successful');
          this.router.navigate(['/', this.authService.restaurantId, 'admin', 'main']);
        },
        error: (error) => {
          this.toastrService.error(error.message || 'Login failed');
        },
      });
  }
}