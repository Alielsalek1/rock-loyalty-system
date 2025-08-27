import {
  AfterViewInit,
  Component,
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import { AuthService } from '../../auth/auth.service';
import { NgForm } from '@angular/forms';
import { User } from '../../shared/modules/user.module';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css',
})
export class ProfileComponent implements OnInit {
  @ViewChild('myForm') myForm: NgForm;
  name: string;
  email: string;
  phoneNumber: string;
  constructor(private authService: AuthService, private toastrService: ToastrService) { }

  ngOnInit() {
    const user: User = this.authService.currentUser;
    this.name = user.name;
    this.email = user.email;
    this.phoneNumber = user.phonenumber;
  }

  onSubmit() {
    const user: User = this.authService.currentUser;
    user.email = this.myForm.value.email;
    user.phonenumber = this.myForm.value.phoneNumber;
    user.name = this.myForm.value.name;
    this.authService.updateUserInfo(user).subscribe({
      next: () => {
        this.toastrService.success("User Info Updated");
      },
      error: () => {
        this.toastrService.error("Unkown error");
      }
    });
  }

  onLogOut() {
    this.authService.LogOut();
  }
}
