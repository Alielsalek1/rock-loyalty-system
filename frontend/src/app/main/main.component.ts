import { Component } from '@angular/core';
import { AuthService } from '../auth/auth.service';

@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrl: './main.component.css',
})
export class MainComponent {
  constructor(private authService: AuthService) {}
  onLogOut() {
    this.authService.LogOut();
  }
  printUser() {
    console.log(this.authService.user.getValue());
  }
}
