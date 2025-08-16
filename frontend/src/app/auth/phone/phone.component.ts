import { Component, ViewChild } from '@angular/core';
import { AuthService } from '../auth.service';
import { NgForm } from '@angular/forms';
import { User } from '../../shared/modules/user.module';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-phone',
  templateUrl: './phone.component.html',
  styleUrl: './phone.component.css',
})
export class PhoneComponent {
  @ViewChild('loginForm') form: NgForm;
  constructor(
    private authService: AuthService,
    private router: Router,
    private toastrService: ToastrService
  ) {}
  loading: boolean = false;
  
  onSubmit() {
    const phone: string = this.form.value.phone;
    const user: User = this.authService.currentUser;
    console.log('Current user before update:', user);
    console.log('Phone to be set:', phone);
    user.phonenumber = phone;
    this.loading = true;
    this.authService
      .updateUserInfo(user)
      .pipe(
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe({
        next: (response) => {
          console.log('Update successful:', response);
          this.toastrService.success('redirecting in 5 seconds');
          setTimeout(() => {
            this.router.navigate([this.authService.restaurantId, 'main']);
          }, 5000);
        },
        error: (error) => {
          console.error('Update failed:', error);
          console.error('Error status:', error.status);
          console.error('Error message:', error.error);
          this.toastrService.error('Failed to update phone number');
        }
      });
  }
}
