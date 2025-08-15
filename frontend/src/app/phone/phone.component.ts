import { Component, ViewChild } from '@angular/core';
import { AuthService } from '../../shared/services/auth.service';
import { NgForm } from '@angular/forms';
import { User } from '../../shared/modules/user/user.module';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-phone',
  templateUrl: './phone.component.html',
  styleUrl: './phone.component.css',
})
export class PhoneComponent {
  @ViewChild('loginForm') form: NgForm;
  constructor(private authService: AuthService, private router: Router) {}
  loading: boolean = false;
  onSubmit() {
    const phone: string = this.form.value.phone;
    const user: User = this.authService.currentUser;
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
        next: () => {
          this.router.navigate(['/main']);
        },
        error: () => {},
      });
  }
}
