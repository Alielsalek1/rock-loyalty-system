import { Component, ViewChild } from '@angular/core';
import { AuthService } from '../auth.service';
import { NgModel } from '@angular/forms';
import { finalize } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-forget-password-request',
  templateUrl: './forget-password-request.component.html',
  styleUrl: './forget-password-request.component.css',
})
export class ForgetPasswordRequestComponent {
  errorMsg: string;
  loading: boolean;
  @ViewChild('emailInput') emailInput: NgModel;
  constructor(
    private authService: AuthService,

    private toastrService: ToastrService
  ) {}
  onUpdate() {
    this.loading = true;
    this.authService
      .forgotPassword(this.emailInput?.value)
      .pipe(
        finalize(() => {
          this.loading = false;
        })
      )
      .subscribe({
        next: () => {
          this.toastrService.success('change password email sent');
        },
        error: (error: Error) => {
          this.toastrService.error(error.message);
        },
      });
  }
}
